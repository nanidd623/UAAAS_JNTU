using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing;
using DotNetOpenAuth.Messaging;
using UAAAS.Models;
using System.Configuration;

namespace UAAAS.Controllers.Reports
{
    public class DeficiencyReportWordController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Deficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                //int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
                
                //  var randomid = UAAAS.Models.Utilities.EncryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]);
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                //string randomcollegeCode = db.jntuh_college_randamcodes.Where(c => c.IsActive == true && c.CollegeId == collegeID).Select(c => c.RandamCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-Deficiency-Report-" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header(collegeID));
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");
                ////str.Append(CommitteeMembers(collegeID));
                ////str.Append("<br />");
                str.Append(DeficienciesInFaculty(collegeID));
                str.Append("<br />");
                ////old code
                ////str.Append(DeficienciesInLabs(collegeID));
                ////New code
                //Commented by Naryana
                str.Append(DeficiencyCollegeLabsAnnexure(collegeID));
                str.Append("<br />");

                ////Old Code
                //// str.Append(CollegeLabsAnnexure(collegeID));
                ////New Old
                //if (!mbacollegeides.Contains(collegeID))
                //{
                    str.Append(DeficiencyPhysicalLabs(collegeID));
                //}
                

                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/DeficiencyReports/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + collegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));

                pdfDoc.Open();

                List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(str.ToString()), null);

                foreach (var htmlElement in parsedHtmlElements)
                {
                    pdfDoc.Add((IElement)htmlElement);
                }

                pdfDoc.Close();

                Response.Output.Write(str.ToString());
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public ActionResult LabsDeficiencies(string id)
        {
            // string id = "375";

            int[] CollegeIds = db.jntuh_college.Join(db.jntuh_college_edit_status, Clg => Clg.id, Edit => Edit.collegeId, (Clg, Edit) => new { Clg = Clg, Edit = Edit }).Where(c => c.Clg.isActive == true && c.Edit.IsCollegeEditable == false).Select(c => c.Clg.id).ToArray();
            ViewBag.Colleges = db.jntuh_college_randamcodes.Where(e => e.IsActive == true && CollegeIds.Contains(e.CollegeId)).Select(e => new { rid = e.Id, RandamCode = e.RandamCode }).OrderBy(e => e.RandamCode).ToList();

            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                var college = db.jntuh_college_randamcodes.Where(c => c.IsActive == true && c.CollegeId == collegeID).Select(c => c).FirstOrDefault();
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + college.RandamCode + "-Deficiency Report" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header(collegeID));
                str.Append("<br />");
                str.Append(CollegeRandomCode(collegeID));
                str.Append("<br />");
                //str.Append(Principal(collegeID));
                //str.Append("<br />");

                //str.Append(DeficienciesInFaculty(collegeID));
                //str.Append("<br />");
                //str.Append(DeficienciesInLabs(collegeID));
                //str.Append("<br />");
                str.Append(DeficiencyCollegeLabsAnnexure(collegeID));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/LabsDeficiencyReports/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + collegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));

                pdfDoc.Open();

                List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(str.ToString()), null);

                foreach (var htmlElement in parsedHtmlElements)
                {
                    pdfDoc.Add((IElement)htmlElement);
                }

                pdfDoc.Close();

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
                //Code Commented By Narayana on 28-03-2018
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{
                //    header += "<table width='100%'>";
                //   // header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
                ////http://112.133.193.228:76/Content/Images/new_logo.jpg
                //    header += "<tr>";
                //    header += "<td>Web &nbsp;&nbsp; : www.jntuh.ac.in<br/>Email &nbsp;: pa2registrar@gmail.com</td>";
                //    header += "<td align='center'><img src='http://10.10.10.5:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                //    header += "<td align='right'>Phone: Off: +91-40-32422256<br/>Res: +91-40-32517275<br/>Fax: +91-40-23158665</td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' width='80%' style='font-size: 14px; font-weight: normal;color: Blue'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' style='font-size: 11px; font-weight: normal;'> (Established by JNTU Act, 2008)</td>";
                //    header += "</tr>";
                //    header += "<tr>";
                //    header += "<td colspan='3' align='center' style='font-size: 12px; font-weight: normal;color: Purple'>Kukatpally, Hyderabad - 500 085, Telangana, India</td>";
                //    header += "</tr>";
                //    header += "</table>";
                //    header += "<p><span style='color: Purple'><b>Dr. N. YADAIAH</b></span><br/><span style='color: blue;font-size: 10px;'>B.E (OUCE), M.Tech, (IIT KGP), Ph.D (JNTU)</span><br/><span style='color: blue;font-size: 10px;'>SMIEEE, FIE, FIETE, MSSI, MISTE</span><br/><span style='color: blue;font-size: 10px;'>Professor of EEE &</span><br/><b>REGISTRAR</b>";
                //    header += "</p>";
                //    header += "<p align='center'><b><u>Speaking Order</u></b></p>";
                //    header += "<p align='right'>Date: " + DateTime.Now.ToString("dd/M/yyyy") + "</p>";
                //    header += "<p>To</p>";
                //    header += "<p>The Principal / Secretary / Chairman</p>";




                //    //header += "<table width='100%'>";
                //    //header += "<tr>";
                //    //header += "<td rowspan='4' align='center' width='20%'><img src='http://112.133.193.232:72/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                //    //header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 417</b></u></td>";
                //    //header += "</tr>";
                //    //header += "<tr>";
                //    //header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2017-2018)</b></u></td>";
                //    //header += "</tr>";
                //    //header += "</table>";
                //}
                //else
                //{
                    header += "<table width='100%'>";
                    header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
                    header += "<tr></tr>";
                    header += "</table>";
                    header += "<table width='100%'>";
                    header += "<tr>";
                    header += "<td rowspan='4' align='center' width='20%'><img src='http://10.10.10.5:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
                    header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
                    header += "</tr>";
                    header += "<tr>";
                    header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
                    header += "</tr>";
                    header += "<tr>";
                    header += "<td  align='center' style='font-weight: normal;'><u><b>DEFICIENCY REPORT AS PER FORM 418</b></u></td>";
                    header += "</tr>";
                    header += "<tr>";
                    header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2018-2019)</b></u></td>";
                    header += "</tr>";
                    header += "</table>";
                //}
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
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{
                //    var address = db.jntuh_address.Where(e => e.collegeId == collegeID && e.addressTye=="COLLEGE").Select(e => e).FirstOrDefault();
                //    collegeInformation += "<table>";
                //    collegeInformation += "<tr><td>" + college.collegeName + "</td></tr>";
                //    collegeInformation += "<tr><td>" + address.address + "," + address.townOrCity + "," + address.mandal + "</td></tr>";
                //    collegeInformation += "<tr><td>" + db.jntuh_district.FirstOrDefault(e=>e.id==address.districtId).districtName + " - " + address.pincode + "</td></tr>";
                //    collegeInformation += "</table>";
                //}
                //else
                //{

                    collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    collegeInformation += "<tr>";
                    collegeInformation += "<td align='left' width='75%'><b>College Name: </b><u>" + college.collegeName +"</u>";
                    collegeInformation += "<td align='left' width='25%'><b>CC:  </b><u>" + college.collegeCode +"</u></td>";
                    collegeInformation += "</tr>";

                    collegeInformation += "</table>";
                //}
            }

            return collegeInformation;
        }
        public string CollegeRandomCode(int? collegeID)
        {
            string collegeInformation = string.Empty;

            var college = db.jntuh_college_randamcodes.Where(c => c.IsActive == true && c.CollegeId == collegeID).Select(c => c).FirstOrDefault();

            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr>";
            collegeInformation += "<td align='left' width='75%'><b>College Name: --- </b>";
            collegeInformation += "<td align='left' width='25%'><b>CC:  </b><u>" + college.RandamCode + "</u></td>";
            collegeInformation += "</tr>";

            collegeInformation += "</table>";

            return collegeInformation;
        }
        #region Special Case Deficiency
        public string DeficiencyHeader()
        {
            string header = string.Empty;
            header += "<table width='100%'>";
            header += "<tr><td align='right' width='80%' style='font-size: 16px; font-weight: normal;' colspan='2'><b><u> Date : " + DateTime.Now.ToString("G") + "</u></b></td></tr></br>";
            header += "<tr></tr>";
            header += "</table>";
            header += "<table width='100%'>";
            header += "<tr>";
            header += "<td rowspan='4' align='center' width='20%'><img src='http://112.133.193.228:75/Content/Images/new_logo.jpg' height='70' width='70' style='text-align: center' align='middle' /></td>";
            header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td align='center' style='font-size: 11px; font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA, INDIA - 500 085</b></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>RECOMMENDATION OF THE SCA BASED ON THE FFCA REPORT</b></u></td>";
            header += "</tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>(for Academic Year 2016-2017)</b></u></td>";
            header += "</tr>";
            header += "</table>";
            return header;
        }
        public ActionResult SpecialCaseDeficiencies(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "-SpecialCaseDeficiency Report" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(DeficiencyHeader());
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(DeficiencyData());
                str.Append("<br />");
                str.Append(CollegeIntake(collegeID));
                str.Append("<br />");
                str.Append(DeficiencyData1());
                str.Append("<br />");
                //str.Append(DeficienciesInLabs(collegeID));
                //str.Append("<br />");
                //str.Append(CollegeLabsAnnexure(collegeID));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/SpecialcaseDeficiencyReports/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + collegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));

                pdfDoc.Open();

                List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(str.ToString()), null);

                foreach (var htmlElement in parsedHtmlElements)
                {
                    pdfDoc.Add((IElement)htmlElement);
                }

                pdfDoc.Close();

                Response.Output.Write(str.ToString());
                Response.Flush();
                Response.End();
            }

            return View();
        }
        public string DeficiencyData()
        {
            string collegeInformation = string.Empty;

            // jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr> <td align='left' width='75%'><b>Based on the FFC report it has been observed that: </b></td></tr>";
            collegeInformation += "<tr> <td align='left' width='75%'>i). More than ----- of the faculty are newly appointed.</td></tr>";
            collegeInformation += "<tr> <td align='left' width='75%'>ii). As per the records the College is running the following approved programs in the previous academic years</td></tr>";
            collegeInformation += "</table>";

            return collegeInformation;
        }
        public string DeficiencyData1()
        {
            string DeficiencyData = string.Empty;

            // jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c).FirstOrDefault();

            DeficiencyData += "<table width='100%' border='0' cellpadding='5' cellspacing='2' line-height='1'>";
            DeficiencyData += "<tr> <td align='left' width='75%'>It appears that the faculty have been brought for the purpose of present FFC inspection only, which is an indication that minimum academic standards have not been maintained during the previous academic year.   Hence, the SCA recommends No Admission Status  for the academic year 2016-17 for your Institution. You are further directed to provide the clear explanation within 10 days regarding the availability of essential requirements for the previous batches for running the respective academic programs.</td></tr>";
            DeficiencyData += "</table>";

            DeficiencyData += "</br><table width='100%'  cellspacing='0'></br>";
            DeficiencyData += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            DeficiencyData += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
            DeficiencyData += "<tr><td></td></tr>"; DeficiencyData += "</table>";

            return DeficiencyData;
        }
        public string CollegeIntake(int collegeId)
        {
            string Intake = string.Empty;
            var colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new
            {
                collegeId = c.id,
                collegeName = c.collegeCode + "-" + c.collegeName
            }).OrderBy(c => c.collegeName).ToList();
            ViewBag.Colleges = colleges.OrderBy(c => c.collegeId).ThenBy(c => c.collegeName).ToList();
            //collegeId = 375;
            if (collegeId != null)
            {
                ViewBag.Status = true;
                int userCollegeID = (int)collegeId;

                ViewBag.collegeId = collegeId;
                var jntuh_academic_year = db.jntuh_academic_year.ToList();

                ViewBag.AcademicYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
                int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                //RAMESH: ADDED to MERGE BOTH EXISTING & PROPOSED INTAKE
                ViewBag.NextYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
                int AY0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();


                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
                int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
                int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
                int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
                int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();

                int enclosureId = db.jntuh_enclosures.Where(e => e.documentName == "AICTE INTAKE APPROVAL LETTER").Select(e => e.id).FirstOrDefault();
                var AICTEApprovalLettr = db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID).Select(e => e.path).FirstOrDefault();
                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;

                Intake += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                Intake += "<tr>";
                Intake += "<th style='text-align: left; vertical-align: top;' >S.no</th>";
                Intake += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                Intake += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                Intake += "</tr>";
                int DegreeId = 0, DepartMentId = 0;
                string Degree = "", Specialization = "";
                foreach (var item in intake.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList())
                {
                    DepartMentId = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    DegreeId = jntuh_department.Where(d => d.id == DepartMentId).Select(d => d.degreeId).FirstOrDefault();
                    Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    Degree = jntuh_degree.Where(d => d.id == DegreeId).Select(d => d.degree).FirstOrDefault();
                    Intake += "<tr>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (intake.IndexOf(item) + 1) + "</td>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + Degree + "</td>";
                    Intake += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + Specialization + "</td>";
                    Intake += "</tr>";
                    //CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    //newIntake.id = item.id;
                    //newIntake.collegeId = item.collegeId;
                    //newIntake.academicYearId = item.academicYearId;
                    //newIntake.shiftId = item.shiftId;
                    //newIntake.isActive = item.isActive;
                    //newIntake.nbaFrom = item.nbaFrom;
                    //newIntake.nbaTo = item.nbaTo;
                    //newIntake.specializationId = item.specializationId;
                    //newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    //newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    //newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    //newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    //newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                    //newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    //newIntake.shiftId = item.shiftId;
                    //newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    //newIntake.AICTEApprovalLettr = AICTEApprovalLettr;
                    //collegeIntakeExisting.Add(newIntake);
                }
                Intake += "</table>";


            }
            return Intake;

        }
        #endregion

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
                    //if (regdata.Absent == true)
                    //{
                    //    Reason = "NOT AVAILABLE" + ",";
                    //}
                    //if (regdata.NotQualifiedAsperAICTE == true)
                    //{
                    //    Reason += "NOT QUALIFIED " + ",";
                    //}
                    //if (regdata.InvalidPANNumber == true)
                    //{
                    //    Reason += "NO PAN" + ",";
                    //}
                    //if (regdata.FalsePAN == true)
                    //{
                    //    Reason += "FALSE PAN" + ",";
                    //}
                    //if (regdata.NoSCM == true)
                    //{
                    //    Reason += "NO SCM/RATIFICATION" + ",";
                    //}
                    //if (regdata.IncompleteCertificates == true)
                    //{
                    //    Reason += "Incomplete Certificates" + ",";
                    //}
                    //if (regdata.PHDundertakingnotsubmitted == true)
                    //{
                    //    Reason += "No Undertaking" + ",";
                    //}
                    //if (regdata.Blacklistfaculy == true)
                    //{
                    //    Reason += "Blacklisted" + ",";
                    //}
                    if (!string.IsNullOrEmpty(regdata.DeactivationReason))
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                        //Reason.Substring(0, Reason.Length - 1);
                        facultydata.DeactivationNew = "Yes";
                        OriginalReason = regdata.DeactivationReason;
                    }
                    else
                    {
                        Reason = "Dr. " + facultydata.FirstName.First().ToString().ToUpper() + facultydata.FirstName.Substring(1) + " " + facultydata.LastName.First().ToString().ToUpper() + facultydata.LastName.Substring(1);
                       // facultydata.DeactivationNew = "Principal No Deficiency";

                    }

                }
            }

            else
            {
                Reason = "NOT AVAILABLE";
                facultydata.DeactivationNew = "Yes";
            }


             var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //if (collegeStatus.CollegeStatus == true || collegeStatus.AICTEStatus == true)
                //{
                   
                   
                //}
                //else
                //{
                    principal += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    principal += "<tr>";
                    //principal += "<td align='left'><b>Principal: </b><img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Qualified &nbsp; <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Ratified &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; Deficiency: <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> Yes <img alt='' src='http://112.133.193.228:75//Content/Images/checkbox_no.png' height='16' width='16' /> No";
                    principal += "<td align='left'><b>Principal: </b>" + Reason +
                                 "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";
                    //<b> Reason: </b>" + OriginalReason
                    if (!string.IsNullOrEmpty(facultydata.DeactivationNew))
                        principal += "<b> Deficiency: </b>" + facultydata.DeactivationNew +
                                     "&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;";


                    if (!string.IsNullOrEmpty(OriginalReason))
                        principal += "<b> Reason: </b>" + OriginalReason;
                    principal += "</td>";
                    principal += "</tr>";

                    principal += "</table>";
                //}
            }
            return principal;
        }

        //public string CommitteeMembers(int? collegeID)
        //{
        //    string members = string.Empty;

        //    members += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    members += "<tr>";
        //    members += "<td align='left'><b>Members of FFC Team: </b>";
        //    members += "</tr>";

        //    string collegeCode = db.jntuh_college.Find(collegeID).collegeCode;
        //    committeemembers2015 committee = db.committeemembers2015.Where(c => c.CC == collegeCode).Select(c => c).FirstOrDefault();

        //    if (committee != null)
        //    {
        //        string strCommittee = string.Empty;

        //        if (!string.IsNullOrEmpty(committee.TeamMember1))
        //        {
        //            strCommittee += "1. " + committee.TeamMember1 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember2))
        //        {
        //            strCommittee += "2. " + committee.TeamMember2 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember3))
        //        {
        //            strCommittee += "3. " + committee.TeamMember3 + "<br />";
        //        }
        //        if (!string.IsNullOrEmpty(committee.TeamMember4))
        //        {
        //            strCommittee += "4. " + committee.TeamMember4 + "<br />";
        //        }

        //        members += "<tr>";
        //        members += "<td align='left'>" + strCommittee + "</td>";
        //        members += "</tr>";
        //    }

        //    members += "</table>";

        //    return members;
        //}

        public string DeficienciesInFaculty(int? collegeID)
        {

            string facultyclosure = string.Empty;
            string twintyfivepercentbelowcurces = string.Empty;

            string faculty = string.Empty;
            string facultyAdmittedIntakeZero = string.Empty;
            string facultyAdmittedIntakeZeroTable2 = string.Empty;
            int facultycount = 0;
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);
            int[] shiftids = {1,2};
            List<CollegeFacultyWithIntakeReport> facultyCountslist = new List<CollegeFacultyWithIntakeReport>();
           // List<CollegeFacultyWithIntakeReport> facultyCountslist =collegeFaculty(collegeID).Where(c => shiftids.Contains(c.shiftId) && c.Degree== "M.Tech").Select(e => e).ToList();
            //List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1||c.shiftId==2).Select(e => e).ToList();//Where(c => c.shiftId == 1)
            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 || c.shiftId == 2).Select(e => e).ToList();//Where(c => c.shiftId == 1)
            List<CollegeFacultyWithIntakeReport> facultyCountsmetechsecond = facultyCounts.Where(c => c.shiftId == 2).Select(e => e).ToList();
            foreach (var item in facultyCountsmetechsecond)
            {
                int id =
                    facultyCounts.Where(
                        s => s.specializationId == item.specializationId && s.shiftId == 1 && s.Degree == "M.Tech" && s.approvedIntake1 != 0)
                        .Select(s => s.shiftId)
                        .FirstOrDefault();
                if (id == 0)
                {
                    facultyCounts.Remove(item);
                }
            }
            List<CollegeFacultyWithIntakeReport> facultyCountper = collegeFaculty(collegeID).Where(c => (c.ispercentage == true && c.approvedIntake1 != 0 && c.Degree == "B.Tech") || c.approvedIntake1 == 0 && c.Degree == "B.Tech").Select(e => e).ToList();
            foreach(var itemmtech in facultyCountper)
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
                       s => s.Department == itemmtech.Department && s.Degree == "M.Tech" && s.approvedIntake1 != 0)
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
            //
            //var departments=db.jntuh_department.Where()

            //facultyCounts=facultyCounts.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            //facultyCounts.AddRange();
            //List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1 || (c.shiftId == 1 && c.shiftId == 2)).Select(e => e).ToList();//Where(c => c.shiftId == 1)

            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.SIStatus == true)
                {
                    // facultyCounts = facultyCounts.Where(e => e.Degree != "M.Tech" || e.Degree != "MCA" || e.Degree != "MBA").Select(e => e).ToList();
                    facultyCounts = facultyCounts.Where(e => e.Degree == "B.Tech").Select(e => e).ToList();
                }
            }

            if (collegeStatus != null)
            {
                var CollegeSubmissionDate = Convert.ToDateTime(db.jntuh_college_edit_status.Where(e => e.collegeId == collegeID && e.IsCollegeEditable == false).Select(e => e.updatedOn).FirstOrDefault()).Date;
               // DateTime data = Convert.ToDateTime(CollegeSubmissionDate).Date;
                if (collegeStatus.AICTEStatus == true)
                {

                    var RegisteredFaculty = (from Reg in db.jntuh_registered_faculty
                                             join Clg in db.jntuh_college_faculty_registered on Reg.RegistrationNumber equals Clg.RegistrationNumber
                                             join ClgEdit in db.jntuh_college_edit_status on Clg.collegeId equals ClgEdit.collegeId
                                             where ClgEdit.IsCollegeEditable == false && Clg.collegeId == collegeID
                                             select Reg).ToList();

                    int Absebtfaculty = RegisteredFaculty.Count(e => e.Absent == true);
                    int TotalFaculty = RegisteredFaculty.Count();

                    var Percentage = Math.Ceiling((((double)Absebtfaculty / (double)TotalFaculty) * 100));



                    //faculty += "<p>As per the FFC report it is observed that:</p>";
                    //faculty += "<ol type='i'>";
                    //faculty += "<li>Acute deficiency of faculty due to " + Percentage + " % of faculty absent during FFC inspection.</li>";
                    //faculty += "<li>As per the records the College is running the following approved programs in the previous academic years:</li>";
                    //faculty += "</ol>";
                    //faculty += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                    //faculty += "<tr>";
                    //faculty += "<th>S.No.</th>";
                    //faculty += "<th>Degree</th>";
                    //faculty += "<th>Department</th>";
                    //faculty += "<th>Specialization</th>";
                    //faculty += "</tr>";

                    //var CollegeIntakeData = (from Intake in db.jntuh_college_intake_existing
                    //                         join Spec in db.jntuh_specialization on Intake.specializationId equals Spec.id
                    //                         join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                    //                         join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                    //                         where Intake.collegeId == collegeID && Intake.academicYearId == 8
                    //                         select new
                    //                         {
                    //                             Degree = Deg.degree,
                    //                             Department = Dept.departmentName,
                    //                             SpecializationName = Spec.specializationName,
                    //                             DegreeId = Deg.id
                    //                         }).OrderBy(e => e.Degree).ToList();
                    //int collegecountindex = 1;
                    //foreach (var collegeitem in CollegeIntakeData)
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<td>" + collegecountindex + "</td>";
                    //    faculty += "<td>" + collegeitem.Degree + "</td>";
                    //    faculty += "<td>" + collegeitem.Department + "</td>";
                    //    faculty += "<td>" + collegeitem.SpecializationName + "</td>";
                    //    faculty += "</tr>";
                    //    collegecountindex++;
                    //}




                    //faculty += "</table>";
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2018-19- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";

                    faculty += "<p style='text-align:justfy'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;As per the fact finding Committee inspection report it is noticed that " + Percentage + " of faculty members were absent against the number of faculty shown in online application for affiliation A117. It indicates that the College is not maintaining the minimum essential requirements for running the academic programs. Under these circumstances the SCA recommends <b><u>'No Admission Status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter</p>";

                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";


                }
                else if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Sub: - Affiliation for the Academic Year: 2018-19- Reg.</p>";
                    faculty += "<br/>";
                    faculty += "<table><tr><td style='vertical-align:top'>Ref:</td><td>";
                    faculty += "<ol type='1'>";
                    faculty += "<li>Your college online application dated:" + CollegeSubmissionDate + " for grant of Affiliation for the Academic Year 2017-18.</li>";
                    faculty += "<li>Your college FFC Inspection on  dated: ----------- </li>";
                    faculty += "</ol></td></tr></table>";
                    faculty += "<br/>";
                    faculty += "<p align='center'><b>****</b></p>";
                }
               
            }


            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };

            int[] OthersSpecIds = new int[] { 155, 156, 157, 158 };
            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech"&& !departments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 160);
            int remainingFaculty = 0;
            int remainingSpecializationFaculty = 0;
            int remainingPHDFaculty = 0;
            int remainingSpecializationwisePHDFaculty = 0;
            int SpecializationwisePHDFaculty = 0;
            int SpecializationwisePGFaculty = 0;
            int TotalCount = 0;
            int HumantitiesminimamRequireMet = 0;
            string HumantitiesminimamRequireMetStatus = "Yes";



            string[] ProposedIntakeZeroDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.approvedIntake1 == 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            string[] twentyfivePersentbelowDeptName = facultyCounts.Where(e => e.Degree == "B.Tech" && (e.ispercentage == true) && (e.approvedIntake1 != 0) && !departments.Contains(e.Department)).Select(e => e.Specialization).Distinct().ToArray();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {

                    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    faculty += "<tr>";
                    faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
                    faculty += "</tr>";
                    faculty += "</table>";
                    faculty += "<table  border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    faculty += "<tr>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;'>SNo</th>";
                    faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Dept</th>";
                    faculty += "<th  rowspan='2' style='text-align: left; vertical-align: top;' >Degree</th>";
                    faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Specialization</th>";
                    //Written by Narayana
                    //faculty += "<th rowspan='2' style='text-align: left; vertical-align: top;' >Shift</th>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<th colspan='3' rowspan='2' style='text-align: center; vertical-align: top;' >Sum of Proposed &<br/> Previous Sanctioned Intake</th>";
                    //}
                    //else
                    //{
                    //    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //}
                    faculty += "<th colspan='3' style='text-align: center; vertical-align: top;' >Intake of Previous Three A.Y</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Total Department Faculty</th>";
                    faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
                    faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >PG Specialization Faculty</th>";
                    faculty += "<th colspan='2' style='text-align: center; vertical-align: top;' >Faculty with Ph.D</th>";
                    //faculty += "<th rowspan='2' style='text-align: center; vertical-align: top;' >Available Ph.D faculty</th>";
                    faculty += "<th 'rowspan='2 style='text-align: center; vertical-align: top;' >Remarks</th>";
                    faculty += "</tr>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    //faculty += "<th colspan='3' style='text-align: center; vertical-align: top;'></th>";
                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";

                    //    //faculty += "<th style='text-align: center; vertical-align: top;'></th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    //    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    //    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    //    faculty += "</tr>";
                    //}

                    faculty += "<tr>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>JNTUH <br/>Sanctioned</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Admitted</th>";

                    faculty += "<th style='text-align: center; vertical-align: top;'>#Divisions</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Required</th>";
                    faculty += "<th style='text-align: center; vertical-align: top;'>Available</th>";
                    faculty += "</tr>";
                    facultyAdmittedIntakeZero += "<p><b>In so for as the remaining programs/courses applied for affiliation the University has issued a separate show cause notice Dt.___________ based on the discrepancy in faculty uploaded by the College to AICTE and University and zero admissions for more than one previous A.Y. in those programs/courses. </b></p>";
                    facultyAdmittedIntakeZero += "<p><b>The admission status in certain programs/courses in the previous three academic years in which there were zero admissions for two or more than two academic years either due to non grant of affiliation or unable to make admissions at the Institute is shown in the following table. It is also observed that there is a mismatch of faculty information that the Institute has submitted to AICTE and the University. The EOA obtained from AICTE is based on the faculty data uploaded by the Institute </b></p>";
                    facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    facultyAdmittedIntakeZero += "<tr>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                    facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                    facultyAdmittedIntakeZero += "</tr>";


                    facultyAdmittedIntakeZeroTable2 += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                    facultyAdmittedIntakeZeroTable2 += "<tr>";
                    facultyAdmittedIntakeZeroTable2 += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                    facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                    facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                    facultyAdmittedIntakeZeroTable2 += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                    facultyAdmittedIntakeZeroTable2 += "</tr>";

                    //var AutonamousStatus=db.jntuh_college_affiliation.
                    // string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();





                    string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.approvedIntake2 == 0 && e.approvedIntake3 == 0) || (e.approvedIntake3 == 0 && e.approvedIntake4 == 0) || (e.approvedIntake2 == 0 && e.approvedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
                    //


                    #region Intake not equal to Zero

                    // Getting M.Tech Civil Specialization ID's
                    int[] SpecializationIDS =
                        db.jntuh_specialization.Where(S => S.departmentId == 43).Select(S => S.id).ToArray();
                    int remainingFaculty2 = 0;
                    int facultyIndexnotEqualtoZeroIndex = 1;
                    int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                    int btechcount = facultyCounts.Where(
                        e =>
                            e.approvedIntake1 != 0 && e.ispercentage == false &&e.Degree=="B.Tech" &&
                            !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                            !departments.Contains(e.Department)).Select(e => e).ToList().Count;
                    if (btechcount==0)
                    {
                    foreach (var dep in departments)
                    {
                        CollegeFacultyWithIntakeReport CollegeFacultyWithIntakeReport =
                            facultyCounts.Where(d => d.Department == dep).Select(s => s).FirstOrDefault();
                        facultyCounts.Remove(CollegeFacultyWithIntakeReport);
                    }
                    }
                   

                    foreach (
                        var item in
                            facultyCounts.Where(
                                e =>
                                    e.approvedIntake1 != 0 && e.ispercentage == false &&
                                    !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())
                    {
                       

                            //  if (((item.approvedIntake2 == 0 && item.approvedIntake3 == 0) &&(item.approvedIntake3 == 0 && item.approvedIntake4 == 0) &&(item.approvedIntake2 == 0 && item.approvedIntake4 == 0) ||admittedIntakeTwoormoreZeroDeptName1.Contains(item.Department)) &&!departments.Contains(item.Department))
                            //  {

                            #region Admitted Intake With Zero

                            //if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                            //{

                            //    if (item.Degree == "M.Tech")
                            //    {
                            //        SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1);
                            //    }
                            //    else if (item.Degree == "MCA")
                            //    {
                            //        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                            //    }
                            //    else if (item.Degree == "MBA")
                            //    {
                            //        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                            //    }


                            //}
                            //else if (item.Degree == "B.Tech")
                            //{
                            //    SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            //}
                            //TotalCount =
                            //    facultyCounts.Where(
                            //        D =>
                            //            D.Department == item.Department &&
                            //            (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.approvedIntake1 != 0)
                            //        .Distinct()
                            //        .Count();



                            ////if (SpecializationIDS.Contains(item.specializationId))
                            ////{
                            ////    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                            ////    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                            ////}
                            //int indexnow = facultyCounts.IndexOf(item);



                            //if (indexnow > 0 &&
                            //    facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            //{
                            //    deptloop = 1;
                            //}

                            ////  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                            //string minimumRequirementMet = string.Empty;

                            //string PhdminimumRequirementMet = string.Empty;


                            //int facultyShortage = 0, tFaculty = 0;
                            //int adjustedFaculty = 0;
                            //int adjustedPHDFaculty = 0;
                            //int othersRequiredfaculty = 0;

                            //if (item.Department == "MBA" || item.Department == "MCA")
                            //    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            //else
                            //    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                            ////item.totalFaculty
                            //int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                            //if (departments.Contains(item.Department))
                            //{
                            //    if (OthersSpecIds.Contains(item.specializationId))
                            //    {
                            //        rFaculty = ((int)firstYearRequired) / 2;
                            //        if (rFaculty <= 2)
                            //        {
                            //            rFaculty = 2;
                            //            othersRequiredfaculty = 2;
                            //        }
                            //        else
                            //        {
                            //            othersRequiredfaculty = rFaculty;
                            //        }

                            //    }
                            //    else
                            //    {
                            //        rFaculty = (int)firstYearRequired;
                            //    }

                            //    // departmentWiseRequiredFaculty = (int)firstYearRequired;
                            //}

                            //var degreeType =
                            //    degrees.Where(d => d.degree == item.Degree)
                            //        .Select(d => d.jntuh_degree_type.degreeType)
                            //        .FirstOrDefault();

                            //if (deptloop == 1)
                            //{
                            //    if (rFaculty <= tFaculty)
                            //    {
                            //        minimumRequirementMet = "YES";
                            //        item.deficiency = false;
                            //        remainingFaculty = tFaculty - rFaculty;
                            //        adjustedFaculty = rFaculty; //tFaculty
                            //    }
                            //    else
                            //    {
                            //        minimumRequirementMet = "NO";
                            //        item.deficiency = true;
                            //        adjustedFaculty = tFaculty;
                            //        // facultyShortage = rFaculty - tFaculty;
                            //    }

                            //    remainingPHDFaculty = item.phdFaculty;

                            //    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                            //        item.SpecializationsphdFaculty > 0)
                            //    {

                            //        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            //        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            //        PhdminimumRequirementMet = "YES";
                            //    }
                            //    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                            //             item.SpecializationsphdFaculty > 0)
                            //    {
                            //        adjustedPHDFaculty = remainingPHDFaculty;
                            //        remainingPHDFaculty = remainingPHDFaculty - 1;
                            //        PhdminimumRequirementMet = "NO";
                            //    }
                            //    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                            //             remainingPHDFaculty > 0)
                            //    {

                            //        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                            //        {
                            //            remainingPHDFaculty = remainingPHDFaculty -
                            //                                  item.SpecializationsphdFaculty;
                            //            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            //            PhdminimumRequirementMet = "YES";
                            //        }
                            //        else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                            //        {
                            //            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                            //            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                            //            PhdminimumRequirementMet = "YES";
                            //        }



                            //    }
                            //    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                            //             remainingPHDFaculty <= 0)
                            //    {
                            //        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                            //        adjustedPHDFaculty = remainingPHDFaculty;
                            //        PhdminimumRequirementMet = "NO";

                            //    }
                            //    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            //    {
                            //        PhdminimumRequirementMet = "NO";
                            //    }
                            //    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            //    {
                            //        PhdminimumRequirementMet = "YES";
                            //    }

                            //}
                            //else
                            //{
                            //    if (rFaculty <= remainingFaculty)
                            //    {
                            //        minimumRequirementMet = "YES";
                            //        item.deficiency = false;
                            //        if (rFaculty <= item.specializationWiseFaculty)
                            //        {
                            //            remainingFaculty = remainingFaculty - rFaculty;
                            //            adjustedFaculty = rFaculty;
                            //        }

                            //        else if (rFaculty >= item.specializationWiseFaculty)
                            //        {
                            //            remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                            //            adjustedFaculty = item.specializationWiseFaculty;
                            //        }


                            //    }
                            //    else
                            //    {



                            //        minimumRequirementMet = "NO";
                            //        item.deficiency = true;
                            //        adjustedFaculty = remainingFaculty;
                            //        facultyShortage = rFaculty - remainingFaculty;
                            //        remainingFaculty = 0;
                            //    }

                            //    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                            //        item.SpecializationsphdFaculty > 0)
                            //    {
                            //        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                            //        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            //        PhdminimumRequirementMet = "YES";
                            //    }
                            //    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                            //             item.SpecializationsphdFaculty > 0)
                            //    {
                            //        adjustedPHDFaculty = remainingPHDFaculty;
                            //        remainingPHDFaculty = remainingPHDFaculty - 1;
                            //        PhdminimumRequirementMet = "NO";
                            //    }
                            //    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                            //             remainingPHDFaculty > 0)
                            //    {

                            //        if (item.SpecializationsphdFaculty >= SpecializationwisePHDFaculty)
                            //        {
                            //            remainingPHDFaculty = remainingPHDFaculty -
                            //                                  item.SpecializationsphdFaculty;
                            //            adjustedPHDFaculty = item.SpecializationsphdFaculty;
                            //            PhdminimumRequirementMet = "YES";
                            //        }
                            //        else if (item.SpecializationsphdFaculty <= SpecializationwisePHDFaculty)
                            //        {
                            //            remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                            //            adjustedPHDFaculty = SpecializationwisePHDFaculty;
                            //            PhdminimumRequirementMet = "YES";
                            //        }

                            //    }
                            //    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                            //             remainingPHDFaculty <= 0)
                            //    {

                            //        adjustedPHDFaculty = remainingPHDFaculty;
                            //        PhdminimumRequirementMet = "NO";
                            //    }
                            //    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                            //    {
                            //        PhdminimumRequirementMet = "NO";
                            //    }
                            //    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                            //    {
                            //        PhdminimumRequirementMet = "YES";
                            //    }
                            //}




                            //facultyAdmittedIntakeZero += "<tr>";
                            //facultyAdmittedIntakeZero +=
                            //    "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                            //    facultyIndexnotEqualtoZeroAdmittedIntakeIndex + "</td>";
                            ////(facultyCounts.Where(e => e.approvedIntake1 != 0).Select(e => e).ToList().IndexOf(item) + 1) 
                            //facultyAdmittedIntakeZero +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +
                            //    "</td>";
                            //facultyAdmittedIntakeZero +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                            //    "</td>";
                            //facultyAdmittedIntakeZero +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +
                            //    "</td>";



                            //facultyAdmittedIntakeZeroTable2 += "<tr>";
                            //facultyAdmittedIntakeZeroTable2 +=
                            //    "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                            //    facultyIndexnotEqualtoZeroAdmittedIntakeIndex + "</td>";
                            ////(facultyCounts.Where(e => e.approvedIntake1 != 0).Select(e => e).ToList().IndexOf(item) + 1) 
                            //facultyAdmittedIntakeZeroTable2 +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +
                            //    "</td>";
                            //facultyAdmittedIntakeZeroTable2 +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                            //    "</td>";
                            //facultyAdmittedIntakeZeroTable2 +=
                            //    "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +
                            //    "</td>";





                            //if (departments.Contains(item.Department))
                            //{
                            //    // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //        totalBtechFirstYearIntake + "</td>";


                            //    if (deptloop == 1)
                            //        facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount +
                            //                                     "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                            //                                     item.totalFaculty + "</td>";
                            //    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            //    if (OthersSpecIds.Contains(item.specializationId))
                            //    {
                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //            othersRequiredfaculty + "</td>";
                            //        HumantitiesminimamRequireMet += item.totalFaculty;
                            //    }
                            //    else
                            //    {
                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //            Math.Ceiling(firstYearRequired) + "</td>";
                            //        HumantitiesminimamRequireMet += item.totalFaculty;
                            //    }

                            //    //
                            //    if (minimumRequirementMet != "YES")
                            //    {
                            //        HumantitiesminimamRequireMetStatus = "NO";
                            //    }
                            //}
                            //else
                            //{
                            //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";


                            //    if (item.Degree == "B.Tech")
                            //    {
                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //            item.totalAdmittedIntake + "</td>";

                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>";
                            //        facultyAdmittedIntakeZero +=
                            //            "<table width='100%' border='1'  cellspacing='0' style='border-color: #ccc;'>";
                            //        if (item.approvedIntake2 == 0)
                            //            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake2 +
                            //                                         "(II Year)</span></td></tr>";
                            //        if (item.approvedIntake3 == 0)
                            //            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake3 +
                            //                                         "(III Year)</span></td></tr>";
                            //        if (item.approvedIntake4 == 0)
                            //            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake4 +
                            //                                         "(IV Year)</span></td></tr>";
                            //        facultyAdmittedIntakeZero += "</table></td>";
                            //    }
                            //    else
                            //    {
                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //            item.totalIntake + "</td>";
                            //        facultyAdmittedIntakeZero +=
                            //            "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //            item.totalIntake + " </td>";
                            //    }
                            //    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                            //        item.approvedIntake1 + "</td>";
                            //    // if (deptloop == 1)
                            //    //  facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";
                            //    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                            //    //  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                            //}
                            //facultycount = facultycount + item.specializationWiseFaculty;
                            //if (adjustedFaculty > 0)
                            //    adjustedFaculty = adjustedFaculty;
                            //else
                            //    adjustedFaculty = 0;

                            //if (adjustedPHDFaculty > 0)
                            //    adjustedPHDFaculty = adjustedPHDFaculty;
                            //else
                            //    adjustedPHDFaculty = 0;

                            //// facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";


                            ////if (item.Degree == "M.Tech")
                            ////    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                            ////else
                            ////    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";
                            //if (item.AffiliationStatus2 == true)
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +
                            //        "</td>";


                            //if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            //{
                            //    if (rFaculty <= adjustedFaculty)
                            //        minimumRequirementMet = "No Deficiency In faculty";
                            //    else
                            //        minimumRequirementMet = "Deficiency In faculty";

                            //    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //        minimumRequirementMet += " and No Deficiency In Phd";
                            //    else
                            //        minimumRequirementMet += " and Deficiency In Phd";

                            //}

                            //else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            //{
                            //    if (rFaculty == adjustedFaculty)
                            //        minimumRequirementMet = "No Deficiency In faculty";
                            //    else
                            //        minimumRequirementMet = "Deficiency In faculty";


                            //    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                            //        minimumRequirementMet += " and No Deficiency In Phd";
                            //    else
                            //        minimumRequirementMet += " and Deficiency In Phd";
                            //}
                            //else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            //{
                            //    if (rFaculty <= adjustedFaculty)
                            //        minimumRequirementMet = "No Deficiency In faculty";
                            //    else
                            //        minimumRequirementMet = "Deficiency In faculty";

                            //    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //        minimumRequirementMet += " and No Deficiency In Phd";
                            //    else
                            //        minimumRequirementMet += " and Deficiency In Phd";

                            //}

                            //else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            //{
                            //    if (rFaculty == adjustedFaculty)
                            //        minimumRequirementMet = "No Deficiency In faculty";
                            //    else
                            //        minimumRequirementMet = "Deficiency In faculty";


                            //    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                            //        minimumRequirementMet += " and No Deficiency In Phd";
                            //    else
                            //        minimumRequirementMet += " and Deficiency In Phd";
                            //}






                            //// facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                            //if (item.AffiliationStatus3 == true)
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +
                            //        "</td>";

                            //// facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                            //if (item.AffiliationStatus4 == true)
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                            //else
                            //    facultyAdmittedIntakeZero +=
                            //        "<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +
                            //        "</td>";
                            ////  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";


                            //facultyAdmittedIntakeZero += "</tr>";
                            //facultyAdmittedIntakeZeroTable2 += "</tr>";
                            //deptloop++;
                            //SpecializationwisePHDFaculty = 0;
                            //facultyIndexnotEqualtoZeroAdmittedIntakeIndex++;

                            #endregion

                            //  }
                            //  else
                            //  {

                            #region Old Code without calculation admitted Intake

                            if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA" ||
                                item.Degree == "5-Year MBA(Integrated)")
                            {

                                if (item.Degree == "M.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1,
                                        item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }
                                else if (item.Degree == "MBA" || item.Degree == "5-Year MBA(Integrated)")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }


                            }
                            else if (item.Degree == "B.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.approvedIntake1 != 0)
                                    .Distinct()
                                    .Count();



                            //if (SpecializationIDS.Contains(item.specializationId))
                            //{
                            //    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                            //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                            //}
                            int indexnow = facultyCounts.IndexOf(item);



                            if (indexnow > 0 &&
                                facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            {
                                deptloop = 1;
                            }
                            if ((item.collegeId == 72 && item.Department == "IT") || (item.collegeId == 130 && item.Department == "IT"))
                            {
                                deptloop = 1;
                            }

                            //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                            string minimumRequirementMet = string.Empty;
                            string PhdminimumRequirementMet = string.Empty;
                            int facultyShortage = 0, tFaculty = 0;
                            int adjustedFaculty = 0;
                            int adjustedPHDFaculty = 0;
                            int othersRequiredfaculty = 0;

                            if (item.Department == "MBA" || item.Department == "MCA")
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            else
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                            //item.totalFaculty
                            int rFaculty = (int) Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    double rid = (double) (firstYearRequired/2);
                                    rFaculty = (int) (Math.Ceiling(rid));

                                    //othersRequiredfaculty = rFaculty;
                                    //Commented By Narayana on 12-03-2018
                                    //rFaculty = 1;
                                    // othersRequiredfaculty = 1;

                                    //rFaculty = ((int)firstYearRequired)/2;
                                    //if (rFaculty <= 2)
                                    //{
                                    //    rFaculty = 2;
                                    //    othersRequiredfaculty = 2;
                                    //}
                                    //else
                                    //{
                                    //    othersRequiredfaculty = rFaculty;
                                    //}

                                }
                                else
                                {
                                    rFaculty = (int) firstYearRequired;
                                }

                                // departmentWiseRequiredFaculty = (int)firstYearRequired;
                            }

                            var degreeType =
                                degrees.Where(d => d.degree == item.Degree)
                                    .Select(d => d.jntuh_degree_type.degreeType)
                                    .FirstOrDefault();

                            if (deptloop == 1)
                            {
                                if (rFaculty <= tFaculty)
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    remainingFaculty = tFaculty - rFaculty;
                                    adjustedFaculty = rFaculty; //tFaculty
                                }
                                else
                                {
                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = tFaculty;
                                    remainingFaculty = 0;
                                    // facultyShortage = rFaculty - tFaculty;
                                }

                                remainingPHDFaculty = item.phdFaculty;

                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                    {
                                        //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        //adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        if (item.shiftId==1)
                                        {
                                            //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            //adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            adjustedPHDFaculty = item.SpecializationsphdFaculty;
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

                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
                                {
                                    //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                    //adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                    if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                        //item.SpecializationsphdFaculty
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        //item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (remainingPHDFaculty <= SpecializationwisePHDFaculty)
                                        //item.SpecializationsphdFaculty
                                    {

                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        //SpecializationwisePHDFaculty;
                                        remainingPHDFaculty = 0;
                                        //remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        // adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }



                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";

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
                                else if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) &&
                                         item.SpecializationsphdFaculty > 0)
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
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) &&
                                         item.SpecializationsphdFaculty >= 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty == 0 &&
                                         (degreeType.Equals("Dual Degree")))
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
                                        //if (rFaculty <= item.specializationWiseFaculty)
                                        //{
                                        //    remainingFaculty = remainingFaculty - rFaculty;
                                        //    adjustedFaculty = rFaculty;
                                        //}

                                        //else if (rFaculty >= item.specializationWiseFaculty)
                                        //{
                                        //    remainingFaculty = remainingFaculty - item.specializationWiseFaculty;
                                        //    adjustedFaculty = item.specializationWiseFaculty;
                                        //}

                                       //New Code Written By M
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
                                            if (item.shiftId==2)
                                            {
                                                int firstshiftrequiredFaculty = (int)Math.Ceiling(facultyCounts.Where(e =>
                                             e.approvedIntake1 != 0 && e.ispercentage == false &&
                                             !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                             e.specializationId == item.specializationId && e.shiftId == 1)
                                             .Select(e => e.requiredFaculty)
                                             .FirstOrDefault());

                                                item.specializationWiseFaculty = item.specializationWiseFaculty -
                                                                                 firstshiftrequiredFaculty;
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
                                        remainingFaculty = 0;
                                    }

                                }
                                //  remainingPHDFaculty = item.phdFaculty;
                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    //adjustedPHDFaculty = item.SpecializationsphdFaculty;   

                                    //PhdminimumRequirementMet = "YES";
                                    if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                    {
                                        if (item.shiftId == 1)
                                        {
                                            //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            //adjustedPHDFaculty = SpecializationwisePHDFaculty;
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
                                            //if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                            //{
                                            //    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                            //    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                            //    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            //        PhdminimumRequirementMet = "YES";
                                            //    else
                                            //        PhdminimumRequirementMet = "NO";
                                            //}
                                            //else
                                            //{
                                            //    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            //    adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                            //    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                            //        PhdminimumRequirementMet = "YES";
                                            //    else
                                            //        PhdminimumRequirementMet = "NO";
                                            //}
                                           
                                        }
                                        else
                                        {
                                            //remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                            //adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                            if (item.shiftId==2)
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
                                                e.approvedIntake1 != 0 && e.ispercentage == false &&
                                                !ProposedIntakeZeroDeptName.Contains(e.Specialization) &&
                                                e.specializationId == item.specializationId && e.shiftId == 1)
                                                .Select(e => e.approvedIntake1)
                                                .FirstOrDefault();
                                            int firstshiftPHDFaculty = IntakeWisePhdForMtech(firstshiftintake, 1);
                                            item.SpecializationsphdFaculty = item.SpecializationsphdFaculty -
                                                                             firstshiftPHDFaculty;
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
                                            remainingPHDFaculty = remainingPHDFaculty;
                                            adjustedPHDFaculty = remainingPHDFaculty;

                                            if (remainingPHDFaculty >= SpecializationwisePHDFaculty)
                                                PhdminimumRequirementMet = "YES";
                                            else
                                                PhdminimumRequirementMet = "NO";
                                        }

                                        
                                    }

                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    if (item.shiftId == 1)
                                    {
                                        //remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        //adjustedPHDFaculty = SpecializationwisePHDFaculty;
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
                                        //if (remainingPHDFaculty >= item.SpecializationsphdFaculty)
                                        //{
                                        //    remainingPHDFaculty = remainingPHDFaculty - SpecializationwisePHDFaculty;
                                        //    adjustedPHDFaculty = SpecializationwisePHDFaculty;
                                        //    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        //        PhdminimumRequirementMet = "YES";
                                        //    else
                                        //        PhdminimumRequirementMet = "NO";
                                        //}
                                        //else
                                        //{
                                        //    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        //    adjustedPHDFaculty = item.SpecializationsphdFaculty;

                                        //    if (adjustedPHDFaculty >= SpecializationwisePHDFaculty)
                                        //        PhdminimumRequirementMet = "YES";
                                        //    else
                                        //        PhdminimumRequirementMet = "NO";
                                        //}

                                    }                                   
                                    else if (item.shiftId == 2)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";
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
                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("Dual Degree")) &&
                                    item.SpecializationsphdFaculty > 0)
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
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("Dual Degree")) &&
                                         item.SpecializationsphdFaculty > 0)
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
                            faculty +=
                                "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                facultyIndexnotEqualtoZeroIndex + "</td>";
                            //(facultyCounts.Where(e=>e.approvedIntake1!=0).Select(e=>e).ToList().IndexOf(item) + 1)
                        if(item.isstarcourses==true)
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department + "#" + "</td>";
                        }
                        else
                        {
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                       item.Department +"</td>";
                        }
                            faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                       "</td>";
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";
                            if (item.shiftId == 1)
                            {
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                           item.Specialization + "</td>";
                            }
                            else
                            {
                                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                           item.Specialization + " -2" + "</td>";
                            }
                            //Written by Narayana
                            //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.shiftId + "</td>";

                            if (departments.Contains(item.Department))
                            {
                                // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                faculty +=
                                    "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           totalBtechFirstYearIntake + "</td>";


                                if (deptloop == 1)
                                    faculty += "<td rowspan='" + TotalCount +
                                               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.totalFaculty + "</td>";
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               Math.Ceiling(firstYearRequired/2) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }

                                //
                                if (minimumRequirementMet != "YES")
                                {
                                    HumantitiesminimamRequireMetStatus = "NO";
                                }
                            }
                            else
                            {
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";


                                if (item.Degree == "B.Tech")
                                {
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";
                                    faculty +=
                                        "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        item.SanctionIntake2 + "</span></td></tr><tr><td><span>" + item.SanctionIntake3 +
                                        "</span></td></tr><tr><td><span>" + item.SanctionIntake4 +
                                        "</span></td></tr></table></td>";
                                    faculty +=
                                        "<td class='col2' style='text-align: center; vertical-align: top;'><table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'><tr><td><span>" +
                                        item.approvedIntake2 + "</span></td></tr><tr><td><span>" + item.approvedIntake3 +
                                        "</span></td></tr><tr><td><span>" + item.approvedIntake4 +
                                        "</span></td></tr></table></td>";
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                    faculty +=
                                        "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                                    //Written New on 03-02-2018
                                    int SanctionIntakeHigest = Max(item.approvedIntake2, item.approvedIntake3,
                                        item.approvedIntake4);
                                    SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                                    if (SanctionIntakeHigest >= item.approvedIntake1)
                                    {
                                        faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.approvedIntake2) +
                                                   "(A)" + "</span></td>";
                                        faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.approvedIntake3) +
                                                   "(A)" + "</span></td></tr>";
                                        faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.approvedIntake4) +
                                                   "(A)" + "</span></td></tr>";

                                    }
                                    else
                                    {
                                        faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake2) +
                                                   "(S)" +
                                                   "</span></td>";
                                        faculty += "</tr><tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake3) +
                                                   "(S)" +
                                                   "</span></td></tr>";
                                        faculty += "<tr><td><span>" + GetSectionBasedonIntake(item.SanctionIntake4) +
                                                   "(S)" +
                                                   "</span></td></tr>";

                                    }
                                    faculty += "</table></td>";
                                }
                                else
                                {
                                    //if (mbacollegeides.Contains(cid))
                                    //{
                                        
                                    //    faculty +=
                                    //        "<td colspan='3' class='col2' style='text-align: center; vertical-align: top;'>" +
                                    //        item.totalIntake + " </td>";
                                    //}
                                    //else
                                    //{
                                        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.totalIntake + "</td>";
                                        faculty +=
                                            "<td colspan='2' class='col2' style='text-align: center; vertical-align: top;'>" +
                                            item.totalIntake + " </td>";
                                    //}
                                    
                                }
                                //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                if (String.IsNullOrEmpty(item.Note))
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.approvedIntake1 + "</td>";
                                }
                                else
                                {
                                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.approvedIntake1 + " " + item.Note + "</td>";
                                }
                                if (deptloop == 1)
                                    faculty += "<td rowspan='" + TotalCount +
                                               "'  class='col2' style='text-align: center; vertical-align: top;'>" +
                                               item.totalFaculty + "</td>";
                                //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           Math.Ceiling(item.requiredFaculty) + "</td>";

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

                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       adjustedFaculty + "</td>";
                            //    if (SpecializationIDS.Contains(item.specializationId))
                            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePGFaculty + "</td>";
                            //else
                            //        faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            if (item.Degree == "M.Tech")
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                           item.SpecializationspgFaculty + "</td>";
                            else
                                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" +
                                           "</td>";

                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.form16count + "</td>";
                            //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.aictecount + "</td>";
                            if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            {
                                //if (rFaculty <= adjustedFaculty)
                                //    minimumRequirementMet = "-";//No Deficiency In faculty
                                //else
                                //    minimumRequirementMet = "Deficiency In faculty";

                                //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                //else
                                //    minimumRequirementMet += " and Deficiency In Phd";

                                if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "-";
                                }
                                else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In faculty";
                                }
                                else if (rFaculty <= adjustedFaculty &&
                                         SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In Ph.D";
                                }
                                else
                                {
                                    minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                }


                            }
                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            {
                                //if (rFaculty == adjustedFaculty)
                                //    minimumRequirementMet = "-";//No Deficiency In faculty
                                //else
                                //    minimumRequirementMet = "Deficiency In faculty";


                                //if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                //    minimumRequirementMet += "-";//and No Deficiency In Phd
                                //else
                                //    minimumRequirementMet += " and Deficiency In Phd";

                                if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "-";
                                }
                                else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In faculty";
                                }
                                else if (rFaculty <= adjustedFaculty &&
                                         SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In Ph.D";
                                }
                                else
                                {
                                    minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                }

                            }
                            else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            {
                                //if (rFaculty <= adjustedFaculty)
                                //    minimumRequirementMet = "-";//No Deficiency In faculty
                                //else
                                //    minimumRequirementMet = "Deficiency In faculty";

                                //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                //else
                                //    minimumRequirementMet += " and Deficiency In Phd";
                                if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "-";
                                }
                                else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In faculty";
                                }
                                else if (rFaculty <= adjustedFaculty &&
                                         SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In Ph.D";
                                }
                                else
                                {
                                    minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                }

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            {
                                //if (rFaculty == adjustedFaculty)
                                //    minimumRequirementMet = "-";//No Deficiency In faculty
                                //else
                                //    minimumRequirementMet = "Deficiency In faculty";


                                //if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                //    minimumRequirementMet += "-";// and No Deficiency In Phd
                                //else
                                //    minimumRequirementMet += " and Deficiency In Phd";
                                if (rFaculty <= adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "-";
                                }
                                else if (rFaculty > adjustedFaculty && SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In faculty";
                                }
                                else if (rFaculty <= adjustedFaculty &&
                                         SpecializationwisePHDFaculty > adjustedPHDFaculty)
                                {
                                    minimumRequirementMet = "Deficiency In Ph.D";
                                }
                                else
                                {
                                    minimumRequirementMet = "Deficiency In faculty and Ph.D";
                                }
                            }






                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       SpecializationwisePHDFaculty + "</td>";
                            //if (deptloop == 1)
                            //    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            //faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            //else
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       adjustedPHDFaculty + "</td>";
                            faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" +
                                       minimumRequirementMet + "</td>";
                            //if (deptloop == 1)
                            //    faculty += "<td rowspan='" + TotalCount + "' class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";



                            //else
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";






                            //if (adjustedPHDFaculty > 0)
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                            //}
                            //else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                            //}
                            //else
                            //{
                            //    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                            //}

                            faculty += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            facultyIndexnotEqualtoZeroIndex++;

                            #endregion

                            //  }
                       
                    }
                    faculty += "</table>";


                    #endregion

                    //
                    
                    faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    //if (mbacollegeides.Contains(cid))
                    //{
                    //    faculty += "<tr>";
                    //    faculty += "<td align='left'>* I & II Year for MBA</td>";
                    //    faculty += "</tr>";
                    //}
                    //else
                    //{
                        faculty += "<tr>";
                        faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech</td>";
                        faculty +=
                            "<td align='left'># Every 60 or part there of admitted is considered as one division.</td>";
                        faculty += "</tr>";
                        faculty += "<tr>";
                        faculty +=
                            "<td align='left' colspan='2'>* The department wise faculty requirement is calculated based on Admitted Intake in Multiples of 60, provided the Proposed Intake is less than or equal to Maximum Admitted Intake(in Multiples of 60) in Previous 3 Academic years, otherwise the faculty requirement is calculated based on JNTUH Sanctioned Intake, year wise.</td>";
                        //faculty += "<td align='left'># Every 60 or part there of admitted is considered as one division.</td>";
                        faculty += "</tr>";
                        faculty += "<tr>";
                        faculty += "<td align='left'>* (S): JNTUH Sanctioned Intake.</td>";
                        faculty += "<td align='left'>* (A): Admitted Intake.</td>";
                        faculty += "</tr>";
                    //}
                    faculty += "</table>";


                    if (facultyCounts.Select(e => e.Degree).Contains("B.Tech"))
                    {
                        if (Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) <=
                            HumantitiesminimamRequireMet && HumantitiesminimamRequireMetStatus != "NO")
                        {
                            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<td align='left'><b><u>Humanities Deficiency</u></b></td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                            faculty += "<br/><table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty +="<th align='left'>S.NO</th><th align='left'>Department</th><th align='left'>Total Intake</th><th align='left'>Required</th><th align='left'>Available</th><th>Deficiency</th>";
                            faculty += "</tr>";
                            faculty += "<tr>";
                            // faculty += "<td align='left'>No Deficiency in Humanities</td>";
                            faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake +"</td><td>" + Math.Ceiling(Convert.ToDecimal((totalBtechFirstYearIntake / 20))) +"</td><td>" + HumantitiesminimamRequireMet +"</td><td align='left'>No Deficiency in H&S and Others</td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                        }
                        else
                        {
                            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty += "<td align='left'><b><u>Humanities Deficiency</u></b></td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                            faculty += "<br/><table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                            faculty += "<tr>";
                            faculty +="<th align='left'>S.NO</th><th align='left'>Department</th><th align='left'>Total Intake</th><th align='left'>Required</th><th align='left'>Available</th><th>Deficiency</th>";
                            faculty += "</tr>";
                            faculty += "<tr>";
                            faculty += "<td>1</td><td align='left'>H&S and Others</td><td>" + totalBtechFirstYearIntake +"</td><td>" + (totalBtechFirstYearIntake / 20) + "</td><td>" +HumantitiesminimamRequireMet +"</td><td align='left'>Overall deficiency in H&S and Others</td>";
                            faculty += "</tr>";
                            faculty += "</table>";
                        }
                    }




                    facultyAdmittedIntakeZeroTable2 += "</table>";
                    facultyAdmittedIntakeZero += "</table>";

                    facultyAdmittedIntakeZero += "<p>Under these circumstances the SCA recommends for <b><u>'No Admission status'</u></b> for the A.Y. 2017-18 for the following programs/courses proposed by the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter.<br/><b> Programs / Courses for which <b><u>'No Admission status'</u></b>  recommended:</b></p>";

                    facultyAdmittedIntakeZero += facultyAdmittedIntakeZeroTable2;
                    if (admittedIntakeTwoormoreZeroDeptName1.Count() != 0 &&facultyIndexnotEqualtoZeroAdmittedIntakeIndex > 1)
                    {
                      //  faculty += facultyAdmittedIntakeZero;
                    }



                    if (collegeStatus != null)
                    {
                        if (collegeStatus.SIStatus == true)
                        {
                            faculty += "<p><b>Note :In so far as P.G. programs are concerned the University has issued a separate show cause notice Dt. 15.05.2017 based on Surprise Inspection Committee Reports.</b></p>";
                            //The University has conducted surprise inspection to the Institute as a part of academic audit. As per Surprise Inspection Committee report, it is observed that no academic activity is taking place at the Institute for Post Graduate (P.G.) programs. It indicates that you have no inclination for maintaining the minimum essential requirements for running the P.G. programs.</b></p><p><b> Under the circumstances and as per clause 12.12 and point 9 of 3.27 of University affiliation regulations, SCA has recommended for suspension of affiliation to P.G. programs and <br/><b><u>'No Admission status'</u></b> for all P.G. programs of your Institute for the A.Y. 2017-18
                        }
                    }


                    #region Faculty with Proposed Intake Zero and Closure

                    //if (!mbacollegeides.Contains(cid))
                    //{

                        facultyclosure +=
                            "<p>The Courses for which college has applied for Closure or Proposed Zero Intake for A.Y. 2018-19 are as follows.</p>";
                        facultyclosure +=
                            "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        facultyclosure += "<tr>";
                        facultyclosure += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                        facultyclosure += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                        facultyclosure += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                        facultyclosure += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";

                        //facultyclosure += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                        //facultyclosure += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";


                        //facultyclosure += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";

                        facultyclosure += "</tr>";


                        int ProposedIntakeZeroIndex = 1;
                        foreach (
                            var item in
                                facultyCounts.Where(
                                    e => e.approvedIntake1 == 0 || ProposedIntakeZeroDeptName.Contains(e.Specialization))
                                    .Select(e => e)
                                    .ToList())
                        {
                            // distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                            if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                            {

                                //Old code
                                //if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
                                //else if (item.Degree == "MCA")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
                                //else if (item.Degree == "MBA")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
                                //SpecializationwisePHDFaculty = SpecializationwisePHDFaculty*2;

                                if (item.Degree == "M.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1,
                                        item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }
                                else if (item.Degree == "MBA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }


                            }
                            else if (item.Degree == "B.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();



                            //if (SpecializationIDS.Contains(item.specializationId))
                            //{
                            //    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                            //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                            //}
                            int indexnow = facultyCounts.IndexOf(item);



                            if (indexnow > 0 &&
                                facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            {
                                deptloop = 1;
                            }

                            //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                            string minimumRequirementMet = string.Empty;

                            string PhdminimumRequirementMet = string.Empty;


                            int facultyShortage = 0, tFaculty = 0;
                            int adjustedFaculty = 0;
                            int adjustedPHDFaculty = 0;
                            int othersRequiredfaculty = 0;

                            if (item.Department == "MBA" || item.Department == "MCA")
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            else
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                                    //item.totalFaculty
                            int rFaculty = (int) Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    rFaculty = ((int) firstYearRequired)/2;
                                    if (rFaculty <= 2)
                                    {
                                        rFaculty = 2;
                                        othersRequiredfaculty = 2;
                                    }
                                    else
                                    {
                                        othersRequiredfaculty = rFaculty;
                                    }

                                }
                                else
                                {
                                    rFaculty = (int) firstYearRequired;
                                }

                                // departmentWiseRequiredFaculty = (int)firstYearRequired;
                            }

                            var degreeType =
                                degrees.Where(d => d.degree == item.Degree)
                                    .Select(d => d.jntuh_degree_type.degreeType)
                                    .FirstOrDefault();

                            if (deptloop == 1)
                            {
                                if (rFaculty <= tFaculty)
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    remainingFaculty = tFaculty - rFaculty;
                                    adjustedFaculty = rFaculty; //tFaculty
                                }
                                else
                                {
                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = tFaculty;
                                    // facultyShortage = rFaculty - tFaculty;
                                }

                                remainingPHDFaculty = item.phdFaculty;

                                if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    //adjustedPHDFaculty = remainingPHDFaculty;
                                    //remainingPHDFaculty = remainingPHDFaculty - 1;
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";

                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }

                            }
                            else
                            {
                                if (rFaculty <= remainingFaculty)
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
                                else
                                {



                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = remainingFaculty;
                                    facultyShortage = rFaculty - remainingFaculty;
                                    remainingFaculty = 0;
                                }
                                //  remainingPHDFaculty = item.phdFaculty;
                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    //adjustedPHDFaculty = remainingPHDFaculty;
                                    //remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }
                            }




                            facultyclosure += "<tr>";
                            facultyclosure +=
                                "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                ProposedIntakeZeroIndex + "</td>";
                            facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                              item.Department + "</td>";
                            facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                              item.Degree + "</td>";
                            facultyclosure += "<td class='col2' style='text-align: left; vertical-align: top;'>" +
                                              item.Specialization + "</td>";

                            if (departments.Contains(item.Department))
                            {

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";


                                //if (deptloop == 1)
                                //    facultyclosure += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";

                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    // facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + othersRequiredfaculty + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    //  facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }


                                if (minimumRequirementMet != "YES")
                                {
                                    HumantitiesminimamRequireMetStatus = "NO";
                                }
                            }
                            else
                            {



                                if (item.Degree == "B.Tech")
                                {
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                    //facultyclosure += "<table width='100%' border='1'  cellspacing='0' style='border-color: #ccc;'>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake2 + "(II Year)</span></td></tr>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake3 + "(III Year)</span></td></tr>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake4 + "(IV Year)</span></td></tr>";
                                    //facultyclosure += "</table></td>";



                                }
                                else
                                {
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + " </td>";
                                }


                                //  facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.approvedIntake1 + "</td>";
                                //if (deptloop == 1)
                                //    facultyclosure += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";

                                // facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
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





                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";


                            //if (item.Degree == "M.Tech")
                            //    facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                            //else
                            //    facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";


                            if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }
                            else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }






                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "' class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";



                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            facultyclosure += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            ProposedIntakeZeroIndex++;
                        }
                        facultyclosure += "</table>";
                    //}


                    faculty += facultyclosure;

                    #endregion

                    #region 25% Admitted Intake Cources
                    //The Courses for which college has applied for Closure or Zero Proposed Intake for A.Y. 2018-19 are as follows.
                    //if (
                    //    facultyCounts.Where(
                    //        e =>
                    //            e.Degree == "B.Tech" && e.ispercentage == true &&
                    //            twentyfivePersentbelowDeptName.Contains(e.Specialization)).Select(e => e).ToList().Count !=
                    //    0)
                    //{
                        //twintyfivepercentbelowcurces +=
                        //    "<p>The following Courses are under <b>&lsquo;No Admission Status&rsquo;</b> for the A.Y.2018-19 as per clause: 3.30 of Affiliation Regulations 2018-19.</p>";
                        twintyfivepercentbelowcurces +=
                            "<p><b>#</b> The following Courses have either Zero Admissions due to non-grant of Affiliation or Admitted intake is less than 25% of JNTUH Sanctioned intake for the previous three academic years.Hence SCA has recommended for <b>&lsquo;No Admission Status&rsquo;</b> for these courses for the A.Y.2018-19 as per clause: 3.30 of Affiliation Regulations 2018-19.However, if the admitted intake in any of the applied courses is atleast 15 or above, in any one of the previous three years,then such courses will be considered for Affiliation with a minimum of 60 intake provided they meet other requirements as per norms.</p>";
                        twintyfivepercentbelowcurces +=
                            "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        twintyfivepercentbelowcurces += "<tr>";
                        twintyfivepercentbelowcurces += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Department</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                        twintyfivepercentbelowcurces +=
                            "<th style='text-align: left; vertical-align: top;' >Proposed Intake</th>";
                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";


                        facultyclosure += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";

                        facultyclosure += "</tr>";


                        int ProposedIntakeZeroIndex1 = 1;
                        foreach (
                            var item in
                                facultyCounts.Where(
                                    e =>
                                        e.Degree == "B.Tech" && e.ispercentage == true &&
                                        twentyfivePersentbelowDeptName.Contains(e.Specialization))
                                    .Select(e => e)
                                    .ToList())
                        {
                            // distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();
                            if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                            {

                                //Old code
                                //if (item.Degree == "M.Tech" || item.Degree == "B.Tech")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "M.Tech").Distinct().Count();
                                //else if (item.Degree == "MCA")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MCA").Distinct().Count();
                                //else if (item.Degree == "MBA")
                                //    SpecializationwisePHDFaculty =facultyCounts.Where(D => D.Department == item.Department && D.Degree == "MBA").Distinct().Count();
                                //SpecializationwisePHDFaculty = SpecializationwisePHDFaculty*2;

                                if (item.Degree == "M.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1,
                                        item.shiftId);
                                }
                                else if (item.Degree == "MCA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }
                                else if (item.Degree == "MBA")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                }


                            }
                            else if (item.Degree == "B.Tech")
                            {
                                SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                            }
                            TotalCount =
                                facultyCounts.Where(
                                    D =>
                                        D.Department == item.Department &&
                                        (D.Degree == "M.Tech" || D.Degree == "B.Tech")).Distinct().Count();



                            //if (SpecializationIDS.Contains(item.specializationId))
                            //{
                            //    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                            //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                            //}
                            int indexnow = facultyCounts.IndexOf(item);



                            if (indexnow > 0 &&
                                facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                            {
                                deptloop = 1;
                            }

                            //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                            string minimumRequirementMet = string.Empty;

                            string PhdminimumRequirementMet = string.Empty;


                            int facultyShortage = 0, tFaculty = 0;
                            int adjustedFaculty = 0;
                            int adjustedPHDFaculty = 0;
                            int othersRequiredfaculty = 0;

                            if (item.Department == "MBA" || item.Department == "MCA")
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                            else
                                tFaculty = (int) Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                            //item.totalFaculty
                            int rFaculty = (int) Math.Ceiling(item.requiredFaculty);

                            if (departments.Contains(item.Department))
                            {
                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    rFaculty = ((int) firstYearRequired)/2;
                                    if (rFaculty <= 2)
                                    {
                                        rFaculty = 2;
                                        othersRequiredfaculty = 2;
                                    }
                                    else
                                    {
                                        othersRequiredfaculty = rFaculty;
                                    }

                                }
                                else
                                {
                                    rFaculty = (int) firstYearRequired;
                                }

                                // departmentWiseRequiredFaculty = (int)firstYearRequired;
                            }

                            var degreeType =
                                degrees.Where(d => d.degree == item.Degree)
                                    .Select(d => d.jntuh_degree_type.degreeType)
                                    .FirstOrDefault();

                            if (deptloop == 1)
                            {
                                if (rFaculty <= tFaculty)
                                {
                                    minimumRequirementMet = "YES";
                                    item.deficiency = false;
                                    remainingFaculty = tFaculty - rFaculty;
                                    adjustedFaculty = rFaculty; //tFaculty
                                }
                                else
                                {
                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = tFaculty;
                                    // facultyShortage = rFaculty - tFaculty;
                                }

                                remainingPHDFaculty = item.phdFaculty;

                                if (remainingPHDFaculty > 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    //adjustedPHDFaculty = remainingPHDFaculty;
                                    //remainingPHDFaculty = remainingPHDFaculty - 1;
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";

                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }

                            }
                            else
                            {
                                if (rFaculty <= remainingFaculty)
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
                                else
                                {



                                    minimumRequirementMet = "NO";
                                    item.deficiency = true;
                                    adjustedFaculty = remainingFaculty;
                                    facultyShortage = rFaculty - remainingFaculty;
                                    remainingFaculty = 0;
                                }
                                //  remainingPHDFaculty = item.phdFaculty;
                                if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                    item.SpecializationsphdFaculty > 0)
                                {
                                    remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                    adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                    //adjustedPHDFaculty = remainingPHDFaculty;
                                    //remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "YES";
                                }
                                else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                         item.SpecializationsphdFaculty > 0)
                                {

                                    adjustedPHDFaculty = remainingPHDFaculty;

                                    remainingPHDFaculty = remainingPHDFaculty - 1;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty > 0)
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
                                else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                         remainingPHDFaculty <= 0)
                                {
                                    // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                    adjustedPHDFaculty = remainingPHDFaculty;
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                {
                                    PhdminimumRequirementMet = "NO";
                                }
                                else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                {
                                    PhdminimumRequirementMet = "YES";
                                }
                            }




                            twintyfivepercentbelowcurces += "<tr>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +
                                ProposedIntakeZeroIndex1 + "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                                "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +
                                "</td>";
                            twintyfivepercentbelowcurces +=
                               "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.approvedIntake1 +
                               "</td>";

                            if (departments.Contains(item.Department))
                            {

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";

                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";


                                //if (deptloop == 1)
                                //    facultyclosure += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";

                                if (OthersSpecIds.Contains(item.specializationId))
                                {
                                    // facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + othersRequiredfaculty + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }
                                else
                                {
                                    //  facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                                    HumantitiesminimamRequireMet += item.totalFaculty;
                                }


                                if (minimumRequirementMet != "YES")
                                {
                                    HumantitiesminimamRequireMetStatus = "NO";
                                }
                            }
                            else
                            {



                                if (item.Degree == "B.Tech")
                                {
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalAdmittedIntake + "</td>";
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>";
                                    //facultyclosure += "<table width='100%' border='1'  cellspacing='0' style='border-color: #ccc;'>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake2 + "(II Year)</span></td></tr>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake3 + "(III Year)</span></td></tr>";
                                    //facultyclosure += "<tr><td><span>" + item.approvedIntake4 + "(IV Year)</span></td></tr>";
                                    //facultyclosure += "</table></td>";



                                }
                                else
                                {
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                                    //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + " </td>";
                                }


                                //  facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.approvedIntake1 + "</td>";
                                //if (deptloop == 1)
                                //    facultyclosure += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";

                                // facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
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





                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";


                            //if (item.Degree == "M.Tech")
                            //    facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                            //else
                            //    facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";


                            if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }
                            else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty <= adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";

                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";

                            }

                            else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                            {
                                if (rFaculty == adjustedFaculty)
                                    minimumRequirementMet = "No Deficiency In faculty";
                                else
                                    minimumRequirementMet = "Deficiency In faculty";


                                if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                    minimumRequirementMet += " and No Deficiency In Phd";
                                else
                                    minimumRequirementMet += " and Deficiency In Phd";
                            }






                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////faculty += "<td   class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";

                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                            //facultyclosure += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                            ////if (deptloop == 1)
                            ////    faculty += "<td rowspan='" + TotalCount + "' class='col2' style='text-align: center; vertical-align: top;'>" + item.phdFaculty + "</td>";



                            ////else
                            ////    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";

                            twintyfivepercentbelowcurces += "</tr>";

                            deptloop++;
                            SpecializationwisePHDFaculty = 0;
                            ProposedIntakeZeroIndex1++;
                        }
                        twintyfivepercentbelowcurces += "</table>";



                        faculty += twintyfivepercentbelowcurces;
                    //}

                    #endregion
                   
                }
                else
                {
                   
                    if (collegeStatus.CollegeStatus == true)
                    {
                        faculty += "<p>As per the records the programs/courses in which there were zero admissions or no grant of affiliation by the University in the previous three academic years are shown in the following table. It is observed that for two or more than two academic years there were zero admissions in all the programs/courses either due to non grant of affiliation or unable to make admissions at your Institute. As per the records it is also observed that there is a mismatch of faculty information that you have submitted to AICTE and the University. </p>";
                        string[] admittedIntakeTwoormoreZeroDeptName1 = facultyCounts.Where(e => e.Degree == "B.Tech" && ((e.approvedIntake2 == 0 && e.approvedIntake3 == 0) || (e.approvedIntake3 == 0 && e.approvedIntake4 == 0) || (e.approvedIntake2 == 0 && e.approvedIntake4 == 0)) && !departments.Contains(e.Department)).Select(e => e.Department).Distinct().ToArray();
                        facultyAdmittedIntakeZero += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
                        facultyAdmittedIntakeZero += "<tr>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Department</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Sanction Intake</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Admitted Intake</th>";
                        facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Proposed Intake</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2016-17</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2015-16</th>";
                        //facultyAdmittedIntakeZero += "<th style='text-align: center; vertical-align: top;' >Affiliation Status 2014-15</th>";
                        facultyAdmittedIntakeZero += "</tr>";

                        int facultyIndexnotEqualtoZeroAdmittedIntakeIndex = 1;
                        foreach (var item in facultyCounts.Where(e => e.Degree == "B.Tech" &&e.ispercentage==false&& !departments.Contains(e.Department) && e.approvedIntake1 != 0 && !ProposedIntakeZeroDeptName.Contains(e.Specialization)).Select(e => e).ToList())//e.approvedIntake1 != 0 && !ProposedIntakeZeroDeptName.Contains(e.Department) &&
                        {



                           // if (((item.approvedIntake2 == 0 && item.approvedIntake3 == 0) && (item.approvedIntake3 == 0 && item.approvedIntake4 == 0) && (item.approvedIntake2 == 0 && item.approvedIntake4 == 0) || admittedIntakeTwoormoreZeroDeptName1.Contains(item.Department)) && !departments.Contains(item.Department))
                           // {

                                #region Admitted Intake With Zero

                                if (item.Degree == "M.Tech" || item.Degree == "MCA" || item.Degree == "MBA")
                                {

                                    if (item.Degree == "M.Tech")
                                    {
                                        SpecializationwisePHDFaculty = IntakeWisePhdForMtech(item.approvedIntake1,item.shiftId);
                                    }
                                    else if (item.Degree == "MCA")
                                    {
                                        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                    }
                                    else if (item.Degree == "MBA")
                                    {
                                        SpecializationwisePHDFaculty = IntakeWisePhdForMBAandMCA(item.approvedIntake1);
                                    }


                                }
                                else if (item.Degree == "B.Tech")
                                {
                                    SpecializationwisePHDFaculty = IntakeWisePhdForBtech(item.approvedIntake1);
                                }
                                TotalCount =
                                    facultyCounts.Where(
                                        D =>
                                            D.Department == item.Department &&
                                            (D.Degree == "M.Tech" || D.Degree == "B.Tech") && D.approvedIntake1 != 0)
                                        .Distinct()
                                        .Count();



                                //if (SpecializationIDS.Contains(item.specializationId))
                                //{
                                //    int SpecializationwisePGFaculty1 = facultyCounts.Count(S => S.specializationId==item.specializationId);
                                //    SpecializationwisePGFaculty = facultyCounts.Where(S => S.specializationId==item.specializationId).Select(S => S.SpecializationspgFaculty).FirstOrDefault();

                                //}
                                int indexnow = facultyCounts.IndexOf(item);



                                if (indexnow > 0 &&
                                    facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                                {
                                    deptloop = 1;
                                }

                                //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                                string minimumRequirementMet = string.Empty;

                                string PhdminimumRequirementMet = string.Empty;


                                int facultyShortage = 0, tFaculty = 0;
                                int adjustedFaculty = 0;
                                int adjustedPHDFaculty = 0;
                                int othersRequiredfaculty = 0;

                                if (item.Department == "MBA" || item.Department == "MCA")
                                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty)); //item.totalFaculty
                                else
                                    tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.specializationWiseFaculty));
                                //item.totalFaculty
                                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                                if (departments.Contains(item.Department))
                                {
                                    if (OthersSpecIds.Contains(item.specializationId))
                                    {
                                        rFaculty = ((int)firstYearRequired) / 2;
                                        if (rFaculty <= 2)
                                        {
                                            rFaculty = 2;
                                            othersRequiredfaculty = 2;
                                        }
                                        else
                                        {
                                            othersRequiredfaculty = rFaculty;
                                        }

                                    }
                                    else
                                    {
                                        rFaculty = (int)firstYearRequired;
                                    }

                                    // departmentWiseRequiredFaculty = (int)firstYearRequired;
                                }

                                var degreeType =
                                    degrees.Where(d => d.degree == item.Degree)
                                        .Select(d => d.jntuh_degree_type.degreeType)
                                        .FirstOrDefault();

                                if (deptloop == 1)
                                {
                                    if (rFaculty <= tFaculty)
                                    {
                                        minimumRequirementMet = "YES";
                                        item.deficiency = false;
                                        remainingFaculty = tFaculty - rFaculty;
                                        adjustedFaculty = rFaculty; //tFaculty
                                    }
                                    else
                                    {
                                        minimumRequirementMet = "NO";
                                        item.deficiency = true;
                                        adjustedFaculty = tFaculty;
                                        // facultyShortage = rFaculty - tFaculty;
                                    }

                                    remainingPHDFaculty = item.phdFaculty;

                                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                        item.SpecializationsphdFaculty > 0)
                                    {

                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                             item.SpecializationsphdFaculty > 0)
                                    {
                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        remainingPHDFaculty = remainingPHDFaculty - 1;
                                        PhdminimumRequirementMet = "NO";
                                    }
                                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                             remainingPHDFaculty > 0)
                                    {

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
                                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                             remainingPHDFaculty <= 0)
                                    {
                                        // remainingPHDFaculty = SpecializationwisePHDFaculty - remainingPHDFaculty;
                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        PhdminimumRequirementMet = "NO";

                                    }
                                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                    {
                                        PhdminimumRequirementMet = "NO";
                                    }
                                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                    {
                                        PhdminimumRequirementMet = "YES";
                                    }

                                }
                                else
                                {
                                    if (rFaculty <= remainingFaculty)
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
                                    else
                                    {



                                        minimumRequirementMet = "NO";
                                        item.deficiency = true;
                                        adjustedFaculty = remainingFaculty;
                                        facultyShortage = rFaculty - remainingFaculty;
                                        remainingFaculty = 0;
                                    }

                                    if (remainingPHDFaculty >= 2 && (degreeType.Equals("PG")) &&
                                        item.SpecializationsphdFaculty > 0)
                                    {
                                        remainingPHDFaculty = remainingPHDFaculty - item.SpecializationsphdFaculty;
                                        adjustedPHDFaculty = item.SpecializationsphdFaculty;
                                        PhdminimumRequirementMet = "YES";
                                    }
                                    else if (remainingPHDFaculty <= 1 && (degreeType.Equals("PG")) &&
                                             item.SpecializationsphdFaculty > 0)
                                    {
                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        remainingPHDFaculty = remainingPHDFaculty - 1;
                                        PhdminimumRequirementMet = "NO";
                                    }
                                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                             remainingPHDFaculty > 0)
                                    {

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
                                    else if (SpecializationwisePHDFaculty > 0 && (degreeType.Equals("UG")) &&
                                             remainingPHDFaculty <= 0)
                                    {

                                        adjustedPHDFaculty = remainingPHDFaculty;
                                        PhdminimumRequirementMet = "NO";
                                    }
                                    else if (SpecializationwisePHDFaculty == 0 && (degreeType.Equals("UG")))
                                    {
                                        PhdminimumRequirementMet = "NO";
                                    }
                                    else if (adjustedPHDFaculty <= 0 && (degreeType.Equals("PG")))
                                    {
                                        PhdminimumRequirementMet = "YES";
                                    }
                                }




                                facultyAdmittedIntakeZero += "<tr>";
                                facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" +facultyIndexnotEqualtoZeroAdmittedIntakeIndex + "</td>";
                                //(facultyCounts.Where(e => e.approvedIntake1 != 0).Select(e => e).ToList().IndexOf(item) + 1) 
                                facultyAdmittedIntakeZero +="<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department +"</td>";
                                facultyAdmittedIntakeZero +="<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree +"</td>";
                                facultyAdmittedIntakeZero +="<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization +"</td>";



                              





                                if (departments.Contains(item.Department))
                                {
                                    // faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'> </td>";
                                    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +totalBtechFirstYearIntake + "</td>";


                                    if (deptloop == 1)
                                        facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount +"'  class='col2' style='text-align: center; vertical-align: top;'>" +item.totalFaculty + "</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    if (OthersSpecIds.Contains(item.specializationId))
                                    {
                                        facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +othersRequiredfaculty + "</td>";
                                        HumantitiesminimamRequireMet += item.totalFaculty;
                                    }
                                    else
                                    {
                                        facultyAdmittedIntakeZero +=
                                            "<td class='col2' style='text-align: center; vertical-align: top;'>" +Math.Ceiling(firstYearRequired) + "</td>";
                                        HumantitiesminimamRequireMet += item.totalFaculty;
                                    }

                                    //
                                    if (minimumRequirementMet != "YES")
                                    {
                                        HumantitiesminimamRequireMetStatus = "NO";
                                    }
                                }
                                else
                                {
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;font-weight: bold'>" + item.AffliationStatus + "</td>";


                                    if (item.Degree == "B.Tech")
                                    {
                                        facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +item.totalAdmittedIntake + "</td>";

                                        facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>";
                                        facultyAdmittedIntakeZero +="<table width='100%' border='1'  cellspacing='0' style='border-color: #ccc;'>";
                                        if (item.approvedIntake2 == 0)
                                            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake2 +"(II Year)</span></td></tr>";
                                        if (item.approvedIntake3 == 0)
                                            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake3 +"(III Year)</span></td></tr>";
                                        if (item.approvedIntake4 == 0)
                                            facultyAdmittedIntakeZero += "<tr><td><span>" + item.approvedIntake4 +"(IV Year)</span></td></tr>";
                                        facultyAdmittedIntakeZero += "</table></td>";
                                    }
                                    else
                                    {
                                        facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +item.totalIntake + "</td>";
                                        facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +item.totalIntake + " </td>";
                                    }
                                    //faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>;
                                    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" +item.approvedIntake1 + "</td>";
                                    // if (deptloop == 1)
                                    //  facultyAdmittedIntakeZero += "<td rowspan='" + TotalCount + "'  class='col2' style='text-align: center; vertical-align: top;'>" + item.totalFaculty + "</td>";
                                    //faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                                    //  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
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

                                // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";


                                //if (item.Degree == "M.Tech")
                                //    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.SpecializationspgFaculty + "</td>";
                                //else
                                //    facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + "" + "</td>";
                                //if (item.AffiliationStatus2 == true)
                                //  facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                                //else
                                //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";


                                if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "YES")
                                {
                                    if (rFaculty <= adjustedFaculty)
                                        minimumRequirementMet = "No Deficiency In faculty";
                                    else
                                        minimumRequirementMet = "Deficiency In faculty";

                                    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                        minimumRequirementMet += " and No Deficiency In Phd";
                                    else
                                        minimumRequirementMet += " and Deficiency In Phd";

                                }

                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "YES")
                                {
                                    if (rFaculty == adjustedFaculty)
                                        minimumRequirementMet = "No Deficiency In faculty";
                                    else
                                        minimumRequirementMet = "Deficiency In faculty";


                                    if (SpecializationwisePHDFaculty <= adjustedPHDFaculty)
                                        minimumRequirementMet += " and No Deficiency In Phd";
                                    else
                                        minimumRequirementMet += " and Deficiency In Phd";
                                }
                                else if (minimumRequirementMet == "YES" && PhdminimumRequirementMet == "NO")
                                {
                                    if (rFaculty <= adjustedFaculty)
                                        minimumRequirementMet = "No Deficiency In faculty";
                                    else
                                        minimumRequirementMet = "Deficiency In faculty";

                                    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                        minimumRequirementMet += " and No Deficiency In Phd";
                                    else
                                        minimumRequirementMet += " and Deficiency In Phd";

                                }

                                else if (minimumRequirementMet == "NO" && PhdminimumRequirementMet == "NO")
                                {
                                    if (rFaculty == adjustedFaculty)
                                        minimumRequirementMet = "No Deficiency In faculty";
                                    else
                                        minimumRequirementMet = "Deficiency In faculty";


                                    if (SpecializationwisePHDFaculty == adjustedPHDFaculty)
                                        minimumRequirementMet += " and No Deficiency In Phd";
                                    else
                                        minimumRequirementMet += " and Deficiency In Phd";
                                }






                                // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + SpecializationwisePHDFaculty + "</td>";
                                //if (item.AffiliationStatus3 == true)
                                //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                                //else
                                //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";

                                // facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedPHDFaculty + "</td>";
                                //if (item.AffiliationStatus4 == true)
                                //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>Yes</td>";
                                //else
                                //    facultyAdmittedIntakeZero +="<td class='col2' style='text-align: center; vertical-align: top;'>" + "No" +"</td>";
                                //  facultyAdmittedIntakeZero += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";


                                facultyAdmittedIntakeZero += "</tr>";
                              //  facultyAdmittedIntakeZeroTable2 += "</tr>";
                                deptloop++;
                                SpecializationwisePHDFaculty = 0;
                                facultyIndexnotEqualtoZeroAdmittedIntakeIndex++;

                                #endregion

                           // }

                        }

                        facultyAdmittedIntakeZero += "</table>";
                        faculty += facultyAdmittedIntakeZero;
                    }
                }
            }

          


            //if (ProposedIntakeZeroDeptName.Count() != 0)
            //{

            //}

            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == true)
                {
                    faculty += "<p>Under these circumstances the SCA recommends for <b><u>'No Admission status'</u></b> for the A.Y. 2017-18 for the Institute. Clarification or explanation if any may be submitted within 10 days from the date of this letter.</p>";
                    faculty += "<table width='100%'  cellspacing='0'>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    faculty += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr>";
                    faculty += "</table>";
                }
            }






            var collegeFacultycount = 0;
            string[] collegefacultyRegistrationNo = db.jntuh_college_faculty_registered.AsNoTracking().Where(e => e.collegeId == collegeID).Select(e => e.RegistrationNumber).ToArray();

            var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeID).ToList();
            var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

            var registeredFaculty = db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            //principalRegno != null ? db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList(): db.jntuh_registered_faculty.Where(rf => collegefacultyRegistrationNo.Contains(rf.RegistrationNumber.Trim())).ToList();
            var jntuh_registered_faculty1 =
                    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.OriginalCertificatesNotShown == false) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.BAS != "Yes" && rf.InvalidAadhaar != "Yes")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27))
                //  .Where(rf => rf.DepartmentId != null && ((rf.Absent != true) && (rf.NotQualifiedAsperAICTE != true)&& (rf.NoSCM != true) && (rf.PANNumber != null) && rf.Notin116 != true && rf.Blacklistfaculy != true && rf.PHDundertakingnotsubmitted != true) && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.type != "Adjunct")
                             .Select(rf => new
                        {
                            //Departmentid = rf.DepartmentId,
                            RegistrationNumber = rf.RegistrationNumber,
                           // Department = rf.jntuh_department.departmentName,
                            HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                            IsApproved = rf.isApproved,
                            PanNumber = rf.PANNumber,
                            AadhaarNumber = rf.AadhaarNumber,
                            NoForm16 = rf.NoForm16,
                            TotalExperience = rf.TotalExperience
                        }).ToList();
            jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();

            collegeFacultycount = jntuh_registered_faculty1.Count;



            var lastyearfacultycount = db.jntuh_notin415faculty.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();



            int count1 = 0;
            //var nodocumentsdetails =db.jntuh_deficiencyrepoprt_college_pendingdocuments.Where(i => i.CollegeId == collegeID).Select(i => i).FirstOrDefault();
            var TotalAictecount = db.jntuh_college_aictefaculty.Where(e => e.CollegeId == collegeID).ToList().Count();
            string[] regnos = db.jntuh_college_faculty_registered.Where(c => c.collegeId == collegeID).Select(s=>s.RegistrationNumber).Distinct().ToArray();
            var ToatalFacultyon418 =
                db.jntuh_registered_faculty.Where(r => regnos.Contains(r.RegistrationNumber)&&r.Absent==false).ToList().Distinct().Count();
            var AffiliationFee =db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID&&e.FeeTypeID==5).Select(e => e).FirstOrDefault();
            var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == collegeID && e.FeeTypeID == 3).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {
                    //PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:


                    List<int?> NoBAS = new List<int?> { 2, 59, 247, 415, 419, 239, 447 };



                    #region PENDING SUPPORTING DOCUMENTS FOR ONLINE  DATA:

                    faculty += "<br/><table><tr><td align='left'><b><u>Pending Issues:</u></b></td>";
                    faculty += "</tr></table>";
                    faculty += "<ul style='list-style-type:disc'>";
                    if (NoBAS.Contains(collegeID))
                    {
                        faculty += "<li>Biometric Attendance System not implemented.</li>";
                    }

                    if (AffiliationFee != null)
                    {
                        if (CommanserviceFee.paidAmount != null)
                        {
                            faculty += "<li>Common Service Fee Due:<b> Rs." + CommanserviceFee.paidAmount + "</b></li>";
                        }

                        if (AffiliationFee.duesAmount != null)
                        {
                            faculty += "<li>Affiliation Fee Due: <b>Rs." + AffiliationFee.duesAmount + "</b></li>";
                        }
                    }
                    
                   
                    //if (nodocumentsdetails != null && nodocumentsdetails.Antiraging == true)
                    //{
                    //    faculty += "<li>Antiragging Committee Details.</li>";

                    //}
                    //if (nodocumentsdetails != null && nodocumentsdetails.AuditedStatement == true)
                    //{
                    //    faculty += "<li>Audited Statement.</li>";
                    //}
                    //if (nodocumentsdetails != null && nodocumentsdetails.LandUsedCertificate == true)
                    //{
                    //    faculty += "<li>Land Use Certificate.</li>";
                    //}

                    faculty += "</ul>";

                    #endregion

                    //faculty +="<p><b>Note:-</b>Affiliattion and Common Service Fee Due Subject to Verification</p>";
                    //OTHER OBSERVATIONS/  REMARKS

                    #region OTHER OBSERVATIONS/  REMARKS

                    int Count2 = 0;

                    faculty += "<table><tr><td align='left'><b><u>Other Observations/ Remarks:</u></b></td>";
                    faculty += "</tr></table>";
                    faculty += "<ul style='list-style-type:disc'>";


                    //if (nodocumentsdetails != null && nodocumentsdetails.FFCTeamComments != "")
                    //{
                    //    faculty += "<li>" + nodocumentsdetails.FFCTeamComments + "</li>";

                    //}
                    if (TotalAictecount != null && TotalAictecount != 0)
                    {

                        faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + TotalAictecount + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                        faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";
                    }
                    else
                    {
                        faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : 0.</li>";
                        faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";

                    }
                    if (lastyearfacultycount != null)
                    {

                        faculty +="<li>Number of faculty recruited after the last inspection  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " +lastyearfacultycount.RegistrationNumber + ".</li>";
                    }
                    else
                    {

                        faculty +="<li>Number of faculty recruited after the last inspection &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   :0. </li>";
                        //Total Available Faculty is " + collegeFacultycount + ".
                    }

                    //if (nodocumentsdetails != null && nodocumentsdetails.AICTENoOfFaculty != 0)
                    //{

                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : " + nodocumentsdetails.AICTENoOfFaculty + ".</li>";//facultyCounts.Select(i => i.specializationWiseFaculty).Sum()
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";
                    //}
                    //else
                    //{
                    //    faculty += "<li>Total faculty uploaded to AICTE for Extension of Approval for AY 2018-19 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; : 0.</li>";
                    //    faculty += "<li>Total faculty available during inspection with  qualifications as prescribed by AICTE &nbsp;&nbsp;&nbsp;:<b> " + collegeFacultycount + "</b>.</li>";

                    //}
                   


                    int currentyearfaculty = 0;
                    //if (nodocumentsdetails != null && nodocumentsdetails.Currentyearfaculty != 0)
                    //{
                    // //   currentyearfaculty = (int)nodocumentsdetails.Currentyearfaculty;
                    //}

                    //  if (nodocumentsdetails != null && nodocumentsdetails.Lastyearfaculty != 0)



                    faculty += "</ul>";

                    #endregion

                }
            }




            return faculty;
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

        private int IntakeWisePhdForMtech(int Intake,int shiftid)
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
         
            if (Intake >= 0 && Intake <= 60)
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

        public string DeficienciesInLabs(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratories</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();


            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
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
            foreach (var item in deficiencies)
            {

                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";

                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
                string[] labcodes = db.jntuh_college_laboratories_deficiency.Where(d => d.CollegeId == (int)collegeID && d.Deficiency == true).Select(d => d.LabCode).ToArray();
                //Hospital & Clinical Pharmacy
                var aa = labsCount.Where(l => l.specializationName == "Hospital & Clinical Pharmacy").ToList();
                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).Distinct().ToList();

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
                        //int[] labmasterids=0;
                        //if (CollegeAffiliationStatus == "YES")
                        //{
                        //     labmasterids= labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        //}
                        //else if (CollegeAffiliationStatus == "NO" || CollegeAffiliationStatus == null)
                        //{
                        //     labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        //}
                        int[] labmasterids = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).Distinct().ToArray();
                        int[] collegelabequipmentids = jntuh_college_laboratories.Where(i => labmasterids.Contains(i.EquipmentID) && i.EquipmentNo == 1).Select(i => i.id).Distinct().ToArray();



                        //  string[] labcodes = labMaster.Where(m => m.SpecializationID == specializationid).Select(m => m.Labcode).ToArray();
                        if (requiredCount > availableCount && labmasterids.Count() != collegelabequipmentids.Count())
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


            }

            labs += "</table>";

            return labs;
        }
        //public string DeficienciesInLabs(int? collegeID)
        //{
        //    string labs = string.Empty;

        //    labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
        //    labs += "<tr>";
        //    labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
        //    labs += "</tr>";
        //    labs += "</table>";

        //    List<Lab> labsCount = collegeLabs(collegeID);

        //    var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
        //                                .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
        //                                .ToList();

        //    labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
        //    labs += "<tr>";
        //    labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
        //    labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
        //    labs += "<th style='text-align: left; width: 10%;'>Department</th>";
        //    labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
        //    labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
        //    labs += "</tr>";

        //    var labMaster = db.jntuh_lab_master.ToList();
        //    var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();

        //    foreach (var item in deficiencies)
        //    {

        //        labs += "<tr>";
        //        labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
        //        labs += "<td style=''>" + item.degree + "</td>";
        //        labs += "<td style=''>" + item.department + "</td>";
        //        labs += "<td style=''>" + item.specializationName + "</td>";

        //        string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();
        //        string[] labcodes = db.jntuh_college_laboratories_deficiency.Where(d => d.CollegeId == (int)collegeID && d.Deficiency == true).Select(d => d.LabCode).ToArray();
        //        //Hospital & Clinical Pharmacy
        //        var aa = labsCount.Where(l => l.specializationName == "Hospital & Clinical Pharmacy").ToList();
        //        var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
        //            .Select(l => new { Deficiency = l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();

        //        List<string> defs = new List<string>();
        //        labsWithDeficiency.ForEach(l =>
        //        {
        //            if (l.Equals("No Equipement Uploaded"))
        //            {
        //                defs.Add(l);
        //            }
        //            else
        //            {
        //                string[] strLab = l.Split('-');

        //                int specializationid = Convert.ToInt32(strLab[3]);
        //                int year = Convert.ToInt32(strLab[0]);
        //                int semester = Convert.ToInt32(strLab[1]);
        //                string labCode = strLab[2].Replace("$", "-");


        //                var requiredLabs = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode ).Select(m => m.id).ToList();
        //                int requiredCount = requiredLabs.Count();
        //                int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();
        //              //  string[] labcodes = labMaster.Where(m => m.SpecializationID == specializationid).Select(m => m.Labcode).ToArray();
        //                if (requiredCount > availableCount)
        //                {
        //                    string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
        //                    defs.Add(year + "-" + semester + "-" + labName);
        //                }
        //            }
        //        });

        //        labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs))) + "</td>";
        //        labs += "</tr>";
        //    }

        //    labs += "</table>";

        //    return labs;
        //}

        //FACULTY
        public List<CollegeFacultyWithIntakeReport> collegeFaculty(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            var jntuh_departments = db.jntuh_department.ToList();
            int studentcount = Convert.ToInt32(ConfigurationManager.AppSettings["studentcount"]); ;
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
                var jntuh_faculty_student_ratio_norms =
                    db.jntuh_faculty_student_ratio_norms.Where(f => f.isActive == true).ToList();
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
                    newIntake.degreeDisplayOrder =item.jntuh_specialization.jntuh_department.jntuh_degree.degreeDisplayOrder;
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = item.jntuh_shift.shiftName;
                    newIntake.courseStatus = item.courseStatus;
                    collegeIntakeExisting.Add(newIntake);
                }
                var collegedepts = collegeIntakeExisting.Select(i => i.DepartmentID).Distinct().ToList();

                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();


                var DeptNameBasedOnSpecialization = (from a in db.jntuh_department
                                                     join b in db.jntuh_specialization on a.id equals b.departmentId
                                                     select new
                                                     {
                                                         DeptId = a.id,
                                                         DeptName = a.departmentName,
                                                         //SpecName = b.specializationName,
                                                         Specid = b.id
                                                     }).ToList();





                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId&&cf.DepartmentId!=null).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber.Trim()).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == collegeId).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim()) && rf.RegistrationNumber != principalRegno).ToList(): db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();
                //  var reg1=registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var collegeaffliations = db.jntuh_college_affliationstatus.AsNoTracking().ToList();
                var regfacultywithdepts = registeredFaculty.Where(rf => rf.DepartmentId == null).ToList();
                //02-04-2018 BASStatus Columns consider as-AadhaarFlag;BASStatusOld column Consider as -Basflag
                var jntuh_registered_faculty1 =
                    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false || rf.Absent == null) && (rf.OriginalCertificatesNotShown == false || rf.OriginalCertificatesNotShown==null) && (rf.Xeroxcopyofcertificates == false || rf.Xeroxcopyofcertificates == null) && (rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                        && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false || rf.IncompleteCertificates == null) && (rf.Blacklistfaculy == false) && rf.NoRelevantUG != "Yes" && rf.NoRelevantPG != "Yes" && rf.NORelevantPHD != "Yes" && (rf.NotIdentityfiedForanyProgram == false || rf.NotIdentityfiedForanyProgram == null) && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.OriginalsVerifiedPHD != true) && (rf.OriginalsVerifiedUG != true) && (rf.Invaliddegree == false || rf.Invaliddegree == null) && rf.BAS != "Yes") && rf.InvalidAadhaar != "Yes" && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                                                        {
                                                            //Departmentid = rf.DepartmentId,
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
                                                        }).ToList();
                //BAS Flag
                int variable = 2;
               
                //foreach (var bas in jntuh_registered_faculty1)
                //{
                //    List<jntuh_college_basreport> jntuh_college_basreport =
                //        db.jntuh_college_basreport.Where(a => a.RegistrationNumber == bas.RegistrationNumber && a.collegeId == collegeId)
                //            .Select(s => s)
                //            .ToList();
                //    string[] months = { "July", "August", "September", "October", "November", "December", "January","February"};
                //    foreach (var month in months)
                //    {
                //        int totalworkingdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s=>s.totalworkingDays).FirstOrDefault();
                //        int presentdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s => s.NoofPresentDays).FirstOrDefault();
                //        int requiedpresentdays = totalworkingdays - variable;
                //    }

                //}
                //var jntuh_registered_faculty1 =
                //    registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                //                                        && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.BASStatusOld == "Y")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new
                //                                          {
                //                                              //Departmentid = rf.DepartmentId,
                //                                              RegistrationNumber = rf.RegistrationNumber.Trim(),
                //                                              // Department = rf.jntuh_department.departmentName,
                //                                              DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                //                                              SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //                                              HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                //                                              IsApproved = rf.isApproved,
                //                                              PanNumber = rf.PANNumber,
                //                                              AadhaarNumber = rf.AadhaarNumber,
                //                                              NoForm16 = rf.NoForm16,
                //                                              TotalExperience = rf.TotalExperience
                //                                          }).ToList();
                // var reg11 = registeredFaculty.Where(f => f.RegistrationNumber.Trim() == "9251-150414-062519").ToList();
                var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID < 4).ToList();
                jntuh_registered_faculty1 = jntuh_registered_faculty1.Where(e => e.HighestDegreeID >= 4).ToList();
                int fcount = jntuh_registered_faculty1.Count();
                var jntuh_registered_faculty = jntuh_registered_faculty1.Select(rf => new
                {
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                    //Specialization = rf.DeptId != null ? jntuh_specialization.Where(e => e.departmentId == rf.DeptId).Select(e => e.specializationName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.SpecName).FirstOrDefault(),
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    rf.SpecializationId,
                    // jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    noform16 = rf.NoForm16,
                    TotalExperience = rf.TotalExperience
                }).Where(e => e.Department != null).ToList();
                var form16Count = registeredFaculty.Where(i => i.NoForm16 == true).ToList();
                var aictecount = registeredFaculty.Where(i => i.NotQualifiedAsperAICTE == true).ToList();
                int[] StrPharmacy = new[] { 26, 27, 36, 39 };
                foreach (var item in collegeIntakeExisting.Where(D => !StrPharmacy.Contains(D.DepartmentID)).ToList())
                {
                    var intakedetails = new CollegeFacultyWithIntakeReport();
                    intakedetails.ispercentage = false;
                    intakedetails.isstarcourses = false;
                    int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationphdFaculty = 0; int SpecializationpgFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    item.courseStatus = db.jntuh_college_intake_existing.Where(
                        e =>
                            e.specializationId == item.specializationId && e.shiftId == item.shiftId &&
                            e.academicYearId == 10 &&
                            e.collegeId == item.collegeId).Select(s => s.courseStatus).FirstOrDefault();
                    if (item.Specialization == "Industrial Pharmacy")
                    {

                    }
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

                    intakedetails.AffiliationStatus2 = GetAcademicYear(item.collegeId, AY1, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus3 = GetAcademicYear(item.collegeId, AY2, item.specializationId, item.shiftId, item.degreeID);
                    intakedetails.AffiliationStatus4 = GetAcademicYear(item.collegeId, AY3, item.specializationId, item.shiftId, item.degreeID);

                    //Get sanction Inake for Btech
                    if (item.degreeID == 4)
                    {
                        intakedetails.SanctionIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 0, item.degreeID);
                        intakedetails.SanctionIntake5 = GetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 0, item.degreeID);
                    }


                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == item.DepartmentID) : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == item.DepartmentID) : 0;


                    if (item.Degree == "B.Tech")
                    {
                        
                        //Take Higest of 3 Years Of Admitated Intake
                        //approvedIntake1 means Proposed Intake of Present Year
                        int SanctionIntakeHigest = Max(intakedetails.approvedIntake2, intakedetails.approvedIntake3, intakedetails.approvedIntake4);
                        SanctionIntakeHigest = GetBtechAdmittedIntake(SanctionIntakeHigest);
                        int senondyearpercentage = 0;
                        int thirdyearpercentage = 0;
                        int fourthyearpercentage = 0;
                        //Comment on 26-07--2018 Due Count
                        //Comment on 02-05-2018 Due Count
                        //if (intakedetails.SanctionIntake2 != 0)
                        //{
                        //    senondyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.approvedIntake2) / Convert.ToDecimal(intakedetails.SanctionIntake2)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake3 != 0)
                        //{
                        //    thirdyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.approvedIntake3) / Convert.ToDecimal(intakedetails.SanctionIntake3)) * 100));
                        //}
                        //if (intakedetails.SanctionIntake4 != 0)
                        //{
                        //    fourthyearpercentage = (int)Math.Ceiling(Convert.ToDecimal((Convert.ToDecimal(intakedetails.approvedIntake4) / Convert.ToDecimal(intakedetails.SanctionIntake4)) * 100));
                        //}

                        //if (senondyearpercentage < 25 && thirdyearpercentage < 25 && fourthyearpercentage < 25)
                        //{
                        //    intakedetails.ispercentage = true;
                        //    //studentcount
                        //    if ((intakedetails.approvedIntake2 >= studentcount || intakedetails.approvedIntake3 >= studentcount || intakedetails.approvedIntake4 >= studentcount) && intakedetails.approvedIntake1 != 0)
                        //    {
                        //        intakedetails.ispercentage = false;
                        //        intakedetails.isstarcourses = true;
                        //        //intakedetails.ReducedInatke = 60;
                        //        //if (intakedetails.approvedIntake1 != 60)
                        //        //{
                        //        //    intakedetails.Note = "(Reduced from Proposed Intake of <b>";
                        //        //    intakedetails.Note += intakedetails.approvedIntake1;
                        //        //    intakedetails.Note += "</b> as per 25% Clause)";
                        //        //    intakedetails.approvedIntake1 = 60;
                        //        //}
                        //    }
                        //}
                        if (intakedetails.ispercentage==false)
                        {
                            if (item.courseStatus == "New")
                            {
                                intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                intakedetails.admittedIntake3 = 0;
                                intakedetails.admittedIntake4 = 0;
                            }
                            else if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                            {
                                //New Code 
                                intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                                intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                                intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                            }
                            else
                            {
                                intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                                intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                                intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                            }

                            intakedetails.totalIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) +
                                                        (intakedetails.admittedIntake4);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);


                            intakedetails.totalAdmittedIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);
                        }
                        //else
                        //{
                        //    if (SanctionIntakeHigest >= intakedetails.approvedIntake1)
                        //    {
                        //        //New Code 
                        //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.approvedIntake2);
                        //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.approvedIntake3);
                        //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.approvedIntake4);


                        //    }
                        //    else
                        //    {
                        //        intakedetails.admittedIntake2 = GetBtechAdmittedIntake(intakedetails.SanctionIntake2);
                        //        intakedetails.admittedIntake3 = GetBtechAdmittedIntake(intakedetails.SanctionIntake3);
                        //        intakedetails.admittedIntake4 = GetBtechAdmittedIntake(intakedetails.SanctionIntake4);
                        //    }

                        //    intakedetails.totalIntake = (intakedetails.admittedIntake2) + (intakedetails.admittedIntake3) +
                        //                                (intakedetails.admittedIntake4);
                        //    facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //                   Convert.ToDecimal(facultystudentRatio);


                        //    intakedetails.totalAdmittedIntake = (intakedetails.SanctionIntake2) + (intakedetails.SanctionIntake3) + (intakedetails.SanctionIntake4);
                        //}
                        
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "MBA")
                    {
                        //intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        //facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                        //               Convert.ToDecimal(facultystudentRatio);
                        //New Code Written by Narayana
                        if (item.Degree == "M.Tech"&&item.shiftId==1)
                        {
                            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int) facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                        }
                        if (item.Degree == "M.Tech" && item.shiftId == 2)
                        {
                            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                            int fratio = (int)facultyRatio;
                            if (fratio < 3)
                            {
                                fratio = 3;
                                facultyRatio = Convert.ToDecimal(fratio);
                            }
                            facultyRatio = facultyRatio/2;
                        }
                        if (item.Degree == "MBA")
                        {
                            intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                            facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) /
                                           Convert.ToDecimal(facultystudentRatio);
                        }
                        

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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D" && f.Recruitedfor == "UG");
                        }
                        else if (item.Degree == "Pharm.D PB")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharm.D PB" && f.Recruitedfor == "UG");
                        }
                        else
                        {

                            var regno = jntuh_registered_faculty.Where(f => f.Department == item.Department).Select(f => f.RegistrationNumber);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department);//&& (f.Recruitedfor == "UG" || f.Recruitedfor == null)
                        }
                    }

                    if (strdegreetype == "PG")
                    {
                        if (item.Degree == "M.Pharmacy")
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);//&& (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")
                        }
                        else
                        {
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == item.Department &&
                                        f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));//f.Specialization == item.Specialization
                        }
                    }
                    if (strdegreetype == "Dual Degree")
                    {
                        intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId);//(f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") &&
                    }
                    //intakedetails.id =
                    //    jntuh_college_faculty_deficiency.Where(
                    //        fd =>
                    //            fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId &&
                    //            fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    //if (intakedetails.id > 0)
                    //{
                    //    int? swf =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.SpecializationWiseFaculty)
                    //            .FirstOrDefault();
                    //    if (swf != null)
                    //    {
                    //        intakedetails.specializationWiseFaculty = (int) swf;
                    //    }
                    //    intakedetails.deficiency =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Deficiency)
                    //            .FirstOrDefault();
                    //    intakedetails.shortage =
                    //        jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id)
                    //            .Select(fd => fd.Shortage)
                    //            .FirstOrDefault();
                    //}

                    //============================================

                    int noPanOrAadhaarcount = 0;

                    if (item.Degree == "B.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        //intakedetails.Department = "Pharmacy";
                    }
                    if (item.Degree == "M.Pharmacy")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) &&
                                    f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "Pharmacy" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount =registeredFaculty.Where(f =>f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null &&
                        //            (f.isApproved == null || f.isApproved == true)).Count();
                        //intakedetails.Department = "Pharmacy";
                    }
                    else if (item.Degree == "Pharm.D")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == item.Department);
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                        var PhdReg = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();

                        //var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                        //if (item.Department == "MBA")
                        //    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                        // string REG=
                        if (item.Degree == "B.Tech")
                            SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        else
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
                    intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree ||
                                 i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                    intakedetails.facultyWithoutPANAndAadhaar = noPanOrAadhaarcount;
                    //=============//

                    intakedetailsList.Add(intakedetails);
                }

                intakedetailsList = intakedetailsList.OrderBy(ei => ei.Department).ThenBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

                string[] strOtherDepartments = { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
                var btechdegreecount = intakedetailsList.Count(d => d.Degree == "B.Tech");
                if (btechdegreecount > 0)
                {
                    foreach (var department in strOtherDepartments)
                    {
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        var deptname = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.departmentName).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == department);
                        int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department);
                        var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.Recruitedfor || "M.Phil / Other PG Degree" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                        //var testcount = jntuh_registered_faculty.Where(f => f.Department == department).ToList();
                        int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

                        intakedetailsList.Add(new CollegeFacultyWithIntakeReport
                        {
                            collegeId = (int)collegeId,
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
                            form16count = form16Count.Count > 0 ? form16Count.Count(i => i.DepartmentId == deptid) : 0,
                            aictecount = aictecount.Count > 0 ? aictecount.Count(i => i.DepartmentId == deptid) : 0,
                            A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            specializationWiseFaculty = jntuh_registered_faculty.Count(f => f.Department == deptname),
                            approvedIntake1 = 1
                        });
                    }
                }
            }

            return intakedetailsList;
        }

        private int Max(int AdmittedIntake2, int AdmittedIntake3, int AdmittedIntake4   )
        {
            return Math.Max(AdmittedIntake2, Math.Max(AdmittedIntake3, AdmittedIntake4));
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

        //Bas Flag Faculty Excel Downloads

        public ActionResult BasFlagDeficiencyFaculty()
        {
            var collegeids = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).Select(e => e.collegeId).ToList();
            List<jntuh_college_basreport> basFlagTotal = db.jntuh_college_basreport.Select(e => e).ToList();
            // List<jntuh_college_basreport> basFlag = basFlagGroupBy(e=>e.RegistrationNumber).Select(e=>e).ToList();
            //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(e => collegeids.Contains(e.collegeId)).Select(e => e).ToList();
            //var jntuh_college = db.jntuh_college.Where(e => e.isActive == true && collegeids.Contains(e.id)).ToList();
            int[] jntuh_college = db.jntuh_college_edit_status.Where(e=>e.IsCollegeEditable==false||e.collegeId==252 ||e.collegeId==210).Select(s=>s.collegeId).ToArray();
            //List<BasFlagClass> BasClass = new List<BasFlagClass>();
            int array2 = 0;
            int days = 6;
            int months = 2;
            List<CollegeTeachingFacultywithbasflag> CollegeTeachingFacultywithbasflaglist =new List<CollegeTeachingFacultywithbasflag>();
            List<BasFlagClass> BasFlagClasslist = new List<BasFlagClass>();
            string[] totalmonths = { "July", "August", "September", "October", "November", "December", "January", "February", };

            foreach (int college in jntuh_college)
            {
                int CollegeWiseFacultyBasCount = 0;
                BasFlagClass BasFlagClass = new BasFlagClass();
                var Faculty = db.jntuh_college_faculty_registered.Where(e => e.collegeId == college).Select(e => e).ToList();
                foreach (var EachFaculty in Faculty)
                {
                    CollegeTeachingFacultywithbasflag CollegeTeachingFacultywithbasflag =
                        new CollegeTeachingFacultywithbasflag();
                    CollegeTeachingFacultywithbasflag.RegistrationNumber = EachFaculty.RegistrationNumber;
                    CollegeTeachingFacultywithbasflag.collegeId = EachFaculty.collegeId;
                    CollegeTeachingFacultywithbasflag.Basflag = false;
                    bool FalgBas = true;
                    int FacultyBasFlagCount = 0;
                    var BasData =
                        basFlagTotal.Where(
                            e =>
                                e.RegistrationNumber.Trim() == EachFaculty.RegistrationNumber &&
                                totalmonths.Contains(e.month) && e.collegeId == college).Select(e => e).ToList();
                    int totalfacultymonts = BasData.Count();
                    //string[] facultymonths = BasData.Select(s => s.month).ToArray();
                    foreach (var item in BasData)
                    {
                        int totalworkingdays = (int) item.totalworkingDays;
                        int totalpresentdays = (int) item.NoofPresentDays;
                        int RequiredPresentDays = totalworkingdays - days;
                        if (CollegeTeachingFacultywithbasflag.Basflag == false)
                        {
                            if (RequiredPresentDays > totalpresentdays)
                            {
                                CollegeTeachingFacultywithbasflag.Basflag = true;
                            }
                            else
                            {
                                
                            }
                        }
                    }
                    CollegeTeachingFacultywithbasflaglist.Add(CollegeTeachingFacultywithbasflag);
                }
                BasFlagClass.CollegeCode =
                    db.jntuh_college.Where(c => c.id == college).Select(s => s.collegeCode).FirstOrDefault();
                BasFlagClass.CollegeName =
                    db.jntuh_college.Where(c => c.id == college).Select(s => s.collegeName).FirstOrDefault();
                BasFlagClass.TotalTeachingFaculty =
                    CollegeTeachingFacultywithbasflaglist.Where(c => c.collegeId == college).ToList().Count();
                BasFlagClass.BasFlagFacultyCount =
                    CollegeTeachingFacultywithbasflaglist.Where(c => c.collegeId == college&&c.Basflag==true).ToList().Count();
                BasFlagClasslist.Add(BasFlagClass);
            }
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=BasFaculty.xls");
            Response.ContentType = "application/vnd.ms-excel";
            return View("~/Views/Reports/_ExcelFileDoenload.cshtml", BasFlagClasslist.ToList());

        }

        public class CollegeTeachingFacultywithbasflag
        {
            public string RegistrationNumber { get; set; }
            public int collegeId { get; set; }
            public bool Basflag { get; set; }
        }
        public class BasFlagClass
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Registraionnumber { get; set; }
            public int TotalTeachingFaculty { get; set; }
            public int BasFlagFacultyCount { get; set; }
            public int Count { get; set; }

        }

        //public ActionResult BasFlagDeficiencyFaculty()
        //{
        //    int[] submitcollegeids =
        //        db.jntuh_college_edit_status.Where(
        //            e => e.IsCollegeEditable == false || e.collegeId == 210 || e.collegeId == 252)
        //            .Select(s => s.collegeId)
        //            .ToArray();
        //    List<jntuh_college_faculty_registered> jntuh_college_faculty_registered =
        //        db.jntuh_college_faculty_registered.Where(cf => submitcollegeids.Contains(cf.collegeId))
        //            .Select(s => s)
        //            .ToList();
        //    foreach (int id in submitcollegeids)
        //    {
                
        //    }
        //    int variable = 2;
        //    foreach (var bas in jntuh_college_faculty_registered)
        //    {
        //        List<jntuh_college_basreport> jntuh_college_basreport =
        //            db.jntuh_college_basreport.Where(a => a.RegistrationNumber == bas.RegistrationNumber)
        //                .Select(s => s)
        //                .ToList();
        //        string[] months = { "July", "August", "September", "October", "November", "December", "January", "February" };
        //        foreach (var month in months)
        //        {
        //            int totalworkingdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s => s.totalworkingDays).FirstOrDefault();
        //            int presentdays = (int)jntuh_college_basreport.Where(m => m.month == month).Select(s => s.NoofPresentDays).FirstOrDefault();
        //            int requiedpresentdays = totalworkingdays - variable;
        //            if (presentdays >= requiedpresentdays)
        //            {
                        
        //            }
        //        }

        //    }

        //    return View();
        //}




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
            //

            public int totalIntake { get; set; }
            public decimal requiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }

            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }
            public int form16count { get; set; }
            public int aictecount { get; set; }
            public int A416TotalFaculty { get; set; }

            public int NOSCM { get; set; }
            public int DeactivationReasionsCount { get; set; }
            public int FacultyAbsentCount { get; set; }
            public int InvalidPanCount { get; set; }
            public int totalBtechFirstYearIntake { get; set; }
            public int incompletecerificatesCount { get; set; }
            public int firstYearRequired { get; set; }
            public string minimumRequirementMet { get; set; }
            public string PHDminimumRequirementMet { get; set; }
            public int adjustedFaculty { get; set; }
            public int adjointfacultycount { get; set; }

            public string AffliationStatus { get; set; }

            //public bool AffiliationStatus1 { get; set; }
            public bool AffiliationStatus2 { get; set; }
            public bool AffiliationStatus3 { get; set; }
            public bool AffiliationStatus4 { get; set; }

            public bool ispercentage { get; set; }
            public bool isstarcourses { get; set; }
            public string Note { get; set; }
            public int ReducedInatke { get; set; }
            public string coursesstatus { get; set; }

        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;

            //Degree B.Tech  
            if (DegreeId == 4)
            {
                //admitted
                if (flag == 1 && (academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 10)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else   //approved
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                }
            }
            else
            {
                //approved
                if (flag == 1 && academicYearId != 10)
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

                }
                else if (flag == 1 && academicYearId == 10)
                {
                    var inta = db.jntuh_college_intake_existing.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
                    if (inta != null)
                    {
                        intake = Convert.ToInt32(inta.proposedIntake);
                    }

                }
                else //admitted
                {
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
                }
            }
            return intake;
        }


        private bool GetAcademicYear(int collegeId, int academicYearId, int specializationId, int shiftId, int DegreeId)
        {
            var firstOrDefault = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.courseAffiliatedStatus).FirstOrDefault();
            return firstOrDefault ?? false;
        }


        //LABS
        public List<Lab> collegeLabs(int? collegeID)
        {
            List<Lab> lstlaboratories = new List<Lab>();
            List<Lab> collegeLabMaster = new List<Lab>();
            var jntuh_college_laboratories_deficiency = db.jntuh_college_laboratories_deficiency.Where(c => c.CollegeId == collegeID).ToList();

            string strcollegecode = db.jntuh_college_randamcodes.Where(r => r.CollegeId == collegeID).Select(r => r.RandamCode).FirstOrDefault();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.proposedIntake!=0 && e.courseStatus!="Closure").Select(e => e.specializationId).Distinct().ToArray();

            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();


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



            var jntuh_college_laboratories = db.jntuh_college_laboratories_dataentry2.AsNoTracking().Where(l => l.CollegeID == collegeID).ToList();

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
                    lstlabs.deficiency = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.Year == item.year && ld.Semister == item.Semester && ld.CollegeId == collegeID).Select(ld => ld.Deficiency).FirstOrDefault();
                    lstlabs.id = jntuh_college_laboratories_deficiency.Where(ld => ld.LabCode == item.Labcode && ld.CollegeId == collegeID).Select(ld => ld.Id).FirstOrDefault();
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
        public string CollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;
            List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => specializationIds.Contains(l.SpecializationID))
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
                                                            LabName = l.LabName,
                                                            EquipmentName = l.EquipmentName
                                                        })
                                                        .OrderBy(l => l.degreeDisplayOrder)
                                                        .ThenBy(l => l.department)
                                                        .ThenBy(l => l.specializationName)
                                                        .ThenBy(l => l.year).ThenBy(l => l.Semester)
                                                        .ToList();



            string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

            if (CollegeAffiliationStatus == "Yes")
            {
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                           .Where(l => specializationIds.Contains(l.SpecializationID))
                                                           .Select(l => new FacultyVerificationController.AnonymousLabclass
                                                           {
                                                               id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
                                                               EquipmentID = l.id,
                                                               LabName = l.LabName,
                                                               EquipmentName = l.EquipmentName,
                                                               LabCode = l.Labcode,
                                                               year = l.Year,
                                                               Semester = l.Semester
                                                           })
                                                           .OrderBy(l => l.LabName)
                                                           .ThenBy(l => l.EquipmentName)
                                                           .ToList();

            }
            else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
            {
                collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
                                                              .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
                                                              .Select(l => new FacultyVerificationController.AnonymousLabclass
                                                              {
                                                                  id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
                                                                  EquipmentID = l.id,
                                                                  LabName = l.LabName,
                                                                  EquipmentName = l.EquipmentName,
                                                                  LabCode = l.Labcode,
                                                                  year = l.Year,
                                                                  Semester = l.Semester
                                                              })
                                                              .OrderBy(l => l.LabName)
                                                              .ThenBy(l => l.EquipmentName)
                                                              .ToList();
            }








            //var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

            var list = collegeLabMaster.Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                                       .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

            var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


            list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();
            list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            #region this code written by suresh

            int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

            int[] clgequipmentIDs =
                db.jntuh_college_laboratories.Where(
                    l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID))
                    .Select(i => i.EquipmentID)
                    .ToArray();

            list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID))
                    .ToList();


            #endregion




            //list
            if (list1.Count() > 0)
            {
                annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<th align='center' colspan='3'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
                annexure += "</tr>";
                annexure += "<tr>";
                annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                annexure += "</tr>";
                int LabsCount = 0;
                int EquipmentsCount = 0;
                string LabNmae = "", EquipmentName = "";
                foreach (var item in list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
                {
                    int indexnow = list1.IndexOf(item);



                    if (indexnow > 0 && list1[indexnow].LabName == list1[indexnow - 1].LabName)

                        LabsCount++;

                    else if (indexnow == 0 && (list1[indexnow].LabName == null || list1[indexnow].LabName == ""))
                        LabsCount++;

                    if (indexnow > 0 && list1[indexnow].EquipmentName == list1[indexnow - 1].EquipmentName)

                        EquipmentsCount++;

                    else if (indexnow == 0 && (list1[indexnow].EquipmentName == null || list1[indexnow].EquipmentName == ""))
                        EquipmentsCount++;

                    if (string.IsNullOrEmpty(item.LabName.Trim()) && LabsCount > 0)
                    {
                        //if (indexnow > 0 && (item.LabName.Trim() == null ||item.LabName.Trim() == ""))
                        //    LabNmae = "No Labs Uploaded";
                    }
                    else
                    {
                        LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                    }
                    if (string.IsNullOrEmpty(item.EquipmentName) && EquipmentsCount > 0)
                    {
                        //if (indexnow > 0 && (item.EquipmentName == null || item.EquipmentName == ""))
                        //    LabNmae = "No Equipments Uploaded";
                    }
                    else
                    {
                        EquipmentName = item.EquipmentName;
                    }


                    annexure += "<tr>";
                    annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                    annexure += "</tr>";



                    //if (string.IsNullOrEmpty(item.LabName))
                    //    LabsCount++;
                    //if (string.IsNullOrEmpty(item.EquipmentName))
                    //    EquipmentsCount++;

                    // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";\

                    #region code

                    //if (indexnow != list1.Count() - 1)
                    //{
                    //    annexure += "<tr>";
                    //    annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                    //    if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                    //    {
                    //        // annexure += "<tr>";
                    //        //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                    //        annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                    //        // annexure += "</tr>";
                    //    }
                    //    annexure += "</tr>";
                    //}
                    //else
                    //{
                    //    if (list1[indexnow].LabName != list1[indexnow].LabName)
                    //    {
                    //        //annexure += "<tr>";
                    //        //annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                    //        //annexure += "</tr>";
                    //        annexure += "<tr>";
                    //        annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                    //        if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                    //        {
                    //            // annexure += "<tr>";
                    //            //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                    //            annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                    //            // annexure += "</tr>";
                    //        }
                    //        annexure += "</tr>";
                    //    }
                    //}

                    #endregion



                    // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                    //annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabCode + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";

                    if (!string.IsNullOrEmpty(item.LabName))
                        LabsCount = 0;
                    if (!string.IsNullOrEmpty(item.EquipmentName))
                        EquipmentsCount = 0;
                }

                annexure += "</table>";
            }
            annexure += "</br><table width='100%'  cellspacing='0'></br>";
            annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
            annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
            annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                       "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
            annexure += "<tr><td></td></tr>"; annexure += "</table>";
            return annexure;
        }

        public string DeficiencyCollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;

            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                //if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                //{
                    List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass =new List<FacultyVerificationController.AnonymousLabclass>();
                    //Commented by Narayana
                    //List<int> specializationIds =db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9 && e.proposedIntake!=0).Select(e => e.specializationId).Distinct().ToList();
                    List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 10 && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
                    int[] Equipmentsids =db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeID).Select(C => C.EquipmentID).ToArray();
                    var DepartmentsData =db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                    var DegreeIds =db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

                    if (DegreeIds.Contains(4))
                    {
                        specializationIds.Add(39);
                    }







                    string CollegeAffiliationStatus =db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

                    if (CollegeAffiliationStatus == "Yes")
                    {
                        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeID && specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id)).Select(l => new FacultyVerificationController.AnonymousLabclass
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
                        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l =>specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) &&l.CollegeId == null).Select(l => new FacultyVerificationController.AnonymousLabclass
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








                    //var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

                    //var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                    //                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                    //var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

                    var list1 =collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


                    // list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();
                    //list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

                    #region this code written by suresh

                    int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

                    int[] clgequipmentIDs =db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID)).Select(i => i.EquipmentID).ToArray();

                    list1 = list1.Where(l => !clgequipmentIDs.Contains((int) l.EquipmentID)).ToList();


                    #endregion

                    int[] SpecializationIDs;
                    if (DegreeIds.Contains(4))
                        SpecializationIDs =(from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
                    //labs.Select(l => l.specializationId).Distinct().ToArray();
                    else
                        SpecializationIDs =(from a in collegeLabAnonymousLabclass where a.specializationId != 39 orderby a.Department select a.specializationId).Distinct().ToArray();


                    //list


                    if (list1.Count() > 0)
                    {
                        var specializations = db.jntuh_specialization.Where(it => SpecializationIDs.Contains(it.id)).Select(s => new
                        {
                            s.id,
                            specialization = s.specializationName,
                            department = s.jntuh_department.departmentName,
                            degree = s.jntuh_department.jntuh_degree.degree,
                            deptId = s.jntuh_department.id,

                        }).OrderBy(e => e.deptId).ToList();


                        annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                        annexure += "<tr>";
                        annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                        annexure += "</tr>";
                        annexure += "</table>";
                        foreach (var speclializationId in SpecializationIDs)
                        {
                            string LabNmae = "", EquipmentName = "", DepartmentName = "";
                            var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                            DepartmentName = list1.Where(l => l.specializationId == speclializationId).Select(l => l.Department).FirstOrDefault();
                            annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                            annexure += "<tr>";
                            annexure += "<th align='left' colspan='3'> " + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</th>";
                            annexure += "</tr>";
                            annexure += "<tr>";
                            annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                            annexure += "</tr>";
                            int LabsCount = 0;
                            int EquipmentsCount = 0;

                            var labs = list1.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).ToList();
                            int indexnow = 1;
                            foreach (var item in labs.ToList())
                            {

                                LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                                EquipmentName = item.EquipmentName;
                                // int indexnow = list1.IndexOf(item);



                                //if (indexnow > 0 && list1[indexnow].LabName == list1[indexnow - 1].LabName)

                                //    LabsCount++;

                                //else if (indexnow == 0 && (list1[indexnow].LabName == null || list1[indexnow].LabName == ""))
                                //    LabsCount++;

                                //if (indexnow > 0 && list1[indexnow].EquipmentName == list1[indexnow - 1].EquipmentName)

                                //    EquipmentsCount++;

                                //else if (indexnow == 0 && (list1[indexnow].EquipmentName == null || list1[indexnow].EquipmentName == ""))
                                //    EquipmentsCount++;

                                //if (string.IsNullOrEmpty(item.LabName.Trim()) && LabsCount > 0)
                                //{
                                //    //if (indexnow > 0 && (item.LabName.Trim() == null ||item.LabName.Trim() == ""))
                                //    //    LabNmae = "No Labs Uploaded";
                                //}
                                //else
                                //{
                                //    LabNmae = item.LabName.Trim() != null ? item.year + "-" + item.Semester + "-" + item.LabName : null;
                                //}
                                //if (string.IsNullOrEmpty(item.EquipmentName) && EquipmentsCount > 0)
                                //{
                                //    //if (indexnow > 0 && (item.EquipmentName == null || item.EquipmentName == ""))
                                //    //    LabNmae = "No Equipments Uploaded";
                                //}
                                //else
                                //{
                                //    EquipmentName = item.EquipmentName;
                                //}
                                //if (string.IsNullOrEmpty(item.Department))
                                //{
                                //    DepartmentName = item.Department;
                                //}
                                //else
                                //{
                                //    DepartmentName = "";
                                //}


                                annexure += "<tr>";
                                annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                                annexure += "</tr>";



                                //if (string.IsNullOrEmpty(item.LabName))
                                //    LabsCount++;
                                //if (string.IsNullOrEmpty(item.EquipmentName))
                                //    EquipmentsCount++;

                                // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";\

                                #region code

                                //if (indexnow != list1.Count() - 1)
                                //{
                                //    annexure += "<tr>";
                                //    annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                                //    if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                                //    {
                                //        // annexure += "<tr>";
                                //        //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                                //        annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                                //        // annexure += "</tr>";
                                //    }
                                //    annexure += "</tr>";
                                //}
                                //else
                                //{
                                //    if (list1[indexnow].LabName != list1[indexnow].LabName)
                                //    {
                                //        //annexure += "<tr>";
                                //        //annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                                //        //annexure += "</tr>";
                                //        annexure += "<tr>";
                                //        annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>";
                                //        if (list1[indexnow].LabName != list1[indexnow + 1].LabName)
                                //        {
                                //            // annexure += "<tr>";
                                //            //<td align='left'>" + (list1.IndexOf(item) + 1) + "</td>
                                //            annexure += "<td rowspan='" + LabsCount + "' align='left'>" + LabNmae + "</td><td rowspan='" + EquipmentsCount + "' align='left'>" + EquipmentName + "</td>";
                                //            // annexure += "</tr>";
                                //        }
                                //        annexure += "</tr>";
                                //    }
                                //}

                                #endregion



                                // annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td  align='left'>" + LabNmae + "</td><td  align='left'>" + EquipmentName + "</td>";
                                //annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabCode + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";

                                if (!string.IsNullOrEmpty(item.LabName))
                                    LabsCount = 0;
                                if (!string.IsNullOrEmpty(item.EquipmentName))
                                    EquipmentsCount = 0;
                                indexnow++;
                            }

                            annexure += "</table>";
                            annexure += "<br/>";
                        }
                    }
                    else
                    {
                        annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                        annexure += "<tr>";
                        annexure += "<td align='left'><b><u>Deficiencies in Laboratory  </u></b></td>";
                        annexure += "</tr>";
                        annexure += "</table>";
                        annexure += "<table width='100%' border='1'  cellspacing='0'>";
                        annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                        annexure += "</table>";
                    }

                    //Deficiencies Labs Video Verification is commented by Narayana on 31/03/2018

                    //var CollegelaboratoriesInActive = (from a in db.jntuh_college_laboratories
                    //    join b in db.jntuh_lab_master on a.EquipmentID equals b.id
                    //    where a.CollegeID == collegeID && a.isActive == false
                    //    select new
                    //    {
                    //        LabName = b.LabName,
                    //        SpecId = b.SpecializationID,
                    //        EquipmentNAme = a.EquipmentName,
                    //        Year = b.Year,
                    //        Semester = b.Semester
                    //    }).ToList();
                    //if (CollegelaboratoriesInActive.Count != 0)
                    //{
                    //    annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    //    annexure += "<tr>";
                    //    annexure += "<td align='left'><b><u>Deficiencies Labs Video Verification</u></b></td>";
                    //    annexure += "</tr>";
                    //    annexure += "</table>";
                    //    var SpecIds = CollegelaboratoriesInActive.Select(e => e.SpecId).Distinct().ToArray();
                    //    var specializations =
                    //        db.jntuh_specialization.Where(it => SpecIds.Contains(it.id)).Select(s => new
                    //        {
                    //            s.id,
                    //            specialization = s.specializationName,
                    //            department = s.jntuh_department.departmentName,
                    //            degree = s.jntuh_department.jntuh_degree.degree,
                    //            deptId = s.jntuh_department.id,

                    //        }).OrderBy(e => e.deptId).ToList();


                    //    foreach (var Id in SpecIds)
                    //    {
                    //        var specializationDetails =
                    //            specializations.Where(e => e.id == Id).Select(e => e).FirstOrDefault();
                    //        annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                    //        annexure += "<tr>";
                    //        annexure += "<th align='left' colspan='3'> " + specializationDetails.degree + " -" +
                    //                    specializationDetails.department + "-" + specializationDetails.specialization +
                    //                    "</th>";
                    //        annexure += "</tr>";
                    //        annexure += "<tr>";
                    //        annexure +=
                    //            "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
                    //        annexure += "</tr>";
                    //        var CollegeInactiveLabsSpecializationWise =
                    //            CollegelaboratoriesInActive.Where(e => e.SpecId == Id).Select(e => e).ToList();
                    //        int count = 1;
                    //        foreach (var data in CollegeInactiveLabsSpecializationWise)
                    //        {
                    //            annexure += "<tr>";
                    //            annexure += "<td align='left'>" + count + "</td>";
                    //            annexure += "<td align='left'>" + data.Year + "-" + data.Semester + "-" + data.LabName +
                    //                        "</td>";
                    //            annexure += "<td align='left'>" + data.EquipmentNAme + "</td>";
                    //            // annexure += "<td align='left'>" + "" + "</td>";
                    //            annexure += "</tr>";
                    //            count++;
                    //        }
                    //        annexure += "</table>";
                    //    }
                    //}
                    //else
                    //{
                    //    annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                    //    annexure += "<tr>";
                    //    annexure += "<td align='left'><b><u>Deficiencies In Labs Video Verification</u></b></td>";
                    //    annexure += "</tr>";
                    //    annexure += "</table>";
                    //    annexure += "<table width='100%' border='1'  cellspacing='0'>";
                    //    annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                    //    annexure += "</table>";
                    //}








                    //annexure += "</br><table width='100%'  cellspacing='0'></br>";
                    //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    //annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
                    //annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                    //           "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
                    //annexure += "<tr><td></td></tr>"; annexure += "</table>";
                }
            //}
            return annexure;
        }


        //public string CollegeLabsAnnexure(int? collegeID)
        //{
        //    string annexure = string.Empty;
        //    List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
        //    int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
        //    List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
        //                                                .Where(l => specializationIds.Contains(l.SpecializationID))
        //                                                .Select(l => new Lab
        //                                                {
        //                                                    EquipmentID = l.id,
        //                                                    degreeId = l.DegreeID,
        //                                                    degree = l.jntuh_degree.degree,
        //                                                    degreeDisplayOrder = l.jntuh_degree.degreeDisplayOrder,
        //                                                    departmentId = l.DepartmentID,
        //                                                    department = l.jntuh_department.departmentName,
        //                                                    specializationId = l.SpecializationID,
        //                                                    specializationName = l.jntuh_specialization.specializationName,
        //                                                    year = l.Year,
        //                                                    Semester = l.Semester,
        //                                                    Labcode = l.Labcode,
        //                                                    LabName = l.LabName,
        //                                                    EquipmentName = l.EquipmentName
        //                                                })
        //                                                .OrderBy(l => l.degreeDisplayOrder)
        //                                                .ThenBy(l => l.department)
        //                                                .ThenBy(l => l.specializationName)
        //                                                .ThenBy(l => l.year).ThenBy(l => l.Semester)
        //                                                .ToList();



        //    string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

        //    if (CollegeAffiliationStatus == "Yes")
        //    {
        //        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
        //                                                   .Where(l => specializationIds.Contains(l.SpecializationID))
        //                                                   .Select(l => new FacultyVerificationController.AnonymousLabclass
        //                                                   {
        //                                                       id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
        //                                                       EquipmentID = l.id,
        //                                                       LabName = l.LabName,
        //                                                       EquipmentName = l.EquipmentName,
        //                                                       LabCode = l.Labcode,
        //                                                   })
        //                                                   .OrderBy(l => l.LabName)
        //                                                   .ThenBy(l => l.EquipmentName)
        //                                                   .ToList();

        //    }
        //    else if (CollegeAffiliationStatus == "No" || CollegeAffiliationStatus == null)
        //    {
        //        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking()
        //                                                      .Where(l => specializationIds.Contains(l.SpecializationID) && l.Labcode != "TMP-CL")
        //                                                      .Select(l => new FacultyVerificationController.AnonymousLabclass
        //                                                      {
        //                                                          id = db.jntuh_appeal_college_laboratories.Where(l1 => l1.EquipmentID == l.id && l1.EquipmentNo == 1 && l1.CollegeID == collegeID).Select(l1 => l1.id).FirstOrDefault(),
        //                                                          EquipmentID = l.id,
        //                                                          LabName = l.LabName,
        //                                                          EquipmentName = l.EquipmentName,
        //                                                          LabCode = l.Labcode,
        //                                                      })
        //                                                      .OrderBy(l => l.LabName)
        //                                                      .ThenBy(l => l.EquipmentName)
        //                                                      .ToList();
        //    }


        //    var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

        //    var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
        //                               .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

        //    var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

        //    var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();


        //    list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();
        //    list1 = list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

        //    #region this code written by suresh

        //    int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

        //    int[] clgequipmentIDs =
        //        db.jntuh_college_laboratories.Where(
        //            l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID))
        //            .Select(i => i.EquipmentID)
        //            .ToArray();

        //    list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID))
        //            .ToList();


        //    #endregion




        //    //list
        //    if (list1.Count() > 0)
        //    {
        //        annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
        //        annexure += "<tr>";
        //        annexure += "<th align='center' colspan='3'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
        //        annexure += "</tr>";
        //        annexure += "<tr>";
        //        annexure += "<th align='left'>S.No</th><th align='left'>Lab Name</th><th align='left'>Equipment Name</th>";
        //        annexure += "</tr>";
        //        int LabsCount = 0;
        //        int EquipmentsCount = 0;
        //        string LabNmae = "", EquipmentName = "";
        //        foreach (var item in list1.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
        //        {
        //            int indexnow = list1.IndexOf(item);



        //            //if (indexnow > 0 && list1[indexnow].LabName == list1[indexnow - 1].LabName)

        //            //    LabsCount ++;

        //            //else if (indexnow == 0 && (list1[indexnow].LabName == null || list1[indexnow].LabName == ""))
        //            //    LabsCount++;

        //            //if (indexnow > 0 && list1[indexnow].EquipmentName == list1[indexnow - 1].EquipmentName)

        //            //    EquipmentsCount++;

        //            //else if (indexnow == 0 && (list1[indexnow].EquipmentName == null || list1[indexnow].EquipmentName == ""))
        //            //    EquipmentsCount++;

        //            //if (string.IsNullOrEmpty(item.LabName.Trim()) && LabsCount>0)
        //            //{
        //            //    if (indexnow > 0 && (item.LabName.Trim() == null ||item.LabName.Trim() == ""))
        //            //        LabNmae = "No Labs Uploaded";
        //            //}
        //            //else
        //            //{
        //            //    LabNmae = item.LabName.Trim();
        //            //}
        //            //if (string.IsNullOrEmpty(item.EquipmentName) && EquipmentsCount>0)
        //            //{
        //            //    if (indexnow > 0 && (item.EquipmentName == null || item.EquipmentName == ""))
        //            //        LabNmae = "No Equipments Uploaded";
        //            //}
        //            //else
        //            //{
        //            //    EquipmentName = item.EquipmentName;
        //            //}

        //            //if (string.IsNullOrEmpty(item.LabName))
        //            //    LabsCount++;
        //            //if (string.IsNullOrEmpty(item.EquipmentName))
        //            //    EquipmentsCount++;
        //            annexure += "<tr>";
        //            annexure += "<td align='left'>" + (list1.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabName + "</td><td  align='left'>" + item.EquipmentName + "</td>";
        //            //annexure += "<td align='left'>" + (list.IndexOf(item) + 1) + "</td><td align='left'>" + item.LabCode + "</td><td align='left'>" + item.LabName + "</td><td align='left'>" + item.EquipmentName + "</td>";
        //            annexure += "</tr>";
        //            if (!string.IsNullOrEmpty(item.LabName))
        //                LabsCount=0;
        //            if (!string.IsNullOrEmpty(item.EquipmentName))
        //                EquipmentsCount=0;
        //        }

        //        annexure += "</table>";
        //    }
        //    annexure += "</br><table width='100%'  cellspacing='0'></br>";
        //    annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
        //    annexure += "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
        //    annexure += "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
        //               "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
        //    annexure += "<tr><td></td></tr>"; annexure += "</table>";
        //    return annexure;
        //}

        //========================//


        public ActionResult DeficienciesNew(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));

                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeID).Select(c => c.collegeCode).FirstOrDefault();
                Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "- DeficiencyCorrections" + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(Header(collegeID));
                str.Append("<br />");
                str.Append(CollegeInformation(collegeID));
                str.Append("<br />");
                str.Append(Principal(collegeID));
                str.Append("<br />");
                //str.Append(CommitteeMembers(collegeID));
                str.Append("<br />");
                str.Append(DeficienciesInFacultyNew(collegeID));
                str.Append("<br />");
                str.Append(DeficienciesInLabsNew(collegeID));
                str.Append("<br />");
                str.Append(CollegeLabsAnnexureNew(collegeID));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/DeficiencyReports/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + collegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.Create));

                pdfDoc.Open();

                List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(str.ToString()), null);

                foreach (var htmlElement in parsedHtmlElements)
                {
                    pdfDoc.Add((IElement)htmlElement);
                }

                pdfDoc.Close();

                Response.Output.Write(str.ToString());
                Response.Flush();
                Response.End();
            }

            return View();
        }

        public string DeficienciesInFacultyNew(int? collegeID)
        {
            string faculty = string.Empty;

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
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Required Faculty Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Available Faculty Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency Corrections</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Remarks</th>";
            faculty += "</tr>";

            foreach (var item in facultyCounts)
            {
                // distDeptcount = facultyCounts.Where(d => d.Department == item.Department).Distinct().Count();

                int indexnow = facultyCounts.IndexOf(item);

                if (indexnow > 0 && facultyCounts[indexnow].Department != facultyCounts[indexnow - 1].Department)
                {
                    deptloop = 1;
                }

                //  departmentWiseRequiredFaculty = facultyCounts.Where(d => d.Department == item.Department).Select(d => d.requiredFaculty).Sum();

                string minimumRequirementMet = string.Empty;
                int facultyShortage = 0;
                int adjustedFaculty = 0;
                int adjustedPHDFaculty = 0;

                int tFaculty = (int)Math.Ceiling(Convert.ToDecimal(item.totalFaculty));
                int rFaculty = (int)Math.Ceiling(item.requiredFaculty);

                if (departments.Contains(item.Department))
                {
                    rFaculty = (int)firstYearRequired;
                    //   departmentWiseRequiredFaculty = (int)firstYearRequired;
                }

                var degreeType = db.jntuh_degree.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = tFaculty;
                        //  facultyShortage = rFaculty - tFaculty;
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
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = remainingFaculty;
                        //  facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }

                    if (remainingPHDFaculty > 0 && degreeType.Equals("PG"))
                    {
                        adjustedPHDFaculty = 1;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
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
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                }

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";

                if (adjustedPHDFaculty > 0)
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
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'></td>";

                faculty += "</tr>";

                deptloop++;
            }

            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            faculty += "</tr>";
            faculty += "</table>";

            return faculty;
        }

        public string DeficienciesInLabsNew(int? collegeID)
        {
            string labs = string.Empty;

            labs += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            labs += "<tr>";
            labs += "<td align='left'><b><u>Deficiencies in Laboratory</u></b> (Department/ Specialization Wise):";
            labs += "</tr>";
            labs += "</table>";

            List<Lab> labsCount = collegeLabs(collegeID);

            var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName })
                                        .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationName = l.Key.specializationName, deficiencies = string.Empty })
                                        .ToList();

            labs += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            labs += "<tr>";
            labs += "<th style='text-align: center; width: 5%;'>S.No</th>";
            labs += "<th style='text-align: left; width: 10%;'>Degree</th>";
            labs += "<th style='text-align: left; width: 10%;'>Department</th>";
            labs += "<th style='text-align: left; width: 20%;'>Specialization</th>";
            labs += "<th style='text-align: center; '>Names of the Labs with Deficiency (Details Annexed)</th>";
            labs += "<th style='text-align: center; vertical-align: top;' colspan='3'>Remarks</th>";
            labs += "</tr>";

            foreach (var item in deficiencies)
            {

                labs += "<tr>";
                labs += "<td style='text-align: center; width: 5%; '>" + (deficiencies.IndexOf(item) + 1) + "</td>";
                labs += "<td style=''>" + item.degree + "</td>";
                labs += "<td style=''>" + item.department + "</td>";
                labs += "<td style=''>" + item.specializationName + "</td>";

                string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
                    .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.LabName }).Select(l => l.Deficiency).ToList();

                labs += "<td style='; text-align: center'>" + (labsWithDeficiency.Count() == 0 ? "NIL" : String.Join(", ", labsWithDeficiency)) + "</td>";
                labs += "<td style='; text-align: center' colspan='3'></td>";
                labs += "</tr>";
            }

            labs += "</table>";

            return labs;
        }

        public string CollegeLabsAnnexureNew(int? collegeID)
        {
            string annexure = string.Empty;

            int[] specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID).Select(e => e.specializationId).Distinct().ToArray();
            List<Lab> collegeLabMaster = db.jntuh_lab_master.AsNoTracking()
                                                        .Where(l => specializationIds.Contains(l.SpecializationID))
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
                                                            LabName = l.LabName,
                                                            EquipmentName = l.EquipmentName
                                                        })
                                                        .OrderBy(l => l.degreeDisplayOrder)
                                                        .ThenBy(l => l.department)
                                                        .ThenBy(l => l.specializationName)
                                                        .ThenBy(l => l.year).ThenBy(l => l.Semester)
                                                        .ToList();

            var collegeEquipments = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

            var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                                       .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

            var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

            list = list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList();

            if (list.Count() > 0)
            {
                annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                annexure += "<tr>";
                annexure += "<th align='center' colspan='12'>LIST OF EQUIPMENT NOT AVAILABLE</th>";
                annexure += "</tr>";
                annexure += "<tr>";
                annexure += "<th align='left' colspan='1'>S.No</th>";
                annexure += "<th align='left' colspan='3'>Lab Name</th>";
                annexure += "<th align='left' colspan='3'>Equipment Name</th>";
                annexure += "<th align='left' colspan='5'>Remarks</th>";

                annexure += "</tr>";

                foreach (var item in list.Where(l => labDeficiencies.Contains(l.LabCode)).ToList())
                {
                    annexure += "<tr>";
                    annexure += "<td align='left' colspan='1'>" + (list.IndexOf(item) + 1) + "</td>";
                    annexure += "<td align='left' colspan='3'>" + item.LabName + "</td>";
                    annexure += "<td align='left' colspan='3'>" + item.EquipmentName + "</td>";
                    annexure += "<td colspan='5'></td>";
                    annexure += "</tr>";
                }

                annexure += "</table>";
            }

            return annexure;
        }

        //========================//

        ////For ExportAllCOlleges intake
        private int academicYearId = 0;
        private int AcademicYear = 0;
        private int nextAcademicYearId = 0;
        private int[] DegreeIds;
        private int[] CollegeIds;
        private int ClosureCourseId = 0;
        private int[] SubmitteCollegesId;
        private string ReportHeader = null;
        private string[] degrees;

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


        public class physicalLabs
        {
            public int? id { get; set; }
            public int Labid { get; set; }
            public int? collegeId { get; set; }
            public int? degreeid { get; set; }
            public int departmentid { get; set; }
            public int? specializationid { get; set; }
            public string degree { get; set; }
            // public string degree { get; set; }
            public string department { get; set; }
            public string specialization { get; set; }
            public int? year { get; set; }
            public int? semister { get; set; }
            public string Labname { get; set; }
            public string LabCode { get; set; }
            public string Remarks { get; set; }
            public int? NoOfRequiredLabs { get; set; }
            public int? NoOfAvailabeLabs { get; set; }
            public string PhysicalLab { get; set; }
            public int physicalId { get; set; }
            public int? Intake { get; set; }


        }


        private void GetIds()
        {

            //Get present academic Year Id
            academicYearId = db.jntuh_academic_year
                                   .Where(year => year.isActive == true &&
                                                  year.isPresentAcademicYear == true)
                                   .Select(year => year.id)
                                   .FirstOrDefault();
            //Get Present academic Year
            AcademicYear = db.jntuh_academic_year.Where(year => year.isActive == true &&
                                                                year.id == academicYearId)
                                                 .Select(year => year.actualYear)
                                                 .FirstOrDefault();

            //Get next academic year
            nextAcademicYearId = db.jntuh_academic_year
                                   .Where(year => year.actualYear == (AcademicYear + 1))
                                   .Select(year => year.id)
                                   .FirstOrDefault();

            //InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            //get old inspection phases
            //ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
            //                               join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
            //                               join a in db.jntuh_academic_year on p.academicYearId equals a.id
            //                               select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().OrderByDescending(p => p.id).ToList();

            //DEO Submitted colleges Ids
            //SubmitteCollegesId = db.jntuh_dataentry_allotment
            //    //.Where(d => d.isCompleted == true)
            //                       .Where(d => d.isCompleted == true)// && d.InspectionPhaseId == InspectionPhaseId) 
            //                       .Select(d => d.collegeID)
            //                       .ToArray();

            SubmitteCollegesId = db.jntuh_college_edit_status
                //.Where(d => d.isCompleted == true)
                                   .Where(d => d.IsCollegeEditable == false)// && d.InspectionPhaseId == InspectionPhaseId) 
                                   .Select(d => d.collegeId)
                                   .ToArray();

            //colleges Ids
            //CollegeIds = db.jntuh_college
            //             .Where(c => c.isActive == true && c.isNew == false &&
            //                         SubmitteCollegesId.Contains(c.id))
            //             .Select(c => c.id)
            //             .ToArray();

            CollegeIds = db.jntuh_college_news
                         .Where(n => n.title.Contains("Grant of Affiliation is available at your portal for download"))
                         .Select(c => c.collegeId)
                         .ToArray();

            //CollegeIds = db.college_clusters.Where(c => c.clusterName.Equals("2015-court-cases")).Select(c => (int)c.collegeId).ToArray();

            //To fill college dropdown list
            ViewBag.Colleges = db.jntuh_college
                                 .Where(college => college.isActive == true &&
                                                   SubmitteCollegesId.Contains(college.id))
                                 .Select(college => new
                                 {
                                     ID = college.id,
                                     CollegeName = college.collegeCode + " - " + college.collegeName
                                 }).ToList();

            //To get all degrees
            degrees = db.jntuh_degree.Where(d => d.isActive == true).Select(d => d.degree).ToArray();

        }


        public List<CollegeFacultyLabs> DeficienciesInFaculty1(int? collegeID)
        {
            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFaculty(collegeID).Where(c => c.shiftId == 1).ToList();

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 120);
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            var degreeTypes = db.jntuh_degree_type.Select(t => t).ToList();
            var degrees = db.jntuh_degree.Select(t => t).ToList();

            List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

            foreach (var item in facultyCounts)
            {
                //item.requiredFaculty = (item.requiredFaculty - item.requiredFaculty * 10 / 100);
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
                        minimumRequirementMet = "NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
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
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
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
                    newFaculty.Available = item.totalFaculty;
                }
                else
                {
                    //newFaculty.TotalIntake = item.totalIntake;
                    newFaculty.Required = (int)Math.Ceiling(item.requiredFaculty);
                    newFaculty.Available = adjustedFaculty;
                }

                newFaculty.Deficiency = minimumRequirementMet;

                if (adjustedPHDFaculty > 0)
                {
                    newFaculty.PhdDeficiency = "NO";
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

            return lstFaculty;
        }

        //public List<CollegeFacultyLabs> DeficienciesInLabs1(int? collegeID)
        //{
        //    List<Lab> labsCount = collegeLabs(collegeID);

        //    var deficiencies = labsCount.GroupBy(l => new { l.degree, l.department, l.specializationName, l.specializationId })
        //                                .Select(l => new { degree = l.Key.degree, department = l.Key.department, specializationid = l.Key.specializationId, specializationName = l.Key.specializationName, deficiencies = string.Empty })
        //                                .ToList();

        //    var labMaster = db.jntuh_lab_master.ToList();
        //    var collegeLabMaster = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).ToList();

        //    List<CollegeFacultyLabs> lstFaculty = new List<CollegeFacultyLabs>();

        //    foreach (var item in deficiencies)
        //    {
        //        string degreeType = db.jntuh_degree.Where(d => d.degree == item.degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

        //        var labsWithDeficiency = labsCount.Where(l => l.degree == item.degree && l.department == item.department && l.specializationName == item.specializationName && l.deficiency == true)
        //            .Select(l => new { Deficiency = degreeType.Equals("PG") ? "No Equipement Uploaded" : l.year + "-" + l.Semester + "-" + l.Labcode.Replace("-", "$") + "-" + l.specializationId }).Select(l => l.Deficiency).ToList();

        //        List<string> defs = new List<string>();
        //        labsWithDeficiency.ForEach(l =>
        //        {
        //            if (l.Equals("No Equipement Uploaded"))
        //            {
        //                defs.Add(l);
        //            }
        //            else
        //            {
        //                string[] strLab = l.Split('-');

        //                int specializationid = Convert.ToInt32(strLab[3]);
        //                int year = Convert.ToInt32(strLab[0]);
        //                int semester = Convert.ToInt32(strLab[1]);
        //                string labCode = strLab[2].Replace("$", "-");

        //                var requiredLabs = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.id).ToList();
        //                int requiredCount = requiredLabs.Count();
        //                //int availableCount = collegeLabMaster.Where(m => requiredLabs.Contains(m.EquipmentID)).Count();

        //                if (requiredCount > availableCount)
        //                {
        //                    string labName = labMaster.Where(m => m.SpecializationID == specializationid && m.Year == year && m.Semester == semester && m.Labcode == labCode).Select(m => m.LabName).FirstOrDefault();
        //                    defs.Add(year + "-" + semester + "-" + labName);
        //                }
        //            }
        //        });

        //        CollegeFacultyLabs newFaculty = new CollegeFacultyLabs();
        //        newFaculty.Degree = item.degree;
        //        newFaculty.Department = item.department;
        //        newFaculty.Specialization = item.specializationName;
        //        newFaculty.SpecializationId = item.specializationid;
        //        newFaculty.LabsDeficiency = (labsWithDeficiency.Count() == 0 ? "NIL" : (defs.Count() == 0 ? "NIL" : String.Join(", ", defs)));

        //        lstFaculty.Add(newFaculty);
        //    }

        //    return lstFaculty;
        //}
        //public List<CollegeFacultyLabs> CollegeCoursesNotClear(int collegeId)
        //{
        //    List<CollegeFacultyLabs> notAffiliatedCourses = new List<CollegeFacultyLabs>();

        //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

        //    //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    int rowCount = 0; int assessmentCount = 0;

        //    List<CollegeFacultyLabs> faculty = DeficienciesInFaculty1(collegeId);
        //    List<CollegeFacultyLabs> labs = DeficienciesInLabs1(collegeId);

        //    List<CollegeFacultyLabs> clearedCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
        //                                .Where(a => a.f.Deficiency == "NO" && a.f.PhdDeficiency != "YES" && a.l.LabsDeficiency == "NIL")
        //                                .Select(a => new CollegeFacultyLabs
        //                                {
        //                                    DegreeType = a.f.DegreeType,
        //                                    DegreeDisplayOrder = a.f.DegreeDisplayOrder,
        //                                    Degree = a.f.Degree,
        //                                    Department = a.f.Department,
        //                                    SpecializationId = a.f.SpecializationId,
        //                                    Specialization = a.f.Specialization,
        //                                    TotalIntake = a.f.TotalIntake,
        //                                    Required = a.f.Required,
        //                                    Available = a.f.Available,
        //                                    Deficiency = a.f.Deficiency,
        //                                    PhdDeficiency = a.f.PhdDeficiency,
        //                                    LabsDeficiency = a.l.LabsDeficiency
        //                                })
        //                                .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
        //                                .ToList();

        //    List<CollegeFacultyLabs> deficiencyCourses = faculty.Join(labs, f => f.SpecializationId, l => l.SpecializationId, (f, l) => new { f, l })
        //                                .Where(a => (a.f.Deficiency == "YES" || a.f.PhdDeficiency == "YES") || a.l.LabsDeficiency != "NIL")
        //                                .Select(a => new CollegeFacultyLabs
        //                                {
        //                                    DegreeType = a.f.DegreeType,
        //                                    DegreeDisplayOrder = a.f.DegreeDisplayOrder,
        //                                    Degree = a.f.Degree,
        //                                    Department = a.f.Department,
        //                                    SpecializationId = a.f.SpecializationId,
        //                                    Specialization = a.f.Specialization,
        //                                    TotalIntake = a.f.TotalIntake,
        //                                    Required = a.f.Required,
        //                                    Available = a.f.Available,
        //                                    Deficiency = a.f.Deficiency,
        //                                    PhdDeficiency = a.f.PhdDeficiency,
        //                                    LabsDeficiency = a.l.LabsDeficiency
        //                                })
        //                                .OrderBy(a => a.DegreeDisplayOrder).ThenBy(a => a.Department).ThenBy(a => a.Specialization)
        //                                .ToList();

        //    int affiliatedCount = 0; int defiencyRows = 0;

        //    List<string> deficiencyDepartments = new List<string>();

        //    foreach (var course in deficiencyCourses)
        //    {
        //        if (!deficiencyDepartments.Contains(course.Department))
        //        {
        //            deficiencyDepartments.Add(course.Department);
        //        }

        //        //FIVE percent relaxation for faculty only
        //        decimal percentage = 0;

        //        if (course.LabsDeficiency == "NIL")
        //        {
        //            percentage = (course.Required * 10) / 100;
        //        }

        //        bool isCourseAffiliated = false;

        //        var affiliation = db.jntuh_college_intake_existing_datentry2.Where(d => d.collegeId == collegeId && d.specializationId == course.SpecializationId && d.isAffiliated == true).Select(d => d).FirstOrDefault();

        //        if (affiliation != null)
        //        {
        //            if (affiliation.isAffiliated == true)
        //            {
        //                isCourseAffiliated = true;
        //            }
        //        }

        //        if (((course.Required - course.Available) <= percentage && course.PhdDeficiency != "YES" && course.LabsDeficiency == "NIL") || isCourseAffiliated != false)
        //        {
        //            if (course.TotalIntake != 0)
        //            {
        //                affiliatedCount++;
        //                rowCount++;
        //            }
        //        }
        //        else if (isCourseAffiliated == false)
        //        {
        //            defiencyRows++; notAffiliatedCourses.Add(course);
        //            assessmentCount++;
        //        }
        //    }

        //    foreach (var course in clearedCourses)
        //    {
        //        string degreeType = db.jntuh_degree.Join(db.jntuh_degree_type, d => d.degreeTypeId, t => t.id, (d, t) => new { d, t })
        //                                           .Where(a => a.d.degree == course.Degree).Select(a => a.t.degreeType).FirstOrDefault();

        //        List<string> clearedDepartments = clearedCourses.Where(a => a.DegreeType == "UG").Select(a => a.Department).ToList();

        //        if (deficiencyDepartments.Contains(course.Department) && !clearedDepartments.Contains(course.Department))
        //        {
        //            defiencyRows++;
        //            assessmentCount++;
        //        }
        //        else
        //        {
        //            if (course.TotalIntake != 0)
        //            {
        //                affiliatedCount++; //affiliatedCourses.Add(course);
        //                rowCount++;
        //            }
        //        }
        //    }

        //    return notAffiliatedCourses;
        //}



        //private List<DegreewiseCollegeSpecializations> GetDegreewiseCollegeSpecializations(int CollegeId)
        //{
        //    List<CollegeFacultyLabs> affiliatedCourses = CollegeCoursesNotClear(CollegeId);
        //    int[] specializationids = affiliatedCourses.Select(a => a.SpecializationId).ToArray();

        //    List<DegreewiseCollegeSpecializations> DegreewiseCollegeSpecializationsList = new List<DegreewiseCollegeSpecializations>();
        //    List<Specializations> SpecializationList = (from ci in db.jntuh_college_intake_existing
        //                                                join s in db.jntuh_specialization on ci.specializationId equals s.id
        //                                                join de in db.jntuh_department on s.departmentId equals de.id
        //                                                join d in db.jntuh_degree on de.degreeId equals d.id
        //                                                where (specializationids.Contains(ci.specializationId) && ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && ci.collegeId == CollegeId && ci.academicYearId == nextAcademicYearId)//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                                        
        //                                                orderby d.degreeDisplayOrder
        //                                                select new Specializations
        //                                                {
        //                                                    CollegeId = ci.collegeId,
        //                                                    ShiftId = ci.shiftId,
        //                                                    ProposedIntake = ci.proposedIntake == null ? 0 : (int)ci.proposedIntake,//ci.approvedIntake == null ? 0 : (int)ci.approvedIntake
        //                                                    SpecializationId = ci.specializationId,
        //                                                    DepartmentId = de.id,
        //                                                    DegreeId = d.id,
        //                                                }).ToList();

        //    DegreeIds = db.jntuh_degree.Where(d => d.isActive == true && degrees.Contains(d.degree)).Select(d => d.id).ToArray();
        //    SpecializationList = SpecializationList.Where(d => DegreeIds.Contains(d.DegreeId)).ToList();
        //    var jntuh_degree = db.jntuh_degree.AsNoTracking().ToList();
        //    var jntuh_degree_type = db.jntuh_degree_type.AsNoTracking().ToList();
        //    var jntuh_specialization = db.jntuh_specialization.AsNoTracking().ToList();
        //    foreach (var item in SpecializationList)
        //    {
        //        string NewSpecialization = string.Empty;
        //        string NewIntake = string.Empty;
        //        DegreewiseCollegeSpecializations DegreewiseCollegeSpecializations = new DegreewiseCollegeSpecializations();
        //        item.DegreeTypeId = jntuh_degree.Where(degree => degree.id == item.DegreeId).Select(degree => degree.degreeTypeId).FirstOrDefault();
        //        string degreeType = jntuh_degree_type.Where(degree => degree.id == item.DegreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();
        //        string degreeName = jntuh_degree.Where(degree => degree.id == item.DegreeId && degree.isActive == true).Select(degree => degree.degree).FirstOrDefault();
        //        string specializationName = jntuh_specialization.Where(specialization => specialization.id == item.SpecializationId)
        //                                              .Select(specialization => specialization.specializationName).FirstOrDefault();
        //        if (degreeType != null)
        //        {
        //            if (degreeType.ToUpper().Trim() == "PG" || degreeType.ToUpper().Trim() == "UG")
        //            {
        //                if (degreeName == "MBA" || degreeName == "MCA")
        //                {
        //                    //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
        //                    NewIntake = item.ProposedIntake.ToString();
        //                    NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";
        //                }
        //                else
        //                {
        //                    //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
        //                    NewIntake = item.ProposedIntake.ToString();
        //                    NewSpecialization = item.ShiftId == 1 ? degreeName + "-" + specializationName : degreeName + "-" + specializationName + "-" + "(II Shift)";
        //                }
        //            }
        //            else
        //            {
        //                //NewIntake = GetIntake(item.DegreeId, item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
        //                NewIntake = item.ProposedIntake.ToString();
        //                NewSpecialization = item.ShiftId == 1 ? specializationName : specializationName + "-" + "(II Shift)";

        //            }
        //            DegreewiseCollegeSpecializations.Specialization = NewSpecialization;
        //            DegreewiseCollegeSpecializations.Intake = NewIntake;
        //            DegreewiseCollegeSpecializations.AvailableFaculty = affiliatedCourses.Where(i => i.SpecializationId == item.SpecializationId).FirstOrDefault().Available;
        //            DegreewiseCollegeSpecializations.RequiredFaculty = affiliatedCourses.Where(i => i.SpecializationId == item.SpecializationId).FirstOrDefault().Required;
        //            DegreewiseCollegeSpecializationsList.Add(DegreewiseCollegeSpecializations);
        //        }
        //    }
        //    return DegreewiseCollegeSpecializationsList;
        //}

        private string CollegeAddress(int CollegeId)
        {
            string Address = string.Empty;
            string District = string.Empty;
            jntuh_address CollegeAddress = db.jntuh_address
                                           .Where(a => a.addressTye == "COLLEGE" &&
                                                       a.collegeId == CollegeId)
                                           .FirstOrDefault();
            if (CollegeAddress.townOrCity != null || CollegeAddress.townOrCity != string.Empty)
            {
                Address += CollegeAddress.townOrCity + ",";
            }
            if (CollegeAddress.mandal != null || CollegeAddress.mandal != string.Empty)
            {
                Address += CollegeAddress.mandal + ",";
            }
            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district
                             .Where(d => d.id == CollegeAddress.districtId)
                             .Select(d => d.districtName)
                             .FirstOrDefault();
                Address += District + ",";
            }
            if (CollegeAddress.pincode != 0)
            {
                Address += CollegeAddress.pincode.ToString();
            }
            return Address;
        }
        private string CollegeDistrict(int CollegeId)
        {
            string District = string.Empty;
            jntuh_address CollegeAddress = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == CollegeId).FirstOrDefault();

            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district.Where(d => d.id == CollegeAddress.districtId).Select(d => d.districtName).FirstOrDefault();
            }

            return District;
        }

        private string CollegeEstablishYear(int CollegeId)
        {
            string Year = string.Empty;
            int EstablishYear = db.jntuh_college_establishment
                                  .Where(e => e.collegeId == CollegeId)
                                  .Select(e => e.instituteEstablishedYear)
                                  .FirstOrDefault();
            if (EstablishYear != 0)
            {
                Year = EstablishYear.ToString();
            }
            return Year;
        }
        private List<CounsellingReport> CounsellingReportList(int? CollegeId, string cmd)
        {
            if (cmd == "ALL" || cmd == "Export ALL")
            {
                DegreeIds = db.jntuh_degree.Where(d => d.isActive == true).Select(d => d.id).ToArray();
                ReportHeader = "ALL.xls";

                ViewBag.Name = cmd;
            }
            else
            {
                if (cmd == "B.Tech/B.Pharm" || cmd == "Export B.Tech/B.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech" || d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Tech/B.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Tech/M.Pharm" || cmd == "Export M.Tech/M.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Tech" || d.degree == "M.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Tech/M.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MBA/MCA" || cmd == "Export MBA/MCA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MBA" || d.degree == "MCA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MBA/MCA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "Pharm.D/Pharm.D PB" || cmd == "Export Pharm.D/Pharm.D PB")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D" || d.degree == "Pharm.D PB")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D/Pharm.D PB.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "MAM/MTM" || cmd == "Export MAM/MTM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM" || d.degree == "MTM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MAM/MTM.xls";
                    ViewBag.Name = cmd;
                }


                //------------------
                else if (cmd == "B.Tech" || cmd == "Export B.Tech")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Tech.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "B.Pharm" || cmd == "Export B.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "B.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Tech" || cmd == "Export M.Tech")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Tech")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Tech.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "M.Pharm" || cmd == "Export M.Pharm")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "M.Pharmacy")).Select(d => d.degree).ToArray();
                    ReportHeader = "M.Pharm.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MBA" || cmd == "Export MBA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MBA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MBA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MCA" || cmd == "Export MCA")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MCA")).Select(d => d.degree).ToArray();
                    ReportHeader = "MCA.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "Pharm.D" || cmd == "Export Pharm.D")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "Pharm.D PB" || cmd == "Export Pharm.D PB")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "Pharm.D PB")).Select(d => d.degree).ToArray();
                    ReportHeader = "Pharm.D PB.xls";
                    ViewBag.Name = cmd + "-";
                }
                else if (cmd == "MAM" || cmd == "Export MAM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MAM.xls";
                    ViewBag.Name = cmd;
                }
                else if (cmd == "MTM" || cmd == "Export MTM")
                {
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MTM")).Select(d => d.degree).ToArray();
                    ReportHeader = "MTM.xls";
                    ViewBag.Name = cmd;
                }
                //------------------
                else
                {
                    ViewBag.Name = "MAM/MTM";
                    degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "MAM" || d.degree == "MTM")).Select(d => d.degree).ToArray();
                    //degrees = db.jntuh_degree.Where(d => d.isActive == true && (d.degree == "B.Tech" || d.degree == "B.Pharmacy")).Select(d => d.degree).ToArray();           
                }
                DegreeIds = db.jntuh_degree.Where(d => d.isActive == true && degrees.Contains(d.degree)).Select(d => d.id).ToArray();
            }

            List<CounsellingReport> CounsellingReportList = new List<CounsellingReport>();
            if (CollegeId == null)
            {
                CounsellingReportList = (from ci in db.jntuh_college_intake_existing
                                         join s in db.jntuh_specialization on ci.specializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on ci.collegeId equals c.id
                                         where (ci.isActive == true && s.isActive == true&&ci.collegeId==366 && de.isActive == true && d.isActive == true && CollegeIds.Contains(c.id) && ci.academicYearId == nextAcademicYearId && DegreeIds.Contains(d.id))//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                         
                                         orderby d.degreeDisplayOrder
                                         select new CounsellingReport
                                         {
                                             CollegeId = ci.collegeId,
                                             CollegeCode = c.collegeCode,
                                             CollegeName = c.collegeName
                                         }).Distinct().ToList();


            }
            else
            {
                CounsellingReportList = (from ci in db.jntuh_college_intake_existing
                                         join s in db.jntuh_specialization on ci.specializationId equals s.id
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         join c in db.jntuh_college on ci.collegeId equals c.id
                                         where (ci.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && c.id == CollegeId && ci.academicYearId == nextAcademicYearId && DegreeIds.Contains(d.id))//&& ci.courseAffiliationStatusCodeId != ClosureCourseId)                                        
                                         orderby d.degreeDisplayOrder
                                         select new CounsellingReport
                                         {
                                             CollegeId = ci.collegeId,
                                             CollegeCode = c.collegeCode,
                                             CollegeName = c.collegeName
                                         }).Distinct().ToList();


            }

            foreach (var item in CounsellingReportList)
            {
                //Binding collegeCode CollegeName and specializationDetails Data to table
                //item.CollegeSpecializations = GetDegreewiseCollegeSpecializations(item.CollegeId);
                //item.CollegeAddress = CollegeAddress(item.CollegeId);
                //item.Establishyear = CollegeEstablishYear(item.CollegeId);
                //item.Grade = CollegeDistrict(item.CollegeId);
            }

            CounsellingReportList = CounsellingReportList.Where(c => c.CollegeSpecializations.Count() > 0).OrderBy(c => c.CollegeName).ToList();
            ViewBag.CollegeSpecializations = CounsellingReportList;
            //ViewBag.CollegeSpecializations = CounsellingReportList;
            ViewBag.Count = CounsellingReportList.Count();
            return CounsellingReportList;
        }

        public ActionResult CounsellingReportNew()
        {
            List<CounsellingReport> CounsellingList = new List<CounsellingReport>();
            //int CollegeId = CollegeId;
            GetIds();

            CounsellingList = CounsellingReportList(null, "ALL");


            ViewBag.CollegeSpecializations = CounsellingList.OrderBy(c => c.CollegeName).ToList();
            //ViewBag.CollegeSpecializations = CounsellingList;
            ViewBag.Count = CounsellingList.Count();
            int Count = CounsellingList.Count();


            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Reports/_CounsellingReport.cshtml", CounsellingList);

        }
        ////For ExportAllCOlleges intake



        //Physical Labs Code written by Srinivas
        public string DeficiencyPhysicalLabs(int? collegeID)
        {
            string annexure = string.Empty;
            int[] mbacollegeides = { 5, 67, 119, 174, 246, 296, 343, 355, 386, 394, 411, 413, 421, 424, 430, 449, 452 };
            int cid = Convert.ToInt32(collegeID);

             var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                if (collegeStatus.CollegeStatus == false && collegeStatus.AICTEStatus == false)
                {

                    List<physicalLabs> LabMaster = new List<physicalLabs>();
                    List<physicalLabs> CollegePhysicalLabMaster = new List<physicalLabs>();
                    //var jntuh_college_intake_existings = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9).Select(e => e).ToArray();

                    //int[] specializationIds = jntuh_college_intake_existings.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9).Select(e => e.specializationId).ToArray();
                    //string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();
                    //var jntuh_lab_masters = db.jntuh_lab_master.Where(L => L.DegreeID == 4 && specializationIds.Contains(L.SpecializationID)).GroupBy(L => new { L.Labcode, L.DepartmentID }).Select(L => L.FirstOrDefault()).ToList();

                    //if (CollegeAffiliationStatus == "Yes")
                    //    jntuh_lab_masters = jntuh_lab_masters.Where(L => L.CollegeId == collegeID).Select(L => L).ToList();
                    //else
                    //    jntuh_lab_masters = jntuh_lab_masters.Where(L => specializationIds.Contains(L.SpecializationID) && L.CollegeId == null).Select(L => L).ToList();
                    //string[] LabMaserLabCodes = jntuh_lab_masters.Select(L => L.Labcode).Distinct().ToArray();
                    //var jntuh_physical_labmasters = db.jntuh_physical_labmaster.Where(P => P.Collegeid == collegeID).GroupBy(P => new { P.Labcode, P.DepartmentId }).Select(P => P.FirstOrDefault()).ToList();
                    //var jntuh_degrees = db.jntuh_degree.Where(D => D.isActive == true).Select(D => D).ToList();
                    //LabMaster = jntuh_lab_masters.Select(L => new physicalLabs
                    //{
                    //    LabCode = L.Labcode,
                    //    Labname = L.LabName,
                    //    specializationid = L.SpecializationID,
                    //    departmentid = L.DepartmentID,
                    //    degreeid = L.DegreeID,
                    //    collegeId = L.CollegeId
                    //}).ToList();
                    ////.GroupBy(L => new { L.LabCode, L.department })
                    //foreach (var item in LabMaster)
                    //{
                    //    physicalLabs CollegephysicalLabs = new physicalLabs();
                    //    CollegephysicalLabs.Intake = jntuh_college_intake_existings.Where(E => E.specializationId == item.specializationid).Select(E => E.proposedIntake).FirstOrDefault();
                    //    if (CollegephysicalLabs.Intake > 120)
                    //    {
                    //        CollegephysicalLabs.NoOfAvailabeLabs = jntuh_physical_labmasters.Where(P => P.DepartmentId == item.departmentid && P.Numberofavilablelabs > 1).Sum(P => P.Numberofavilablelabs);
                    //        if (CollegephysicalLabs.NoOfAvailabeLabs < 4)
                    //        {
                    //            CollegephysicalLabs.LabCode = item.LabCode;
                    //            CollegephysicalLabs.Labname = item.Labname;
                    //            CollegephysicalLabs.departmentid = item.departmentid;
                    //            CollegephysicalLabs.specializationid = item.specializationid;
                    //            CollegephysicalLabs.NoOfRequiredLabs = 4;
                    //            CollegephysicalLabs.NoOfAvailabeLabs = CollegephysicalLabs.NoOfAvailabeLabs;//item.NoOfAvailabeLabs;
                    //            CollegephysicalLabs.degree = jntuh_degrees.Where(D => D.id == item.degreeid).Select(D => D.degree).FirstOrDefault();
                    //            CollegePhysicalLabMaster.Add(CollegephysicalLabs);
                    //        }

                    //    }


                    //}
                    //int?[] specializationIDS = CollegePhysicalLabMaster.Select(L => L.specializationid).Distinct().ToArray();
                    // var CollegePhysicalLabMasters=CollegePhysicalLabMaster.ToList();
                    int indexnow = 1;
                    string Department = "", Degree = "", Specialization = "";

                    //var jntuh_department = db.jntuh_department.Where(e => e.isActive == true).Select(e => e).ToList();
                    //var jntuh_specialization = db.jntuh_specialization.Where(e => e.isActive == true).Select(e => e).ToList();
                    
                    //Old Code Comment on 29-03-2018
                    //CollegePhysicalLabMaster =
                    //    db.jntuh_college_laboratories_deficiency.Where(e => e.CollegeId == collegeID)
                    //        .Select(e => new physicalLabs
                    //        {
                    //            department = e.LabCode,
                    //            NoOfAvailabeLabs = e.Semister
                    //        }).ToList();

                    CollegePhysicalLabMaster =
                        db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeID && e.Numberofrequiredlabs != null)
                            .Select(e => new physicalLabs
                            {
                                department = db.jntuh_department.Where(d => d.id == e.DepartmentId).Select(s => s.departmentName).FirstOrDefault(),
                                NoOfRequiredLabs = e.Numberofrequiredlabs,
                                Labname = e.LabName,
                                year = e.Year,
                                semister = e.Semister,
                                LabCode = e.Labcode,
                                NoOfAvailabeLabs = e.Numberofavilablelabs == null ? 0 : e.Numberofavilablelabs
                            }).ToList();
                    //CollegePhysicalLabMaster =
                    //    db.jntuh_physical_labmaster_copy.Where(e => e.Collegeid == collegeID&&e.Numberofrequiredlabs!=null)
                    //        .Select(e => new physicalLabs
                    //        {
                    //            department =db.jntuh_department.Where(d=>d.id==e.DepartmentId).Select(s=>s.departmentName).FirstOrDefault(),
                    //            NoOfRequiredLabs = e.Numberofrequiredlabs,
                    //            Labname = e.LabName,
                    //            year = e.Year,
                    //            semister = e.Semister,
                    //            LabCode = e.Labcode,
                    //            NoOfAvailabeLabs = db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeID &&  a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault() == null ? 0 : db.jntuh_physical_labmaster_copy.Where(a => a.Collegeid == collegeID && a.DepartmentId == e.DepartmentId && a.Semister == e.Semister && a.Year == e.Year && a.Labcode == e.Labcode).Select(s => s.Numberofavilablelabs).FirstOrDefault()
                    //        }).ToList();

                    //if (!mbacollegeides.Contains(cid))
                    //{
                        annexure += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
                        annexure += "<tr>";
                        annexure += "<td align='left'><b><u>Deficiencies in Physical Labs</u></b></td>";
                        annexure += "</tr>";
                        annexure += "</table>";
                        annexure += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";

                        annexure += "<tr>";
                        annexure +=
                            "<th align='left'>S.No</th><th align='left'>Department</th><th align='left'>Lab Name</th><th align='left'>Required Extra labs</th><th align='left'>Available Extra Labs</th><th align='left'>Deficiency</th>";
                        annexure += "</tr>";
                        if (CollegePhysicalLabMaster.Count != 0)
                        {
                            foreach (var item in CollegePhysicalLabMaster)
                            {
                                if (item.NoOfAvailabeLabs < item.NoOfRequiredLabs)
                                {
                                    annexure += "<tr>";
                                    annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + item.department +
                                                "</td><td  align='left'>" + item.year + "-" + item.semister + "--" + item.Labname +
                                                "</td><td  align='left'>" + item.NoOfRequiredLabs + "</td><td  align='left'>" +
                                                item.NoOfAvailabeLabs + "</td><td>Yes</td>";
                                    annexure += "</tr>";
                                    indexnow++;
                                }

                            }
                            if (indexnow == 1)
                            {
                                annexure += "<tr>";
                                annexure += "<td colspan='6' align='center'><b>NIL</b></td>";
                                annexure += "</tr>";
                            }
                        }
                        else
                        {
                            annexure += "<tr>";
                            annexure += "<td colspan='6' align='center'><b>NIL</b></td>";
                            annexure += "</tr>";
                        }


                        annexure += "</table>";
                        annexure +=
                       "<p><b>NOTE:</b> The Physical Verification of the faculty and their presence at the time of Inspection by the FFC, automatically does not mean that the college is entitled for Affiliation based on numbers. Those of the faculty who are having the requisite qualifications, Biometric attendance and credentials are verified and found correct will be taken into account for the purpose of granting affiliation.</p>";
                    //}

                   



                    //foreach (var speclializationId in specializationIDS)
                    //{
                    //    if (speclializationId != null)
                    //    {
                    //        var CollegePhysicalLabMasters = CollegePhysicalLabMaster.Where(L => L.specializationid == speclializationId).Select(L => L).FirstOrDefault();
                    //        Department = jntuh_department.Where(e => e.id == CollegePhysicalLabMasters.departmentid).Select(e => e.departmentName).FirstOrDefault();
                    //        Specialization = jntuh_specialization.Where(e => e.id == CollegePhysicalLabMasters.specializationid).Select(e => e.specializationName).FirstOrDefault();
                    //        Degree = CollegePhysicalLabMasters.degree;
                    //        //annexure += "<table width='100%' border='1' cellpadding='7' cellspacing='0'>";
                    //        //annexure += "<tr>";
                    //        //annexure += "<th align='center' colspan='5'>LIST OF EQUIPMENT NOT AVAILABLE IN " + Degree + " -" + Department + "-" + Specialization + "</th>";
                    //        //annexure += "</tr>";
                    //        //annexure += "<tr>";
                    //        //annexure += "<th align='left'>S.No</th><th align='left'>Lab Code</th><th align='left'>Required</th><th align='left'>Avilable</th><th align='left'>Lab Name</th>";
                    //        //annexure += "</tr>";
                    //        //foreach (var item in CollegePhysicalLabMaster.ToList())
                    //        //{
                    //        //    annexure += "<tr>";
                    //        //    annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + item.department + "</td><td  align='left'>" + item.LabCode + "</td><td  align='left'>" + item.NoOfRequiredLabs + "</td><td  align='left'>" + item.NoOfAvailabeLabs + "</td>";
                    //        //    annexure += "</tr>";
                    //        //    indexnow++;
                    //        //}
                    //        //annexure += "</table>";
                    //    }
                    //    else
                    //        annexure = " No Deficiency PhysicalLabs ";
                    //}


                   

                    annexure += "</br><table width='100%'  cellspacing='0'></br>";
                    annexure +=
                        "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>Sd /-</b></td></tr>";
                    annexure +=
                        "<tr><td align='right' width='80%' style='font-size: 14px; font-weight: normal;'> <b>REGISTRAR</b></td></tr></br></br>";
                    annexure +=
                        "</br><tr><td align='center' width='80%' style='font-size: 14px; font-weight: normal;' colspan='2'><b>The College shall submit Appeal, if any through Online mode only in the format " +
                        "prescribed within 10 Days from the date of this letter." + "</b></td></tr></br>";
                    annexure += "<tr><td></td></tr>";
                    annexure += "</table>";
                }
            }
            return annexure;
        }




        # region for excel download
        public ActionResult DeficienciesInFacultyExcel(int? collegeID)
        {
            string faculty = string.Empty;

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'><b><u>Deficiencies in Faculty</u></b> (Department/ Specialization Wise):";
            faculty += "</tr>";
            faculty += "</table>";

            List<CollegeFacultyWithIntakeReport> facultyCounts = collegeFacultyExcel(collegeID).Where(c => c.shiftId == 1).ToList();//Where(c => c.shiftId == 1)

            var count = facultyCounts.Count();
            var distDeptcount = 1;
            var deptloop = 1;
            decimal departmentWiseRequiredFaculty = 0;

            string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others" };

            int totalBtechFirstYearIntake = facultyCounts.Where(d => d.Degree == "B.Tech").Select(d => d.approvedIntake1).Sum();
            var degrees = db.jntuh_degree.ToList();
            var firstYearRequired = Math.Ceiling((double)totalBtechFirstYearIntake / 150);//120
            int remainingFaculty = 0;
            int remainingPHDFaculty = 0;

            faculty += "<table width='100%' border='1' cellpadding='3' cellspacing='0' style='border-color: #ccc;'>";
            faculty += "<tr>";
            faculty += "<th style='text-align: center; vertical-align: top;'>SNo</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Department</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Degree</th>";
            faculty += "<th style='text-align: left; vertical-align: top;' >Specialization</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Intake *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Total Faculty *</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Required</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Specialization Wise Faculty Available</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Form16 faculty</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Not Qualified as per AICTE faculty</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >SCM</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >DR</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >FA</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >IP</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency</th>";
            faculty += "<th style='text-align: center; vertical-align: top;' >Deficiency of Ph.D faculty</th>";
            faculty += "</tr>";

            foreach (var item in facultyCounts)
            {
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
                    item.totalBtechFirstYearIntake = totalBtechFirstYearIntake;
                    item.firstYearRequired = (int)firstYearRequired;
                }

                var degreeType = degrees.Where(d => d.degree == item.Degree).Select(d => d.jntuh_degree_type.degreeType).FirstOrDefault();

                if (deptloop == 1)
                {
                    if (rFaculty <= tFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = tFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                        item.adjustedFaculty = adjustedFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = tFaculty;
                        item.adjustedFaculty = adjustedFaculty;
                        facultyShortage = rFaculty - tFaculty;
                    }

                    remainingPHDFaculty = item.phdFaculty;

                    if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG"))) //degreeType.Equals("PG")
                    {
                        //adjustedPHDFaculty = 1;
                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                else
                {
                    if (rFaculty <= remainingFaculty)
                    {
                        minimumRequirementMet = "NO";
                        remainingFaculty = remainingFaculty - rFaculty;
                        adjustedFaculty = rFaculty;
                        item.adjustedFaculty = adjustedFaculty;
                    }
                    else
                    {
                        minimumRequirementMet = "YES";
                        adjustedFaculty = remainingFaculty;
                        item.adjustedFaculty = adjustedFaculty;
                        facultyShortage = rFaculty - remainingFaculty;
                        remainingFaculty = 0;
                    }
                    remainingPHDFaculty = item.phdFaculty;
                    if (remainingPHDFaculty > 0 && (degreeType.Equals("PG") || degreeType.Equals("UG")))
                    {
                        //adjustedPHDFaculty = 1;
                        adjustedPHDFaculty = remainingPHDFaculty;
                        remainingPHDFaculty = remainingPHDFaculty - 1;
                    }
                }
                item.minimumRequirementMet = minimumRequirementMet;
                faculty += "<tr>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;width:10px' rowspan='1'>" + (facultyCounts.IndexOf(item) + 1) + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Department + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Degree + "</td>";
                faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.Specialization + "</td>";

                if (departments.Contains(item.Department))
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + totalBtechFirstYearIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(firstYearRequired) + "</td>";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.totalIntake + "</td>";
                    faculty += "<td class='col2' style='text-align: left; vertical-align: top;'>" + item.A416TotalFaculty + "</td>";
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + Math.Ceiling(item.requiredFaculty) + "</td>";
                }

                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + adjustedFaculty + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.form16count + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.aictecount + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.NOSCM + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.DeactivationReasionsCount + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.FacultyAbsentCount + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + item.InvalidPanCount + "</td>";
                faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>" + minimumRequirementMet + "</td>";

                if (adjustedPHDFaculty > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                    item.PHDminimumRequirementMet = "NO";
                }
                else if (degreeType.Equals("PG") && item.approvedIntake1 > 0)
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>YES</td>";
                    item.PHDminimumRequirementMet = "YES";
                }
                else
                {
                    faculty += "<td class='col2' style='text-align: center; vertical-align: top;'>NO</td>";
                    item.PHDminimumRequirementMet = "NO";
                }

                faculty += "</tr>";


                deptloop++;
            }

            faculty += "</table>";

            faculty += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            faculty += "<tr>";
            faculty += "<td align='left'>* II, III & IV Year for B.Tech; I & II Year for M.Tech";
            faculty += "</tr>";
            faculty += "</table>";
            //return faculty;

            ViewBag.facultylist = facultyCounts;
            ReportHeader = facultyCounts.Count > 0 ? facultyCounts.FirstOrDefault().collegeCode + "- AllFlags.xls" : "AllFlags.xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Reports/_DeficiencyExcelReport.cshtml", facultyCounts);
        }


        public List<CollegeFacultyWithIntakeReport> collegeFacultyExcel(int? collegeId)
        {
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
                var collegedepts = collegeIntakeExisting.Select(i => i.DepartmentID).Distinct().ToList();
                collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId, r.collegeId }).Select(r => r.First()).ToList();

                //college Reg nos
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == collegeId).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();

                //education categoryIds UG,PG,PHD...........

                int pharmacyDeptId = db.jntuh_department.Where(d => d.departmentName == "Pharmacy").Select(d => d.id).FirstOrDefault();

                var jntuh_education_category = db.jntuh_education_category.ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber) && (rf.collegeId == null || rf.collegeId == collegeId)).ToList();
                // var registeredFaculty = db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber)).ToList();
                //Reg nos related online facultyIds`-- (rf.isApproved == null || rf.isApproved == true)
                var regfacultywithadjunct = registeredFaculty.Where(rf => rf.type == "Adjunct" && rf.DepartmentId != null && (rf.Absent == false && rf.NoSCM == false && rf.NoForm16 == false
                    && rf.InvalidPANNumber == false && rf.NotQualifiedAsperAICTE == false && rf.IncompleteCertificates == false)
                    && (rf.PANNumber != null || rf.AadhaarNumber != null)).ToList();

                var jntuh_registered_faculty1 = registeredFaculty.Where(rf => rf.type != "Adjunct" && rf.DepartmentId != null && (rf.Absent == false && rf.NoSCM == false && rf.NoForm16 == false
                    && rf.InvalidPANNumber == false && rf.NotQualifiedAsperAICTE == false && rf.IncompleteCertificates == false)
                    && (rf.PANNumber != null || rf.AadhaarNumber != null))
                                                 .Select(rf => new
                                                 {
                                                     //Departmentid = rf.DepartmentId,
                                                     RegistrationNumber = rf.RegistrationNumber,
                                                     Department = rf.jntuh_department.departmentName,
                                                     HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                                                     IsApproved = rf.isApproved,
                                                     PanNumber = rf.PANNumber,
                                                     AadhaarNumber = rf.AadhaarNumber,
                                                     NoForm16 = rf.NoForm16
                                                 }).ToList();
                //var nohighestdegree = jntuh_registered_faculty1.Where(e => e.HighestDegreeID == 0).ToList();
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
                    noform16 = rf.NoForm16
                }).Where(e => e.Department != null).ToList();
                var form16count = registeredFaculty.Where(i => i.NoForm16 == true).ToList();
                var aictecount = registeredFaculty.Where(i => i.NotQualifiedAsperAICTE == true).ToList();
                var NOSCMCount = registeredFaculty.Where(i => i.NoSCM == true).ToList();
                var DeactivationReasions = registeredFaculty.Where(i => !string.IsNullOrEmpty(i.PanDeactivationReason) || !string.IsNullOrEmpty(i.DeactivationReason)).ToList();
                var FacultyAbsent = registeredFaculty.Where(i => i.Absent == true).ToList();
                var InvalidPan = registeredFaculty.Where(i => i.InvalidPANNumber == true).ToList();
                var incompletecerificates = registeredFaculty.Where(i => i.IncompleteCertificates == true).ToList();
                foreach (var item in collegeIntakeExisting)
                {
                    CollegeFacultyWithIntakeReport intakedetails = new CollegeFacultyWithIntakeReport();
                    int phdFaculty = 0;
                    int pgFaculty = 0;
                    int ugFaculty = 0;

                    intakedetails.collegeId = item.collegeId;
                    intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    intakedetails.Degree = item.Degree;
                    intakedetails.Department = item.Department;
                    intakedetails.Specialization = item.Specialization;
                    intakedetails.specializationId = item.specializationId;
                    intakedetails.DepartmentID = item.DepartmentID;
                    intakedetails.shiftId = item.shiftId;

                    intakedetails.approvedIntake1 = GetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake2 = GetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake3 = GetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                    intakedetails.approvedIntake4 = GetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                    facultystudentRatio = Convert.ToInt32(jntuh_faculty_student_ratio_norms.Where(fn => fn.degreeId == item.degreeID).Select(fn => fn.Norms).FirstOrDefault().Split(':')[1].ToString());

                    var csedept = jntuh_registered_faculty.Where(i => i.Department == item.Department).ToList();
                    intakedetails.form16count = form16count != null ? form16count.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.aictecount = aictecount.Count > 0 ? aictecount.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.NOSCM = NOSCMCount.Count > 0 ? NOSCMCount.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.DeactivationReasionsCount = DeactivationReasions.Count > 0 ? DeactivationReasions.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.FacultyAbsentCount = FacultyAbsent.Count > 0 ? FacultyAbsent.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.InvalidPanCount = InvalidPan.Count > 0 ? InvalidPan.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.incompletecerificatesCount = incompletecerificates.Count > 0 ? incompletecerificates.Where(i => i.DepartmentId == item.DepartmentID).Count() : 0;
                    intakedetails.adjointfacultycount = regfacultywithadjunct.Where(i => i.DepartmentId == item.DepartmentID).Count();
                    if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                        //intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == "B.Tech" || i.jntuh_department.jntuh_degree.degree == "B.Pharmacy")).ToList().Count;
                    }
                    else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);

                    }
                    else if (item.Degree == "MCA")
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3);
                        facultyRatio = Convert.ToDecimal(intakedetails.totalIntake) / Convert.ToDecimal(facultystudentRatio);
                    }
                    else //MAM MTM Pharm.D Pharm.D PB
                    {
                        intakedetails.totalIntake = (intakedetails.approvedIntake1) + (intakedetails.approvedIntake2) + (intakedetails.approvedIntake3) + (intakedetails.approvedIntake4);
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && f.Recruitedfor == "UG").Count();
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
                            intakedetails.specializationWiseFaculty = jntuh_registered_faculty.Where(f => f.Department == item.Department && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG") && f.SpecializationId == item.specializationId).Count();
                        }
                    }
                    intakedetails.id = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == item.specializationId && fd.ShiftId == item.shiftId).Select(fd => fd.Id).FirstOrDefault();

                    if (intakedetails.id > 0)
                    {
                        int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                        if (swf != null)
                        {
                            intakedetails.specializationWiseFaculty = (int)swf;
                        }
                        intakedetails.deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Deficiency).FirstOrDefault();
                        intakedetails.shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == intakedetails.id).Select(fd => fd.Shortage).FirstOrDefault();
                    }

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
                        ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == item.Department).Count();
                        pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department).Count();
                        phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Count();
                        noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == item.DepartmentID && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                    }

                    intakedetails.phdFaculty = phdFaculty;
                    intakedetails.pgFaculty = pgFaculty;
                    intakedetails.ugFaculty = ugFaculty;
                    intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);
                    intakedetails.A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree || i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
                    //intakedetails.NOSCM = registeredFaculty.Where(i => i.DepartmentId == item.DepartmentID && (i.jntuh_department.jntuh_degree.degree == item.Degree || i.jntuh_department.jntuh_degree.degree == item.Degree)).ToList().Count;
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
                        var deptid = jntuh_departments.Where(i => i.departmentName == department).Select(i => i.id).FirstOrDefault();
                        int speId = jntuh_specialization.Where(s => s.jntuh_department.departmentName == department).Select(s => s.id).FirstOrDefault();
                        int ugFaculty = jntuh_registered_faculty.Where(f => "UG" == f.HighestDegree && f.Department == department).Count();
                        int pgFaculty = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Count();
                        int phdFaculty = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == department).Count();

                        int facultydeficiencyId = jntuh_college_faculty_deficiency.Where(fd => fd.CollegeId == collegeId && fd.SpecializationId == speId && fd.ShiftId == 1).Select(fd => fd.Id).FirstOrDefault();
                        if (facultydeficiencyId == 0)
                        {
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
                                form16count = form16count != null ? form16count.Where(i => i.DepartmentId == deptid).Count() : 0,
                                aictecount = aictecount.Count > 0 ? aictecount.Where(i => i.DepartmentId == deptid).Count() : 0,
                                NOSCM = NOSCMCount.Count > 0 ? NOSCMCount.Where(i => i.DepartmentId == deptid).Count() : 0,
                                DeactivationReasionsCount = DeactivationReasions.Count > 0 ? DeactivationReasions.Where(i => i.DepartmentId == deptid).Count() : 0,
                                FacultyAbsentCount = FacultyAbsent.Count > 0 ? FacultyAbsent.Where(i => i.DepartmentId == deptid).Count() : 0,
                                InvalidPanCount = InvalidPan.Count > 0 ? InvalidPan.Where(i => i.DepartmentId == deptid).Count() : 0,
                                A416TotalFaculty = registeredFaculty.Where(i => i.DepartmentId == deptid).ToList().Count,
                            });
                        }
                        else
                        {
                            int? swf = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.SpecializationWiseFaculty).FirstOrDefault();
                            bool deficiency = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Deficiency).FirstOrDefault();
                            int shortage = jntuh_college_faculty_deficiency.Where(fd => fd.Id == facultydeficiencyId).Select(fd => fd.Shortage).FirstOrDefault();
                            intakedetailsList.Add(new CollegeFacultyWithIntakeReport { id = facultydeficiencyId, collegeId = (int)collegeId, Degree = "B.Tech", Department = department, Specialization = department, ugFaculty = ugFaculty, pgFaculty = pgFaculty, phdFaculty = phdFaculty, totalFaculty = ugFaculty + pgFaculty + phdFaculty, specializationId = speId, shiftId = 1, specializationWiseFaculty = (int)swf, deficiency = deficiency, shortage = shortage });
                        }
                    }
                }
            }

            return intakedetailsList;
        }
        #endregion
    }
}
