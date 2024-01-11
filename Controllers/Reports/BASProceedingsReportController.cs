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
    public class BASProceedingsReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        int PresentYearDb = 0;
        public BASProceedingsReportController()
        {
            PresentYearDb = 2022;
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult Index()
        {
            int[] collegeIds = { 4, 6, 7, 8, 9, 11, 12, 20, 22, 23, 24, 26, 27, 29, 30, 32, 34, 35, 38, 39, 40, 41, 42, 43, 47, 48, 50, 52, 54, 55, 56, 58, 60, 65, 67, 68, 69, 70, 72, 75, 77, 78, 80, 84, 85, 86, 87, 88, 90, 95, 97, 100, 101, 103, 104, 105, 106, 107, 108, 109, 110, 111, 113, 114, 115, 116, 117, 118, 120, 123, 128, 130, 132, 134, 135, 136, 137, 139, 140, 141, 143, 144, 145, 146, 147, 148, 150, 152, 153, 156, 157, 158, 159, 162, 164, 165, 166, 168, 169, 171, 173, 174, 175, 176, 177, 178, 179, 181, 182, 183, 184, 187, 188, 193, 195, 196, 198, 201, 202, 203, 204, 206, 210, 211, 213, 214, 218, 219, 222, 223, 225, 228, 234, 237, 242, 244, 245, 246, 250, 253, 254, 256, 260, 261, 262, 263, 264, 266, 267, 273, 276, 282, 283, 287, 290, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 310, 313, 314, 315, 316, 317, 318, 319, 320, 322, 324, 327, 329, 332, 334, 335, 338, 350, 352, 353, 360, 364, 365, 366, 367, 368, 370, 373, 374, 379, 380, 384, 385, 389, 391, 392, 393, 395, 399, 400, 403, 410, 411, 414, 415, 420, 423, 428, 429, 430, 435, 436, 441, 445, 449, 455 };
            //var collegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false && e.academicyearId == 14).Select(e => e.collegeId).ToList();
            foreach (var item in collegeIds)
            {
                string path = SaveCollegeBASLetterPdf(item);
            }
            //string path = SaveCollegeBASLetterPdf(26);
            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Content/PDFReports/BASProceedings/" + PresentYearDb + "/"));

            ZipFile zip = new ZipFile();
            zip.AlternateEncodingUsage = ZipOption.AsNecessary;
            zip.AddDirectoryByName("Faculty_BASReport");

            foreach (var row in filePaths)
            {
                string filePath = row;
                zip.AddFile(filePath, "AllCollegesFaculty_BASReport");
            }

            Response.Clear();
            Response.BufferOutput = false;
            string zipName = String.Format("FacultyBASProceedings_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"));
            Response.ContentType = "application/zip";
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName);
            zip.Save(Response.OutputStream);
            Response.End();
            return View();
        }

        public string SaveCollegeBASLetterPdf(int collegeId)
        {
            string fullPath = string.Empty;

            var collegeCode = db.jntuh_college.Where(C => C.id == collegeId && C.isActive == true).Select(C => C.collegeCode).FirstOrDefault();
            string ECollegeid = UAAAS.Models.Utilities.EncryptString(collegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
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

            string path = Server.MapPath("~/Content/PDFReports/BASProceedings/2022-23/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode.ToUpper() + "-" + ECollegeid + "-BASProceedings.pdf";
            var file = collegeCode.ToUpper() + "-" + ECollegeid + "-BASProceedings.pdf";
            var strfullPath = string.Format("{0}/{1}", "/Content/PDFReports/BASProceedings/2022-23/", file);

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
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/BASProceedings.html"));

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
            contents = contents.Replace("##COURSE_TABLE##", CollegeCoursesAll(collegeid));
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

        public string CollegeCoursesAll(int collegeid)
        {
            var details = string.Empty;
            var deptname = string.Empty;
            if (collegeid != null)
            {
                var clgfacultylst = new List<FacultyDetails>();
                var collegeFaculty = db.jntuh_college_faculty_registered.AsNoTracking().Where(c => c.collegeId == collegeid).Select(i => i).ToList();
                var collegeFacultyRegNos = collegeFaculty.Select(i => i.RegistrationNumber).ToList();
                var regFaculty = db.jntuh_registered_faculty.AsNoTracking().Where(i => collegeFacultyRegNos.Contains(i.RegistrationNumber) && i.BAS == "Yes").ToList();
                var depts = db.jntuh_department.AsNoTracking().Where(i => i.isActive == true);
                foreach (var item in regFaculty)
                {
                    var clgdept = collegeFaculty.Where(i => i.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).FirstOrDefault();
                    if (clgdept.DepartmentId != null)
                    {
                        deptname = depts.Where(i => i.id == clgdept.DepartmentId).FirstOrDefault().departmentName;
                    }
                    else
                    {
                        deptname = string.Empty;
                    }
                    var facultydetails = new FacultyDetails()
                    {
                        RegistrationNumber = item.RegistrationNumber,
                        FacultyName = item.FirstName + " " + (item.MiddleName != null ? item.MiddleName : "") + " " + item.LastName,
                        Department = deptname
                    };
                    clgfacultylst.Add(facultydetails);
                }

                details += "<br/>";
                details += "<table border='1' cellpadding='3' cellspacing='0' width='100%' height='50%' style='font-family: Times New Roman; font-size: 9px; font-weight: normal; background-color: #fff; margin: 0 auto;'>";
                details += "<tbody>";
                details += "<tr style='font-weight:bold;'>";
                details += "<td width='4%' ><p align='center'>S.No</p></td>";
                details += "<td width='12%'><p align='center'>Registration Number</p></td>";
                details += "<td width='18%'><p align='center'>Name of the Faculty</p></td>";
                details += "<td width='15%'><p align='center'>Department</p></td>";
                details += "</tr>";
                int rowCount = 1;
                foreach (var item in clgfacultylst.OrderBy(w => w.Department).ToList())
                {
                    details += "<tr>";
                    details += "<td width='4%'  style='font-size: 10px;'><p align='center'>" + (rowCount++) + "</p></td>";
                    details += "<td width='12%' style='font-size: 10px;'><p align='left'>" + item.RegistrationNumber + "</p></td>";
                    details += "<td width='18%' style='font-size: 10px;'><p align='left'>" + item.FacultyName.ToUpper() + "</p></td>";
                    details += "<td width='15%' style='font-size: 10px;'><p align='left'>" + item.Department + "</p></td>";
                    details += "</tr>";
                }
                details += "</tbody>";
                details += "</table>";
                return details;
            }
            return details;
        }

        public class FacultyDetails
        {
            public string RegistrationNumber { get; set; }
            public string FacultyName { get; set; }
            public string Department { get; set; }
        }
    }
}
