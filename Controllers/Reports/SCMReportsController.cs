using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using UAAAS.Controllers.College;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class SCMReportsController : Controller
    {

        uaaasDBContext db=new uaaasDBContext();
        private string serverURL;

        #region SCM Download
        public ActionResult CollegeScmPrint(int collegeId)
        {
            if (collegeId != 0)
            {
                var collegedata =db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
                    var pdfPath = string.Empty;
                    int preview = 0;
                    if (preview == 0)
                    {
                        pdfPath = SaveSCMReportPdf(preview, collegeId);
                        pdfPath = pdfPath.Replace("/", "\\");
                    }
                if (collegedata != null)
                    return File(pdfPath, "application/pdf", collegedata.collegeCode + "- SCM Report File - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");
            }
            return RedirectToAction("SCMRequestsList", "CollegeSCMProceedingsRequest");
        }

        public string SaveSCMReportPdf(int preview, int collegeId)
        {
            string fullPath = string.Empty;
            var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMReportDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMReportDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMReportDownload"));
            }

            if (preview == 0)
            {
                fullPath = path + "/" + collegedata.collegeCode + "- SCM Report Download -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeCode = collegedata.collegeCode; 
                iTextEvents.CollegeName = collegedata.collegeName;
                iTextEvents.formType = "Scm Report Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMReportDownload.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

          //  contents = contents.Replace("##SERVERURL##", serverURL);


            contents = GetScmReportData(contents,collegeId);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
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


        public string GetScmReportData(string contents,int collegeId)
        {
            string contentdata = string.Empty;

            var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking().ToList();


           

            var collegedata =db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            var scmregdata = (from a in db.jntuh_scmproceedingsrequests
                join b in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId into abdata
                from ab in abdata.DefaultIfEmpty()
                join c in db.jntuh_registered_faculty on ab.RegistrationNumber equals c.RegistrationNumber into abcdata
                from abc in abcdata.DefaultIfEmpty()
                //join d in db.jntuh_registered_faculty_education on abc.id equals d.facultyId into abcddata
                //from abcd in abcddata.DefaultIfEmpty()
                where a.CollegeId == collegeId
                select new
                {
                    FacultyName=abc.FirstName+" "+abc.LastName,
                    RegNo=abc.RegistrationNumber,
                    PANNo=abc.PANNumber,
                    Experience=abc.TotalExperience,
                   facultyId=(int?)abc.id,
                   
                }).ToList();


            scmregdata = scmregdata.Where(e => e.RegNo != null && e.facultyId != null).Select(e => e).ToList();




            if (collegedata != null)
            {
                contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
                contentdata += "<tr><td style='text-align:left' width='90%'><b>College Name : </b>" + collegedata.collegeName + "</td>";
                contentdata += "<td style='text-align:left' width='20%'><b>Code : </b>" + collegedata.collegeCode + "</td></tr>";
                contentdata += "</table>";
            }
            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='70%'><b>Proceeding No : </b></td>";
            contentdata += "<td style='text-align:left' width='30%'><b>Date : </b></td></tr>";
            contentdata += "</table>";

            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:center'><b>Affiliated to</b></td></tr>";
            contentdata += "<tr><td style='text-align:center'><b>(JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD KUKATPALLY, HYDERABAD)</b></td></tr>";
            contentdata += "</table>";

            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left'>Minutes of the Selection Committee Meeting held on--------------------- at ------------------------</td></tr></table>";
           

            contentdata += "<br/><table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='30%'><b>Post</b></td><td>:</td><td></td></tr>";
            contentdata += "<tr><td style='text-align:left' width='30%'><b>Department</b></td><td>:</td><td></td></tr>";
            contentdata += "<tr><td style='text-align:left' width='30%'><b>Scale of Pay</b></td><td>:</td><td></td></tr>";
            contentdata += "</table><br/>";

            contentdata += "<table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='8%'><b>Sno</b></td>";
            contentdata += "<td style='text-align:left' width='22%'><b>Faculty Name</b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b>Reg. Number</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>PAN Number</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>UG Branch</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>PG Branch</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Ph.D</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Experience</b></td>";
            
            if (scmregdata.Count() != 0)
            {
                for (int i = 0; i < scmregdata.Count(); i++)
                {

                    string ugBranch = string.Empty;
                    string pgBranch = string.Empty;
                    string phdBranch = string.Empty;

                  
                    if(scmregdata[i].facultyId!=null)
                    { 
                    var ugdata =jntuh_registered_faculty_education.Where(e => e.educationId == 3 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                    if (ugdata != null)
                        ugBranch = ugdata;

                    var pgdata = jntuh_registered_faculty_education.Where(e => e.educationId == 4 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                    if (pgdata != null)
                        pgBranch = pgdata;

                    var phddata = jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                    if (phddata != null)
                        phdBranch = phddata;
                    }

                    
                            
                   


                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left' width='8%'>" + (i + 1) + "</td>";
                    contentdata += "<td style='text-align:left' width='22%'>" + scmregdata[i].FacultyName + "</td>";
                    contentdata += "<td style='text-align:left' width='20%'>"+scmregdata[i].RegNo+"</td>";
                    contentdata += "<td style='text-align:left' width='10%'>"+scmregdata[i].PANNo+"</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + ugBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + pgBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + phdBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].Experience + "</td>";
                   
                    contentdata += "</tr>";
                }
            }
            contentdata += "</table><br/>";


            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='10%'><b>Sno</b></td>";
            contentdata += "<td style='text-align:left' width='30%' colspan='2'><b>Role</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b>Name</b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b>Signature</b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>1</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>Chairperson</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>2</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>Principal</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>3</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 1</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>4</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 2</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>5</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>Expert 1</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>6</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>Expert 2</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "<tr><td style='text-align:left' width='10%'><b>7</b></td>";
            contentdata += "<td style='text-align:left' width='25%'><b>SC/ST/OBC/Women/<br/>Differently Abled if any</b></td>";
            contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            contentdata += "</table>";
            contents = contents.Replace("##SCMREPORT##", contentdata);
            return contents;
        }

        #endregion



        #region SCM Download Based On The Branch Wise
        public ActionResult CollegeScmPrintDeptWise(int collegeId, int specializationId)
        {
            if (collegeId != 0 && specializationId != 0)
            {
                var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
                var pdfPath = string.Empty;
                int preview = 0;
                if (preview == 0)
                {
                    pdfPath = SaveSCMReportPdfDeptWise(preview, collegeId, specializationId);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                if (collegedata != null)
                    return File(pdfPath, "application/pdf", collegedata.collegeCode + "- SCM Report File - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");
            }
            return RedirectToAction("SCMRequestsList", "CollegeSCMProceedingsRequest");
        }

        public string SaveSCMReportPdfDeptWise(int preview, int collegeId, int specializationId)
        {
            string fullPath = string.Empty;
            var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            var speci = db.jntuh_specialization.Where(e => e.id == specializationId).Select(e => e).FirstOrDefault();
            var Dept = "";
            if (speci != null)
            {
                 Dept = speci.jntuh_department.jntuh_degree.degree + "-" + speci.specializationName;
            }
            //Set page size as A4
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            string path = Server.MapPath("~/Content/PDFReports/SCMReportDownload");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMReportDownload")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMReportDownload"));
            }

            if (preview == 0)
            {
                fullPath = path + "/" + collegedata.collegeCode + "- SCM Report Download -" + Dept + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeCode = collegedata.collegeCode;
                iTextEvents.CollegeName = collegedata.collegeName;
                iTextEvents.formType = "Scm Report Download" + Dept;
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMReportDownloadDeptWise.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            //  contents = contents.Replace("##SERVERURL##", serverURL);


            contents = GetScmReportDataDeptWise(contents, collegeId, specializationId);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
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


        public string GetScmReportDataDeptWise(string contents, int collegeId, int specializationId)
        {
            string contentdata = string.Empty;

            var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.Where(e => e.isActive == true).Select(e => e).ToList();
            var collegedata = jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            var scmregdata = (from scmreq in db.jntuh_scmproceedingsrequests
                              join scmaddfaculty in db.jntuh_scmproceedingrequest_addfaculty on scmreq.ID equals scmaddfaculty.ScmProceedingId
                              join regfaculty in db.jntuh_registered_faculty on scmaddfaculty.RegistrationNumber equals regfaculty.RegistrationNumber 
                              join speci in db.jntuh_specialization on scmreq.SpecializationId equals speci.id
                              join deg in db.jntuh_degree on scmreq.DegreeId equals deg.id
                              join dept in db.jntuh_department on scmreq.DEpartmentId equals dept.id
                              where scmreq.CollegeId == collegeId && scmreq.SpecializationId == specializationId
                              select new
                              {
                                  FacultyName = regfaculty.FirstName + " " + regfaculty.LastName,
                                  RegNo = regfaculty.RegistrationNumber,
                                  PANNo = regfaculty.PANNumber,
                                  Experience = regfaculty.TotalExperience,
                                  facultyId = (int?)regfaculty.id,
                                 Branch=deg.degree+"-"+speci.specializationName,
                                  DeactivatioReason = scmaddfaculty.DeactiviationReason ?? "",
                                  PreviousCollege = scmaddfaculty.PreviousCollegeId,
                                  Remarks=scmreq.Remarks
                                // PreviousCollege = jntuh_college.Where(e => e.id == scmaddfaculty.PreviousCollegeId).Select(e=>e.collegeName).FirstOrDefault(),
                              }).ToList();


            scmregdata = scmregdata.Where(e => e.RegNo != null && e.facultyId != null).Select(e => e).ToList();




            if (collegedata != null)
            {
                contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
                contentdata += "<tr><td style='text-align:left' width='90%'><b>College Name : </b>" + collegedata.collegeName + "</td>";
                contentdata += "<td style='text-align:left' width='20%'><b>Code : </b>" + collegedata.collegeCode + "</td></tr>";
                contentdata += "</table>";
            }
            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='70%'><b>Proceeding No : </b></td>";
            contentdata += "<td style='text-align:left' width='30%'><b>Date : </b></td></tr>";
            contentdata += "</table>";

            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:center'><b>Affiliated to</b></td></tr>";
            contentdata += "<tr><td style='text-align:center'><b>(JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD KUKATPALLY, HYDERABAD)</b></td></tr>";
            contentdata += "</table>";

            contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left'>Minutes of the Selection Committee Meeting held on--------------------- at ------------------------</td></tr></table>";


            contentdata += "<br/><table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='30%'><b>Post</b></td><td  width='5%'>:</td><td  style='text-align:left'></td></tr>";
            if (scmregdata.Count()!=0)
            {
                contentdata += "<tr><td style='text-align:left' width='30%'><b>Department</b></td><td width='5%'>:</td><td style='text-align:left'>" + scmregdata[0].Branch + "</td></tr>";
            }
            else
            {
                contentdata += "<tr><td style='text-align:left' width='30%'><b>Department</b></td><td width='5%'>:</td><td style='text-align:left'></td></tr>";
            }
            contentdata += "<tr><td style='text-align:left' width='30%'><b>Scale of Pay</b></td><td width='5%'>:</td><td style='text-align:left'></td></tr>";
            contentdata += "</table><br/>";

            contentdata += "<table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='8%'><b>Sno</b></td>";
            contentdata += "<td style='text-align:left' width='22%'><b>Faculty Name</b></td>";
            contentdata += "<td style='text-align:left' width='20%'><b>Reg. Number</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>PAN Number</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>UG Branch</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>PG Branch</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Ph.D</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Experience</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Previous College</b></td>";
            contentdata += "<td style='text-align:left' width='10%'><b>Remarks</b></td>";
            contentdata += "</tr>";
            if (scmregdata.Count() != 0)
            {
                for (int i = 0; i < scmregdata.Count(); i++)
                {

                    string ugBranch = string.Empty;
                    string pgBranch = string.Empty;
                    string phdBranch = string.Empty;


                    if (scmregdata[i].facultyId != null)
                    {
                        var ugdata = jntuh_registered_faculty_education.Where(e => e.educationId == 3 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (ugdata != null)
                            ugBranch = ugdata;

                        var pgdata = jntuh_registered_faculty_education.Where(e => e.educationId == 4 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (pgdata != null)
                            pgBranch = pgdata;

                        var phddata = jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (phddata != null)
                            phdBranch = phddata;
                    }






                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left' width='8%'>" + (i + 1) + "</td>";
                    contentdata += "<td style='text-align:left' width='22%'>" + scmregdata[i].FacultyName + "</td>";
                    contentdata += "<td style='text-align:left' width='20%'>" + scmregdata[i].RegNo + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].PANNo + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + ugBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + pgBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + phdBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].Experience + "</td>";
                   // if (scmregdata[i].PreviousCollege != null)
                      //  contentdata += "<td style='text-align:left' width='10%'>" + jntuh_college.Where(e => e.id == scmregdata[i].PreviousCollege).Select(e => e.collegeName).FirstOrDefault() + "</td>";
                  //  else
                        contentdata += "<td style='text-align:left' width='10%'></td>";
                    contentdata += "<td style='text-align:left' width='10%'>" + scmregdata[i].Remarks + "</td>";
                    contentdata += "</tr>";
                }
            }
            contentdata += "</table><br/>";


            //contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
            //contentdata += "<tr><td style='text-align:left' width='10%'><b>Sno</b></td>";
            //contentdata += "<td style='text-align:left' width='30%' colspan='2'><b>Role</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b>Name</b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b>Signature</b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>1</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>Chairperson</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>2</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>Principal</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>3</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 1</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>4</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>University Nominee 2</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>5</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>Expert 1</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>6</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>Expert 2</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";

            //contentdata += "<tr><td style='text-align:left' width='10%'><b>7</b></td>";
            //contentdata += "<td style='text-align:left' width='25%'><b>SC/ST/OBC/Women/<br/>Differently Abled if any</b></td>";
            //contentdata += "<td style='text-align:left' width='5%'><b>:</b></td>";
            //contentdata += "<td style='text-align:left' width='40%'><b></b></td>";
            //contentdata += "<td style='text-align:left' width='20%'><b></b></td></tr>";
            //contentdata += "</table>";

            contents = contents.Replace("##SCMREPORT##", contentdata);
            return contents;
        }

        #endregion


        public ActionResult CollegeFacultyDetailsPrint(int collegeId, int scmid)
        {
            if (collegeId != 0)
            {
                serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
                var pdfPath = string.Empty;
                int preview = 0;
                if (preview == 0)
                {
                    pdfPath = SaveSCMFacultyDetailsPdf(preview, collegeId, scmid);
                    pdfPath = pdfPath.Replace("/", "\\");
                }
                if (collegedata != null)
                    return File(pdfPath, "application/pdf", collegedata.collegeCode + "- SCM Faculty Details - " + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf");
            }
            return RedirectToAction("SCMRequestsList", "CollegeSCMProceedingsRequest");
        }


        public string SaveSCMFacultyDetailsPdf(int preview, int collegeId, int scmid)
        {
            var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            string fullPath = string.Empty;
            //Set page size as A4
            //Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);
            Document pdfDoc = new Document(PageSize.A4.Rotate());
           // pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
            string path = Server.MapPath("~/Content/PDFReports/SCMFacultyDetails");
            if (!Directory.Exists(Server.MapPath("~/Content/PDFReports/SCMFacultyDetails")))
            {
                Directory.CreateDirectory(Server.MapPath("~/Content/PDFReports/SCMFacultyDetails"));
            }

            if (preview == 0)
            {
                fullPath = path + "/" + collegedata.collegeCode + "- SCM Faculty Details -" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";//
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeCode = collegedata.collegeCode;
                iTextEvents.CollegeName = collegedata.collegeName; 
                iTextEvents.formType = "Scm Report Download";
                pdfWriter.PageEvent = iTextEvents;
            }
            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/SCMFacultyData.html"));
            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            //  contents = contents.Replace("##SERVERURL##", serverURL);

            contents = contents.Replace("##SERVERURL##", serverURL);
            contents = GetScmFacultyDetailsData(contents, collegeId,scmid);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);
            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;
            int count = 0;
            foreach (var htmlElement in parsedHtmlElements)
            {
                count++;
                if (count == 100)
                {

                }
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


        public string GetScmFacultyDetailsData(string contents, int collegeId, int scmid)
        {
            string contentdata = string.Empty;

            var jntuh_registered_faculty_education = db.jntuh_registered_faculty_education.AsNoTracking().ToList();


          
            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.AsNoTracking().ToList();
            var jntuh_college = db.jntuh_college.ToList();
         

            var collegedata = db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            var scmregdata = (from a in db.jntuh_scmproceedingsrequests
                              join b in db.jntuh_scmproceedingrequest_addfaculty on a.ID equals b.ScmProceedingId into abdata
                              from ab in abdata.DefaultIfEmpty()
                              join c in db.jntuh_registered_faculty on ab.RegistrationNumber equals c.RegistrationNumber into abcdata
                              from abc in abcdata.DefaultIfEmpty()
                             join d in db.jntuh_specialization on a.SpecializationId equals d.id into abcddata
                              from abcd in abcddata.DefaultIfEmpty()
                              join e in db.jntuh_degree on a.DegreeId equals e.id into abcdedata
                              from abcde in abcdedata.DefaultIfEmpty()
                              join f in db.jntuh_department on abcd.departmentId equals f.id into abcdefdata
                              from abcdef in abcdefdata.DefaultIfEmpty()
                              where a.CollegeId == collegeId && ab.ScmProceedingId==scmid
                              select new
                              {
                                  FacultyName = abc.FirstName + " " + abc.LastName,
                                  RegNo = abc.RegistrationNumber,
                                  PANNo = abc.PANNumber,
                                  Experience = abc.TotalExperience,
                                  facultyId = (int?)abc.id,
                                  Depart=abcde.degree+" - "+abcd.specializationName,
                                  DepartmentName=abcdef.departmentName
                              }).ToList();


            scmregdata = scmregdata.Where(e => e.RegNo != null && e.facultyId != null).Select(e => e).ToList();


            var collegeData =
                db.jntuh_college.Where(e => e.isActive == true && e.id == collegeId).Select(e => e).FirstOrDefault();
            if (collegeData != null)
            {
                contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
                    contentdata += "<tr><td style='text-align:left' colspan='2' width='70%'><b>College Name :</b>" +collegedata.collegeName + "</td>";
                    contentdata += "<td style='text-align:left' width='30%'><b>College Code :</b>" + collegedata.collegeCode + "</td></tr>";
                    contentdata += "<tr><td style='text-align:left' width='20%'><b>Department :</b>" + scmregdata[0].DepartmentName + "</td>";
                    contentdata += "<td style='text-align:left' width='40%'><b>Branch/Specialization :</b>" + scmregdata[0].Depart + "</td>";
                    contentdata += "<td style='text-align:left' width='40%'><b>Applied for the post of :</b>Professor/ Associate Prof./ Assistant Prof.</td></tr>";
                contentdata += "</table><br/>";
            }
            else
            {
                contentdata += "<table border='0'cellspacing='0' cellpadding='4' width='100%'>";
                contentdata += "<tr><td style='text-align:left' colspan='2' width='70%'><b>College Name : </b></td>";
                contentdata += "<td style='text-align:left' width='30%'><b>College Code : </b></td></tr>";
                contentdata += "<tr><td style='text-align:left' width='20%'><b>Department :</b></td>";
                contentdata += "<td style='text-align:left' width='40%'><b>Branch/Specialization :</b></td>";
                contentdata += "<td style='text-align:left' width='40%'><b>Applied for the post of :</b>Professor/ Associate Prof./ Assistant Prof.</td></tr>";
                contentdata += "</table><br/>";
            }
            





            contentdata += "<table border='1'cellspacing='0' cellpadding='4' width='100%'>";
            contentdata += "<tr><td style='text-align:left' width='3.5%'><b>SNo</b></td>";
            contentdata += "<td style='text-align:left' width='15%'><b>Faculty Name</b></td>";
            contentdata += "<td style='text-align:left' width='11%'><b>Reg. Number</b></td>";
            contentdata += "<td style='text-align:left' width='9%'><b>PAN Number</b></td>";
            contentdata += "<td style='text-align:left' width='8%'><b>UG Branch</b></td>";
            contentdata += "<td style='text-align:left' width='9%'><b>PG Specialization</b></td>";
            contentdata += "<td style='text-align:left' width='6%'><b>Ph.D</b></td>";
            contentdata += "<td style='text-align:left' width='8%'><b>Experience</b></td>";
            contentdata += "<td style='text-align:left' width='18%'><b>Previous Working College</b></td>";
            contentdata += "<td style='text-align:left' width='12.5%'><b>Remarks<br/>Eligible/Not Eligible</b></td></tr>";
           
            if (scmregdata.Count() != 0)
            {
                for (int i = 0; i < scmregdata.Count(); i++)
                {

                    string ugBranch = string.Empty;
                    string pgBranch = string.Empty;
                    string phdBranch = string.Empty;

                  
                    string othercollege = string.Empty;
                   
                    if (scmregdata[i].facultyId != null)
                    {
                        var ugdata = jntuh_registered_faculty_education.Where(e => e.educationId == 3 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (ugdata != null)
                            ugBranch = ugdata;

                        var pgdata = jntuh_registered_faculty_education.Where(e => e.educationId == 4 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (pgdata != null)
                            pgBranch = pgdata;

                        var phddata = jntuh_registered_faculty_education.Where(e => e.educationId == 6 && e.facultyId == scmregdata[i].facultyId).Select(e => e.specialization).FirstOrDefault();
                        if (phddata != null)
                            phdBranch = phddata;
                    }

                   
                    var CollegeId =
                        jntuh_college_faculty_registered.Where(
                            e => e.RegistrationNumber.Trim() == scmregdata[i].RegNo.Trim())
                            .Select(e => e.collegeId).ToList();

                    var collegeName = "N/A";
                    if (CollegeId.Count() != 0)
                    {
                        collegeName = jntuh_college.Where(e => e.id == CollegeId[0]).Select(e => e.collegeName).FirstOrDefault();
                    }


                    contentdata += "<tr>";
                    contentdata += "<td style='text-align:left' width='3.5%'>" + (i + 1) + "</td>";
                    contentdata += "<td style='text-align:left' width='15%'>" + scmregdata[i].FacultyName + "</td>";
                    contentdata += "<td style='text-align:left' width='11%'>" + scmregdata[i].RegNo + "</td>";
                    contentdata += "<td style='text-align:left' width='9%'>" + scmregdata[i].PANNo + "</td>";
                    contentdata += "<td style='text-align:left' width='8%'>" + ugBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='9%'>" + pgBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='6%'>" + phdBranch + "</td>";
                    contentdata += "<td style='text-align:left' width='8%'>" + scmregdata[i].Experience + "</td>";
                    contentdata += "<td style='text-align:left;font-size:8px' width='18%'>" + collegeName + "</td>";
                    contentdata += "<td style='text-align:left' width='12.5%'></td>";
                    contentdata += "</tr>";
                }
            }
            contentdata += "</table><br/><br/>";
            contentdata += "<p style='text-align:right;font-size:10px'><strong>Signature of Scrutiny Committee</strong></p>";
            contents = contents.Replace("##SCMFACULTY##", contentdata);
            return contents;
        }

    }
}
