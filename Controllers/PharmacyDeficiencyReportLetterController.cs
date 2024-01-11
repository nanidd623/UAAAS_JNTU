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
    public class PharmacyDeficiencyReportLetterController : Controller
    {
        private uaaasDBContext db = new uaaasDBContext();
        private int TotalAreaRequiredFaculty = 0;
        int PresentYearDb = 0;

        public PharmacyDeficiencyReportLetterController()
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
            //if (!string.IsNullOrEmpty(id))
            //{
            //    int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            //    //int collegeID = Convert.ToInt32(id);
            //    string path = SaveCollegePharmacyLetterPdf(collegeID);
            //    path = path.Replace("/", "\\");
            //    if (!string.IsNullOrEmpty(path))
            //    {
            //        return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
            //    }
            //}

            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //int[] collegeIds = { 6, 9, 24, 27, 30, 34, 39, 42, 47, 52, 54, 55, 58, 60, 65, 75, 78, 90, 95, 104, 105, 107, 110, 114, 117, 118, 120, 135, 136, 139, 140, 146, 150, 159, 169, 202, 204, 206, 213, 219, 234, 237, 253, 262, 263, 267, 283, 290, 295, 297, 298, 301, 302, 303, 313, 314, 315, 317, 318, 319, 320, 332, 353, 364, 370, 379, 384, 389, 392, 395, 410, 428, 436, 442, 445 };
                int[] collegeIds = { 140 };
                //int collegeID = Convert.ToInt32(id);
                //string path = SaveCollegePharmacyLetterPdf(collegeID);
                //path = path.Replace("/", "\\");
                //if (!string.IsNullOrEmpty(path))
                //{
                //    return File(path, "application/pdf", path.Substring(path.LastIndexOf('\\') + 1));
                //}
                foreach (var item in collegeIds)
                {
                    string path = SaveCollegePharmacyLetterPdf(item);
                }

                string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/PharmacyDefReport/" + PresentYearDb + "/"));

                ZipFile zip = new ZipFile();
                zip.AlternateEncodingUsage = ZipOption.AsNecessary;
                zip.AddDirectoryByName("Faculty_PharmacyDefReport");

                foreach (var row in filePaths)
                {
                    string filePath = row;
                    zip.AddFile(filePath, "AllCollegesFaculty_PharmacyDefReport");
                }

                Response.Clear();
                Response.BufferOutput = false;
                string zipName = String.Format("FacultyPharmacyDefReportSProceedings_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
                Response.ContentType = "application/zip";
                Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
                zip.Save(Response.OutputStream);
                Response.End();
            }

            return View();
        }

        public string SaveCollegePharmacyLetterPdf(int collegeId)
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

            string path = Server.MapPath("~/Content/PDFReports/PharmacyDefLetters/2023-24/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode.ToUpper() + "-" + ECollegeid + "-MPharmacyDefLetter.pdf";
            var file = collegeCode.ToUpper() + "-" + ECollegeid + "-MPharmacyDefLetter.pdf";
            var strfullPath = string.Format("{0}/{1}", "/Content/PDFReports/PharmacyDefLetters/2023-24/", file);

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
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/PharmacyDeficiencyLetter.html"));

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
            contents = contents.Replace("##BPharmCOURSE_TABLE##", BPharmDeficienciesInFaculty(collegeid));
            contents = contents.Replace("##MPharmCOURSE_TABLE##", MPharmDeficienciesInFaculty(collegeid));
            contents = contents.Replace("##LABS_TABLE##", DeficiencyCollegeLabsAnnexure(collegeid));
            contents = contents.Replace("##MOUCOURSE_TABLE##", CollegeMou(collegeid));
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
                    facultydata.LastName = regdata.LastName;
                    facultydata.RegistrationNumber = regdata.RegistrationNumber;
                    if (regdata.Absent == true)
                    {
                        OriginalReason += "Absent" + ", ";
                    }
                    if (regdata.BAS == "Yes")
                    {
                        OriginalReason += "Not having Sufficient Biometric Attendance ";
                    }
                    if (!string.IsNullOrEmpty(regdata.DeactivationReason) || !string.IsNullOrEmpty(OriginalReason))
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        //Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Yes";
                        OriginalReason += !string.IsNullOrEmpty(OriginalReason) ? "," + regdata.DeactivationReason : regdata.DeactivationReason;
                    }
                    else
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
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

        public string BPharmDeficienciesInFaculty(int? collegeID)
        {
            TotalAreaRequiredFaculty = 0;
            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            int? AdditionalFaculty = 0;
            int SecondYerintake = 0;
            int ThirdYerintake = 0;
            int FourthYerintake = 0;
            int PharmDFirstYerintake = 0;
            int PharmDSecondYerintake = 0;
            int PharmDThirdYerintake = 0;
            int PharmDFourthYerintake = 0;
            int PharmDFifthhYerintake = 0;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            string facultyAdmittedIntakeZero = string.Empty;

            faculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            // faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:12px;border-collapse:collapse;border: 1px;' rules='all'>";
            faculty += "<tr>";
            //faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Group</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Proposed Intake</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Required faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Available faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >No. of Ph.D faculty</th>";
            faculty += "</tr>";

            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == AY0 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //List<CollegeIntakeReport> collegeIntakeExistingList = new List<CollegeIntakeReport>();

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
            string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_facultyBAS = registeredFacultyNew.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                NotconsideredPHD = rf.NotconsideredPHD,
                HighestDegreeID = rf.NotconsideredPHD == true ?
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                            :
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                IsApproved = rf.isApproved,
                TotalExperience = rf.TotalExperience,
                CsePhDFacultyFlag = rf.PhdDeskVerification,
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid && (p.Deficiency == null || !BASRegNos.Contains(p.Deficiency))).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            AdditionalFaculty = FacultyData.Where(a => a.CollegeCode == cid && a.Deficiency != null).Select(z => z.Deficiency).Count();

            int Count = 1;
            var totalReqFaculty = 0;
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
                                Pharmacy.PharmacySpecialization = "Group1 (Pharmaceutics , Industrial Pharmacy , Pharmaceutical Technology , 	Pharmaceutical Biotechnology , RA)";
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
                                Pharmacy.PharmacySpecialization = "Group4 (Pharmacognosy, Pharmaceutical Chemistry , Phytopharmacy & Phytomedicine , NIPER  Natural Products , Pharmaceutical Biotechnology)";
                                Pharmacy.GroupId = "4";
                                break;
                        }

                        Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                        Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                        Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).FirstOrDefault();
                        Pharmacy.NoOfAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId).Select(f => f.SpecializationWiseRequiredFaculty).LastOrDefault();
                        Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();
                        totalReqFaculty = totalReqFaculty + (int)Pharmacy.SpecializationwiseRequiredFaculty;
                        if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                        {
                            Pharmacy.Deficiency = "No Deficiency";
                        }
                        else
                        {
                            Pharmacy.Deficiency = "Deficiency In faculty";
                        }

                        var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.PharmacySpecialization == Pharmacy.GroupId && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                        var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                        //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes")).Select(rf => rf).ToList();
                        //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                        //                                 (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                        var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();
                        var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                        int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                        Pharmacy.PHdFaculty = PhdRegNOCount;

                        //if (i == 1)
                        //{
                        //    SecondYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY1, Pharmacy.SpecializationId, item.shiftId, 1);
                        //    ThirdYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY2, Pharmacy.SpecializationId, item.shiftId, 1);
                        //    FourthYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY3, Pharmacy.SpecializationId, item.shiftId, 1);

                        //    faculty += "<tr>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='5'>" + (Count++) + "</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>Pharmacy</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  rowspan='4'>" + item.Department + "</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4'>" + item.Specialization + "</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;' rowspan='4' >";
                        //    faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:12px'>";
                        //    faculty += "<tr>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>II</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>III</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>IV</td>";
                        //    faculty += "</tr>";
                        //    faculty += "<tr>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SecondYerintake + "</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + ThirdYerintake + "</td>";
                        //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + FourthYerintake + "</td>";
                        //    faculty += "</tr>";
                        //    faculty += "</table>";

                        //    var PharmD = collegeIntakeExisting.Where(a => a.DepartmentID == 27).Select(a => a.Department).FirstOrDefault();
                        //    if (!String.IsNullOrEmpty(PharmD))
                        //    {
                        //        PharmDFirstYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY1, 18, item.shiftId, 1);
                        //        PharmDSecondYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY2, 18, item.shiftId, 1);
                        //        PharmDThirdYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY3, 18, item.shiftId, 1);
                        //        PharmDFourthYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY4, 18, item.shiftId, 1);
                        //        PharmDFifthhYerintake = GetPharmacyIntake(Pharmacy.Collegeid, AY5, 18, item.shiftId, 1);

                        //        faculty += "<br/><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:12px'>";
                        //        faculty += "<tr>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'  colspan='5'><b>Pharm.D</b></td>";
                        //        faculty += "</tr>";
                        //        faculty += "<tr>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>I</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>II</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>III</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>IV</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>V</td>";
                        //        faculty += "</tr>";
                        //        faculty += "<tr>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFirstYerintake + "</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDSecondYerintake + "</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDThirdYerintake + "</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFourthYerintake + "</td>";
                        //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + PharmDFifthhYerintake + "</td>";
                        //        faculty += "</tr>";
                        //        faculty += "</table>";
                        //    }
                        //    //var PharmD = collegeIntakeExisting.Where(a => a.DepartmentID == ).Select(a => a.Department).FirstOrDefault();
                        //    faculty += "</td>";

                        //}
                        //faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:12px'>";
                        if ((Pharmacy.SpecializationwiseRequiredFaculty - Pharmacy.SpecializationwiseAvilableFaculty) > 0)
                        {
                            if (i > 1)
                            {
                                faculty += "<tr>";
                            }

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";

                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PharmacySpecialization + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseRequiredFaculty + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseAvilableFaculty + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                            faculty += "</tr>";
                        }
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
                    BPharmacyObj.Deficiency = PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency In faculty") > 0 ? "Deficiency In faculty" : "No Deficiency";
                    BPharmacyObj.PHdFaculty = PharmacyAppealFaculty.Where(a => a.DepartmentId == 26).Select(q => q.PHdFaculty).Sum();
                    BPharmacyObj.IsActive = true;
                    PharmacyAppealFaculty.Add(BPharmacyObj);
                    TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + totalReqFaculty;
                    faculty += "<tr>";
                    //  faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.Degree + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.Department + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.Specialization + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.TotalIntake + "</td>";
                    if (BPharmacyObj.ProposedIntake > 100)
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>100</td>";
                    else
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.ProposedIntake + "</td>";

                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.NoOfFacultyRequired + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.NoOfAvilableFaculty + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalReqFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.NoOfAvilableFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.Deficiency + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + BPharmacyObj.PHdFaculty + "</td>";
                    faculty += "</tr>";

                    //if (BPharmacyObj.Deficiency == "Deficiency")
                    //    faculty += "<tr><td colspan='13' style='text-align:center;'><b>Note :B.Pharmacy Deficiency Exists & Hence other Degrees will not be considered.</b></td></tr>";
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
                    Pharmacy.TotalIntake = (PharmDFirstYerintake + PharmDSecondYerintake + PharmDThirdYerintake + PharmDFourthYerintake + PharmDFifthhYerintake);
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency In faculty") > 0)
                        Pharmacy.Deficiency = "Deficiency In faculty";
                    else
                        Pharmacy.Deficiency = "No Deficiency";

                    PharmacyAppealFaculty.Add(Pharmacy);

                    faculty += "<tr>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + (Count++) + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Degree + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Department + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Specialization + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    ////faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.TotalIntake + "</td>";
                    if (item.Degree == "Pharm.D")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>30</td>";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>10</td>";
                    }
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                    faculty += "</tr>";
                }

            }

            //string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            //var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            //var commanfaculty = from cf in db.jntuh_college_faculty_registered
            //                    from pf in db.jntuh_college_previous_academic_faculty
            //                    where (cf.RegistrationNumber == pf.RegistrationNumber && cf.collegeId == pf.collegeId && cf.collegeId == collegeID)
            //                    select new
            //                    {
            //                        cf.RegistrationNumber,
            //                        cf.collegeId,
            //                    };
            //int commanfacultycount = commanfaculty.Count();
            //int newfaculty = registeredFacultyNew.Count() - commanfacultycount;
            //var regFacultyWothoutAbsents = registeredFacultyNew.Where(rf => (rf.Absent == false || rf.Absent == null)).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
            //                                          && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf.RegistrationNumber).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
            //                                             (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf.RegistrationNumber).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
            //                                                (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf.RegistrationNumber).ToList();

            //faculty += "<tr><td align='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Additional Faculty  :" + (jntuh_registered_faculty2.Count() - AdditionalFaculty) + "</b></td></tr>";
            //faculty += "<tr><td align='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Total Faculty : " + jntuh_registered_faculty2.Count() + " </b></td></tr>";
            faculty += "</table>";
            faculty += "<p style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>The faculty members having Biometric attendance less than 40% from the date of enabling Biometric attendance are not considered.</p>";
            //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            //faculty += "<tr>";
            //faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy</td>";
            //faculty += "<td align='left'>* I, II Year for M.Pharmacy</td>";
            //faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D</td>";
            //faculty += "<td align='left'>* IV, V Year for Pharm.D PB</td>";
            //faculty += "</tr>";
            //faculty += "</table>";


            //return faculty;
            if (!faculty.Contains("Deficiency In"))
            {
                faculty = string.Empty;
                faculty = "<table  border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tr><th style='text-align: left; vertical-align: top;' ><b>Group</b></th><th style='text-align: center; vertical-align: top;' ><b>Required faculty</b></th><th style='text-align: center; vertical-align: top;'><b>Available faculty</b></th><th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th></tr><tr><td colspan = '4' class='col2' style='text-align: center; vertical-align: top;'><b>NIL</b></td></tr></table>";
            }
            return faculty;
        }

        public string MPharmDeficienciesInFaculty(int? collegeID)
        {
            string faculty = string.Empty;
            int? AddingFacultyCount = 0;
            int? TotalcollegeFaculty = 0;
            int? AdditionalFaculty = 0;
            int SecondYerintake = 0;
            int ThirdYerintake = 0;
            int FourthYerintake = 0;
            int PharmDFirstYerintake = 0;
            int PharmDSecondYerintake = 0;
            int PharmDThirdYerintake = 0;
            int PharmDFourthYerintake = 0;
            int PharmDFifthhYerintake = 0;

            List<PharmacyReportsClass> PharmacyAppealFaculty = new List<PharmacyReportsClass>();
            string facultyAdmittedIntakeZero = string.Empty;

            faculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
            // faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;font-size:12px;border-collapse:collapse;border: 1px;' rules='all'>";
            faculty += "<tr>";
            //faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            //faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Specialization</b></th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Required *</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >No.of Faculty Available</th>";
            //faculty += "<th style='text-align: center; vertical-align: top;' >Pharmacy Specializations *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Required faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Available faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Required Ph.D. faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Available Ph.D. faculty</b></th>";
            faculty += "<th style='text-align: center; vertical-align: top;' ><b>Deficiency</b></th>";
            faculty += "</tr>";
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == presentYear).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeID && i.academicYearId == AY0 && i.proposedIntake != 0 && i.courseStatus != "Closure").ToList();
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //List<CollegeIntakeReport> collegeIntakeExistingList = new List<CollegeIntakeReport>();

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
            string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_facultyBAS = registeredFacultyNew.Where(rf => (rf.BAS == "Yes")).Select(rf => new
            {
                FacultyId = rf.id,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                NotconsideredPHD = rf.NotconsideredPHD,
                HighestDegreeID = rf.NotconsideredPHD == true ?
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 6 && e.educationId != 9).Select(e => e.educationId).Max() : 0
                            :
                            rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ?
                            rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
                IsApproved = rf.isApproved,
                TotalExperience = rf.TotalExperience,
                CsePhDFacultyFlag = rf.PhdDeskVerification,
                BASFlag = rf.BAS
            }).ToList();
            var BASRegNos = jntuh_registered_facultyBAS.Select(rf => rf.RegistrationNumber).ToArray();
            var FacultyData = db.jntuh_appeal_pharmacydata.Where(p => p.CollegeCode == cid && (p.Deficiency == null || !BASRegNos.Contains(p.Deficiency))).ToList();
            string[] AssignedFaculty = FacultyData.Select(p => p.Deficiency).ToArray();
            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => AssignedFaculty.Contains(rf.RegistrationNumber.Trim())).ToList();
            AdditionalFaculty = FacultyData.Where(a => a.CollegeCode == cid && a.Deficiency != null).Select(z => z.Deficiency).Count();

            int Count = 1;
            foreach (var item in collegeIntakeExisting.OrderBy(s => s.specializationId).ThenBy(a => a.shiftId).ToList())
            {
                if (item.Degree == "M.Pharmacy")
                {

                    PharmacyReportsClass Pharmacy = new PharmacyReportsClass();
                    Pharmacy.Collegeid = item.collegeId;
                    Pharmacy.Degree = item.Degree;
                    Pharmacy.DepartmentId = item.DepartmentID;
                    Pharmacy.Department = item.Department;
                    Pharmacy.SpecializationId = item.specializationId;
                    Pharmacy.Specialization = item.Specialization;
                    Pharmacy.ShiftId = item.shiftId;
                    //Pharmacy.ProposedIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.ProposedIntake).FirstOrDefault();
                    //Pharmacy.TotalIntake = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.TotalIntake).FirstOrDefault();
                    Pharmacy.ProposedIntake = GetPharmacyIntake(item.collegeId, AY0, item.specializationId, item.shiftId, 0);
                    var approveIntake1 = GetPharmacyIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1);
                    var approveIntake2 = GetPharmacyIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1);
                    Pharmacy.TotalIntake = (approveIntake1 != null ? approveIntake1 : 0) + (approveIntake2 != null ? approveIntake2 : 0);
                    Pharmacy.NoOfFacultyRequired = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.NoOfFacultyRequired).LastOrDefault();
                    Pharmacy.NoOfAvilableFaculty = FacultyData.Count(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null);
                    Pharmacy.SpecializationwiseRequiredFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString()).Select(f => f.SpecializationWiseRequiredFaculty).LastOrDefault();
                    Pharmacy.SpecializationwiseAvilableFaculty = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(a => a.Deficiency).Distinct().Count();

                    var AvailiableFacultyRegNos = FacultyData.Where(f => f.CollegeCode == cid && f.Specialization == item.specializationId.ToString() && f.Deficiency != null).Select(q => q.Deficiency).Distinct().ToList();

                    var PHDFacultyList = db.jntuh_registered_faculty.Where(q => AvailiableFacultyRegNos.Contains(q.RegistrationNumber)).Select(a => a).ToList();

                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
                    //                                 && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf).ToList();
                    //var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
                    //                                     (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf).ToList();
                    var PHDFacultyCleared = PHDFacultyList.Where(rf => rf.type != "Adjunct" && (rf.BAS != "Yes") && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
                                                            (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf).ToList();

                    var PHDFaculty = PHDFacultyCleared.Select(q => q.id).ToList();
                    int PhdRegNOCount = db.jntuh_registered_faculty_education.Count(q => PHDFaculty.Contains(q.facultyId) && q.educationId == 6);

                    Pharmacy.PHdFaculty = PhdRegNOCount;

                    if (Pharmacy.SpecializationwiseAvilableFaculty >= Pharmacy.SpecializationwiseRequiredFaculty)
                    {
                        if (PharmacyAppealFaculty.Count(a => a.DepartmentId == 26 && a.Deficiency == "Deficiency") > 0)
                            Pharmacy.Deficiency = "Deficiency In faculty ";
                        else
                            Pharmacy.Deficiency = "No Deficiency";
                    }
                    else
                    {
                        Pharmacy.Deficiency = "Deficiency In faculty";
                    }

                    if (Pharmacy.ShiftId == 1)
                    {
                        faculty += "<tr>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + (Count++) + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Degree + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Department + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Specialization + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.TotalIntake + "</td>";
                        if (Pharmacy.ProposedIntake > 15)
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>15</td>";
                        else
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.ProposedIntake + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.ProposedIntake + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseRequiredFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.SpecializationwiseAvilableFaculty + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.NoOfFacultyRequired + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.NoOfAvilableFaculty + "</td>";
                        //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>1</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.PHdFaculty + "</td>";
                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Pharmacy.Deficiency + "</td>";
                        faculty += "</tr>";
                    }
                    TotalAreaRequiredFaculty = TotalAreaRequiredFaculty + 2;
                    PharmacyAppealFaculty.Add(Pharmacy);
                }
            }

            //string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();
            //var registeredFacultyNew = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            //var commanfaculty = from cf in db.jntuh_college_faculty_registered
            //                    from pf in db.jntuh_college_previous_academic_faculty
            //                    where (cf.RegistrationNumber == pf.RegistrationNumber && cf.collegeId == pf.collegeId && cf.collegeId == collegeID)
            //                    select new
            //                    {
            //                        cf.RegistrationNumber,
            //                        cf.collegeId,
            //                    };
            //int commanfacultycount = commanfaculty.Count();
            //int newfaculty = registeredFacultyNew.Count() - commanfacultycount;
            //var regFacultyWothoutAbsents = registeredFacultyNew.Where(rf => (rf.Absent == false || rf.Absent == null)).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NoForm16 == false || rf.NoForm16 == null) && (rf.NoForm26AS == false || rf.NoForm26AS == null) && (rf.Absent == false || rf.Absent == null) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)
            //                                          && (rf.Noclass == false || rf.Noclass == null) && (rf.FakePHD == false || rf.FakePHD == null) && (rf.Genuinenessnotsubmitted == false || rf.Genuinenessnotsubmitted == null) && (rf.NotconsideredPHD == false || rf.NotconsideredPHD == null) && (rf.NoPGspecialization == false || rf.NoPGspecialization == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.InvalidAadhaar != "Yes") && rf.BAS != "Yes").Select(rf => rf.RegistrationNumber).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown == null) &&
            //                                             (rf.NoSCM == false || rf.NoSCM == null) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null))).Select(rf => rf.RegistrationNumber).ToList();
            //var jntuh_registered_faculty2 =
            //      registeredFacultyNew.Where(rf => rf.type != "Adjunct" && (rf.Noclass == false || rf.Noclass == null) && (rf.Absent == false || rf.Absent == null) &&
            //                                                (rf.NoSCM == false || rf.NoSCM == null) && (rf.Blacklistfaculy == false) && (rf.AbsentforVerification == false || rf.AbsentforVerification == null)).Select(rf => rf.RegistrationNumber).ToList();

            //faculty += "<tr><td alin='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Additional Faculty  :" + (jntuh_registered_faculty2.Count() - AdditionalFaculty) + "</b></td></tr>";
            //faculty += "<tr><td align='center' colspan='13' style='font-size: 14px; font-weight: normal;'><b> Total Faculty : " + jntuh_registered_faculty2.Count() + " </b></td></tr>";
            faculty += "</table>";

            //faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            //faculty += "<tr>";
            //faculty += "<td align='left'>* II, III & IV Year for B.Pharmacy</td>";
            //faculty += "<td align='left'>* I, II Year for M.Pharmacy</td>";
            //faculty += "<td align='left'>* I, II, III , IV & V Year for Pharm.D</td>";
            //faculty += "<td align='left'>* IV, V Year for Pharm.D PB</td>";
            //faculty += "</tr>";
            //faculty += "</table>";
            if (PharmacyAppealFaculty.Count == 0)
            {
                faculty = string.Empty;
                faculty += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                faculty += "<tr><td align='center'> <b>NIL</b></td></tr>";
                faculty += "</table>";
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
            duesstr += "<th style='text-align: left; vertical-align: top;'>Affiliation Fee 2022-23</th>";
            duesstr += "<th style='text-align: left; vertical-align: top;' ><b>Affiliation Fee 2021-22</b></th>";
            duesstr += "<th style='text-align: left; vertical-align: top;' ><b>Common Service Fee 2021-22</b></th>";
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
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            string annexure = string.Empty;
            var Departments = db.jntuh_department.Where(d => d.degreeId == 2 || d.degreeId == 5 || d.degreeId == 9 || d.degreeId == 10).ToList();
            int[] Departmentids = Departments.Select(d => d.id).ToArray();
            var Specializations = db.jntuh_specialization.Where(s => Departmentids.Contains(s.departmentId)).ToList();
            List<int> specializationIds1 = Specializations.Select(s => s.id).ToList();

            List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
            List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.academicYearId == AY1 && e.courseStatus != "Closure" && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();

            int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeID).Select(C => C.EquipmentID).ToArray();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();
            int[] degreeids = { 2 };
            //if (DegreeIds.Contains(4))
            //{
            //    specializationIds.Add(39);
            //}
            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                           .Where(l => l.CollegeId == collegeID && degreeids.Contains(l.DegreeID) && specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) && !l.EquipmentName.Contains("desirable"))
                                                           .Select(l => new FacultyVerificationController.AnonymousLabclass
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
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                              .Where(l => degreeids.Contains(l.DegreeID) && specializationIds.Contains(l.SpecializationID)  && !Equipmentsids.Contains(l.id) && l.CollegeId == null && !l.EquipmentName.Contains("desirable"))
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

            var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


            int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs =
                db.jntuh_college_laboratories.Where(
                    l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID))
                    .Select(i => i.EquipmentID)
                    .ToArray();

            list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID))
                    .ToList();
            int[] noDefcollegeIds = { 75 };
            int[] SpecializationIDs;
            if (DegreeIds.Contains(4))
                SpecializationIDs = (from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
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

                //annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                //annexure += "<tr>";
                //annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                //annexure += "</tr>";
                //annexure += "</table>";
                foreach (var speclializationId in SpecializationIDs)
                {
                    string LabNmae = "", EquipmentName = "", DepartmentName = "";
                    var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                    DepartmentName = list1.Where(l => l.specializationId == speclializationId).Select(l => l.Department).FirstOrDefault();
                    annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                    annexure += "<tr>";
                    annexure += "<th align='left' colspan='2'> <b>" + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</b></th>";
                    annexure += "</tr>";
                    annexure += "<tr>";
                    //annexure += "<th style='text-align: left; width: 1%;'><b>S.No</b></th>";
                    annexure += "<th style='text-align: left; width: 10%;'><b>Lab Name</b></th>";
                    annexure += "<th style='text-align: left; width: 10%;'><b>Equipment Name</b></th>";
                    //annexure += "<th style='text-align: left; width: 5%;'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
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
                        var EqpName = string.Join(",", labslst);
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
                    //int indexnow = 1;
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
                //annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                //annexure += "<tr>";
                //annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                //annexure += "</tr>";
                //annexure += "</table>";
                annexure += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                annexure += "</table>";
            }

            //annexure += "</br><p> <b>NOTE:</b> The Physical Verification of the faculty and their presence at the time of Inspection by the FFC, automatically does not mean that the college is entitled for Affiliation based on numbers. Those of the faculty who are having the requisite qualifications and credentials are verified and found correct will be taken into account for the purpose of granting affiliation.</p>";
            return annexure;
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

        public string CollegeMou(int? collegeID)
        {
            var clgMouName = string.Empty;
            var dictionary = new Dictionary<string, string> 
            {
                { "Z1" ,"Mamatha General Hospital"},{ "GN" ,"Aware Global Hospital"},{ "T6" ,"Oile Hospital"},{ "17" ,"Krishna Institute of Medical Sciences"},{ "CE" ,"Dugabai Deshmukh Hospital"},{ "EF" ,"Bhaskar General hospital"},{ "GD" ,"Olive Hospital"},{ "T2" ,"Gandhi Hospital"},{ "7J" ,"CVVM Hospital"},{ "8B" ,"Jyothi Hospitals "},{ "Z5" ,"RVM Institute of Medical Sciences"},{ "GQ" ,"Sunshine Hospital Gachiboli& Secunderabad"},{ "WJ" ,"CARE Hospital"},{ "S7" ,"Sunshine Hospital Secunderabad"},{ "DQ" ,"Mahavir General Hospital"},{ "Z2" ,"Prathima Institute of Medical Sciences"},{ "Z9" ,"Asian Institute of Gastroenterolgy Hospital"},{ "T3" ,"Malla Reddy Narayana Multispeciality Hospital"},{ "HF" ,"Mediciti Institute of Medical Sciences Hospital"},{ "BU" ,"ESI Hospital , Erragadda, Hyd"},{ "CA" ,"Tulasi Hospital "},{ "1F" ,"Thumbay Hospital"},{ "S9" ,"Medicover Hospital"},{ "CJ" ,"Patnam Mahernder Reddy  Hospital "},{ "CK" ,"Vijaya Krishna  Multi Speciality Hospital"},{ "CM" ,"Vijay Marie Hospital "},{ "Y7" ,"Kamineni Hospital "},{ "FT" ,"Sri Tirumala Hospital"},{ "11" ,"Shadan Institute of Medical Sciences"},{ "41" ,"Dr. V.R.K Womens Medical College"},{ "XF" ,"Viranchi Hospital"},{ "1K" ,"Govt District Hospital , Karimnagar"},{ "U2" ,"Global Hospital"},{ "T0" ,"B.B.R Hospital"},{ "WM" ,"Aditya Hospital"},{ "45" ,"Star Hopital"},{ "ED" ,"Maxcare  Hospital "},{ "WN" ,"Chalameda Anand Rao Institute of Medical Sciences"},{ "VM" ,"Kamineni Institute of  Medical Sciences Hospital "},{ "HA" ,"T.R.R Institute of Medical Sciences"},
            };
            string CollegeMouName = string.Empty;
            var clgDetails = db.jntuh_college.AsNoTracking().Where(i => i.id == collegeID).Select(i => i).ToList();
            if (clgDetails.Count > 0)
            {
                CollegeMouName = dictionary.Where(i => i.Key == clgDetails.FirstOrDefault().collegeCode).FirstOrDefault().Value;
            }
            if (string.IsNullOrEmpty(CollegeMouName))
            {
                clgMouName += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                clgMouName += "<tr>";
                clgMouName += "<td colspan='6' align='center'><b>NIL</b></td>";
                clgMouName += "</tr>";
                clgMouName += "</table>";
            }
            else
            {
                clgMouName += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                clgMouName += "<tr>";
                clgMouName += "<td  align='center'><b>" + CollegeMouName + "</b></td>";
                clgMouName += "</tr>";
                clgMouName += "</table>";
            }
            return clgMouName;
        }

        private int GetPharmacyIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            if (flag == 0) //Proposed
            {
                var inta = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).FirstOrDefault();
                if (inta != null && inta.proposedIntake != null)
                {
                    intake = (int)inta.proposedIntake;
                }
            }
            else if (flag == 1) //Approved
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.approvedIntake).FirstOrDefault();
            }
            else if (flag == 2)//Admitted
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == 1).Select(i => i.admittedIntake).FirstOrDefault();
            }
            return intake;
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
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Required Area(Sq.m)</b></p></td>";
                strAdministrativeLandDetails += "<td width='9%'><p align='center'><b>Uploaded Area(Sq.m)</b></p></td>";
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
                        if (item.id == 6) // FacultyRooms
                        {
                            requiredRooms = TotalAreaRequiredFaculty.ToString();
                            requiredArea = (TotalAreaRequiredFaculty * 10).ToString();
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
                            requiredArea = (bpharmrrCount * 75).ToString();
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
                            if (item.requirementType == "Examination Control Office")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Confidential Room</p></td>";
                            }
                            else if (item.requirementType == "Central Stores")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Store Room 1&2</p></td>";
                            }
                            else if (item.requirementType == "Office All Inclusive")
                            {
                                strAdministrativeLandDetails += "<td width='35%'><p>Office-1 Establishment</p></td>";
                            }
                            else if (item.requirementType == "Placement Office")
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
                    }
                    if (indexCount == 0)
                    {
                        strAdministrativeLandDetails += "<tr><td colspan='9' align='center'><b>NIL</b></td></tr>";
                    }
                }
                strAdministrativeLandDetails += "</tbody></table>";
                contents = strAdministrativeLandDetails;
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
                strInstructionalAreaDetails += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'><tbody>";
                strInstructionalAreaDetails += "<tr><td width='35%'><p><b>Requirement Type</b></p></td><td width='9%'><p align='center'><b>Required Rooms</b></p></td><td width='9%'><p align='center'><b>Uploaded Rooms</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td><td width='9%'><p align='center'><b>Required Area(Sq.m)</b></p></td><td width='9%'><p align='center'><b>Uploaded Area (Sq.m)</b></p></td><td width='5%'><p align='center'><b>CF</b></p></td><td width='9%'><p align='center'><b>Deficiency</b></p></td></tr>";
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
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
                                               where ie.isActive == true && ie.academicYearId == (prAy) && (ie.proposedIntake != 0) && ie.collegeId == collegeId && de.id != 10
                                               orderby de.degreeDisplayOrder
                                               select de.degree
                ).Distinct().ToList();

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
                    if (programTypeDegree.programType == "Pharm.D")
                    {
                        strInstructionalAreaDetails += "<td colspan='9' style='width: 200%'><p><b>Pharm.D & Pharm.D PB</b></p></td>";
                    }
                    else
                    {
                        strInstructionalAreaDetails += "<td colspan='9' style='width: 200%'><p><b>" + programTypeDegree.programType + "</b></p></td>";
                    }
                    strInstructionalAreaDetails += "</tr>";
                    foreach (var i in land)
                    {
                        var requiredRooms = string.Empty;
                        var requiredArea = string.Empty;

                        if (i.programId == 6)
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
                                var rarea = (rrooms * 85);
                                requiredRooms = rrooms.ToString();
                                requiredArea = rarea.ToString();
                            }
                            else
                            {
                                requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                                requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                            }
                        }
                        else
                        {
                            requiredRooms = Convert.ToInt16(i.requiredRooms).ToString();
                            requiredArea = Convert.ToDecimal(i.requiredArea).ToString();
                        }
                        if (requiredRooms == "" || requiredArea == "")
                        {
                            requiredRooms = "";
                            requiredArea = "";
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
                    }
                    if (indexcount == 0)
                    {
                        strInstructionalAreaDetails += "<tr><td colspan='9' align='center'><b>NIL</b></td></tr>";
                    }
                }
                strInstructionalAreaDetails += "</tbody>";
                strInstructionalAreaDetails += "</table><br/>";
                contents = strInstructionalAreaDetails;
                return contents;
            }
        }
    }
}
