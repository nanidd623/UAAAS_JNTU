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
namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class A415FormatController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        private string serverURL;
        private string test415PDF;
        private string barcodetext;
        private string LatefeeQrCodetext;
        public A415FormatController()
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
        public ActionResult CollegeData(int preview, int collegeId) //string strcollegeId
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

                string pdfPath = SaveCollegeDataPdf(preview, collegeId);
                string path = pdfPath.Replace("/", "\\");

                string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
                return File(path, "application/pdf", "A-417-" + collegeCode + ".pdf");
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
            //PageEventHelper pageEventHelper = new PageEventHelper();
            //writer.PageEvent = pageEventHelper;

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            string collegeName = db.jntuh_college.Find(collegeId).collegeName;

            if (preview == 0)
            {
                //fullPath = path + "/CollegeData/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                //PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

                //fullPath = path + "/CollegeData/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                fullPath = path + "/CollegeData/A-417_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                // iTextEvents.formType = "A-414";
                iTextEvents.formType = "A-417";
                pdfWriter.PageEvent = iTextEvents;
            }
            else
            {
                //fullPath = path + "/temp/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                // PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

                //fullPath = path + "/CollegeData/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                fullPath = path + "/CollegeData/A-417_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
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
            //sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-414.html"));
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-417.html"));
            //EventLog.WriteEntry("open file", "öpen file");
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            string tMethodNumber = db.jntuh_settings.Where(s => s.isLive == true).Select(s => s.testMobile).FirstOrDefault();
            string tCollegeID = db.jntuh_settings.Where(s => s.isLive == true).Select(s => s.testEmail).FirstOrDefault();

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
            //A117
             //   contents = LaboratoriesDetails1(collegeId, contents);
            //EventLog.WriteEntry("LaboratoriesDetails", "LaboratoriesDetails");
           
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("16")))
               contents = LibraryDetails(collegeId, contents);
            //EventLog.WriteEntry("LibraryDetails", "LibraryDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("17")))
                contents = InternetBandwidthDetails(collegeId, contents);
            //EventLog.WriteEntry("InternetBandwidthDetails", "InternetBandwidthDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("18")))
                contents = LegalSystemSoftwareDetails(collegeId, contents);
            //EventLog.WriteEntry("LegalSystemSoftwareDetails", "LegalSystemSoftwareDetails");
            if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("19")))
                contents = PrintersDetails(collegeId, contents);
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
               contents = StudentsPlacementDetails(collegeId, contents);
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
            System.GC.Collect();
           // EventLog.WriteEntry("DataModifications", "DataModifications");
            //if (collegeId != TestCollegeId || (collegeId == TestCollegeId && !TestMethodNumber.Contains("15")))
            //    contents = ExperimentDetails(collegeId, contents);
           
            
            contents = PaymentBillDetails(collegeId, contents);
            //contents = barcodegenerator(collegeId, contents);

            contents = LateFeePaymentBillDetails(collegeId, contents);
            //contents = LateFeebarcodegenerator(collegeId, contents);
            contents = NOofPhysicalLabs(collegeId, contents);



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
                    //try
                    //{
                   
                    //}
                    //catch (DbEntityValidationException dbEx)
                    //{
                    //    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    //    {
                    //        foreach (var validationError in validationErrors.ValidationErrors)
                    //        {
                    //            Trace.TraceInformation("Property: {0} Error: {1}",
                    //                validationError.PropertyName,
                    //                validationError.ErrorMessage);
                    //        }
                    //    }
                    //}

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

                return contents;
            }
        }

        private string collegeInformation(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
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
                var CollegeType = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                        .Select(c => new { c.jntuh_college_type.collegeType, c.isActive }).Where(s => s.isActive == true).ToList();
                strCollegeType += CollegeType.FirstOrDefault().collegeType;
                contents = contents.Replace("##COLLEGE_TYPE##", strCollegeType);

                string strCollegeStatus = string.Empty;
                //List<jntuh_college_status> jntuh_college_status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();

                var CollegeStatus = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
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
                //int NACId = 0;
                //string affiliationNAAC = string.Empty;
                //int affiliationNAACId = 0;
                //string affiliationNAACFromDate = string.Empty;
                //string affiliationNAACToDate = string.Empty;
                //string affiliationNAACYes = string.Empty;
                //string affiliationNAACNo = string.Empty;
                //string affiliationNAACGrade = string.Empty;
                //string affiliationNAACCGPA = string.Empty;
                //string collegeAffiliationType = string.Empty;
                //string yes = "no_b";
                //string no = "no_b";
                //List<jntuh_affiliation_type> affiliationType = db.jntuh_affiliation_type.OrderBy(a => a.id).Where(a => a.isActive == true).ToList();
                //foreach (var item in affiliationType)
                //{
                //    if (item.affiliationType.Trim() == "NAAC")
                //    {
                //        affiliationNAAC = item.affiliationType.Trim();
                //        affiliationNAACId = item.id;
                //    }
                //    else
                //    {
                //        if (item.affiliationType.Trim() == "NBA Status")
                //        {
                //            collegeAffiliationType += "<tr>";
                //            collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='5'>";
                //            collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                //            collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred, Period </td>";
                //            collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                //            collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                //            collegeAffiliationType += "</tr>";
                //            collegeAffiliationType += "<br />";
                //            NACId = item.id;
                //        }
                //        else
                //        {
                //            collegeAffiliationType += "<tr>";
                //            collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='5'>";
                //            collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                //            collegeAffiliationType += "<td valign='top' colspan='4'>If Yes, Period </td>";
                //            collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //            collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                //            collegeAffiliationType += "To:##AFFILIATIONTYPETODATE" + item.id + "## <br/>";
                //            collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                //            collegeAffiliationType += "</tr>";
                //            collegeAffiliationType += "<br />";
                //        }
                //    }
                //}

                //List<jntuh_college_affiliation> affiliationTypeDetails = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId).ToList();
                //foreach (var affiliation in affiliationTypeDetails)
                //{
                //    if (affiliationNAACId == affiliation.affiliationTypeId)
                //    {
                //        affiliationNAACYes = "yes_b";
                //        affiliationNAACNo = "no_b";
                //        if (affiliation.affiliationGrade != null)
                //        {
                //            affiliationNAACGrade = affiliation.affiliationGrade;
                //        }
                //        if (affiliation.CGPA != null)
                //        {
                //            affiliationNAACCGPA = affiliation.CGPA;
                //        }
                //        if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                //        {
                //            string fromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                //            string toDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                //            affiliationNAACFromDate = fromDate;
                //            affiliationNAACToDate = ((Convert.ToInt32(toDate.Substring(toDate.Length - 4))) - (Convert.ToInt32(fromDate.Substring(fromDate.Length - 4)))).ToString();
                //        }
                //    }
                //    if (affiliation.affiliationTypeId == NACId)
                //    {
                //        string image = string.Empty;
                //        if (affiliation.affiliationFromDate != null)
                //        {
                //            string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                //            if (affiliation.affiliationToDate != null)
                //            {
                //                string duration = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                //                collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", ((Convert.ToInt32(duration.Substring(duration.Length - 4))) - (Convert.ToInt32(fDate.Substring(fDate.Length - 4)))).ToString());
                //            }
                //            else
                //            {
                //                collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", affiliation.affiliationDuration.ToString());
                //            }
                //            image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                //        }
                //        else if (affiliation.affiliationStatus == "Applied")
                //        {
                //            image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                //        }
                //    }
                //    else
                //    {
                //        if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                //        {
                //            yes = "yes_b";
                //            no = "no_b";
                //            string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                //            string tDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                //            string duration = affiliation.affiliationDuration.ToString();
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + affiliation.affiliationTypeId + "##", tDate);
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", duration);
                //            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                //        }
                //    }


                //}
                //foreach (var item in affiliationType)
                //{
                //    if (item.affiliationType.Trim() == "NBA Status")
                //    {
                //        string image = string.Empty;
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                //        image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", image);
                //    }
                //    else
                //    {
                //        yes = "no_b";
                //        no = "yes_b";
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + item.id + "##", string.Empty);
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                //        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                //    }
                //}
                //if (affiliationNAAC == "NAAC")
                //{
                //    string image = string.Empty;
                //    int nackid = db.jntuh_affiliation_type.Where(at => at.affiliationType == "NAAC").Select(at => at.id).FirstOrDefault();

                //    var nackatype = db.jntuh_college_affiliation.Where(at => at.affiliationTypeId == nackid && at.collegeId == collegeId).Select(at => at).FirstOrDefault();
                //    if (nackatype != null)
                //    {
                //        if (nackatype.affiliationFromDate != null && nackatype.affiliationToDate != null)
                //        {
                //            image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //        }
                //        else if (nackatype.affiliationStatus == "Applied")
                //        {
                //            image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //        }
                //        else
                //        {
                //            image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                //        }
                //    }
                //    else
                //    {
                //        image = string.Format("<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");

                //    }
                //    collegeAffiliationType += "<tr>";
                //    if (nackatype != null)
                //    {
                //        collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='5'>";
                //        collegeAffiliationType += image;
                //        collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                //        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='5'>From: " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationFromDate.ToString()).ToString() + "<br/>";
                //        collegeAffiliationType += "To:&nbsp; " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationToDate.ToString()) + "<br/>";
                //        collegeAffiliationType += "Duration: " + nackatype.affiliationDuration + "<br/>";
                //        collegeAffiliationType += "Grade: " + nackatype.affiliationGrade + "<br/>";
                //        collegeAffiliationType += "CGPA: " + nackatype.CGPA + "</td>";
                //    }
                //    else
                //    {
                //        collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='5'>";
                //        collegeAffiliationType += image;
                //        collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                //        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                //        collegeAffiliationType += "<td valign='top' colspan='5'>From: <br/>";
                //        collegeAffiliationType += "To:&nbsp; <br/>";
                //        collegeAffiliationType += "Duration: <br/>";
                //        collegeAffiliationType += "Grade:<br/>";
                //        collegeAffiliationType += "CGPA: </td>";
                //    }

                //    collegeAffiliationType += "</tr>";
                //    collegeAffiliationType += "<br />";
                //}
                //contents = contents.Replace("##COLLEGE_AFFILIATIONTYPES##", collegeAffiliationType);

                #endregion

                #region from jntuh_college_degree table

                string strCollegeDegree = string.Empty;
                strCollegeDegree += "<table border='0' cellspacing='0' cellpadding='0'><tbody><tr>";
                //List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(degree => degree.isActive == true).ToList();
                var collegeDegree = db.jntuh_college_degree.Where(degree => degree.isActive && degree.collegeId == collegeId)
                    .Select(g => new { g.jntuh_degree.degree, g.isActive }).ToList();

                //.Where(d => d.isActive == true).ToList();
                int count = 0;
                foreach (var item in collegeDegree)
                {
                    //strCollegeDegree += "<td width='10%'>" + string.Format("{0}&nbsp; {1}", "##COLLEGEDEGREEIMAGE" + item.id + "##", item.degree) + "</td>";
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
                return contents;
            }
        }

        private string PaymentBillDetails(int collegeId, string contents)
        {
            string Paymentbilldetails = string.Empty;
            string strPaymentDate = string.Empty;
            Paymentbilldetails = "";
            Paymentbilldetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            Paymentbilldetails += "<tbody>";
            Paymentbilldetails += "<tr>";
            Paymentbilldetails += "<td colspan='4'><p align='left'>Payment Date</p></td>";
            Paymentbilldetails += "<td colspan='3'><p align='left'>Reference Number</p></td>";
            Paymentbilldetails += "<td colspan='3'><p align='left'>Transaction Amount</p></td>";
            Paymentbilldetails += "</tr>";
            string collegecode = db.jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeCode).FirstOrDefault();
            List<jntuh_paymentresponse> payment = db.jntuh_paymentresponse.Where(a => a.CollegeId == collegecode && a.AuthStatus == "0300" && (a.PaymentTypeID == 7 || a.PaymentTypeID == 0)).ToList();

            if (payment != null)
            {
                foreach (var item in payment.Take(1))
                {
                    if (item.TxnDate != null)
                    {
                        strPaymentDate = item.TxnDate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.TxnDate.ToString()).ToString();
                    }
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td colspan='4'><p align='left'>" + strPaymentDate + "</p></td>";
                    Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnReferenceNo + "</td>";
                    Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnAmount + "</td>";
                    Paymentbilldetails += "</tr>";
                    barcodetext += ";Payment Date:" + strPaymentDate + ";Customer Id:" + item.CustomerID;
                }
            }
            Paymentbilldetails += "</tbody></table>";
            contents = contents.Replace("##PaymentDetails1##", Paymentbilldetails);
            var Paymentdate = payment.Select(e => e.TxnDate).FirstOrDefault();
            string paymentdatecurrentformat = Paymentdate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(Paymentdate.ToString()).ToString();
            contents = contents.Replace("##PAYMENTDATE##", paymentdatecurrentformat);

            #region application submission Date

            // var Updateondate = (from i in db.jntuh_college_edit_status join j in db.jntuh_college on i.collegeId equals j.id where (i.IsCollegeEditable == false && j.id == collegeId) select j.updatedOn).FirstOrDefault();
            var Updateondate = db.jntuh_college_edit_status.Where(i => i.IsCollegeEditable == false && i.collegeId == collegeId).Select(I => I.updatedOn).FirstOrDefault();
            var datetime = "";
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
                    strDataModifications += "<td><img src=" + serverURL + "" + strimagedetails + " align='left'  width='100' height='100' /></td>";
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
                return contents;
            }
        }

        public string PrincipalDirectorDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                int directorID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeId && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();

                jntuh_college_principal_director director = db.jntuh_college_principal_director.Find(directorID);
                  string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";
                string strPrincipal = string.Empty;
                ////Principal Details
                var regNo = db.jntuh_college_principal_registered.Where(r => r.collegeId == collegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
                if (!string.IsNullOrEmpty(regNo))
                {
                    var PrincipalDetails = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == regNo);//&& (r.collegeId == null || r.collegeId == collegeId)
                    //.Select(r => r).FirstOrDefault();

                    if (PrincipalDetails != null)
                    {
                        var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();

                        strPrincipal += "<p><strong><u>Details of Principal</u></strong></p><br />";
                        strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                        strPrincipal += "<tbody>";
                        strPrincipal += "<tr>";
                        strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        strPrincipal += "<td colspan='5' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
                        strPrincipal += "<td valign='top' colspan='2' align='center'>BAS</td>";
                        strPrincipal += "<td valign='top' colspan='8' align='center'><b>Yes</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;No</b> " + strcheckList + " &nbsp;&nbsp;<b>&nbsp;&nbsp;Error</b> " + strcheckList + "</td>";
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
                        strPrincipal += "<td valign='top' colspan='5'>" + PrincipalDetails.DateOfAppointment + "</td>";
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
                        strPrincipal += "<td valign='top' colspan='4'>Ratified</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        if (PrincipalDetails.isFacultyRatifiedByJNTU == true)
                        {
                            strPrincipal += "<td colspan='5' valign='top'>Yes</td>";
                        }
                        else
                        {
                            strPrincipal += "<td colspan='5' valign='top'>No</td>";
                        }
                        strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                        strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                        if (!string.IsNullOrEmpty(PrincipalDetails.Photo))
                        {
                            string strPrincipalPhoto = string.Empty;
                            string path = @"~/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo;
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            if (System.IO.File.Exists(path))
                            {
                                strPrincipalPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                strPrincipal += "<td colspan='5' valign='top' >" + strPrincipalPhoto + "</td>";
                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    strPrincipalPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  height='50' />";
                                    strPrincipal += "<td colspan='5' valign='top' >" + strPrincipalPhoto + "</td>";
                                }
                                else
                                {
                                    strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                                }
                            }
                        }
                        else
                        {
                            strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'>&nbsp;</td>";
                        }
                        strPrincipal += "</tr>";
                        strPrincipal += "</tbody>";
                        strPrincipal += "</table>";
                        //strPrincipal += "<br />";

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
                    contents = contents.Replace("##DirectorTitle##", "<p><strong><u>Details of Director:</u></strong></p>");
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
                        string strDirectorPhoto = string.Empty;
                        string path = @"~/Content/Upload/PrincipalDirectorPhotos/" + director.photo;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);

                        if (System.IO.File.Exists(path))
                        {
                            strDirectorPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/PrincipalDirectorPhotos/" + director.photo + "'" + " align='center'  height='50'/>";
                            contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                        }
                        else
                        {
                            if (test415PDF.Equals("YES"))
                            {
                                strDirectorPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/PrincipalDirectorPhotos/" + director.photo + "'" + " align='center'  height='50'/>";
                                contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                            }
                            else
                            {
                                contents = contents.Replace("##DirectorPhoto##", "&nbsp;");
                            }
                        }
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
                        //strLandInformationDetails += "<tr><td width='165' valign='top' style='font-size: 8px;'>Compound Wall/Fencing</td><td width='24' valign='top'>:</td><td width='502'><img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp; &nbsp; &nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</td></tr>";
                    }
                    else
                    {
                        // strLandInformationDetails += "<tr><td width='165' valign='top' style='font-size: 8px;'>Compound Wall/Fencing</td><td width='24' valign='top'>:</td><td width='502'><img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp; &nbsp; &nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No</td></tr>";
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
                            // string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //  yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
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
                            //string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //  yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + "&nbsp; &nbsp; &nbsp;";

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
                            //string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";

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
                            //string yesOrNo = "no_b";
                            if (item.selected == 1)
                            {
                                //yesOrNo = "yes_b";
                                strLandInformationDetails += "&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";
                            }
                            //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_" + yesOrNo + ".png' height='10' />&nbsp;" + item.name + " &nbsp; &nbsp; &nbsp;";

                        }
                        strLandInformationDetails += "</td></tr>";
                    }
                    if (jntuh_college_land.IsPurifiedWater == true)
                    {
                        strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'>&nbsp;Yes &nbsp;&nbsp;&nbsp;</td></tr>";
                        //strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'><img alt='' src='" + serverURL + "/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;&nbsp;<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</td></tr>";
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

                    //strLandInformationDetails += "<tr><td width='56'>" + 1 + "</td><td width='135'>" + string.Empty + "</td><td width='69'>" + string.Empty + "</td><td width='147'>" + string.Empty + "</td><td width='144'>" + string.Empty + "</td><td width='126' valign='top'>" + string.Empty + "</td></tr>";
                    //strLandInformationDetails += "<tr><td width='56'>" + 2 + "</td><td width='135'>" + string.Empty + "</td><td width='69'>" + string.Empty + "</td><td width='147'>" + string.Empty + "</td><td width='144'>" + string.Empty + "</td><td width='126' valign='top'>" + string.Empty + "</td></tr>";
                    //strLandInformationDetails += "<tr><td width='56'>" + 3 + "</td><td width='135'>" + string.Empty + "</td><td width='69'>" + string.Empty + "</td><td width='147'>" + string.Empty + "</td><td width='144'>" + string.Empty + "</td><td width='126' valign='top'>" + string.Empty + "</td></tr>";
                    //strLandInformationDetails += "<tr><td width='56'>" + 4 + "</td><td width='135'>" + string.Empty + "</td><td width='69'>" + string.Empty + "</td><td width='147'>" + string.Empty + "</td><td width='144'>" + string.Empty + "</td><td width='126' valign='top'>" + string.Empty + "</td></tr>";
                    //strLandInformationDetails += "<tr><td width='56'>" + 5 + "</td><td width='135'>" + string.Empty + "</td><td width='69'>" + string.Empty + "</td><td width='147'>" + string.Empty + "</td><td width='144'>" + string.Empty + "</td><td width='126' valign='top'>" + string.Empty + "</td></tr>";

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
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Adequate";
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Inadequate";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //WaterSupply
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Water Supply</td><td width='24' valign='top'>:</td><td width='502'>";
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Adequate";
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Inadequate";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";

                    //Drinkingwater

                    strLandInformationDetails += "<tr><td width='165' valign='top'>Drinking Water</td><td width='24' valign='top'>:</td><td width='502'>";
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Municipal Water";
                    //strLandInformationDetails += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Bore Well Water";
                    strLandInformationDetails += "";
                    strLandInformationDetails += "</td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Is Water Purified?</td><td width='24' valign='top'>:</td><td width='502'></td></tr>";
                    strLandInformationDetails += "<tr><td width='165' valign='top'>Potable water</td><td width='24' valign='top'>:</td><td width='502'>_______________(in Liters per day)</td></tr>";
                }

                strLandInformationDetails += "</tbody></table>";
                contents = contents.Replace("##LandInformationDetails##", strLandInformationDetails);
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
                                            jntuh_program_type = db.jntuh_program_type.Where(p => p.id == r.programId).FirstOrDefault(),
                                            availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                            availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
                                        }).Where(g => g.availableRooms != null && g.availableRooms != 0).ToList();
                if (land != null)
                {
                    foreach (var item in land)
                    {
                        string programType = db.jntuh_program_type.Where(p => p.id == item.programId).Select(p => p.programType).FirstOrDefault();
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
                return contents;
            }
        }

        public string InstructionalAreaDetails(int collegeId, string contents)
        {
            using (var db = new UAAAS.Models.uaaasDBContext())
            {
                string strInstructionalAreaDetails = string.Empty;
                decimal totalArea = 0;
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
                var ids =  programIds.Select(it => it.programId).ToList();
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
                                         availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == r.id).Select(a => a.availableRooms).FirstOrDefault(),
                                         availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == r.id).Select(a => a.availableArea).FirstOrDefault()
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
                int totalApproved = 0;
                int totalAdmited = 0;
                int totalPAYApproved = 0;
                int totalPAYAdmited = 0;
                int totalPAYProposed = 0;
                var jntuh_academic_year = db.jntuh_academic_year.AsNoTracking().ToList();
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
                strExistingIntakeDetails += "<td width='56' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
                strExistingIntakeDetails += "<td width='63' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
                strExistingIntakeDetails += "<td width='200' rowspan='3' colspan='4'><p align='left'>Specialization</p><p align='left'>***</p></td>";
                strExistingIntakeDetails += "<td width='42' rowspan='3' colspan='1' style='font-size: 9px; line-height: 10px;'><p align='center'>Shift</p><p align='center'>#</p></td>";
                strExistingIntakeDetails += "<td width='500' colspan='10'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CS</p></td>";
                strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>CF</p></td>";
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

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId).ToList();
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                var specializationIds = intake.Select(it => it.specializationId).ToList();
                
                var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
                   .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
                var departmentIds =   jntuh_specializations.Select(it => it.departmentId).ToList();
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
                foreach (var item in collegeIntakeExisting)
                {
                    sno++;

                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());
                    item.ProposedIntake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();
                    if (item.ProposedIntake != null)
                        totalPAYProposed += (int)item.ProposedIntake;
                    item.courseStatus = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.courseStatus).FirstOrDefault();
                   // if (item.ApprovedIntake != null)
                    //    totalPAYApproved += (int)item.ApprovedIntake;
                    //totalPAYAdmited += item.admittedIntake;
                    if (item.courseStatus == "Closure")
                    {
                        item.courseStatus = "C";
                    }
                    else if (item.courseStatus == "New")
                    {
                        item.courseStatus = "N";
                    }
                    else if (item.courseStatus == "Increase")
                    {
                        item.courseStatus = "I";
                    }
                    else if (item.courseStatus == "Nochange")
                    {
                        item.courseStatus = "NC";
                    }
                    else if (item.courseStatus == "Decrease")
                    {
                        item.courseStatus = "D";
                    }
                    else
                    {
                        item.courseStatus = "";
                    }


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

                    strExistingIntakeDetails += "<tr>";
                    strExistingIntakeDetails += "<td colspan='1' width='28'><p align='center'>" + sno + "</p></td>";
                    strExistingIntakeDetails += "<td colspan='3' width='56'>" + item.Degree + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='63'>" + item.Department + "</td>";
                    strExistingIntakeDetails += "<td colspan='4' width='200'>" + item.Specialization + "</td>";
                    strExistingIntakeDetails += "<td colspan='1' width='42'>" + item.Shift + "</td>";
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
                    strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'>" + item.courseStatus + "</td>";
                    strExistingIntakeDetails += "<td colspan='1'></td>";
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
                // totalAdmited += totalAdmittedIntake1 + totalAdmittedIntake2 + totalAdmittedIntake3 + totalAdmittedIntake4 + totalAdmittedIntake5 + totalPAYAdmited;
                // totalApproved += totalApprovedIntake1 + totalApprovedIntake2 + totalApprovedIntake3 + totalApprovedIntake4 + totalApprovedIntake5 + totalPAYApproved;

                strExistingIntakeDetails += "<tr>";
                strExistingIntakeDetails += "<td width='337' colspan='13'><p align='right'>Total =</p></td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake5 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake5 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake4 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake4 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake3 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake3 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake2 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake2 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalApprovedIntake1 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalAdmittedIntake1 + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYApproved + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td>";
                strExistingIntakeDetails += "<td width='50' colspan='1' valign='top' align='center'></td>";
                //strExistingIntakeDetails += "<td width='50' colspan='1' valign='top' align='center'></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                strExistingIntakeDetails += "<td width='50' colspan='2' valign='top' align='center'></td>";
                strExistingIntakeDetails += "</tr>";
                strExistingIntakeDetails += "<tr><td colspan='13'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='18' width='600'>" + totalAdmited + '/' + totalApproved + "</td></tr>";
                strExistingIntakeDetails += "</tbody></table>";
                contents = contents.Replace("##ExistingIntakeDetails##", strExistingIntakeDetails);

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
                string AcademicYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(a => a.actualYear).FirstOrDefault();
                int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();
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
                var specializationIds=  performance.Select(it => it.specializationId).ToList();
                var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
                    .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
                var departmentIds  = jntuh_specializations.Select(it => it.departmentId).ToList();
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
                else
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
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
                int[] collegeSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId &&e.academicYearId==8).Select(e => e.specializationId).Distinct().ToArray();
                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C => C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();
                if (DegreeIds.Contains(4))
                    SpecializationId = 39;
                else
                    SpecializationId = 0;


                List<Lab> labs=new List<Lab>();

                if (CollegeAffiliationStatus == "Yes")
                {
                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();
                    //labs = (from lm in db.jntuh_lab_master.AsNoTracking()
                    //        join l in db.jntuh_college_laboratories.AsNoTracking() on new { ID = lm.id, collegeId = collegeId } equals new { ID = l.EquipmentID, collegeId = l.CollegeID }
                    //          into all
                    //          from m in all.DefaultIfEmpty()
                    //        where (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId)  //&& ((lm.jntuh_degree.degreeTypeId != 1 && !string.IsNullOrEmpty(m.Make)) || (lm.jntuh_degree.degreeTypeId == 1))
                    //        select new Lab
                    //        {
                    //            EquipmentID = lm.id,
                    //            tempEquipmentId = m.EquipmentID,
                    //            degree = lm.jntuh_degree.degree,
                    //            degreeId = lm.DegreeID,
                    //            department = lm.jntuh_department.departmentName,
                    //            specializationName = lm.jntuh_specialization.specializationName,
                    //            Semester = lm.Semester,
                    //            year = lm.Year,
                    //            Labcode = lm.Labcode,
                    //            LabName = lm.LabName,
                    //            AvailableArea = m.AvailableArea,
                    //            RoomNumber = m.RoomNumber,
                    //            EquipmentName = lm.EquipmentName ?? m.EquipmentName,
                    //            Make = m.Make,
                    //            Model = m.Model,
                    //            EquipmentUniqueID = m.EquipmentUniqueID,
                    //            AvailableUnits = m.AvailableUnits,
                    //            specializationId = lm.SpecializationID,
                    //            CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                    //            isCircuit = lm.jntuh_department.CircuitType,
                    //            DisplayOrder = lm.jntuh_department.DisplayOrder,
                    //            degreeTypeId = lm.jntuh_degree.degreeTypeId,
                    //            ViewEquipmentPhoto = m.EquipmentPhoto,
                    //            EquipmentDateOfPurchasing = m.EquipmentDateOfPurchasing,
                    //            DelivaryChalanaDate = m.DelivaryChalanaDate
                    //        }).ToList();


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
                    //labs = (from lm in db.jntuh_lab_master.AsNoTracking()
                    //        join l in db.jntuh_college_laboratories.AsNoTracking() on new { ID = lm.id } equals new { ID = l.EquipmentID }
                    //         into all
                    //        from m in all.DefaultIfEmpty()
                    //        where (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && lm.CollegeId==null && m.CollegeID==collegeId  //&& ((lm.jntuh_degree.degreeTypeId != 1 && !string.IsNullOrEmpty(m.Make)) || (lm.jntuh_degree.degreeTypeId == 1))
                    //        select new Lab
                    //        {
                    //            EquipmentID = lm.id,
                    //            tempEquipmentId = m.EquipmentID,
                    //            degree = lm.jntuh_degree.degree,
                    //            degreeId = lm.DegreeID,
                    //            department = lm.jntuh_department.departmentName,
                    //            specializationName = lm.jntuh_specialization.specializationName,
                    //            Semester = lm.Semester,
                    //            year = lm.Year,
                    //            Labcode = lm.Labcode,
                    //            LabName = lm.LabName,
                    //            AvailableArea = m.AvailableArea,
                    //            RoomNumber = m.RoomNumber,
                    //            EquipmentName = lm.EquipmentName ?? m.EquipmentName,
                    //            Make = m.Make,
                    //            Model = m.Model,
                    //            EquipmentUniqueID = m.EquipmentUniqueID,
                    //            AvailableUnits = m.AvailableUnits,
                    //            specializationId = lm.SpecializationID,
                    //            CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                    //            isCircuit = lm.jntuh_department.CircuitType,
                    //            DisplayOrder = lm.jntuh_department.DisplayOrder,
                    //            degreeTypeId = lm.jntuh_degree.degreeTypeId,
                    //            ViewEquipmentPhoto = m.EquipmentPhoto,
                    //            EquipmentDateOfPurchasing = m.EquipmentDateOfPurchasing,
                    //            DelivaryChalanaDate = m.DelivaryChalanaDate
                    //        }).ToList();
                    List<Lab> labsuploaded = new List<Lab>();
                    List<Lab> labsnotuploaded = new List<Lab>();

                    labsuploaded = (from lm in db.jntuh_lab_master.AsNoTracking()
                                    join l in db.jntuh_college_laboratories.AsNoTracking() on lm.id equals l.EquipmentID
                                    where Equipmentsids.Contains(lm.id) && (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == SpecializationId) && l.CollegeID ==collegeId && lm.CollegeId==null
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
                                           tempEquipmentId =0,//l.EquipmentID,
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



                
               
                int[] SpecializationIDs;
                if (DegreeIds.Contains(4))
                    SpecializationIDs =(from a in labs orderby a.department select a.specializationId).Distinct().ToArray();
                        //labs.Select(l => l.specializationId).Distinct().ToArray();
                else
                    SpecializationIDs = (from a in labs where a.specializationId!=39 orderby a.department select a.specializationId).Distinct().ToArray();
                        //labs.Where(l => l.specializationId != 39).Select(l => l.specializationId).Distinct().ToArray();
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
                    string strLabName = string.Empty;
                    sno = 1;
                    var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);

                    strLabName = "Degree: " + specializationDetails.degree + ",&nbsp;&nbsp;&nbsp; Department: " + specializationDetails.department + ",&nbsp;&nbsp;&nbsp; Specilization: " + specializationDetails.specialization;// +",&nbsp;&nbsp;&nbsp; Year: " + specilizationdetails.year + ",&nbsp;&nbsp;&nbsp; Semester: " + specilizationdetails.semester;
                    //strLaboratoriesDetails += strLabName + "<br />";
                    strLaboratoriesDetails += "<table border='1' cellspacing='0' cellpadding='3'>";
                    strLaboratoriesDetails += "<thead>";
                    strLaboratoriesDetails += "<tr>";
                    //if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                    //{
                    strLaboratoriesDetails += "<th colspan='39'> <strong>" + strLabName + "</strong></th>";
                    //}
                    //else
                    //{
                    //    strLaboratoriesDetails += "<th colspan='35'> <strong>" + strLabName + "</strong></th>";
                    //}
                    strLaboratoriesDetails += "</tr>";

                    strLaboratoriesDetails += "<tr>";
                    strLaboratoriesDetails += "<th  colspan='1'><p align='center'>S.No</p></th>";
                    //strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Degree</p></th>";
                    //strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Dept.</p></th>";
                    //strLaboratoriesDetails += "<th  colspan='7'><p align='left'>Specialization</p></th>";

                    // Commented By Srinivas
                    //if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                    //{
                    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>Year</p></th>";
                    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>Sem.</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Lab Code</p></th>";
                    //}
                    strLaboratoriesDetails += "<th  colspan='7'><p align='left'>Lab Name</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Area (in Sqm)</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Room No</p></th>";
                    strLaboratoriesDetails += "<th  colspan='6'><p align='left'>Equipment Name</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Make</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Model</p></th>";
                    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Equipment UniqueID</p></th>";
                    strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Equipment Photo</p></th>";
                    strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Equipment DateOfPurchase </p></th>";
                    strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Delivery ChallanDate</p></th>";
                    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>AU</p></th>";
                    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>AUWC</p></th>";
                    strLaboratoriesDetails += "</tr>";
                    strLaboratoriesDetails += "</thead>";

                    strLaboratoriesDetails += "<tbody>";
                    string strLaboratoriesDetails1 = "";





                     List<Lab> SpecializationWiseLabs=new List<Lab>();
                  //  SpecializationWiseLabs =labs.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ThenBy(e => e.Labcode).ToList();
                    foreach (var item in labs.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ThenBy(e => e.Labcode).ToList())//
                    {
                        try
                        {
                            

                            if (item.ViewEquipmentPhoto != null && item.ViewEquipmentPhoto != "")
                            {
                                strviewEquimentdata = "/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto;
                            }
                            else
                            {
                                //strviewEquimentdata = string.Empty;
                                strviewEquimentdata = "/Content/Images/no-photo.gif";

                            }
                            string path = @"~" + strviewEquimentdata.Replace("%20", " ");
                          // serverURL = "http://112.133.193.233:75/";
                            path = System.Web.HttpContext.Current.Server.MapPath(path);

                            if (item.DelivaryChalanaDate != null)
                            {
                                chalanaDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DelivaryChalanaDate.ToString());
                            }
                            else
                            {
                                chalanaDate = "";
                            }


                            if (item.EquipmentDateOfPurchasing != null)
                            {
                                Equipmentdate =
                                    UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.EquipmentDateOfPurchasing.ToString());
                            }
                            else
                            {
                                Equipmentdate = "";
                            }


                            strLaboratoriesDetails += "<tr>";
                            strLaboratoriesDetails += "<td  align='center' colspan='1'><p align='center'>" + sno +
                                                      "</p></td>";
                            //strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.degree + "</td>";
                            //strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.department + "</td>";
                            //strLaboratoriesDetails += "<td  align='left' colspan='7'>" + item.specializationName + "</td>";

                            //  Comented By Srinivas
                            //if (item.degree == "B.Tech" || item.degree == "B.Pharmacy")
                            //{
                            strLaboratoriesDetails += "<td  align='left' colspan='1'>" + item.year + "</td>";
                            strLaboratoriesDetails += "<td  align='left' colspan='1'>" + item.Semester + "</td>";
                            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Labcode + "</td>";
                            //}
                            if (item.LabName != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='7'>" +
                                                          item.LabName.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='7'>" + item.LabName + "</td>";
                            }

                            if (item.AvailableArea != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" +
                                                          item.AvailableArea.ToString().Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.AvailableArea +
                                                          "</td>";
                            }

                            if (item.RoomNumber != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" +
                                                          item.RoomNumber.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.RoomNumber + "</td>";
                            }

                            if (item.EquipmentName != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='6'>" +
                                                          item.EquipmentName.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='6'>" + item.EquipmentName +
                                                          "</td>";
                            }

                            if (item.Make != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" +
                                                          item.Make.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Make + "</td>";
                            }

                            if (item.Model != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" +
                                                          item.Model.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Model + "</td>";
                            }

                            if (item.EquipmentUniqueID != null)
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" +
                                                          item.EquipmentUniqueID.Replace("&", "&amp;") + "</td>";
                            }
                            else
                            {
                                strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.EquipmentUniqueID +
                                                          "</td>";
                            }

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
                                if (System.IO.File.Exists(path))
                                {
                                    //server 3

                                    if (item.ViewEquipmentPhoto == "380-52436-20170308-141942183287-f.jpg")
                                    {
                                        
                                    }

                                    strLaboratoriesDetails1 += "<p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p>";
                                    var ParseEliments = HTMLWorker.ParseToList(new StringReader(strLaboratoriesDetails1), null);

                                    if (path.Contains("."))
                                        strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                                    else
                                        strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>--------</p></td>";


                                    //server 1
                                    // strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";



                                }
                                else
                                {
                                    strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'></p></td>";
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
                                Equipmentdate = chalanaDate = "";
                                continue;
                            }
                            #endregion

                           


                            //if (!string.IsNullOrEmpty(item.ViewEquipmentPhoto))
                            //{
                            //    if (System.IO.File.Exists(path))
                            //    {

                            //        strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";

                            //    }
                            //    else
                            //    {
                            //        strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>---</p></td>";
                            //    }
                            //}
                            //else
                            //{
                            //    strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>---</p></td>";
                            //}






                            #region Html PArsing

                            //strLaboratoriesDetails1 = "";
                            //strLaboratoriesDetails1 = strLaboratoriesDetails;
                            //strLaboratoriesDetails1 += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";
                            //try
                            //{
                            //    // var ParseEliments = HTMLWorker.ParseToList(new StringReader(strLaboratoriesDetails1), null);

                            //    if (System.IO.File.Exists(path))
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
                            //        strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";
                            //        //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";

                            //        //strLaboratoriesDetails += "<td  width='100' align='center' colspan='3'><p align='center'>"+ HtmlEncoder.Encode(path.Trim()).Replace("'", "&#39;") + "</p></td>";

                            //    }
                            //    else
                            //    {
                            //        strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'></p></td>";
                            //    }
                            //}
                            //catch
                            //{
                            //    strLaboratoriesDetails += "<td width='100' align='center' colspan='3'><p align='center'>--</p></td>";
                            //    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                            //    strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";
                            //    strLaboratoriesDetails += "<td  align='right' colspan='1' style='width:10%;' >" + item.AvailableUnits + "</td>";
                            //    strLaboratoriesDetails += "<td  align='left' colspan='1'></td>";
                            //    strLaboratoriesDetails += "</tr>";
                            //    sno++;
                            //    Equipmentdate = chalanaDate = "";
                            //    continue;
                            //}

                            #endregion

                            strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                            strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";
                            strLaboratoriesDetails += "<td  align='right' colspan='1' style='width:10%;' >" +
                                                      item.AvailableUnits + "</td>";
                            strLaboratoriesDetails += "<td  align='left' colspan='1'></td>";
                            strLaboratoriesDetails += "</tr>";
                            sno++;
                            Equipmentdate = chalanaDate = "";
                        }
                        catch (Exception x)
                        {
                            throw x;
                        }


                    }
                    strLaboratoriesDetails += "</tbody></table>";
                }
              //  strLaboratoriesDetails += GetFirstYearLabDetails(collegeId, contents);
                strLaboratoriesDetails += GetNonCircuteLabSummarySheet(labs, collegeId);
                strLaboratoriesDetails += "<br />";
                contents = contents.Replace("##LaboratoriesDetails##", strLaboratoriesDetails);
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

                var jntuh_specialization = db.jntuh_specialization.ToList();

                int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 8).Select(e => e.specializationId).ToArray();
                var DepartmentsData = jntuh_specialization.Where(e => e.isActive == true && specializationIDs.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

                //UAAAS.Controllers.LabsController
                if (DegreeIds.Contains(4))
                {
                    if (specializationIDs.Contains(134))
                    {
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
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
                        ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                          join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                          join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                          join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
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
                }
                else
                {
                    ObjPhysicalLab = (from lab in db.jntuh_lab_master
                                      join deg in db.jntuh_degree on lab.DegreeID equals deg.id
                                      join dep in db.jntuh_department on lab.DepartmentID equals dep.id
                                      join spec in db.jntuh_specialization on lab.SpecializationID equals spec.id
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

            }).OrderBy(e=>e.DeptId).ToList();



            foreach (var deptid in DeptIds)
            {
                string strDepartmentName = string.Empty;
                var departmentDetails = Departments.FirstOrDefault(s => s.DeptId == deptid);

                strDepartmentName = "Degree: " + departmentDetails.degree + ",&nbsp;&nbsp;&nbsp; Department: " + departmentDetails.department;

                strExperimentaldetails += "<strong><u>" + strDepartmentName + "</u></strong> <br /> <br />";
                strExperimentaldetails += "<table border='1' style='font-size: 9px;' cellspacing='0' cellpadding='3'>";
                strExperimentaldetails += "<thead>";
                //strExperimentaldetails += "<tr>";
                //strExperimentaldetails += "<th colspan='20'> <strong>" + strDepartmentName + "</strong></th>";
                //strExperimentaldetails += "</tr>";
                strExperimentaldetails += "<tr>";
                strExperimentaldetails += "<td  colspan='2'><p align='center'>S.No</p></td>";
                //strExperimentaldetails += "<td  colspan='3'><p align='center'>Degree</p></td>";
                //strExperimentaldetails += "<td  colspan='2'><p align='center'>Department</p></td>";
                //strExperimentaldetails += "<td  colspan='2'><p align='center'>Specialization</p></td>";
                strExperimentaldetails += "<td  colspan='2'><p align='center'>Year</p></td>";
                strExperimentaldetails += "<td  colspan='2'><p align='center'>Semister</p></td>";
                strExperimentaldetails += "<td  colspan='2'><p align='center'>Lab Code</p></td>";
                strExperimentaldetails += "<td  colspan='8'><p align='center'>Lab Name</p></td>";
                strExperimentaldetails += "<td  colspan='3'><p align='center'>Physical Labs Avaiable</p></td>";
                strExperimentaldetails += "<td  colspan='3'><p align='center'>Commitee Findings</p></td>";
                strExperimentaldetails += "</tr>";
                strExperimentaldetails += "</thead>";
                strExperimentaldetails += "<tbody>";
                int sno = 1;
                foreach (var data in ObjPhysicalLab.Where(e => e.departmentid == deptid).OrderBy(e=>e.year).ThenBy(e=>e.semister).ToList())
                {

                    //if (data.physicalId == 0)
                    //{
                    //    available = "No";
                    //}
                    //else
                    //{
                    //    available = "Yes";
                    //}

                    strExperimentaldetails += "<tr>";
                    strExperimentaldetails += "<td  align='center' colspan='2'><p align='center'>" + sno + "</p></td>";
                    //strExperimentaldetails += "<td  align='left' colspan='3'>" + data.degree + "</td>";
                    //strExperimentaldetails += "<td  align='left' colspan='2'>" + data.department + "</td>";
                    //strExperimentaldetails += "<td  align='left' colspan='2'>" + data.specialization + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='2'>" + data.year + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='2'>" + data.semister + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='2'>" + data.LabCode + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='8'>" + data.Labname + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='3'>" + data.physicalId + "</td>";
                    strExperimentaldetails += "<td  align='left' colspan='3'>" +""+ "</td>";
                    strExperimentaldetails += "</tr>";
                    sno++;
                }
                strExperimentaldetails += "</tbody>";
                strExperimentaldetails += "</table>";
            }
            contents = contents.Replace("##NoofPhysicalLabs##", strExperimentaldetails);
            return contents;
        }



        //public string ExperimentDetails(int collegeId, string contents)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        //((IObjectContextAdapter)db).ObjectContext.CommandTimeout = 10 * 60; ;
        //        string strExperimentaldetails = string.Empty;
        //        int sno = 1;
        //        string available = string.Empty;
        //        // int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //        //int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
        //        int[] specializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).ToArray();
        //        string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
        //        var jntuh_lab_master_experments = db.jntuh_lab_master_experments.Where(it => specializationIDs.Contains(it.id)).Select(it => new
        //        {
        //            it.id,
        //            it.DegreeID,
        //            it.DepartmentID,
        //            it.SpecializationID,
        //            it.Year,
        //            it.Semester,
        //            it.Labcode,
        //            it.LabName,
        //            it.ExperimentName
        //        }).ToList();
        //        var ids = db.jntuh_lab_master_experments
        //            .Where(it => specializationIDs.Contains(it.id) && (it.ExperimentName == null || it.ExperimentName == ""))
        //            .Select(it => it.id).ToList().Select(it => (int?) it).ToList();
        //        var clgexperiments = db.jntuh_college_experiments.Where(it => ids.Contains(it.ExperimentId)).Select(it => new
        //        {
        //            it.ExperimentId,
        //            it.ExperimentName
        //        }).ToList();
        //      List<Lab> experimentdetails =   jntuh_lab_master_experments.Select(E => new Lab
        //        {
        //            id = E.id,
        //            degreeId = E.DegreeID,
        //            departmentId = E.DepartmentID,
        //            specializationId = E.SpecializationID,
        //            year = E.Year,
        //            Semester = E.Semester,
        //            Labcode = E.Labcode,
        //            LabName = E.LabName,
        //            EquipmentName = !String.IsNullOrEmpty(E.ExperimentName) ? E.ExperimentName : clgexperiments.Where(it => it.ExperimentId == E.id).Select(it => it.ExperimentName).FirstOrDefault()
        //        }).ToList();
        //        //List<Lab> experimentdetails = (from E in db.jntuh_lab_master_experments
        //        //                               join EC in db.jntuh_college_experiments
        //        //                               on E.id equals EC.ExperimentId
        //        //                                   //on new { ID = E.id, collegeId = collegeId } equals new { ID = EC.ExperimentId, collegeId = EC.CollegeId }
        //        //                                into all
        //        //                               from EC1 in all.DefaultIfEmpty()
        //        //                               //orderby E.DegreeID, E.DepartmentID, E.SpecializationID
        //        //                               where (specializationIDs.Contains(E.SpecializationID))
        //        //                               select new Lab
        //        //                               {
        //        //                                   id = E.id,
        //        //                                   degreeId = E.DegreeID,
        //        //                                   departmentId = E.DepartmentID,
        //        //                                   specializationId = E.SpecializationID,
        //        //                                   year = E.Year,
        //        //                                   Semester = E.Semester,
        //        //                                   Labcode = E.Labcode,
        //        //                                   LabName = E.LabName,
        //        //                                   EquipmentName = !String.IsNullOrEmpty(E.ExperimentName) ? E.ExperimentName : EC1.ExperimentName,
        //        //                               }).ToList();
        //        if (CollegeAffiliationStatus == "Yes")
        //        {
        //            experimentdetails = experimentdetails.Where(l => l.collegeId == collegeId).OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList();
        //        }
        //        else
        //        {
        //            experimentdetails = experimentdetails.Where(l => l.collegeId == collegeId && l.Labcode != "TMP-EL").OrderBy(l => l.degree).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList();
        //        }

        //        List<jntuh_lab_master_experments> collegeLabMaster = null;
        //        if (CollegeAffiliationStatus == "Yes")
        //        {
        //            collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID, l.Year, l.Semester }).ToList();
        //        }
        //        else
        //        {
        //            collegeLabMaster = db.jntuh_lab_master_experments.AsNoTracking().Where(l => l.Labcode != "TMP-EL" && specializationIDs.Contains(l.SpecializationID)).OrderBy(l => new { l.DegreeID, l.DepartmentID, l.SpecializationID, l.Year, l.Semester }).ToList();
        //        }
        //        var jntuh_college_Experiments = db.jntuh_college_experiments.AsNoTracking().Where(l => l.CollegeId == collegeId).ToList();
        //        List<Lab> labdetails = new List<Lab>();
        //        foreach (var item in collegeLabMaster)
        //        {
        //            Lab list = new Lab();
        //            list.id = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == collegeId).Select(l => l.Id).FirstOrDefault();
        //            list.degree = item.jntuh_degree.degree;
        //            list.department = item.jntuh_department.departmentName;
        //            list.specializationId = item.SpecializationID;
        //            list.specializationName = item.jntuh_specialization.specializationName;
        //            list.year = item.Year;
        //            list.Semester = item.Semester;
        //            list.ExperimentID = item.ExperimentNO;
        //            list.Labcode = item.Labcode;
        //            list.LabName = item.LabName; //!= null && item.LabName != "" ? item.LabName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == userCollegeID).Select(l => l.).FirstOrDefault();
        //            list.ExperimentName = item.ExperimentName != null && item.ExperimentName != "" ? item.ExperimentName : jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == collegeId).Select(l => l.ExperimentName).FirstOrDefault();
        //            list.ExpCount = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == collegeId).ToList().Count;
        //            var exps = jntuh_college_Experiments.Where(l => l.ExperimentId == item.id && l.CollegeId == collegeId).ToList();
        //            labdetails.Add(list);
        //        }

        //         int[] specializationsids = collegeLabMaster.Select(l => l.SpecializationID).Distinct().ToArray();
        //         foreach (var speclializationId in specializationsids)
        //         {
        //             string strLabName = string.Empty;
        //             sno = 1;
        //             string strviewEquimentdata = string.Empty;
        //             var specializationde = (from i in db.jntuh_specialization
        //                                     join j in db.jntuh_lab_master_experments on i.id equals j.SpecializationID
        //                                     where i.id == speclializationId
        //                                     select new
        //                                     {
        //                                         specialization = i.specializationName,
        //                                         department = i.jntuh_department.departmentName,
        //                                         degree = i.jntuh_department.jntuh_degree.degree,
        //                                         year = j.Year,
        //                                         semester = j.Semester
        //                                     }).FirstOrDefault();

        //             strLabName = specializationde.degree + "-" + specializationde.department + "-" + specializationde.specialization + "-" + specializationde.year + " Year-" + specializationde.semester + " Semester";
        //             strExperimentaldetails += "<strong><u>" + strLabName + "</u></strong><br/><br/>";

        //             strExperimentaldetails += "<table border='1' style='font-size: 9px;' cellspacing='0' cellpadding='3'>";
        //             strExperimentaldetails += "<thead>";
        //             strExperimentaldetails += "<tr>";
        //             strExperimentaldetails += "<td  colspan='2'><p align='center'>S.No</p></td>";
        //             //strExperimentaldetails += "<td  colspan='3'><p align='center'>Degree</p></td>";
        //             //strExperimentaldetails += "<td  colspan='2'><p align='center'>Department</p></td>";
        //             //strExperimentaldetails += "<td  colspan='2'><p align='center'>Specialization</p></td>";
        //             //strExperimentaldetails += "<td  colspan='2'><p align='center'>Year</p></td>";
        //             //strExperimentaldetails += "<td  colspan='2'><p align='center'>Semester</p></td>";
        //             strExperimentaldetails += "<td  colspan='3'><p align='center'>Lab Code</p></td>";
        //             strExperimentaldetails += "<td  colspan='8'><p align='center'>Lab Name</p></td>";
        //             strExperimentaldetails += "<td  colspan='8'><p align='center'>Experiment Name</p></td>";
        //             strExperimentaldetails += "<td  colspan='2'><p align='center'>Available</p></td>";
        //             strExperimentaldetails += "<td  colspan='2'><p align='center'>Committee Findings</p></td>";
        //             strExperimentaldetails += "</tr>";
        //             strExperimentaldetails += "</thead>";
        //             strExperimentaldetails += "<tbody>";
        //             foreach (var item in labdetails.Where(l => l.specializationId == speclializationId).ToList())
        //             {

        //                 if (item.ExpCount == 0)
        //                 {
        //                     available = "No";
        //                 }
        //                 else
        //                 {
        //                     available = "Yes";
        //                 }
        //                 strExperimentaldetails += "<tr>";
        //                 strExperimentaldetails += "<td  align='center' colspan='2'><p align='center'>" + sno + "</p></td>";
        //                 //strExperimentaldetails += "<td  align='left' colspan='3'>" + item.degree + "</td>";
        //                 //strExperimentaldetails += "<td  align='left' colspan='2'>" + item.department + "</td>";
        //                 //strExperimentaldetails += "<td  align='left' colspan='2'>" + item.specializationName + "</td>";
        //                 //strExperimentaldetails += "<td  align='left' colspan='2'>" + item.year + "</td>";
        //                 //strExperimentaldetails += "<td  align='left' colspan='2'>" + item.Semester + "</td>";
        //                 strExperimentaldetails += "<td  align='left' colspan='3'>" + item.Labcode + "</td>";
        //                 strExperimentaldetails += "<td  align='left' colspan='8'>" + item.LabName + "</td>";
        //                 strExperimentaldetails += "<td  align='left' colspan='8'>" + item.ExperimentName + "</td>";
        //                 strExperimentaldetails += "<td  align='left' colspan='2'>" + available + "</td>";
        //                 strExperimentaldetails += "<td  align='left' colspan='2'>" +" " +"</td>";
        //                 strExperimentaldetails += "</tr>";
        //                 sno++;


        //             }
        //             strExperimentaldetails += "</tbody></table>";
        //         }
               
        //        contents = contents.Replace("##ExperimentsDetails##", strExperimentaldetails);


        //        return contents;
        //    }
        //}



        //private string GetFirstYearLabDetails(int collegeId, string contents)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        string strFirstYeardetails = string.Empty;
        //        strFirstYeardetails += "<br /><strong><p><u>14 a).Details of 1st year laboratories for B.Tech Degree </u>:</p></strong> <br />";
        //        strFirstYeardetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
        //        strFirstYeardetails += "<tbody>";
        //        strFirstYeardetails += "<tr>";
        //        strFirstYeardetails += "<td colspan='1' ><p align='center'>S.No</p></td>";
        //        strFirstYeardetails += "<td colspan='5' ><p align='left'>Name of the Lab</p></td>";
        //        strFirstYeardetails += "<td colspan='4' ><p align='left'>No. of Equipment as per CF</p></td>";
        //        strFirstYeardetails += "</tr>";

        //        List<jntuh_lab_first_year> LFirstYear = db.jntuh_lab_first_year.Where(g => g.isActive).ToList();
        //        int Countval = 1;
        //        foreach (var item in LFirstYear)
        //        {
        //            strFirstYeardetails += "<tr>";
        //            strFirstYeardetails += "<td colspan='1' ><p align='center'>" + Countval + "</p></td>";
        //            strFirstYeardetails += "<td colspan='5' ><p align='left'>" + item.LabName + "</p></td>";
        //            strFirstYeardetails += "<td colspan='4' ><p align='left'>&nbsp;</p></td>";
        //            strFirstYeardetails += "</tr>";
        //            Countval++;
        //        }
        //        strFirstYeardetails += "</tbody>";
        //        strFirstYeardetails += "</table>";
        //        return strFirstYeardetails;


                //string strLaboratoriesDetails = string.Empty;
                //int sno = 1;
                //string strviewEquimentdata = string.Empty;
                //string Equipmentdate = string.Empty;
                //string chalanaDate = string.Empty;
                //int[] collegeSpecializationIDs = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId).Select(e => e.specializationId).Distinct().ToArray();
                //string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeId && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

                ////var labMaster = db.jntuh_lab_master;
                //List<Lab> labs = (from lm in db.jntuh_lab_master.AsNoTracking()
                //                  join l in db.jntuh_college_laboratories.AsNoTracking() on new { ID = lm.id, collegeId = collegeId } equals new { ID = l.EquipmentID, collegeId = l.CollegeID }
                //                  into all
                //                  from m in all.DefaultIfEmpty()
                //                  where (collegeSpecializationIDs.Contains(lm.SpecializationID) || lm.SpecializationID == 39)  //&& ((lm.jntuh_degree.degreeTypeId != 1 && !string.IsNullOrEmpty(m.Make)) || (lm.jntuh_degree.degreeTypeId == 1))
                //                  select new Lab
                //                  {
                //                      EquipmentID = lm.id,
                //                      tempEquipmentId = m.EquipmentID,
                //                      degree = lm.jntuh_degree.degree,
                //                      department = lm.jntuh_department.departmentName,
                //                      specializationName = lm.jntuh_specialization.specializationName,
                //                      Semester = lm.Semester,
                //                      year = lm.Year,
                //                      Labcode = lm.Labcode,
                //                      LabName = lm.LabName,
                //                      AvailableArea = m.AvailableArea,
                //                      RoomNumber = m.RoomNumber,
                //                      EquipmentName = lm.EquipmentName ?? m.EquipmentName,
                //                      Make = m.Make,
                //                      Model = m.Model,
                //                      EquipmentUniqueID = m.EquipmentUniqueID,
                //                      AvailableUnits = m.AvailableUnits,
                //                      specializationId = lm.SpecializationID,
                //                      CircuitType = lm.jntuh_department.CircuitType == false ? "A" : "B",
                //                      isCircuit = lm.jntuh_department.CircuitType,
                //                      DisplayOrder = lm.jntuh_department.DisplayOrder,
                //                      degreeTypeId = lm.jntuh_degree.degreeTypeId,
                //                      ViewEquipmentPhoto = m.EquipmentPhoto,
                //                      EquipmentDateOfPurchasing = m.EquipmentDateOfPurchasing,
                //                      DelivaryChalanaDate = m.DelivaryChalanaDate

                //                  }).ToList();

                ////var pgLabs = labs.Where(h => h.degreeTypeId != 1).GroupBy(a => new {  a.degree, a.department, a.specializationName, a.specializationId,a.LabName})
                //var pgLabs = labs.Where(h => h.degreeTypeId != 1).GroupBy(a => new { a.degree, a.department, a.specializationName, a.specializationId, a.LabName })
                //                .Select(g => new
                //                {
                //                    degree = g.Key.degree,
                //                    department = g.Key.department,
                //                    specializationName = g.Key.specializationName,
                //                    specializationId = g.Key.specializationId,

                //                    LabName = g.Key.LabName,
                //                    LabCount = g.Count()
                //                }).ToList()
                //                .Select(c => Enumerable.Range(1, 4 - (c.LabCount > 4 ? 4 : c.LabCount)).Select(z => new Lab
                //                {
                //                    EquipmentID = 0,
                //                    tempEquipmentId = null,
                //                    degree = c.degree,
                //                    department = c.department,
                //                    specializationName = c.specializationName,
                //                    Semester = 0,
                //                    year = 0,
                //                    Labcode = "",
                //                    LabName = c.LabName,
                //                    AvailableArea = null,
                //                    RoomNumber = "",
                //                    EquipmentName = "",
                //                    Make = "",
                //                    Model = "",
                //                    EquipmentUniqueID = "",
                //                    AvailableUnits = null,
                //                    specializationId = c.specializationId,
                //                    CircuitType = "",
                //                    isCircuit = null,
                //                    DisplayOrder = null,
                //                    degreeTypeId = null
                //                })).Select(k => k).ToList();


                //foreach (var p in pgLabs.ToList())
                //{
                //    labs.AddRange(p);
                //}
                //if (CollegeAffiliationStatus == "Yes")
                //{
                //    labs = labs.OrderBy(l => l.degree).ThenBy(l => l.CircuitType).ThenBy(l => l.DisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationName).ThenBy(l => l.year).ThenBy(l => l.Semester).ToList();
                //}
                //else
                //{
                //    labs = labs.Where(l => l.Labcode != "TMP-CL").OrderBy(l => l.degree).ThenBy(l => l.CircuitType).ThenBy(l => l.DisplayOrder).ThenBy(l => l.department).ThenBy(l => l.specializationId).ToList();
                //    //
                //}


                //int[] SpecializationIDs = labs.Select(l => l.specializationId).Distinct().ToArray();
                //var specializations = db.jntuh_specialization.Where(it => SpecializationIDs.Contains(it.id)).Select(s => new
                //{
                //    s.id,
                //    specialization = s.specializationName,
                //    department = s.jntuh_department.departmentName,
                //    degree = s.jntuh_department.jntuh_degree.degree

                //}).ToList();






                //foreach (var speclializationId in SpecializationIDs)
                //{



                //    string strLabName = string.Empty;
                //    sno = 1;
                //    var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);

                //    strLabName = "Degree: " + specializationDetails.degree + ",&nbsp;&nbsp;&nbsp; Department: " + specializationDetails.department + ",&nbsp;&nbsp;&nbsp; Specilization: " + specializationDetails.specialization;// +",&nbsp;&nbsp;&nbsp; Year: " + specilizationdetails.year + ",&nbsp;&nbsp;&nbsp; Semester: " + specilizationdetails.semester;
                //    //strLaboratoriesDetails += strLabName + "<br />";
                //    strLaboratoriesDetails += "<table border='1' cellspacing='0' cellpadding='3'>";
                //    strLaboratoriesDetails += "<thead>";
                //    strLaboratoriesDetails += "<tr>";
                //    //if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                //    //{
                //    strLaboratoriesDetails += "<th colspan='39'> <strong>" + strLabName + "</strong></th>";
                //    //}
                //    //else
                //    //{
                //    //    strLaboratoriesDetails += "<th colspan='35'> <strong>" + strLabName + "</strong></th>";
                //    //}
                //    strLaboratoriesDetails += "</tr>";

                //    strLaboratoriesDetails += "<tr>";
                //    strLaboratoriesDetails += "<th  colspan='1'><p align='center'>S.No</p></th>";
                //    //strLaboratoriesDetails += "<th  colspan='3'><p align='left'>Degree</p></th>";
                //    //strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Dept.</p></th>";
                //    //strLaboratoriesDetails += "<th  colspan='7'><p align='left'>Specialization</p></th>";

                //    // Commented By Srinivas
                //    //if (specializationDetails.degree == "B.Tech" || specializationDetails.degree == "B.Pharmacy")
                //    //{
                //    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>Year</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='1'><p align='left'>Sem.</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Lab Code</p></th>";
                //    //}
                //    strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Lab Name</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Area (in Sqm)</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Room No</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='6'><p align='left'>Equipment Name</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Make</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Model</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Equipment UniqueID</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='4'><p align='left'>Equpment Photo</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='3'><p align='left'>EquipmentDateOfPurchasing </p></th>";
                //    strLaboratoriesDetails += "<th  colspan='3'><p align='left'>DelivaryChalanaDate</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Available Units</p></th>";
                //    strLaboratoriesDetails += "<th  colspan='2'><p align='left'>Available units in working condition</p></th>";
                //    strLaboratoriesDetails += "</tr>";
                //    strLaboratoriesDetails += "</thead>";

                //    strLaboratoriesDetails += "<tbody>";

                //    foreach (var item in labs.Where(l => l.specializationId == speclializationId).ToList())
                //    {

                //        if (item.ViewEquipmentPhoto != null)
                //        {
                //            strviewEquimentdata = "/Content/Upload/EquipmentsPhotos/" + item.ViewEquipmentPhoto;
                //        }
                //        else
                //        {
                //            strviewEquimentdata = string.Empty;
                //        }
                //        string path = @"~" + strviewEquimentdata.Replace("%20", " ");
                //        path = System.Web.HttpContext.Current.Server.MapPath(path);

                //        if (item.DelivaryChalanaDate != null)
                //        {
                //            chalanaDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DelivaryChalanaDate.ToString());
                //        }


                //        if (item.EquipmentDateOfPurchasing != null)
                //        {
                //            Equipmentdate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.EquipmentDateOfPurchasing.ToString());
                //        }


                //        strLaboratoriesDetails += "<tr>";
                //        strLaboratoriesDetails += "<td  align='center' colspan='1'><p align='center'>" + sno + "</p></td>";
                //        //strLaboratoriesDetails += "<td  align='left' colspan='3'>" + item.degree + "</td>";
                //        //strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.department + "</td>";
                //        //strLaboratoriesDetails += "<td  align='left' colspan='7'>" + item.specializationName + "</td>";

                //        //  Comented By Srinivas
                //        //if (item.degree == "B.Tech" || item.degree == "B.Pharmacy")
                //        //{
                //        strLaboratoriesDetails += "<td  align='left' colspan='1'>" + item.year + "</td>";
                //        strLaboratoriesDetails += "<td  align='left' colspan='1'>" + item.Semester + "</td>";
                //        strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Labcode + "</td>";
                //        //}
                //        if (item.LabName != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.LabName.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.LabName + "</td>";
                //        }

                //        if (item.AvailableArea != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.AvailableArea.ToString().Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.AvailableArea + "</td>";
                //        }

                //        if (item.RoomNumber != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.RoomNumber.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.RoomNumber + "</td>";
                //        }

                //        if (item.EquipmentName != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='6'>" + item.EquipmentName.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='4'>" + item.EquipmentName + "</td>";
                //        }

                //        if (item.Make != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Make.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Make + "</td>";
                //        }

                //        if (item.Model != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Model.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.Model + "</td>";
                //        }

                //        if (item.EquipmentUniqueID != null)
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.EquipmentUniqueID.Replace("&", "&amp;") + "</td>";
                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td  align='left' colspan='2'>" + item.EquipmentUniqueID + "</td>";
                //        }

                //        if (System.IO.File.Exists(path))
                //        {
                //            //strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src=" + serverURL + "" + strviewEquimentdata + " align='center'  width='40' height='50' /></p></td>";
                //            strLaboratoriesDetails += "<td  width='100' align='center' colspan='4'><p align='center'><img src='" + HtmlEncoder.Encode(path).Replace("'", "&#39;") + "' align='center'  width='40' height='50' /></p></td>";

                //        }
                //        else
                //        {
                //            strLaboratoriesDetails += "<td width='100' align='center' colspan='4'><p align='center'></p></td></tr>";
                //        }
                //        strLaboratoriesDetails += "<td  align='left' colspan='3'>" + Equipmentdate + "</td>";
                //        strLaboratoriesDetails += "<td  align='left' colspan='3'>" + chalanaDate + "</td>";



                //        strLaboratoriesDetails += "<td  align='right' colspan='2' style='width:10%;' >" + item.AvailableUnits + "</td>";
                //        strLaboratoriesDetails += "<td  align='left' colspan='2'></td>";
                //        strLaboratoriesDetails += "</tr>";
                //        sno++;
                //    }
                //    strLaboratoriesDetails += "</tbody></table>";
                //}
                //return strLaboratoriesDetails;
        //    }
        //}

        private string GetNonCircuteLabSummarySheet(List<Lab> labdata, int collegeId)
        {
            using (var db = new uaaasDBContext())
            {
                 int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeId).Select(C=>C.EquipmentID).ToArray();
                int[] DegreeIds = db.jntuh_lab_master.Where(L => Equipmentsids.Contains(L.id)).Select(L => L.DegreeID).Distinct().ToArray();

                labdata = labdata.ToList();
                   
                var summarySheet = labdata.GroupBy(h => new { h.department, h.year, h.Semester, h.LabName,h.degree,h.specializationName })
                    .Select(s => new
                    {
                        degree = s.Key.degree,
                        specialization= s.Key.specializationName,
                        department = s.Key.department,
                        year = s.Key.year,
                        Semester = s.Key.Semester,
                        LabName = s.Key.LabName,
                        Count = s.Count(g => g.tempEquipmentId != null)
                    });
                string strSummarySheet = string.Empty;
                strSummarySheet += "<br /><strong><p><u>14 a).Lab Summary sheet</u>:</p></strong> <br />";

                if (summarySheet.Count() > 0)
                {
                    strSummarySheet += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
                    strSummarySheet += "<tbody>";
                    strSummarySheet += "<tr>";
                    strSummarySheet += "<td colspan='1' rowspan='2' ><p align='center'>S.No</p></td>";
                    strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Degree</p></td>";
                    strSummarySheet += "<td colspan='2' rowspan='2' ><p align='left'>Department</p></td>";
                    strSummarySheet += "<td colspan='2' rowspan='2' ><p align='left'>Specialization</p></td>";
                    strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Year</p></td>";
                    strSummarySheet += "<td colspan='1' rowspan='2' ><p align='left'>Sem</p></td>";
                    strSummarySheet += "<td colspan='4' rowspan='2' ><p align='left'>Lab Name</p></td>";
                    strSummarySheet += "<td colspan='2' ><p align='left'>No. of Equipment</p></td>";
                    strSummarySheet += "</tr>";
                    strSummarySheet += "<tr>";
                    strSummarySheet += "<td colspan='1' ><p align='left'>Upload</p></td>";
                    strSummarySheet += "<td colspan='1' ><p align='left'>CF</p></td>";
                    strSummarySheet += "</tr>";

                    int countdata = 1;
                    foreach (var item in summarySheet.OrderBy(e => e.department).ThenBy(e=>e.specialization).ThenBy(e => e.year).ThenBy(e => e.Semester))
                    {
                        strSummarySheet += "<tr>";
                        strSummarySheet += "<td colspan='1' ><p align='center'>" + countdata + "</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='center'>" + item.degree + "</p></td>";
                        strSummarySheet += "<td colspan='2' ><p align='left'>" + item.department + "</p></td>";
                        strSummarySheet += "<td colspan='2' ><p align='left'>" + item.specialization + "</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='left'>" + item.year + "</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='left'>" + item.Semester + "</p></td>";
                        strSummarySheet += "<td colspan='4' ><p align='left'>" + item.LabName + "</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='left'>" + item.Count + "</p></td>";
                        strSummarySheet += "<td colspan='1' ><p align='left'>&nbsp;</p></td>";
                        strSummarySheet += "</tr>";
                        countdata++;

                    }
                    strSummarySheet += "</tbody>";
                    strSummarySheet += "</table>";
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
                //strLibraryInformation += "<table border='0' cellpadding='0' cellspacing='0' id='page15' style='font-size: 9px;'><tr>";
                //strLibraryInformation += "<td align='left' valign='top'>";
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
                        it.degreeId, it.availableComputers}).ToList();
                    var jntuh_degree = db.jntuh_degree.Where(it => it.isActive && degreeids.Contains(it.id))
                        .Select(it => new {
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
                return contents;
            }
        }

        public string LibraryDetails(int collegeId, string contents)
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


                //List<LibraryDetails> libraryDetails = (from collegeDegree in db.jntuh_college_degree
                //                                       join degre in db.jntuh_degree on collegeDegree.degreeId equals degre.id
                //                                       where (collegeDegree.collegeId == collegeId && collegeDegree.isActive == true)
                //                                       orderby degre.degree
                //                                       select new LibraryDetails
                //                                       {
                //                                           degreeId = collegeDegree.degreeId,
                //                                           degree = degre.degree,
                //                                           totalTitles = null,
                //                                           totalVolumes = null,
                //                                           totalNationalJournals = null,
                //                                           totalInternationalJournals = null,
                //                                           totalEJournals = null
                //                                       }).ToList();

                //var jntuh_college_library_details = db.jntuh_college_library_details;

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

                        //item.totalTitles = jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                        //                                                          collegeLibrary.degreeId == item.degreeId)
                        //                                                   .Select(collegeLibrary => collegeLibrary.totalTitles)
                        //                                                   .FirstOrDefault();
                        //item.totalVolumes = jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                        //                                                          collegeLibrary.degreeId == item.degreeId)
                        //                                                   .Select(collegeLibrary => collegeLibrary.totalVolumes)
                        //                                                   .FirstOrDefault();
                        //item.totalNationalJournals = jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                        //                                                          collegeLibrary.degreeId == item.degreeId)
                        //                                                   .Select(collegeLibrary => collegeLibrary.totalNationalJournals)
                        //                                                   .FirstOrDefault();
                        //item.totalInternationalJournals = jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                        //                                                          collegeLibrary.degreeId == item.degreeId)
                        //                                                   .Select(collegeLibrary => collegeLibrary.totalInternationalJournals)
                        //                                                   .FirstOrDefault();
                        //item.totalEJournals = jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                        //                                                          collegeLibrary.degreeId == item.degreeId)
                        //                                                   .Select(collegeLibrary => collegeLibrary.totalEJournals)
                        //                                                   .FirstOrDefault();
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
                List<jntuh_college_degree> jntuh_college_degree = db.jntuh_college_degree.Where(d => d.collegeId == collegeId && d.isActive == true).ToList();

                List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();
                if (jntuh_college_degree != null)
                {
                    var jntuh_college_computer_student_ratio = db.jntuh_college_computer_student_ratio.Where(d => d.collegeId == collegeId).ToList();
                    var jntuh_degree = db.jntuh_degree;
                    foreach (var item in jntuh_college_degree)
                    {

                        int degreeId = item.degreeId;
                        string degree = jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                        int availableComputers = jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == collegeId &&
                                                                                                computerStudenRatio.degreeId == item.degreeId)
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
                return contents;
            }
        }

        private string ReadExcelData(List<jntuh_college_enclosures> list)
        {
            using (var db = new uaaasDBContext())
            {
                string enclosures = string.Empty;
                var MacPath = list.Distinct();
                foreach (var item in MacPath)
                {
                    string path = @"~/Content/Upload/CollegeEnclosures/MAC/" + item.path;
                    path = Server.MapPath(path);
                    if (!System.IO.File.Exists(path))
                    {
                        return "";
                    }
                    string excelConnectionString = string.Empty;
                    switch (System.IO.Path.GetExtension(path).ToUpper())
                    {
                        case ".XLS":
                            excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            break;
                        case ".XLSX":
                            excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            break;
                    }
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);

                    string Machead = "MAC-ADDRESS LIST";
                    enclosures += "<p style='font-size: 9px;'><strong><u>" + Machead + "</u></strong> </p><br/>";

                    try
                    {
                        excelConnection.Open();
                        DataTable dt = new DataTable();

                        dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        if (dt == null || dt.Rows[0] == null)
                        {
                            return "";
                        }

                        string sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                        string query = string.Format("Select * from [{0}]", sheetName);
                        DataTable dtComputerData = new DataTable();
                        using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection))
                        {
                            dataAdapter.Fill(dtComputerData);
                        }
                        if (dtComputerData == null || dtComputerData.Rows.Count == 0)
                        {
                            return "";
                        }
                        var lst = dtComputerData.AsEnumerable().Where(d => !string.IsNullOrEmpty(Convert.ToString(d.Field<object>(0))) || !string.IsNullOrEmpty(Convert.ToString(d.Field<object>(1))) || !string.IsNullOrEmpty(Convert.ToString(d.Field<object>(2)))).Select(d => d).ToList();

                       // enclosures += "<table><tr><td>";
                        enclosures += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                        enclosures += "<tr>";
                        enclosures += "<th style='width:5px'><p>SNo.</p></th>";
                        enclosures += "<th style='width:10px' colspan='2'><p>MAC-ADDRESS</p></th>";
                        enclosures += "<th style='width:5px' colspan='2'><p>Location  (Lab/Room Number)</p></th>";
                        enclosures += "<th style='width:30px' colspan='5'><p>Lab Name</p></th>";
                        enclosures += "</tr>";
                        int rowCount = 1;
                        for (int i = 0; i < lst.Count(); i++)// / 2
                        {
                            DataRow ComputerMac = lst[i];
                            enclosures += "<tr>";
                            enclosures += "<td align='center'><p>" + rowCount + "</p></td>";
                            if (ComputerMac.ItemArray.Length > 1)
                            {
                                enclosures += "<td colspan='2'><p>" + Convert.ToString(ComputerMac[1]) + "</p></td>";
                            }
                            else
                            {
                                enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                            }

                            if (ComputerMac.ItemArray.Length > 2)
                            {
                                enclosures += "<td colspan='2'><p>" + Convert.ToString(ComputerMac[2]) + "</p></td>";
                            }
                            else
                            {
                                enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                            }
                            if (ComputerMac.ItemArray.Length > 2)
                            {
                                enclosures += "<td colspan='5'><p>" + Convert.ToString(ComputerMac[3]) + "</p></td>";
                            }
                            else
                            {
                                enclosures += "<td colspan='5'><p>" + string.Empty + "</p></td>";
                            }

                            enclosures += "</tr>";
                            rowCount++;

                        }
                        enclosures += "</table>";//</td>

                        //enclosures += "<td><table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                        //enclosures += "<thead><tr>";
                        //enclosures += "<th style='width:5px'><p>SNo.</p></th>";
                        //enclosures += "<th style='width:15px' colspan='3'><p>MAC-ADDRESS</p></th>";
                        //enclosures += "<th style='width:20px' colspan='3'><p>Location  (Lab/Room Number)</p></th>";
                        //enclosures += "</tr></thead>";
                        //int rowCount1 = lst.Count() / 2 + 1;
                        //for (int i = lst.Count() / 2; i < lst.Count(); i++)
                        //{

                        //    DataRow ComputerMac = lst[i];
                        //    enclosures += "<tr>";
                        //    enclosures += "<td align='center'><p>" + rowCount1 + "</p></td>";
                        //    if (ComputerMac.ItemArray.Length > 1)
                        //    {
                        //        enclosures += "<td colspan='3'><p>" + Convert.ToString(ComputerMac[1]) + "</p></td>";
                        //    }
                        //    else
                        //    {
                        //        enclosures += "<td colspan='3'><p>" + string.Empty + "</p></td>";
                        //    }

                        //    if (ComputerMac.ItemArray.Length > 2)
                        //    {
                        //        enclosures += "<td colspan='3'><p>" + Convert.ToString(ComputerMac[2]) + "</p></td>";
                        //    }
                        //    else
                        //    {
                        //        enclosures += "<td colspan='3'><p>" + string.Empty + "</p></td>";
                        //    }

                        //    enclosures += "</tr>";
                        //    rowCount1++;
                        //}
                        //enclosures += "</table>";
                       // enclosures += "</td></tr></table>";






                        //int rowCount = 1;                   

                        //foreach (DataRow ComputerMac in lst)
                        //{

                        //    enclosures += "<tr>";
                        //    enclosures += "<td align='center'><p>" + rowCount + "</p></td>";
                        //    if (ComputerMac.ItemArray.Length > 1)
                        //    {
                        //        enclosures += "<td colspan='2'><p>" + Convert.ToString(ComputerMac[1]) + "&nbsp;</p></td>";
                        //    }
                        //    else
                        //    {
                        //        enclosures += "<td colspan='2'><p>" + string.Empty + "</p></td>";
                        //    }

                        //    if (ComputerMac.ItemArray.Length > 2)
                        //    {
                        //        enclosures += "<td colspan='4'><p>" + Convert.ToString(ComputerMac[2]) + "&nbsp;</p></td>";
                        //    }
                        //    else
                        //    {
                        //        enclosures += "<td colspan='4'><p>" + string.Empty + "&nbsp;</p></td>";
                        //    }

                        //    enclosures += "<td colspan='2'><p>&nbsp;</p></td>";
                        //    enclosures += "</tr>";
                        //    rowCount++;
                        //}
                        //enclosures += "</table>";

                    }
                    catch (Exception)
                    {
                        enclosures += "<p style='font-size: 9px;'><i>Can't read the Junk file uploaded by the college.</i></p>";
                    }
                    finally { excelConnection.Close(); }
                }

                return enclosures;
            }
        }

        public string InternetBandwidthDetails(int collegeId, string contents)
        {
            using (var db = new uaaasDBContext())
            {
                string strInternetBandwidthDetails = string.Empty;
                strInternetBandwidthDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";

                strInternetBandwidthDetails += "<tr>";
                strInternetBandwidthDetails += "<td  colspan='2'><p><b>Degree</b></p></td>";
                strInternetBandwidthDetails += "<td  colspan='2'><p align='center'><b>Available Bandwidth</b></p></td>";
                strInternetBandwidthDetails += "<td  colspan='2'><p align='center'><b>Committee Findings</b></p></td>";
                strInternetBandwidthDetails += "</tr>";
                List<jntuh_college_degree> collegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == collegeId && d.isActive == true).ToList();
                if (collegeDegree != null)
                {
                    var jntuh_degree = db.jntuh_degree.OrderBy(d => d.degreeDisplayOrder).ToList();
                    var jntuh_college_internet_bandwidth = db.jntuh_college_internet_bandwidth.Where(i => i.collegeId == collegeId).ToList();
                    foreach (var item in collegeDegree)
                    {
                        string degree = jntuh_degree.Where(d => d.isActive == true && d.id == item.degreeId).Select(d => d.degree).FirstOrDefault();
                        decimal availableInternetSpeed = jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == collegeId &&
                                                                                                internetbandwidth.degreeId == item.degreeId)
                                                                                         .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                         .FirstOrDefault();
                        strInternetBandwidthDetails += "<tr>";
                        strInternetBandwidthDetails += "<td  colspan='2'><p>" + degree + "</p></td>";
                        strInternetBandwidthDetails += "<td  align='center' colspan='2'>" + availableInternetSpeed + "</td>";
                        strInternetBandwidthDetails += "<td  align='center' colspan='2'></td>";
                        strInternetBandwidthDetails += "</tr>";
                    }
                }
                strInternetBandwidthDetails += "</tbody></table>";
                contents = contents.Replace("##InternetBandwidthDetails##", strInternetBandwidthDetails);
                return contents;
            }
        }

        public string LegalSystemSoftwareDetails(int collegeId, string contents)
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

                List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in db.jntuh_college_degree
                                                                        join Degree in db.jntuh_degree on CollegeDegree.degreeId equals Degree.id
                                                                        orderby Degree.degreeDisplayOrder
                                                                        where (CollegeDegree.collegeId == collegeId && CollegeDegree.isActive == true && Degree.isActive == true)
                                                                        select new CollegeLegalSoftwarDetails
                                                                        {
                                                                            id =  CollegeDegree.id,
                                                                            degreeId = CollegeDegree.degreeId,
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
                return contents;
            }
        }

        public string PrintersDetails(int collegeId, string contents)
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
                List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in db.jntuh_college_degree
                                                              join Degree in db.jntuh_degree on CollegeDegree.degreeId equals Degree.id
                                                              orderby Degree.degreeDisplayOrder
                                                              where (CollegeDegree.collegeId == collegeId && CollegeDegree.isActive == true)
                                                              select new CollegePrinterDetails
                                                              {
                                                                  degreeId = CollegeDegree.degreeId,
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

        //public string FeeReimbursementDetails(int collegeId, string contents)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        string strFeeReimbursementDetails = string.Empty;
        //        int sno = 0;

        //        string AcademeicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
        //                                                                  a.isPresentAcademicYear == true)
        //                                                      .Select(a => a.academicYear).FirstOrDefault();
        //        strFeeReimbursementDetails += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'><tbody>";
        //        strFeeReimbursementDetails += "<tr><td width='32' rowspan='3'><p align='center'>S.No</p></td><td width='65' rowspan='3'><p align='center'>Degree</p><p align='center'>*</p></td><td width='72' rowspan='3'><p align='center'>Dept.</p><p align='center'>**</p></td><td width='168' rowspan='3'><p align='center'>Specialization</p><p align='center'>***</p></td><td width='42' rowspan='3'><p align='center'>Shift</p><p align='center'>#</p></td><td width='48' rowspan='3'><p align='center'>Year in degree</p><p align='center'>##</p></td><td width='614' colspan='8'><p align='center'>" + AcademeicYear + "</p></td></tr>";
        //        strFeeReimbursementDetails += "<tr><td width='348' colspan='4'><p align='center'>Convener Quota Seats</p></td><td width='266' colspan='4'><p align='center'>Management Quota Seats</p></td></tr>";
        //        strFeeReimbursementDetails += "<tr style='font-size: 8px;'><td width='78'><p align='center'>Without Re-imbursement Seats</p></td><td width='96'><p align='center'>Total Fee</p><p align='center'>(Rs.)</p></td><td width='78'><p align='center'>With Re-imbursement Seats</p></td><td width='96'><p align='center'>Total Fee</p><p align='center'>(Rs.)</p></td><td width='42'><p align='center'>NRI Seats</p></td><td width='90'><p align='center'>Total Fee</p><p align='center'>(Rs.)</p></td><td width='42'><p align='center'>PIO Seats</p></td><td width='92'><p align='center'>Total Fee</p><p align='center'>(Rs.)</p></td></tr>";
        //        List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == collegeId).ToList();

        //        List<CollegeFeeReimbursement> collegeFeeReimbursement = new List<CollegeFeeReimbursement>();
        //        var yearInDegreeIds= reimbursement.Select(r => r.yearInDegreeId).ToList();
        //        var jntuh_year_in_degrees = db.jntuh_year_in_degree
        //            .Where(it => yearInDegreeIds.Contains(it.id))
        //            .Select(it => new
        //            {
        //                it.id,
        //                it.yearInDegree
        //            }).ToList();
        //        var specializationIds= reimbursement.Select(it => it.specializationId).ToList();
        //        var jntuh_specializations = db.jntuh_specialization.Where(s => specializationIds.Contains(s.id))
        //           .Select(it => new { it.id, it.specializationName, it.departmentId }).ToList();
        //        var departmentIds= jntuh_specializations.Select(it => it.departmentId).ToList();
        //        var jntuh_departments = db.jntuh_department.Where(d => departmentIds.Contains(d.id))
        //            .Select(it => new { it.id, it.departmentName, it.degreeId }).ToList();
        //        var degreeIds= jntuh_departments.Select(it1 => it1.degreeId).ToList();
        //        var jntuh_degrees = db.jntuh_degree.Where(it => degreeIds.Contains(it.id))
        //            .Select(it => new { it.id, it.degree, it.degreeDisplayOrder }).ToList();
        //        var shiftIds= reimbursement.Select(p => p.shiftId).ToList();
        //        var jntuh_shifts = db.jntuh_shift.Where(it => shiftIds.Contains(it.id)).Select(it => new { it.id, it.shiftName }).ToList();
        //        foreach (var item in reimbursement)
        //        {
        //            CollegeFeeReimbursement newReimbursement = new CollegeFeeReimbursement();
        //            newReimbursement.id = item.id;
        //            newReimbursement.collegeId = item.collegeId;
        //            newReimbursement.academicYearId = item.academicYearId;
        //            newReimbursement.specializationId = item.specializationId;
        //            newReimbursement.shiftId = item.shiftId;
        //            newReimbursement.yearInDegreeId = item.yearInDegreeId;
        //            newReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
        //            newReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
        //            newReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
        //            newReimbursement.feeWithReimbursement = item.feeWithReimbursement;
        //            newReimbursement.NRISeats = item.NRISeats;
        //            newReimbursement.totalNRIFee = item.totalNRIFee;
        //            newReimbursement.PIOSeats = item.PIOSeats;
        //            newReimbursement.totalPIOFee = item.totalPIOFee;
        //            newReimbursement.yearInDegree = jntuh_year_in_degrees.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
        //            newReimbursement.specialization = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //            newReimbursement.departmentID = jntuh_specializations.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //            newReimbursement.department = jntuh_departments.Where(d => d.id == newReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
        //            newReimbursement.degreeID = jntuh_departments.Where(d => d.id == newReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
        //            newReimbursement.degree = jntuh_degrees.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
        //            newReimbursement.degreeDisplayOrder = jntuh_degrees.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //            newReimbursement.shift = jntuh_shifts.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //            collegeFeeReimbursement.Add(newReimbursement);
        //        }
        //        collegeFeeReimbursement = collegeFeeReimbursement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.yearInDegree).ToList();
        //        foreach (var item in collegeFeeReimbursement)
        //        {
        //            sno++;
        //            string yearInDegree = item.yearInDegree;
        //            string specialization = item.specialization;
        //            int departmentID = item.departmentID;
        //            string department = item.department;
        //            int degreeID = item.degreeID;
        //            string degree = item.degree;
        //            string shift = item.shift;
        //            strFeeReimbursementDetails += "<tr><td width='32'><p align='center'>" + sno + "</p></td><td width='65'>" + degree + "</td><td width='72'>" + department + "</td><td width='168'>" + specialization + "</td><td width='42' align='center'>" + shift + "</td><td width='48' valign='top' align='center'>" + yearInDegree + "</td><td width='78' align='center'>" + item.seatsWithoutReimbursement + "</td><td width='96' align='right'>" + item.feeWithoutReimbursement + "</td><td width='78' align='center'>" + item.seatsWithReimbursement + "</td><td width='96' align='right'>" + item.feeWithReimbursement + "</td><td width='42' align='center'>" + item.NRISeats + "</td><td width='90' align='right'>" + item.totalNRIFee + "</td><td width='42' align='center'>" + item.PIOSeats + "</td><td width='92' align='right'>" + item.totalPIOFee + "</td></tr>";
        //        }

        //        if (collegeFeeReimbursement.Count() == 0)
        //        {
        //            strFeeReimbursementDetails += "<tr><td width='32' height='30'><p align='center'>&nbsp;</p></td><td width='65'></td><td width='72'></td><td width='168'></td><td width='42' align='center'></td><td width='48' valign='top' align='center'></td><td width='78' align='center'></td><td width='96' align='right'></td><td width='78' align='center'></td><td width='96' align='right'></td><td width='42' align='center'></td><td width='90' align='right'></td><td width='42' align='center'></td><td width='92' align='right'></td></tr>";
        //        }

        //        strFeeReimbursementDetails += "</tbody></table>";
        //        contents = contents.Replace("##FeeReimbursementDetails##", strFeeReimbursementDetails);
        //        return contents;
        //    }
        //}

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

                if (complaints != null)
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

                if (antiRaggingComplaints != null)
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

                if (womenProtectionCellComplaints != null)
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

                if (rtiComplaints != null)
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
                int desirableId = db.jntuh_college_desirable_others.Where(a => a.collegeId == collegeId).Select(a => a.id).FirstOrDefault();
                jntuh_college_desirable_others jntuh_college_desirable_others = db.jntuh_college_desirable_others.Find(desirableId);

                strOtherDesirables += "<p style='font-size: 9px;'><strong><u>31. Sports & Games</u> :</strong></p><br />";
                strOtherDesirables += "<table border='0' cellpadding='0' cellspacing='0' id='page20' style='font-size: 9px;'>";
                if (jntuh_college_desirable_others != null)
                {
                    strOtherDesirables += "<tr>";
                    strOtherDesirables += "<td align='left' valign='top' style='font-size: 9px;'>";
                    //strOtherDesirables += "<br /><p>e)<strong> Sports facilities :</strong></p>";
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

                            // string YesOrNo = "no_b";
                            if (item.Checked == 1)
                            {
                                //YesOrNo = "yes_b";
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
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Square";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Rectangle";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Round";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Oval";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cricket";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Transport to reach the Institute :";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;College Transport";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Public Transport";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>&nbsp; &nbsp; Number of buses (own) available in the college : " + string.Empty + "</p><p style='font-size: 9px;'>&nbsp; &nbsp; Number of other transport vehicles (own) in the college : " + string.Empty + "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Payment of Salary :";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cash";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cheque";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Bank Transfer";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
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
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Square";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Rectangle";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Round";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Oval";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cricket";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
                    strOtherDesirables += "";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Indoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>Outdoor games/sports : <br />";
                    strOtherDesirables += "&nbsp;&nbsp;";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Transport to reach the Institute :";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;College Transport";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Public Transport";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
                    strOtherDesirables += "</p>";

                    strOtherDesirables += "<p style='font-size: 9px;'>&nbsp; &nbsp; Number of buses (own) available in the college : " + string.Empty + "</u></p><p style='font-size: 9px;'>&nbsp; &nbsp; Number of other transport vehicles (own) in the college : " + string.Empty + "</u></p>";

                    strOtherDesirables += "<br><p style='font-size: 9px;'>Mode of Payment of Salary :";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cash";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Cheque";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Bank Transfer";
                    //strOtherDesirables += "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no.png' height='10' />&nbsp;Other";
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
                {var incomeTypeIDs= incomeType.Select(it1 => it1.incomeTypeID).ToList();
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
                        item.expenditureAmount = jntuh_college_expenditures.Where(e =>  e.expenditureTypeID == item.expenditureTypeID)
                            .Select(e => e.expenditureAmount).FirstOrDefault();
                        amount += item.expenditureAmount;
                        expenditureDetails += " <tr><td width='84'><p align='center'>" + sno + "</p> </td><td width='446'><p>" + item.expenditure + "</p> </td><td width='160' align='right'>" + item.expenditureAmount + "</td></tr>";
                    }
                    expenditureDetails += "<tr><td width='84'><p align='center'></p></td><td width='446'><p align='right'> TOTAL   </p> </td> <td width='160'> <p align='right'> " + amount + "</p> </td></tr>";
                }
                expenditureDetails += " </tbody></table>";
                contents = contents.Replace("##ExpenditureDetails##", expenditureDetails);
                return contents;
            }
        }

        private string StudentsPlacementDetails(int collegeId, string contents)
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
                decimal actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARTHREE##", String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2)));
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARTWO##", String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2)));
                contents = contents.Replace("##COLLEGESTUDENTSPLACEMENTSACADEMICYEARONE##", String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2)));

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
                int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();

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
                var specializationIds1= collegePlacement.Select(it1 => it1.specializationId).ToList();
                var jntuh_college_placements = db.jntuh_college_placement.Where(it => it.collegeId == collegeId
                       && specializationIds1.Contains(it.specializationId)).ToList();
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
                        string path = @"~" + strScannedCopy;
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        if (System.IO.File.Exists(path))
                        {
                            strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'><img alt='' src=" + serverURL + "" + strScannedCopy + " align='center'  width='80' height='50' /></p></td></tr>";
                        }
                        else
                        {
                            if (test415PDF.Equals("YES"))
                            {
                                strCollegePhotosDetails += "<tr><td width='80' colspan='1'><p align='center'>" + sno + "</p></td><td width='200' colspan='8'><p align='left'>" + item.documentName + "</p></td><td width='100' align='center' colspan='5'><p align='center'><img alt='' src=" + "http://112.133.193.228:75" + "" + strScannedCopy + " align='center'  width='80' height='50' /></p></td></tr>";
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
                // string documentName = string.Empty;
                var jntuh_college_enclosures_hardcopy = db.jntuh_college_enclosures_hardcopy.Where(e => e.collegeID == collegeId).ToList();
                int count = 1;
                foreach (var item in enclosures)
                {
                    //documentName = db.jntuh_enclosures.Where(e => e.id == item.enclosureId && e.isActive == true).Select(e => e.documentName).FirstOrDefault();
                    collegeEnclosures += "<tr>";
                    collegeEnclosures += "<td colspan='1' align='center'><p align='center'>" + count + "</p></td>";
                    collegeEnclosures += "<td colspan='12' align='left'><p>" + item.documentName + "</p></td>";
                    int encount = jntuh_college_enclosures_hardcopy.Where(e => e.enclosureId == item.id && e.collegeID == collegeId && e.isSelected).Count();
                    //if (item.isSelected == true)
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
                    int[] shiftId1 = intakeProposed.Where(e =>  e.specializationId == specializationId )
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

        #region Old Code
        //private string collegeTachingFacultyMembers(int collegeId, string contents)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        string collegeFaculty = string.Empty;
        //        int count = 1;
        //        string gender = string.Empty;
        //        string category = string.Empty;
        //        string department = string.Empty;
        //        string designation = string.Empty;
        //        string qualification = string.Empty;
        //        string dateOfAppointment = string.Empty;
        //        string teachingType = string.Empty;

        //        string ratified = string.Empty;
        //        List<jntuh_college_faculty_registered> cFaculty = db.jntuh_college_faculty_registered.Where(rf => rf.collegeId == collegeId).ToList();
        //        string[] strRegNoS = cFaculty.Select(rf => rf.RegistrationNumber).ToArray();
        //        // var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => rf.collegeId == collegeId).ToList();
        //        var facultyList = db.jntuh_registered_faculty
        //                    .Where(f => strRegNoS.Contains(f.RegistrationNumber) && ((f.collegeId == collegeId && f.type == "ExistFaculty") || f.type == "NewFaculty"))
        //                            .OrderBy(faculty => faculty.DepartmentId)
        //                                 .ThenBy(faculty => faculty.DesignationId)
        //                                 .ThenBy(faculty => faculty.FirstName).Select(f => f).ToList();

        //        var DeptIDs = facultyList.Where(a => a.DepartmentId != null && a.type == "ExistFaculty").Select(d => d.DepartmentId).Distinct().ToList();

        //        if (DeptIDs.Count() > 0)
        //        {
        //            //int index = 0;
        //            foreach (var deptId in DeptIDs)
        //            {
        //                //if (index == 1 || index == 2 || index == 3 || index == 5 || index == 6 || index == 7 || index == 8 || index == 9 || index == 10)
        //                //if (index >=0)
        //                //{
        //                collegeFaculty += TeachingFaculty(facultyList.Where(g => g.DepartmentId == deptId).ToList());
        //                //}
        //                //index++;
        //            }
        //        }

        //        collegeFaculty += "<strong><p style='font-size: 9px;'><u>11 a).Teaching Faculty Summary Sheet</u>:</p></strong> <br />";
        //        collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
        //        collegeFaculty += "<tbody>";
        //        collegeFaculty += "<tr>";
        //        collegeFaculty += "<td colspan='1' rowspan='3'><p align='center'>SNo</p></td>";
        //        collegeFaculty += "<td colspan='1' rowspan='3'><p align='left'>Degree</p></td>";
        //        collegeFaculty += "<td colspan='2' rowspan='3'><p align='center'>Department / Specialization</p></td>";
        //        collegeFaculty += "<td colspan='6'><p align='center'>Total no. of faculty available</p></td>";
        //        collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty ratified </p></td>";
        //        collegeFaculty += "</tr>";
        //        collegeFaculty += "<tr>";
        //        //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
        //        //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //        //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>UG</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>PG</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>UG & PG</p></td>";
        //        //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
        //        collegeFaculty += "</tr>";
        //        collegeFaculty += "<tr>";
        //        //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
        //        //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //        //collegeFaculty += "<td colspan='2'><p align='center'></p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF </p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";

        //        collegeFaculty += "</tr>";

        //        var summarydata = facultyList.Join(cFaculty, i => i.RegistrationNumber, j => j.RegistrationNumber, (i, j) => new { i, j })
        //                                     .GroupBy(a => new { a.i.DepartmentId, a.i.DesignationId })
        //                                     .Select(g => new
        //                                     {
        //                                         deptid = g.Key.DepartmentId,
        //                                         UG = g.Count(d => d.j.IdentifiedFor == "UG"),
        //                                         PG = g.Count(d => d.j.IdentifiedFor == "PG"),
        //                                         UGPG = g.Count(d => d.j.IdentifiedFor == "UG&PG"),
        //                                         FC = g.Count(c => c.i.isFacultyRatifiedByJNTU == true)
        //                                     })
        //                                    .Join(db.jntuh_department, r => r.deptid, ro => ro.id, (r, ro) => new { r, ro })
        //                                    .Join(db.jntuh_degree, x => x.ro.degreeId, y => y.id, (x, y) => new { x, y })
        //                                    .Select(z => new { z.y.degree, z.x.ro.departmentName, z.x.r.UG, z.x.r.PG, z.x.r.UGPG, z.x.r.FC })
        //                                    .GroupBy(h => new { h.degree, h.departmentName })
        //                                    .Select(i => new
        //                                    {
        //                                        i.Key.degree,
        //                                        i.Key.departmentName,
        //                                        UG = i.Sum(j => j.UG),
        //                                        PG = i.Sum(j => j.PG),
        //                                        UGPG = i.Sum(j => j.UGPG),
        //                                        FC = i.Max(j => j.FC)
        //                                    })
        //                                    .ToList();
        //        int countdata = 1;
        //        foreach (var item in summarydata)
        //        {
        //            collegeFaculty += "<tr>";
        //            collegeFaculty += "<td colspan='1'><p align='center'>" + countdata + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='right'>" + item.UG + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='right'>" + item.PG + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='right'>" + item.UGPG + " </p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='right'>" + item.FC + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "</tr>";
        //            countdata++;
        //        }
        //        collegeFaculty += "</tbody>";
        //        collegeFaculty += "</table>";

        //        var type = facultyList.Where(f => f.type == "NewFaculty").ToList();
        //        if (type.Count() > 0)
        //        {
        //            //collegeFaculty += "<strong><p><u>11 b).New Faculty</u>:</p></strong> <br />";
        //            collegeFaculty += TeachingFaculty(facultyList.Where(g => g.type == "NewFaculty").ToList());
        //        }



        //        collegeFaculty += "<strong><p style='font-size: 9px;'><u>11 c).New Faculty Summary Sheet</u>:</p></strong> <br />";
        //        collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
        //        collegeFaculty += "<tbody>";
        //        collegeFaculty += "<tr>";
        //        collegeFaculty += "<td colspan='1' rowspan='2'><p align='center'>SNo</p></td>";
        //        collegeFaculty += "<td colspan='1' rowspan='2'><p align='left'>Degree</p></td>";
        //        collegeFaculty += "<td colspan='2' rowspan='2'><p align='center'>Department / Specialization</p></td>";
        //        collegeFaculty += "<td colspan='6'><p align='center'>Total no. of faculty available as per CF</p></td>";
        //        collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty ratified </p></td>";
        //        collegeFaculty += "</tr>";
        //        collegeFaculty += "<tr>";
        //        //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
        //        //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
        //        //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>UG</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>PG</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>UG & PG</p></td>";
        //        //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
        //        collegeFaculty += "</tr>";



        //        var departments = db.jntuh_college_intake_existing.Where(g => g.collegeId == collegeId)
        //                .Join(db.jntuh_specialization, r => r.specializationId, ro => ro.id, (r, ro) => new { r, ro })
        //                 .Join(db.jntuh_department, d => d.ro.departmentId, C => C.id, (d, C) => new { d, C })
        //                                    .Join(db.jntuh_degree, x => x.C.degreeId, y => y.id, (x, y) => new { x, y })
        //                                    .Select(i => new
        //                                    {
        //                                        i.y.degree,
        //                                        i.x.C.departmentName
        //                                    }).ToList();

        //        int newcountdata = 1;
        //        foreach (var item in summarydata)
        //        {
        //            collegeFaculty += "<tr>";
        //            collegeFaculty += "<td colspan='1'><p align='center'>" + newcountdata + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
        //            collegeFaculty += "</tr>";
        //            newcountdata++;
        //        }
        //        collegeFaculty += "</tbody>";
        //        collegeFaculty += "</table><br />";



        //        //contents += TeachingFaculty(null, facultyList.ToList());
        //        contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);

        //        return contents;
        //    }
        //}

        //private string TeachingFaculty(List<jntuh_registered_faculty> facultyList)
        //{
        //    using (var db = new uaaasDBContext())
        //    {
        //        int count = 1;
        //        string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";
        //        string collegeFaculty = string.Empty;
        //        string ContentFaculty = string.Empty;
        //        string gender = string.Empty;
        //        string category = string.Empty;
        //        string department = string.Empty;
        //        string designation = string.Empty;
        //        string qualification = string.Empty;
        //        string dateOfAppointment = string.Empty;
        //        string teachingType = string.Empty;
        //        string ratified = string.Empty;

        //        int? deptid = facultyList.Count() > 0 ? facultyList.FirstOrDefault().DepartmentId : null;

        //        if (deptid == null)
        //        {
        //            department = "11 b).New Faculty";
        //        }
        //        else
        //        {
        //            department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
        //        }

        //        collegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
        //        collegeFaculty += "<tbody>";
        //        collegeFaculty += "<tr><td>";
        //        collegeFaculty += "<strong><u>" + department + "</u></strong> ";
        //        collegeFaculty += "</td></tr>";
        //        collegeFaculty += "</tbody>";
        //        collegeFaculty += "</table>";

        //        collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
        //        collegeFaculty += "<tbody>";
        //        collegeFaculty += "<tr>";
        //        collegeFaculty += "<td colspan='2'><p align='center'>SNo</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>Faculty Type</p></td>";
        //        collegeFaculty += "<td colspan='3'><p align='left'>Faculty Name</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
        //        collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
        //        collegeFaculty += "<td colspan='3'><p align='left'>Qualification </p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "<td colspan='4'><p align='left'>Specilization</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>EXP</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
        //        collegeFaculty += "<td colspan='3'><p align='center'>Registration Number</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
        //        collegeFaculty += "<td colspan='2'><p align='center'>Ratified as P/ Assoc. P/Asst.p</p></td>";
        //        collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
        //        collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
        //        collegeFaculty += "</tr>";
        //        var designationIds =  facultyList.Select(f => f.DesignationId).ToList();
        //        var jntuh_designations = db.jntuh_designation.Where(it => designationIds.Contains(it.id)).Select(it => new
        //        {
        //            it.id,
        //            it.designation
        //        }).ToList();
        //        var fids =  facultyList.Select(f => f.id).ToList();
        //        var jntuh_registered_faculty_educations = db.jntuh_registered_faculty_education
        //            .OrderByDescending(education => education.educationId)
        //            .Where(it => fids.Contains(it.id)).Select(it => new
        //            {
        //                it.facultyId,
        //                it.courseStudied,
        //                it.passedYear,
        //                it.specialization
        //            }).ToList();
        //        var collegeIds = facultyList.Select(f => f.collegeId).ToList();
        //        var regnumbs = facultyList.Select(f => f.RegistrationNumber).ToList();
        //        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
        //            && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
        //            {
        //                it.collegeId,
        //                it.id,
        //                it.RegistrationNumber,
        //                it.IdentifiedFor
        //            }).ToList();
        //        //var jntuh_faculty_types = db.jntuh_faculty_type.Where(it => facultyList.Select(f => f.facultyTypeId).Contains(it.id)).Select(it => new
        //        //{
        //        //    it.id,
        //        //    it.facultyType
        //        //}).ToList();
        //        string Ftype = "";
        //        foreach (var item in facultyList)
        //        {
        //            //if (count <= 14)
        //            //{
        //            if (item.GenderId == 1)
        //            {
        //                gender = "M";
        //            }
        //            else
        //            {
        //                gender = "F";
        //            }

        //            designation = jntuh_designations.Where(d => d.id == item.DesignationId).Select(d => d.designation).FirstOrDefault();
        //            if (item.DateOfAppointment != null)
        //            {
        //                dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DateOfAppointment.ToString());
        //            }
        //            if (item.isFacultyRatifiedByJNTU == true)
        //            {
        //                ratified = "Yes";
        //            }
        //            else
        //            {
        //                ratified = "No";
        //            }

        //            qualification = jntuh_registered_faculty_educations
        //                                                     .Where(education => education.facultyId == item.id)
        //                                                     .Select(education => education.courseStudied).FirstOrDefault();
        //            string strSpecialization = string.Empty;
        //            if (jntuh_registered_faculty_educations.Where(a => a.facultyId == item.id).Count() > 0)
        //            {
        //                var Specialization = jntuh_registered_faculty_educations.Where(a => a.facultyId == item.id).OrderByDescending(g => g.passedYear).FirstOrDefault().specialization;
        //                strSpecialization = Specialization;
        //            }
        //            string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == item.RegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
        //            //string strcheckList="<img src="" + serverURL + "/Content/Images/checkbox_no.png" height="10" />";
        //            switch (item.type)
        //            {
        //                case "ExistFaculty":
        //                    Ftype = "E";
        //                    break;
        //                case "NewFaculty":
        //                    Ftype = "N";
        //                    break;
        //                case "Adjunct":
        //                    Ftype = "A";
        //                    break;

        //            }
        //            collegeFaculty += "<tr>";
        //            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
        //            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + Ftype + "</p></td>";
        //            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + (item.FirstName + " " + item.MiddleName + " " + item.LastName).ToUpper() + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
        //            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

        //            if (!string.IsNullOrEmpty(designation))
        //            {
        //                collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + designation.ToUpper() + "</p></td>";
        //            }
        //            else
        //            {
        //                collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
        //            }

        //            if (!string.IsNullOrEmpty(qualification))
        //            {
        //                collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
        //            }
        //            else
        //            {
        //                collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
        //            }

        //            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";

        //            if (!string.IsNullOrEmpty(strSpecialization))
        //            {
        //                collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
        //            }
        //            else
        //            {
        //                collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
        //            }

        //            collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
        //            if (identifiedfor != null)
        //            {
        //                collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + identifiedfor.Replace("&", "&amp;") + " </p></td>";
        //            }
        //            else
        //            {
        //                collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
        //            }

        //            collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
        //            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";
        //            collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";

        //            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
        //            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.grosssalary + "</p></td>";
        //            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='center'>" + item.RegistrationNumber + "</p></td>";
        //            collegeFaculty += "<td colspan='2'><p align='center'>" + item.PANNumber + "</p></td>";
        //            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + ratified + "</p></td>";

        //            if (!string.IsNullOrEmpty(item.Photo))
        //            {
        //                string strFacultyPhoto = string.Empty;
        //                string path = @"~/Content/Upload/Faculty/Photos/" + item.Photo;
        //                path = System.Web.HttpContext.Current.Server.MapPath(path);
        //                if (System.IO.File.Exists(path))
        //                {
        //                    strFacultyPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + item.Photo + "'" + " align='center' height='40' />";
        //                    collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
        //                }
        //                else
        //                {
        //                    if (test415PDF.Equals("YES"))
        //                    {
        //                        strFacultyPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + item.Photo + "'" + " align='center' height='40' />";
        //                        collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
        //                    }
        //                    else
        //                    {
        //                        collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
        //            }

        //            collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
        //            collegeFaculty += "</tr>";
        //            //}
        //            count++;
        //        }

        //        collegeFaculty += "</tbody>";
        //        collegeFaculty += "</table>";

        //        return collegeFaculty;
        //    }
        //}
        #endregion

        

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

                string ratified = string.Empty;
                List<jntuh_college_faculty_registered> cFaculty = db.jntuh_college_faculty_registered.Where(rf => rf.collegeId == collegeId).ToList();
                string[] strRegNoS = cFaculty.Select(rf => rf.RegistrationNumber).ToArray();
                // var jntuh_registered_faculty = db.jntuh_registered_faculty.Where(rf => rf.collegeId == collegeId).ToList();
                //var facultyList = db.jntuh_registered_faculty
                //            .Where(f => strRegNoS.Contains(f.RegistrationNumber) && ((f.collegeId == collegeId && f.type == "ExistFaculty" || f.type == "Adjunct") || f.type == "NewFaculty"))
                //                    .OrderBy(faculty => faculty.DepartmentId)
                //                         .ThenBy(faculty => faculty.DesignationId)
                //                         .ThenBy(faculty => faculty.FirstName).Select(f => f).ToList();
                //var ids = facultyList.Select(d => d.DepartmentId).Distinct().ToList();
                ////var DeptIDs = facultyList.Where(a => a.DepartmentId != null && a.type == "ExistFaculty").Select(d => d.DepartmentId).Distinct().ToList();

                //if (ids.Count() > 0)
                //{
                //    //int index = 0;
                //    foreach (var deptId in ids)
                //    {
                //        //if (index == 1 || index == 2 || index == 3 || index == 5 || index == 6 || index == 7 || index == 8 || index == 9 || index == 10)
                //        //if (index >=0)
                //        //{
                //        collegeFaculty += TeachingFaculty(facultyList.Where(g => g.DepartmentId == deptId).ToList());
                //        //}
                //        //index++;
                //    }
                //}

                #region TeachingFacultyLogic Begin
                int Facultytype = db.jntuh_faculty_type.Where(t => t.facultyType == "Teaching").Select(t => t.id).FirstOrDefault();
              //  List<jntuh_college_faculty> jntuh_college_faculty = db.jntuh_college_faculty.Where(f => f.facultyTypeId == Facultytype && f.collegeId == collegeId).OrderBy(f => f.facultyDepartmentId).ThenBy(f => f.facultyDesignationId).ThenBy(f => f.facultyFirstName).ToList();

                List<CollegeFaculty> teachingFaculty = new List<CollegeFaculty>();

                //Commented By Srinivas.T   FacultyVerificationStatus
                //List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == userCollegeID && r.existingFacultyId == null && r.createdBy != 63809).Select(r => r).ToList();
                List<jntuh_college_faculty_registered> regFaculty = db.jntuh_college_faculty_registered.Where(r => r.collegeId == collegeId).Select(r => r).ToList();//&& r.existingFacultyId == null

                var RegistredFacultyLog = db.jntuh_registered_faculty_log.AsNoTracking().Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks,F.DeactivationReason }).ToList();
                var jntuh_registered_facultys = db.jntuh_registered_faculty.AsNoTracking().Select(F => F).ToList();
                //List<jntuh_registered_faculty> rFaculty1 = jntuh_registered_facultys.Where(F => F.RegistrationNumber == "9562-150411-152607").ToList();
                
                foreach (var item in regFaculty)
                {
                    CollegeFaculty collegeFacultynew = new CollegeFaculty();
                    jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == item.RegistrationNumber.Trim()).FirstOrDefault();

                    //if (rFaculty.RegistrationNumber == "9893-150415-173751")
                    //{

                    //}


                    //if (rFaculty.collegeId != null)
                    //{
                    //    collegeFaculty.collegeId = (int)rFaculty.collegeId;
                    //}
                    collegeFacultynew.id = rFaculty.id;
                    collegeFacultynew.TotalExperience = rFaculty.TotalExperience != null ? rFaculty.TotalExperience : 0;
                    collegeFacultynew.collegeId = collegeId;
                    //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
                    collegeFacultynew.facultyType = rFaculty.type;
                    collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                    collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                    collegeFacultynew.facultySurname = rFaculty.LastName;
                   
                    collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                    collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                    collegeFacultynew.photo = rFaculty.Photo;
                    //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;

                    if (rFaculty.DateOfBirth != null)
                        collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                    //if (rFaculty.WorkingStatus == true)
                    //{

                    // rFaculty.DesignationId!=null? (int)rFaculty.DesignationId:0;
                    //collegeFaculty.facultyDesignationId = (int)rFaculty.DesignationId;
                    collegeFacultynew.facultyDesignationId = rFaculty.DesignationId != null ? (int)rFaculty.DesignationId : 0;
                    collegeFacultynew.designation = db.jntuh_designation.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                    collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                    collegeFacultynew.facultyDepartmentId = item.DepartmentId != null ? (int)item.DepartmentId : 0;
                    collegeFacultynew.department = db.jntuh_department.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                    // }
                    collegeFacultynew.grossSalary = rFaculty.grosssalary;
                    collegeFacultynew.facultyEmail = rFaculty.Email;
                    collegeFacultynew.facultyMobile = rFaculty.Mobile;
                    collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                    collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                    collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;
                    collegeFacultynew.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == rFaculty.RegistrationNumber);
                    collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                   
                    collegeFacultynew.FacultyDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == collegeFacultynew.FacultyRegistrationNumber).Select(i => i.DeactivationReason).FirstOrDefault();
                    collegeFacultynew.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                    collegeFacultynew.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == rFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
                    collegeFacultynew.PANDeactivationReasion = rFaculty.PanDeactivationReason;
                    collegeFacultynew.BlackList = (bool)rFaculty.Blacklistfaculy;
                    teachingFaculty.Add(collegeFacultynew);
                }
                var jntuh_college_faculty_registereds = db.jntuh_college_faculty_registered.Where(C => C.collegeId == collegeId).Select(C => C).ToList();

                jntuh_college_faculty_registered eFaculty = null;
                var jntuh_designations = db.jntuh_designation.Select(d => new { d.id, d.designation }).ToList();
                var jntuh_departments = db.jntuh_department.Select(d => new { d.id, d.departmentName }).ToList();


                #region Unused code

                //foreach (var faculty in jntuh_college_faculty)
                //{

                //    CollegeFaculty collegeFacultynew = new CollegeFaculty();
                //    collegeFacultynew.id = faculty.id;

                //    eFaculty = jntuh_college_faculty_registereds.Where(r => r.existingFacultyId == faculty.id).Select(r => r).FirstOrDefault();

                //    if (eFaculty != null)
                //    {
                //        collegeFacultynew.FacultyRegistrationNumber = eFaculty.RegistrationNumber;
                //        //if (eFaculty.RegistrationNumber == "9893-150415-173751")
                //        //{

                //        //}
                //    }

                //    if (string.IsNullOrEmpty(collegeFacultynew.FacultyRegistrationNumber) )
                //    {
                //        if (eFaculty != null)
                //        {
                //            collegeFacultynew.id = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.id).FirstOrDefault();
                //            collegeFacultynew.TotalExperience = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.TotalExperience).FirstOrDefault();
                //            collegeFacultynew.collegeId = faculty.collegeId;
                //            collegeFacultynew.facultyTypeId = faculty.facultyTypeId;
                //            collegeFacultynew.facultyType = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.type).FirstOrDefault();
                //            collegeFacultynew.facultyFirstName = faculty.facultyFirstName;
                //            collegeFacultynew.facultyLastName = faculty.facultyLastName;
                //            collegeFacultynew.facultySurname = faculty.facultySurname;
                //            collegeFacultynew.facultyGenderId = faculty.facultyGenderId;
                //            collegeFacultynew.facultyFatherName = faculty.facultyFatherName;
                //            collegeFacultynew.facultyCategoryId = faculty.facultyCategoryId;
                //            collegeFacultynew.photo = faculty.facultyPhoto;
                //            collegeFacultynew.grossSalary = faculty.grossSalary;
                          
                //            collegeFacultynew.FacultyDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == collegeFacultynew.FacultyRegistrationNumber).Select(i => i.DeactivationReason).FirstOrDefault();
                //            if (faculty.facultyDateOfBirth != null)
                //                collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfBirth.ToString());

                //            collegeFacultynew.facultyDesignationId = faculty.facultyDesignationId;
                //            collegeFacultynew.designation = jntuh_designations.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                //            collegeFacultynew.facultyOtherDesignation = faculty.facultyOtherDesignation;
                //            collegeFacultynew.facultyDepartmentId = faculty.facultyDepartmentId;
                //            collegeFacultynew.department = jntuh_departments.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();

                //            if (faculty.facultyDateOfAppointment != null)
                //                collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfAppointment.ToString());

                //            if (faculty.facultyDateOfResignation != null)
                //                collegeFacultynew.dateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfResignation.ToString());

                //            collegeFacultynew.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;

                //            if (faculty.facultyDateOfRatification != null)
                //                collegeFacultynew.dateOfRatification = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.facultyDateOfRatification.ToString());

                //            collegeFacultynew.facultyDurationOfRatification = faculty.facultyDurationOfRatification;
                //            collegeFacultynew.facultySalary1 = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.grosssalary).FirstOrDefault(); ;
                //            collegeFacultynew.facultyPreviousExperience = Convert.ToInt32(faculty.facultyPreviousExperience);
                //            collegeFacultynew.facultyPayScale = faculty.facultyPayScale;
                //            collegeFacultynew.facultyEmail = faculty.facultyEmail;
                //            collegeFacultynew.facultyMobile = faculty.facultyMobile;
                //            collegeFacultynew.facultyPANNumber = faculty.facultyPANNumber;
                //            collegeFacultynew.facultyAadhaarNumber = faculty.facultyAadhaarNumber;
                //            collegeFacultynew.isRelatedToExamBranch = faculty.isRelatedToExamBranch;
                //            collegeFacultynew.isRelatedToPlacementCell = faculty.isRelatedToPlacementCell;
                //            collegeFacultynew.facultyAchievements1 = faculty.facultyAchievements1;
                //            collegeFacultynew.facultyAchievements2 = faculty.facultyAchievements2;
                //            collegeFacultynew.facultyAchievements3 = faculty.facultyAchievements3;
                //            collegeFacultynew.FacultyRegistrationNumber = string.Empty;
                //            collegeFacultynew.PANDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.PanDeactivationReason).FirstOrDefault();
                //            if (eFaculty != null)
                //            {
                //                collegeFacultynew.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber);
                //                collegeFacultynew.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
                //                collegeFacultynew.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                //            }
                //        }
                //    }
                //    else
                //    {

                //        if (eFaculty != null)
                //        {
                //            jntuh_registered_faculty rFaculty = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).FirstOrDefault();
                //            //if (rFaculty.RegistrationNumber == "9562-150411-152607")
                //            //{

                //            //}
                //            if (rFaculty.collegeId != null)
                //            {
                //                collegeFacultynew.collegeId = (int)rFaculty.collegeId;
                //            }

                //            //collegeFaculty.facultyTypeId = rFaculty.facultyTypeId;
                //            collegeFacultynew.id = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.id).FirstOrDefault();
                //            collegeFacultynew.TotalExperience = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.TotalExperience).FirstOrDefault();
                //            collegeFacultynew.facultyType = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.type).FirstOrDefault();
                //            collegeFacultynew.facultySalary1 = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.grosssalary).FirstOrDefault(); ;
                //            collegeFacultynew.facultyFirstName = rFaculty.FirstName;
                //            collegeFacultynew.facultyLastName = rFaculty.MiddleName;
                //            collegeFacultynew.facultySurname = rFaculty.LastName;
                //            collegeFacultynew.facultyGenderId = rFaculty.GenderId;
                //            collegeFacultynew.facultyFatherName = rFaculty.FatherOrHusbandName;
                //            collegeFacultynew.photo = faculty.facultyPhoto;
                //            collegeFacultynew.grossSalary = faculty.grossSalary;
                //            //collegeFaculty.facultyCategoryId = rFaculty.facultyCategoryId;
                //            if (rFaculty.DateOfAppointment != null)
                //                collegeFacultynew.dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfAppointment.ToString());
                //            if (rFaculty.DateOfBirth != null)
                //                collegeFacultynew.dateOfBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(rFaculty.DateOfBirth.ToString());

                //            if (rFaculty.DesignationId != null)
                //            {
                //                collegeFacultynew.facultyDesignationId = (int)rFaculty.DesignationId;
                //                collegeFacultynew.designation = jntuh_designations.Where(d => d.id == collegeFacultynew.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                //            }

                //            collegeFacultynew.facultyOtherDesignation = rFaculty.OtherDesignation;
                           
                //            collegeFacultynew.FacultyDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == collegeFacultynew.FacultyRegistrationNumber).Select(i => i.DeactivationReason).FirstOrDefault();
                //            if (rFaculty.DepartmentId != null)
                //            {
                //                collegeFacultynew.facultyDepartmentId = (int)rFaculty.DepartmentId;
                //                collegeFacultynew.department = jntuh_departments.Where(d => d.id == collegeFacultynew.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                //            }

                //            collegeFacultynew.facultyOtherDepartment = rFaculty.OtherDepartment;
                //            collegeFacultynew.facultyEmail = rFaculty.Email;
                //            collegeFacultynew.facultyMobile = rFaculty.Mobile;
                //            collegeFacultynew.facultyPANNumber = rFaculty.PANNumber;
                //            collegeFacultynew.facultyAadhaarNumber = rFaculty.AadhaarNumber;
                //            collegeFacultynew.FacultyRegistrationNumber = rFaculty.RegistrationNumber;

                //            collegeFacultynew.facultyCount = RegistredFacultyLog.Count(F => F.RegistrationNumber == eFaculty.RegistrationNumber);
                //            collegeFacultynew.Remarks = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.Remarks).FirstOrDefault();
                //            collegeFacultynew.FacultyVerificationStatus = RegistredFacultyLog.Where(F => F.RegistrationNumber == eFaculty.RegistrationNumber).Select(F => F.FacultyApprovedStatus).FirstOrDefault();
                //            collegeFacultynew.PANDeactivationReasion = jntuh_registered_facultys.Where(f => f.RegistrationNumber == eFaculty.RegistrationNumber.Trim()).Select(F => F.PanDeactivationReason).FirstOrDefault();
                //        }

                //        teachingFaculty.Add(collegeFacultynew);
                //    }

                //}
                #endregion EndUnusedCode
                #endregion TeachingFacultyLogic End

                var facultyList =teachingFaculty.Where(f => f.facultyType != "Adjunct" && f.BlackList == false).Select(e => e).OrderBy(e => e.department).ToList();
                var RegNum = facultyList.Where(F => F.FacultyRegistrationNumber == "33150405-143554").Select(F => F);

                int[] DeptIDs = facultyList.Select(d => d.facultyDepartmentId).Distinct().ToArray();

                var noregnos = facultyList.Where(i => i.FacultyRegistrationNumber == null).ToList();

                var type = teachingFaculty.Where(f => f.facultyType == "Adjunct"&&f.BlackList==false).ToList();

                var BlackListFaculty = teachingFaculty.Where(e => e.BlackList == true).Select(e => e).ToList();

                if (DeptIDs.Count() > 0)
                {
                    foreach (int deptId in DeptIDs)
                    {
                        collegeFaculty += TeachingFaculty(facultyList.Where(g => g.facultyDepartmentId == deptId).OrderBy(g=>g.facultyFirstName).ToList());
                    }
                }

                if (type.Count() > 0)
                {
                    collegeFaculty += TeachingFaculty(type);
                }

                if (BlackListFaculty.Count()!=0)
                {
                   int blackcount = 1;
                   collegeFaculty += "<strong><p style='font-size: 9px;'><u><b>Blacklisted Faculty:</b></u></p></strong>";
                   collegeFaculty += "<p style='font-size: 9px;'>The following faculty uploaded by the college are blacklisted during Academic Year 2016-17 Affiliation Process and hence will not be considered during this Affiliation Process for the Academic Year 2017-18.</p> <br/>";
                   collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                    collegeFaculty += "<tbody>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='2'><p align='center'>SNo</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='center'>Status</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>Faculty Name</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Desg</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>Qualification </p></td>";
                    collegeFaculty += "<td colspan='4'><p align='left'>Specilization</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>Reg.No.</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
                    collegeFaculty += "</tr>";
                    foreach (var blacklistitem in BlackListFaculty)
                    {
                        string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == blacklistitem.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                        if (blacklistitem.facultyGenderId == 1)
                        {
                            gender = "M";
                        }
                        else
                        {
                            gender = "F";
                        }

                        designation = jntuh_designations.Where(d => d.id == blacklistitem.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                        if (blacklistitem.dateOfAppointment != null)
                        {
                            dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(blacklistitem.dateOfAppointment.ToString());
                        }
                        if (blacklistitem.isFacultyRatifiedByJNTU == true)
                        {
                            ratified = "Yes";
                        }
                        else
                        {
                            ratified = "No";
                        }

                        qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                                 .Where(education => education.facultyId == blacklistitem.id)
                                                                 .Select(education => education.courseStudied).FirstOrDefault();
                        string strSpecialization = string.Empty;
                        if (db.jntuh_registered_faculty_education.Where(a => a.facultyId == blacklistitem.id).Count() > 0)
                        {
                            var Specialization = db.jntuh_registered_faculty_education.Where(a => a.facultyId == blacklistitem.id).OrderByDescending(g => g.passedYear).FirstOrDefault().specialization;
                            strSpecialization = Specialization;
                        }

                        var collegeIds = facultyList.Select(f => f.collegeId).ToList();
                        var regnumbs = facultyList.Select(f => f.FacultyRegistrationNumber).ToList();

                        var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
                               && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
                               {
                                   it.collegeId,
                                   it.id,
                                   it.RegistrationNumber,
                                   it.IdentifiedFor
                               }).ToList();



                        string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == blacklistitem.FacultyRegistrationNumber && f.collegeId == blacklistitem.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                       

                      //  var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();

                        var blacklistFtype = RegistredFacultyLog.Where(i => i.RegistrationNumber == blacklistitem.FacultyRegistrationNumber).Select(i => i.Remarks).FirstOrDefault();
                        switch (blacklistFtype)
                        {
                            case "Approved":
                                blacklistFtype = "C";
                                break;
                            case "Pending":
                                blacklistFtype = "P";
                                break;
                            case null:
                                blacklistFtype = "C";
                                break;
                            default:
                                blacklistFtype = "NA";
                                break;

                        }

                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + blackcount + "</p></td>";

                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + (blacklistitem.facultyFirstName + " " + blacklistitem.facultySurname + " " + blacklistitem.facultyLastName).ToUpper() + "</p></td>";
                         collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                        if (!string.IsNullOrEmpty(designation))
                        {
                            // collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + designation.ToUpper() + "</p></td>";
                            if (blacklistitem.facultyDesignationId == 1)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" + "P" + "</p></td>";
                            }
                            else if (blacklistitem.facultyDesignationId == 2)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" + "ASP" + "</p></td>";
                            }
                            else if (blacklistitem.facultyDesignationId == 3)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" + "AP" + "</p></td>";
                            }
                            else
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                            }
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (!string.IsNullOrEmpty(qualification))
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                       
                        if (!string.IsNullOrEmpty(strSpecialization))
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (identifiedfor != null)
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + identifiedfor.Replace("&", "&amp;") + " </p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";


                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + blacklistitem.FacultyRegistrationNumber + "</p></td>";

                        collegeFaculty += "<td colspan='2'><p align='center'>" + blacklistitem.facultyPANNumber + "</p></td>";
                  
                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string strFacultyPhoto = string.Empty;
                            string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            if (System.IO.File.Exists(path))
                            {
                                strFacultyPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    strFacultyPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                    collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                                }
                                else
                                {
                                    collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                                }
                            }

                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
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
                collegeFaculty += "<th><b>Total Adjunct Faculty:</b></th>";
                collegeFaculty += "<td style='text-align:center'><b>" + type.Count() + "</b></td>";
                collegeFaculty += "<th><b>Total Absent Faculty:</b></th>";
                collegeFaculty += "<td></td>";
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
                    collegeFaculty += "<td colspan='6'><p align='center'>Total no. of faculty available</p></td>";
                    //collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty Ratified </p></td>";
                    collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty Selected After A416 </p></td>";
                  
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>UG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>PG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>UG & PG</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'></p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF </p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>Available as per CF</p></td>";

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
                                                }).OrderBy(e=>e.departmentName)
                                                .ToList();
                    int countdata = 1;
                    foreach (var item in summarydata)
                    {
                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + countdata + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='right'>" + item.UG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='right'>" + item.PG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='right'>" + item.UGPG + " </p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        //collegeFaculty += "<td colspan='1'><p align='right'>" + item.FC + "</p></td>";
                        //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "</tr>";
                        countdata++;
                    }
                    collegeFaculty += "</tbody>";
                    collegeFaculty += "</table><br />";
                }

                if (type.Count() > 0)
                {
                    collegeFaculty += "<strong><p style='font-size: 9px;'><u>11 b).Adjunct Faculty Summary Sheet</u>:</p></strong> <br />";
                    collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                    collegeFaculty += "<tbody>";
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1' rowspan='3'><p align='center'>SNo</p></td>";
                    collegeFaculty += "<td colspan='1' rowspan='3'><p align='left'>Degree</p></td>";
                    collegeFaculty += "<td colspan='2' rowspan='3'><p align='center'>Department / Specialization</p></td>";
                    collegeFaculty += "<td colspan='6'><p align='center'>Total no. of faculty available as per CF</p></td>";
                    //collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty ratified </p></td>";
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>UG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>PG</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>UG & PG</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                    collegeFaculty += "</tr>";
                    collegeFaculty += "<tr>";
                    //collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'></p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF </p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>Uploaded</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='left'>Available as per CF</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='left'>Available as per CF</p></td>";

                    collegeFaculty += "</tr>";
                    var summarydata = type.Join(cFaculty, i => i.FacultyRegistrationNumber, j => j.RegistrationNumber, (i, j) => new { i, j })
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
                                                }).ToList();
                    int countdata = 1;
                    foreach (var item in summarydata)
                    {
                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + countdata + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.UG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.PG + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>" + item.UGPG + " </p></td>";
                        collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        //collegeFaculty += "<td colspan='1'><p align='right'>" + item.FC + "</p></td>";
                        //collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                        collegeFaculty += "</tr>";
                        countdata++;
                    }
                    collegeFaculty += "</tbody>";
                    collegeFaculty += "</table><br />";
                }


                //var type = facultyList.Where(f => f.type == "NewFaculty").ToList();
                //if (type.Count() > 0)
                //{
                //    //collegeFaculty += "<strong><p><u>11 b).New Faculty</u>:</p></strong> <br />";
                //    collegeFaculty += TeachingFaculty(facultyList.Where(g => g.type == "NewFaculty").ToList());
                //}



                //collegeFaculty += "<strong><p style='font-size: 9px;'><u>11 c).New Faculty Summary Sheet</u>:</p></strong> <br />";
                //collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                //collegeFaculty += "<tbody>";
                //collegeFaculty += "<tr>";
                //collegeFaculty += "<td colspan='1' rowspan='2'><p align='center'>SNo</p></td>";
                //collegeFaculty += "<td colspan='1' rowspan='2'><p align='left'>Degree</p></td>";
                //collegeFaculty += "<td colspan='2' rowspan='2'><p align='center'>Department / Specialization</p></td>";
                //collegeFaculty += "<td colspan='6'><p align='center'>Total no. of faculty available as per CF</p></td>";
                //collegeFaculty += "<td colspan='2' rowspan='2'><p align='left'>Total faculty ratified </p></td>";
                //collegeFaculty += "</tr>";
                //collegeFaculty += "<tr>";
                ////collegeFaculty += "<td colspan='1'><p align='center'> </p></td>";
                ////collegeFaculty += "<td colspan='1'><p align='left'>&nbsp;</p></td>";
                ////collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                //collegeFaculty += "<td colspan='2'><p align='left'>UG</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='left'>PG</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='left'>UG & PG</p></td>";
                ////collegeFaculty += "<td colspan='2'><p align='center'> </p></td>";
                //collegeFaculty += "</tr>";



                //var departments = db.jntuh_college_intake_existing.Where(g => g.collegeId == collegeId)
                //        .Join(db.jntuh_specialization, r => r.specializationId, ro => ro.id, (r, ro) => new { r, ro })
                //         .Join(db.jntuh_department, d => d.ro.departmentId, C => C.id, (d, C) => new { d, C })
                //                            .Join(db.jntuh_degree, x => x.C.degreeId, y => y.id, (x, y) => new { x, y })
                //                            .Select(i => new
                //                            {
                //                                i.y.degree,
                //                                i.x.C.departmentName
                //                            }).ToList();

                //int newcountdata = 1;
                //foreach (var item in summarydata)
                //{
                //    collegeFaculty += "<tr>";
                //    collegeFaculty += "<td colspan='1'><p align='center'>" + newcountdata + "</p></td>";
                //    collegeFaculty += "<td colspan='1'><p align='left'>" + item.degree + "</p></td>";
                //    collegeFaculty += "<td colspan='2'><p align='left'>" + item.departmentName + "</p></td>";
                //    collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                //    collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                //    collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                //    collegeFaculty += "<td colspan='2'><p align='left'>&nbsp;</p></td>";
                //    collegeFaculty += "</tr>";
                //    newcountdata++;
                //}
                //collegeFaculty += "</tbody>";
                //collegeFaculty += "</table><br />";



                //contents += TeachingFaculty(null, facultyList.ToList());
                contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);

                return contents;
            }
        }

        private string TeachingFaculty(List<CollegeFaculty> facultyList)
        {
            int count = 1;
            string strcheckList = "<img alt='' src='" + serverURL + "/Content/Images/checkbox_no_b.png' height='8' />";
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

            //int deptid = facultyList.Count() > 0 ? facultyList.FirstOrDefault().facultyDepartmentId : null;

            //if (deptid == null)
            //{
            //    department = "11 b).New Faculty";
            //}
            //else
            //{
            //    department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
            //}
            int? deptid = null;
            if (facultyList.Count() > 0)
            {
                deptid = facultyList.FirstOrDefault().facultyDepartmentId;
            }
            else
                deptid = null;
            if (deptid == null || deptid == 0)
            {
                department = "Others (No Department/Specialization)";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
            }
            var jntuh_registered_facultys = db.jntuh_registered_faculty.Select(F => new { F.RegistrationNumber, F.Photo }).ToList();
            if (facultyList.Count() > 0 && facultyList.FirstOrDefault().facultyType == "Adjunct")
            {
                department = "Adjunct Faculty";

                #region adjunct
               
                collegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<strong><u>" + department + "</u></strong> ";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<b>NOC-</b> No Objection Certificate From Parent Organization,<b>PBM-</b> Professional Body Members,<b>ISCM-</b> Internal Selection Committee Minutes";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";                
                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='2'><p align='center'>SNo</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>Status</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='4'><p align='left'>Specilization</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>EXP</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>Reg.No.</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>NOC</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>PBM</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>ISCM</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
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
                    .Where(it => fids.Contains(it.id)).Select(it => new
                    {
                        it.facultyId,
                        it.courseStudied,
                        it.passedYear,
                        it.specialization
                    }).ToList();
                var collegeIds = facultyList.Select(f => f.collegeId).ToList();
                var regnumbs = facultyList.Select(f => f.FacultyRegistrationNumber).ToList();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
                    && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
                    {
                        it.collegeId,
                        it.id,
                        it.RegistrationNumber,
                        it.IdentifiedFor
                    }).ToList();
                //var jntuh_faculty_types = db.jntuh_faculty_type.Where(it => facultyList.Select(f => f.facultyTypeId).Contains(it.id)).Select(it => new
                //{
                //    it.id,
                //    it.facultyType
                //}).ToList();
                string Ftype = "";
               
                foreach (var item in facultyList)
                {
                    //if (count <= 14)
                    //{
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
                    if (item.isFacultyRatifiedByJNTU == true)
                    {
                        ratified = "Yes";
                    }
                    else
                    {
                        ratified = "No";
                    }

                    qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                             .Where(education => education.facultyId == item.id)
                                                             .Select(education => education.courseStudied).FirstOrDefault();
                    string strSpecialization = string.Empty;
                    if (db.jntuh_registered_faculty_education.Where(a => a.facultyId == item.id).Count() > 0)
                    {
                        var Specialization = db.jntuh_registered_faculty_education.Where(a => a.facultyId == item.id).OrderByDescending(g => g.passedYear).FirstOrDefault().specialization;
                        strSpecialization = Specialization;
                    }
                    string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                    //string strcheckList="<img src="" + serverURL + "/Content/Images/checkbox_no.png" height="10" />";
                    //switch (item.facultyType)
                    //{
                    //    case "ExistFaculty":
                    //        Ftype = "E";
                    //        break;
                    //    case "NewFaculty":
                    //        Ftype = "N";
                    //        break;
                    //    case "Adjunct":
                    //        Ftype = "A";
                    //        break;

                    //}


                    var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();

                    Ftype = RegistredFacultyLog.Where(i => i.RegistrationNumber == item.FacultyRegistrationNumber).Select(i => i.Remarks).FirstOrDefault();
                    switch (Ftype)
                    {
                        case "Approved":
                            Ftype = "C";
                            break;
                        case "Pending":
                            Ftype = "P";
                            break;
                        case null:
                            Ftype = "C";
                            break;
                        default:
                            Ftype = "NA";
                            break;

                    }
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
                    //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + Ftype + "</p></td>";
                    collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + (item.facultyFirstName + " " + item.facultySurname + " " + item.facultyLastName).ToUpper() + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                    if (!string.IsNullOrEmpty(designation))
                    {
                       // collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + designation.ToUpper() + "</p></td>";
                        if (item.facultyDesignationId == 1)
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 10px;'><p align='left'><strong>" + "P" + "</strong></p></td>";
                        }
                        else if (item.facultyDesignationId == 2)
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 10px;'><p align='left'><strong>" + "ASP" + "</strong></p></td>";
                        }
                        else if (item.facultyDesignationId == 3)
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 10px;'><p align='left'><strong>" + "AP" + "</strong></p></td>";
                        }
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                    }

                    if (!string.IsNullOrEmpty(qualification))
                    {
                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                    }

                    collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";

                    if (!string.IsNullOrEmpty(strSpecialization))
                    {
                        collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                    }

                    collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                    if (identifiedfor != null)
                    {
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + identifiedfor.Replace("&", "&amp;") + " </p></td>";
                    }
                    else
                    {
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                    }
                    string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                    collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";

                    //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
                    //collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.grossSalary + "</p></td>";
                    collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                    //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                   // collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + ratified + "</p></td>";
                    collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                    if (!string.IsNullOrEmpty(Facultyphoto))
                    {
                        string strFacultyPhoto = string.Empty;
                        string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                        path = System.Web.HttpContext.Current.Server.MapPath(path);
                        if (System.IO.File.Exists(path))
                        {
                            strFacultyPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                            collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                        }
                        else
                        {
                            if (test415PDF.Equals("YES"))
                            {
                                strFacultyPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                            }
                            else
                            {
                                collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                            }
                        }

                    }
                    else
                    {
                        collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                    }

                    collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                    collegeFaculty += "</tr>";
                    //}
                    count++;
                }

                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";

                #endregion
            }
            else
            {
                #region teaching Faculty
               
                collegeFaculty += "<br /><table border='0' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr><td>";
                collegeFaculty += "<strong><u>" + department + "</u></strong> ";
                collegeFaculty += "</td></tr>";
                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";

                collegeFaculty += "<table border='1' cellspacing='0' cellpadding='3' style='font-size: 9px;'>";
                collegeFaculty += "<tbody>";
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='2'><p align='center'>SNo</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>Status</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Faculty Name</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
                collegeFaculty += "<td colspan='1'><p align='left'>Desg</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='4'><p align='left'>Specilization</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>EXP</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Reg.No.</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Pan Number</p></td>";
                //collegeFaculty += "<td colspan='3'><p align='center'>Status</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>SCM</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>F16</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>BAS</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>CF</p></td>";
                collegeFaculty += "<td colspan='3'><p align='center'>Remarks</p></td>";
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
                    .Where(it => fids.Contains(it.id)).Select(it => new
                    {
                        it.facultyId,
                        it.courseStudied,
                        it.passedYear,
                        it.specialization
                    }).ToList();
                var collegeIds = facultyList.Select(f => f.collegeId).ToList();
                var regnumbs = facultyList.Select(f => f.FacultyRegistrationNumber).ToList();
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(it => collegeIds.Contains(it.collegeId)
                    && regnumbs.Contains(it.RegistrationNumber)).Select(it => new
                    {
                        it.collegeId,
                        it.id,
                        it.RegistrationNumber,
                        it.IdentifiedFor
                    }).ToList();
                //var jntuh_faculty_types = db.jntuh_faculty_type.Where(it => facultyList.Select(f => f.facultyTypeId).Contains(it.id)).Select(it => new
                //{
                //    it.id,
                //    it.facultyType
                //}).ToList();
                string Ftype = "";
                var nophotos = facultyList.Select(i => i.photo).ToList();
                foreach (var item in facultyList)
                {
                    //if (count <= 14)
                    //{
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
                    if (item.isFacultyRatifiedByJNTU == true)
                    {
                        ratified = "Yes";
                    }
                    else
                    {
                        ratified = "No";
                    }

                    qualification = db.jntuh_registered_faculty_education.OrderByDescending(education => education.educationId)
                                                             .Where(education => education.facultyId == item.id)
                                                             .Select(education => education.courseStudied).FirstOrDefault();
                    string strSpecialization = string.Empty;
                    if (db.jntuh_registered_faculty_education.Where(a => a.facultyId == item.id).Count() > 0)
                    {
                        var Specialization = db.jntuh_registered_faculty_education.Where(a => a.facultyId == item.id).OrderByDescending(g => g.passedYear).FirstOrDefault().specialization;
                        strSpecialization = Specialization;
                    }
                    string identifiedfor = jntuh_college_faculty_registered.Where(f => f.RegistrationNumber == item.FacultyRegistrationNumber && f.collegeId == item.collegeId).Select(f => f.IdentifiedFor).FirstOrDefault();
                    //string strcheckList="<img src="" + serverURL + "/Content/Images/checkbox_no.png" height="10" />";
                    //switch (item.facultyType)
                    //{
                    //    case "ExistFaculty":
                    //        Ftype = "E";
                    //        break;
                    //    case "NewFaculty":
                    //        Ftype = "N";
                    //        break;
                    //    case "Adjunct":
                    //        Ftype = "A";
                    //        break;

                    //}


                    var RegistredFacultyLog = db.jntuh_registered_faculty_log.Select(F => new { F.RegistrationNumber, F.FacultyApprovedStatus, F.Remarks }).ToList();

                    Ftype = RegistredFacultyLog.Where(i => i.RegistrationNumber == item.FacultyRegistrationNumber).Select(i => i.Remarks).FirstOrDefault();
                    switch (Ftype)
                    {
                        case "Approved":
                            Ftype = "C";
                            break;
                        case "Pending":
                            Ftype = "P";
                            break;
                        case null:
                            Ftype = "C";
                            break;
                        default:
                            Ftype = "NA";
                            break;

                    }
                    string Facultyphoto = jntuh_registered_facultys.Where(F => F.RegistrationNumber == item.FacultyRegistrationNumber).Select(F => F.Photo).FirstOrDefault();
                    if ((item.PANDeactivationReasion !=null && item.PANDeactivationReasion !="" ) || (item.FacultyDeactivationReasion !=null && item.FacultyDeactivationReasion !=""))
                    {
                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
                        //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + Ftype + "</p></td>";
                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + (item.facultyFirstName + " " + item.facultySurname + " " + item.facultyLastName).ToUpper() + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                        if (!string.IsNullOrEmpty(designation))
                        {
                         //   collegeFaculty += "<td colspan='3' style='font-size: 10px;'><p align='left'><strong>" + designation.ToUpper() + "</strong></p></td>";
                            if (item.facultyDesignationId == 1)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>" + "P" + "</p></td>";
                            }else if (item.facultyDesignationId == 2)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>" + "ASP" + "</p></td>";
                            }
                            else if (item.facultyDesignationId == 3)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>" +
                                                  "AP" + "</p></td>";
                            }
                            else
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                            }
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (!string.IsNullOrEmpty(qualification))
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";

                        if (!string.IsNullOrEmpty(strSpecialization))
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        if (identifiedfor != null)
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + identifiedfor.Replace("&", "&amp;") + " </p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";

                        //collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='center'><strong>" + item.TotalExperience.ToString().Replace(".00", "") + "</strong></p></td>";
                        //collegeFaculty += "<td colspan='2' style='font-size: 10px;'><p align='center'><strong>" + item.grossSalary + "</strong></p></td>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                        //collegeFaculty += "<td colspan='3' style='font-size: 8px;font-style:italic'><p align='center'><strong>" + item.PANDeactivationReasion + "# "+item.FacultyDeactivationReasion + "</strong></p></td>";
                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='center'><b>Y</b> " + strcheckList + "<br/><b>N</b> " + strcheckList + "<br/><b>E</b> " + strcheckList + "</p></td>";
                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string strFacultyPhoto = string.Empty;
                            string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            if (System.IO.File.Exists(path))
                            {
                                strFacultyPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    strFacultyPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                    collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                                }
                                else
                                {
                                    collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                                }
                            }

                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='3'><p align='center'>" + "" + "</p></td>";
                        collegeFaculty += "</tr>";
                        //}
                        count++;
                    }
                    else
                    {
                        collegeFaculty += "<tr>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + count + "</p></td>";
                        //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + Ftype + "</p></td>";
                        collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + (item.facultyFirstName + " " + item.facultySurname + " " + item.facultyLastName).ToUpper() + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + gender + "</p></td>";

                        if (!string.IsNullOrEmpty(designation))
                        {
                           // collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + designation.ToUpper() + "</p></td>";
                            if (item.facultyDesignationId == 1)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" + "P" + "</p></td>";
                            }
                            else if (item.facultyDesignationId == 2)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" + "ASP" + "</p></td>";
                            }
                            else if (item.facultyDesignationId == 3)
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 10px;'><p align='left'>" +"AP" + "</p></td>";
                            }
                            else
                            {
                                collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                            }
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        if (!string.IsNullOrEmpty(qualification))
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>" + qualification.ToUpper() + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";

                        if (!string.IsNullOrEmpty(strSpecialization))
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>" + strSpecialization.ToUpper().Replace("&", "&amp;") + "</p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='4' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        if (identifiedfor != null)
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + identifiedfor.Replace("&", "&amp;") + " </p></td>";
                        }
                        else
                        {
                            collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + dateOfAppointment + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";

                        //collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
                        //collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.grossSalary + "</p></td>";
                        collegeFaculty += "<td colspan='2' style='font-size: 8px;'><p align='center'>" + item.FacultyRegistrationNumber + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPANNumber + "</p></td>";
                        //collegeFaculty += "<td colspan='3' style='font-size: 8px;'><p align='center'><strong><u>" + item.PANDeactivationReasion + "</u></strong></p></td>";
                        collegeFaculty += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='2'><p align='center'><b>Y</b> " + strcheckList + "<br/><b>N</b> " + strcheckList + "<br/><b>E</b> " + strcheckList + "</p></td>";
                        if (!string.IsNullOrEmpty(Facultyphoto))
                        {
                            string strFacultyPhoto = string.Empty;
                            string path = @"~/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim();
                            path = System.Web.HttpContext.Current.Server.MapPath(path);
                            if (System.IO.File.Exists(path))
                            {
                                strFacultyPhoto = "<img alt='' src='" + serverURL + "/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                            }
                            else
                            {
                                if (test415PDF.Equals("YES"))
                                {
                                    strFacultyPhoto = "<img alt='' src='http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + Facultyphoto.Trim() + "'" + " align='center' height='40' />";
                                    collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                                }
                                else
                                {
                                    collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                                }
                            }

                        }
                        else
                        {
                            collegeFaculty += "<td colspan='3'><p align='center'>&nbsp;</p></td>";
                        }

                        collegeFaculty += "<td colspan='1'><p align='center'>" + strcheckList + "</p></td>";
                        collegeFaculty += "<td colspan='3'><p align='center'>" + "" + "</p></td>";
                        collegeFaculty += "</tr>";
                        //}
                        count++;
                    }
                    
                }

                collegeFaculty += "</tbody>";
                collegeFaculty += "</table>";
                #endregion
            }
            

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
                int teachingFacultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Non-Teaching").Select(f => f.id).FirstOrDefault();
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
                //collegeFaculty += "<td colspan='2'><p align='center'>Scale of Pay</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Net Salary</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Bank Name</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Branch Name</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>Ratified</p></td>";
                //collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
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
                var facultyTypeIds= facultyList.Select(f => f.facultyTypeId).ToList();
                var jntuh_faculty_types = db.jntuh_faculty_type.Where(it => facultyTypeIds.Contains(it.id)).Select(it => new
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
                    //category = db.jntuh_faculty_category.Where(f => f.id == item.facultyCategoryId).Select(f => f.facultyCategory).FirstOrDefault();
                    //department = db.jntuh_department.Where(d => d.id == item.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
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


                    qualification = jntuh_faculty_educations
                                                             .Where(education => education.facultyId == item.id)
                                                             .Select(education => education.courseStudied).FirstOrDefault();

                    teachingType = jntuh_faculty_types.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    collegeFaculty += "<td colspan='5'><p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";
                    //collegeFaculty += "<td colspan='3'><p align='left'>" + department + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + qualification + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + dateOfAppointment + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPayScale + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.netSalary + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBankName + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBranchName + "</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='center'>" + ratified + "</p></td>";                
                    //if (!string.IsNullOrEmpty(item.facultyPhoto))
                    //{                    
                    //   // string strFacultyPhoto = "<img src='" + serverURL + "'" + item.facultyPhoto+ "'" + " align='center' height='50' />";
                    //    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                    //}
                    //else
                    //{
                    //    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                    //}
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
                collegeFaculty += "<td colspan='6'><p align='left'>Total no. Of Technical Faculty  </p></td>";
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
                    collegeFaculty += "<td colspan='3'><p align='left'>" + item.uplodedcount + "</p></td>";
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
                //collegeFaculty += "<td colspan='3'><p align='left'>Department</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Qualification </p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Experience</p></td>";
                collegeFaculty += "<td colspan='2'><p align='left'>Date of Appointment</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Scale of Pay</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Net Salary</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Bank Name</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>Branch Name</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>Ratified</p></td>";
                //collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
                collegeFaculty += "</tr>";
                var facultyDepartmentIds=  facultyList.Select(f => f.facultyDepartmentId).ToList();
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

                    qualification = db.jntuh_faculty_education.OrderByDescending(education => education.educationId)
                                                              .Where(education => education.facultyId == item.id)
                                                              .Select(education => education.courseStudied).FirstOrDefault();
                    teachingType = db.jntuh_faculty_type.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty += "<tr>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    collegeFaculty += "<td colspan='5'><p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                    collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";
                    //collegeFaculty += "<td colspan='3'><p align='left'>" + department + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + qualification + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='left'>" + dateOfAppointment + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPayScale + "</p></td>";
                    collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.netSalary + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBankName + "</p></td>";
                    //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBranchName + "</p></td>";
                    //collegeFaculty += "<td colspan='1'><p align='center'>" + ratified + "</p></td>";               
                    //if (!string.IsNullOrEmpty(item.facultyPhoto))
                    //{
                    //    //string strFacultyPhoto = "<img src='" + serverURL + "'" + item.facultyPhoto + "'" + " align='center' height='50' />";
                    //    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                    //}
                    //else
                    //{
                    //    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                    //}
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
            Paymentbilldetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            Paymentbilldetails += "<tbody>";
            Paymentbilldetails += "<tr>";
            Paymentbilldetails += "<td colspan='4'><p align='left'>Payment Date</p></td>";
            Paymentbilldetails += "<td colspan='3'><p align='left'>Reference Number</p></td>";
            Paymentbilldetails += "<td colspan='3'><p align='left'>Transaction Amount</p></td>";
            Paymentbilldetails += "</tr>";
            string collegecode = db.jntuh_college.Where(e => e.id == collegeId).Select(e => e.collegeCode).FirstOrDefault();
            List<jntuh_paymentresponse> payment = db.jntuh_paymentresponse.Where(a => a.CollegeId == collegecode && a.AuthStatus == "0300" && a.PaymentTypeID == 8).ToList();

            if (payment != null)
            {
                foreach (var item in payment)
                {
                    if (item.TxnDate != null)
                    {
                        strPaymentDate = item.TxnDate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.TxnDate.ToString()).ToString();
                    }
                    Paymentbilldetails += "<tr>";
                    Paymentbilldetails += "<td colspan='4'><p align='left'>" + strPaymentDate + "</p></td>";
                    Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnReferenceNo + "</td>";
                    Paymentbilldetails += "<td  colspan='3' align='left'>" + item.TxnAmount + "</td>";
                    Paymentbilldetails += "</tr>";
                     LatefeeQrCodetext += ";Payment Date:" + strPaymentDate + ";Customer Id:" + item.CustomerID;
                }
            }
            Paymentbilldetails += "</tbody></table>";
            contents = contents.Replace("##LateFeePaymentDetails1##", Paymentbilldetails);
            var Paymentdate = payment.Select(e => e.TxnDate).FirstOrDefault();
            string paymentdatecurrentformat = Paymentdate.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(Paymentdate.ToString()).ToString();
            contents = contents.Replace("##LATEFEEPAYMENTDATE##", paymentdatecurrentformat);

            #region application submission Date

            // var Updateondate = (from i in db.jntuh_college_edit_status join j in db.jntuh_college on i.collegeId equals j.id where (i.IsCollegeEditable == false && j.id == collegeId) select j.updatedOn).FirstOrDefault();
            var Updateondate = db.jntuh_college_edit_status.Where(i => i.IsCollegeEditable == false && i.collegeId == collegeId).Select(I => I.updatedOn).FirstOrDefault();
            var datetime = "";
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

              LatefeeQrCodetext += ";Online Application Date:" + datetime;
            contents = contents.Replace("##LATEFEESUBMITTEDDATE##", datetime);


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
                    strDataModifications += "<td><img src=" + serverURL + "" + strimagedetails + " align='left'  width='100' height='100' /></td>";
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


    }

    //our own ImageTagProcessor to support processing of base 64 images
    //public class CustomImageTagProcessor : iTextSharp.tool.xml.html.Image
    //{
    //    public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
    //    {
    //        IDictionary<string, string> attributes = tag.Attributes;
    //        string src;
    //        if (!attributes.TryGetValue(HTML.Attribute.SRC, out src))
    //            return new List<IElement>(1);

    //        if (string.IsNullOrEmpty(src))
    //            return new List<IElement>(1);

    //        if (src.StartsWith("data:image/", StringComparison.InvariantCultureIgnoreCase))
    //        {
    //            // data:[<MIME-type>][;charset=<encoding>][;base64],<data>
    //            var base64Data = src.Substring(src.IndexOf(",") + 1);
    //            var imagedata = Convert.FromBase64String(base64Data);
    //            var image = iTextSharp.text.Image.GetInstance(imagedata);

    //            var list = new List<IElement>();
    //            var htmlPipelineContext = GetHtmlPipelineContext(ctx);
    //            list.Add(GetCssAppliers().Apply(new Chunk((iTextSharp.text.Image)GetCssAppliers().Apply(image, tag, htmlPipelineContext), 0, 0, true), tag, htmlPipelineContext));
    //            return list;
    //        }
    //        else
    //        {
    //            return base.End(ctx, tag, currentContent);
    //        }
    //    }
    //}
}
