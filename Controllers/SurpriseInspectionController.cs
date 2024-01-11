using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class SurpriseInspectionController : Controller
    {
        //
        // GET: /SurpriseInspection/
        private uaaasDBContext db = new uaaasDBContext();
        public List<string> DegreeFacultyRegnos = new List<string>();

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult SurpriseInspection(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
                Response.ClearContent();
                Response.ClearHeaders();
                Response.Buffer = true;
                var CollegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault();

                Response.AddHeader("content-disposition", "attachment; filename=" + CollegeCode + "-SurpriseInspection-Deficiency-Report-" + id + ".doc");
                Response.ContentType = "application/vnd.ms-word ";
                Response.Charset = string.Empty;
                StringBuilder str = new StringBuilder();
                str.Append(SurpriseHeader(collegeId));
                str.Append("<br />");
                str.Append(SurpriseCollegeInformation(collegeId));
                str.Append("<br />");
                str.Append(GrantedCourses(collegeId));
                str.Append("<br />");
                str.Append(SurpriseStaff(collegeId));
                str.Append("<br />");
                str.Append(CollegeDetails(collegeId));
                str.Append("<br />");
                //str.Append(SurpriseClasswork(collegeId));
                str.Append(DeficiencyCollegeLabsAnnexure(collegeId));
                str.Append("<br />");
                str.Append(FinalCommentsAndMembers(collegeId));
                str.Append("<br />");

                Document pdfDoc = new Document(PageSize.A4.Rotate(), 60, 50, 60, 60);

                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                pdfDoc.SetMargins(60, 50, 60, 60);

                string path = Server.MapPath("~/Content/PDFReports/SurpriceInspection/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path = path + CollegeCode + "-" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
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

        public string SurpriseHeader(int? collegeId)
        {
            string header = string.Empty;


            header += "<table width='100%'>";
            header += "<tr>";
            header += "<td rowspan='4' align='center' width='20%'><img src='http://jntuhaac.in/Content/images/jntuhlogo.png' height='80' width='80' style='text-align: center' align='middle' /></td>";
            header += "<td align='center' width='80%' style='font-size: 14px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD</b></td>";
            header += "</tr>";
            //header += "<td align='center'><img src='http://jntuhaac.in/Content/images/jntuhlogo.png' height='70' width='70' style='text-align: center' align='middle' /></td></tr>";
            //header += "<tr><td align='center' width='80%' style='font-size: 20px; font-weight: normal;'><b>JAWAHARLAL NEHRU TECHNOLOGICAL UNIVERSITY HYDERABAD,</b></td>";

            header += "</tr>";
            header += "<tr><td align='center' width='80%' style='font-weight: normal;'><b>KUKATPALLY, HYDERABAD, TELANGANA-500085</b></td></tr>";
            header += "<tr>";
            header += "<td  align='center' style='font-weight: normal;'><u><b>REPORT OF THE INSPECTION COMMITTEE DURING THE A.Y.2022-23</b></u></td>";
            header += "</tr>";
            header += "</table>";

            return header;
        }

        public string SurpriseCollegeInformation(int? collegeId)
        {
            string collegeInformation = string.Empty;

            jntuh_college college = db.jntuh_college.Where(c => c.isActive == true && c.id == collegeId).Select(c => c).FirstOrDefault();

            var NameofPrincipal = (from p in db.jntuh_college_principal_registered
                                   join f in db.jntuh_registered_faculty on p.RegistrationNumber equals f.RegistrationNumber
                                   where p.collegeId == collegeId
                                   select new
                                   {
                                       RegisNo = f.RegistrationNumber.Trim(),
                                       FirstName = f.FirstName,
                                       Middlename = f.MiddleName,
                                       LastName = f.LastName
                                   }).SingleOrDefault();


            collegeInformation += "<table width='100%' border='0' cellpadding='5' cellspacing='0'>";
            collegeInformation += "<tr>";
            collegeInformation += "<td align='left'><b>1.College Name : </b><u>" + college.collegeName + "(" + college.collegeCode + ")</u></td>";
            //collegeInformation += "<td align='left' width='40%'><b>2.CollegeCode :</b><u>" + college.collegeCode + "</u></td>";
            collegeInformation += "</tr>";
            collegeInformation += "<tr>";
            collegeInformation += "<td align='left' width='100%'><b>2.Date of Visit :</b></td></tr>";
            collegeInformation += "<tr><td align='left' width='50%'><b>3.Name of the Principal : </b>" + NameofPrincipal.FirstName + " " + NameofPrincipal.Middlename + " " + NameofPrincipal.LastName + "(" + NameofPrincipal.RegisNo + ")</td>";
            //collegeInformation += "<td align='left' width='50%'><b>5.Registration Number : </b>" + NameofPrincipal.RegisNo + "</td></tr>";
            collegeInformation += "<tr><td align='left' width='60%'><b>4.Selected as Principal by the University : </b> Yes/No </td></tr>";
            collegeInformation += "<tr><td align='left' width='40%'><b>5.Date of Selection : </b> </td></tr>";
            //collegeInformation += "<tr><td align='left' width='100%'><u><b>5.Student & Classwork Particulars:</b></u></td></tr>";
            collegeInformation += "</table>";

            //collegeInformation += "<table>";
            //collegeInformation += "<tr>";
            //collegeInformation += "<td align='left' width='75%'><b>1.College Name:(2) </b><u>" + college.collegeName + "</u></td>";
            //collegeInformation += "<td align='left' width='25%'><b>2.CollegeCode:</b><u>" + college.collegeCode + "</u></td>";
            //collegeInformation += "</tr>";
            //collegeInformation += "<tr>";
            //collegeInformation += "<td align='left' width='100%'><b>3.Date of Surprise Visit:</b></td>";
            //collegeInformation += "<tr><td align='left' width='100%'><b>4.Name of the Principal & Availiability: </b>" + NameofPrincipal.FirstName + " " + NameofPrincipal.Middlename + " " + NameofPrincipal.LastName + "</td></tr>";
            //collegeInformation += "<tr><td align='left' width='100%'><u><b>5.Student & Classwork Particulars:</b></u></td></tr>";
            //collegeInformation += "</table>";

            return collegeInformation;
        }

        public string SurpriseClasswork(int? collegeId)
        {
            string Classwork = string.Empty;
            int Count = 1;
            int FirstYear = 1;
            int SecondYear = 2;
            int ThirdYear = 3;
            int FourYear = 4;
            int FiveYear = 5;

            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            //int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            //int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            //int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            //int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            //int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            //int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();



            // List<jntuh_approvedadmitted_intake> intake = db.jntuh_approvedadmitted_intake.Where(c => c.collegeId == collegeId && (c.AcademicYearId != 4 && c.AcademicYearId != 5 && c.AcademicYearId != 7 && c.AcademicYearId != 6 && c.AcademicYearId != 1)).ToList();

            List<CollegeFacultyWithIntakeReport> intakedetailsList = StudentAndClasswork(collegeId);

            var Edudata = intakedetailsList;
            Classwork += "<span align='left'><u><b>9. Student & Classwork Particulars: </b></u></span>";
            Classwork += "<br />";
            Classwork += "<table border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'>";
            Classwork += "<tr><td style='text-align: center;'>S.No</td>";
            Classwork += "<td  style='text-align: center;'>Branch</td>";
            Classwork += "<td  style='text-align: center;'>Specialization</td>";
            Classwork += "<td style='text-align: center;'>Year-Semester</td>";
            Classwork += "<td style='text-align: center;'>No. of Student On Rolls</td>";
            Classwork += "<td style='text-align: center;'>No. of Student Present</td>";
            Classwork += "<td style='text-align: center;'>Name of Subject/Lab</td>";
            Classwork += "<td style='text-align: center;'>Time of visit</td>";
            Classwork += "<td style='text-align: center;'>Classwork conducted or not as per Time Table(Y/N)</td>";
            Classwork += "<td style='text-align: center;'>Remarks</td>";
            Classwork += "</tr>";

            string SpecName = string.Empty;

            foreach (var item in intakedetailsList.Where(i => i.shiftId == 1).ToList())
            {
                var deptloop = 0;

                if (item.DegreeID == 4 || item.DegreeID == 5)
                {
                    Classwork += "<tr>";
                    Classwork += "<td rowspan='4' style='text-align:center;'>" + Count + "</td>";
                    Classwork += "<td rowspan='4' style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        Classwork += "<td rowspan='4' style='text-align:center;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        Classwork += "<td rowspan='4' style='text-align:center;'>" + item.Specialization + "<span>(II)</span></td>";
                    Classwork += "<td style='text-align:center;'>" + FirstYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "</tr>";

                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + SecondYear + "</td>";

                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + ThirdYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + FourYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                }
                else if (item.DegreeID == 1 || item.DegreeID == 6 || item.DegreeID == 2)
                {
                    Classwork += "<tr>";
                    Classwork += "<td rowspan='2' style='text-align:center;'>" + Count + "</td>";
                    Classwork += "<td rowspan='2' style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        Classwork += "<td rowspan='2' style='text-align:center;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        Classwork += "<td rowspan='2' style='text-align:center;'>" + item.Specialization + "<span>(II)</span></td>";
                    Classwork += "<td style='text-align:center;'>" + FirstYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "</tr>";

                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + SecondYear + "</td>";

                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    //Classwork += "<tr>";
                    //Classwork += "<td style='text-align:center;'>" + ThirdYear + "</td>";
                    //Classwork += "<td style='text-align:center;'>" + item.approvedIntake3 + "</td><td></td><td></td><td></td><td></td><td></td></tr>";
                    //Classwork += "<tr>";
                    //Classwork += "<td style='text-align:center;'>" + FourYear + "</td>";
                    //Classwork += "<td style='text-align:center;'>" + item.approvedIntake4 + "</td><td></td><td></td><td></td><td></td><td></td></tr>";

                }
                else if (item.DegreeID == 3)
                {
                    Classwork += "<tr>";
                    Classwork += "<td rowspan='3' style='text-align:center;'>" + Count + "</td>";
                    Classwork += "<td rowspan='3' style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        Classwork += "<td rowspan='3' style='text-align:center;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        Classwork += "<td rowspan='3' style='text-align:center;'>" + item.Specialization + "<span>(II)</span></td>";
                    Classwork += "<td style='text-align:center;'>" + FirstYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "</tr>";

                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + SecondYear + "</td>";

                    Classwork += "<td style='text-align:center;'>" + item.approvedIntake2 + "</td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + ThirdYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    //Classwork += "<tr>";
                    //Classwork += "<td style='text-align:center;'>" + FourYear + "</td>";
                    //Classwork += "<td style='text-align:center;'>" + item.approvedIntake4 + "</td><td></td><td></td><td></td><td></td><td></td></tr>";
                }
                else if (item.DegreeID == 7 || item.DegreeID == 9)
                {
                    Classwork += "<tr>";
                    Classwork += "<td rowspan='5' style='text-align:center;'>" + Count + "</td>";
                    Classwork += "<td rowspan='5' style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        Classwork += "<td rowspan='5' style='text-align:center;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        Classwork += "<td rowspan='5' style='text-align:center;'>" + item.Specialization + "<span>(II)</span></td>";
                    Classwork += "<td style='text-align:center;'>" + FirstYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "</tr>";

                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + SecondYear + "</td>";

                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + ThirdYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + FourYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + FiveYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                }
                else if (item.DegreeID == 10)
                {
                    Classwork += "<tr>";
                    Classwork += "<td rowspan='6' style='text-align:center;'>" + Count + "</td>";
                    Classwork += "<td rowspan='6' style='text-align:center;'>" + item.Degree + "</td>";
                    if (item.shiftId == 1)
                        Classwork += "<td rowspan='6' style='text-align:center;'>" + item.Specialization + "</td>";
                    else if (item.shiftId == 2)
                        Classwork += "<td rowspan='6' style='text-align:center;'>" + item.Specialization + "<span>(II)</span></td>";
                    Classwork += "<td style='text-align:center;'>" + FirstYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "<td></td>";
                    Classwork += "</tr>";

                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + SecondYear + "</td>";

                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + ThirdYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + FourYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + FiveYear + "</td>";
                    Classwork += "<td style='text-align:center;'></td><td></td><td></td><td></td><td></td><td></td></tr>";
                    Classwork += "<tr>";
                    Classwork += "<td style='text-align:center;'>" + 6 + "</td>";
                    Classwork += "<td style='text-align:center;'>" + 0 + "</td><td></td><td></td><td></td><td></td><td></td></tr>";
                }

                Count++;
            }
            #region OtherDepartments
            //string[] departments = new string[] { "English", "Mathematics", "Physics", "Chemistry", "Others(CSE/IT)", "Others(CIVIL/MECH)", "Others(ECE/EEE)", "Others(MNGT/H&S)" };
            //int totalBtechFirstYearIntake = intakedetailsList.Where(d => d.Degree == "B.Tech" && !departments.Contains(d.Department)).Select(d => d.approvedIntake1).Sum();
            //foreach (var item in departments)
            //{
            //    Classwork += "<tr>";
            //    Classwork += "<td style='text-align:center;'>" + Count + "</td>";
            //    Classwork += "<td  style='text-align:center;'>B.Tech</td>";               
            //    Classwork += "<td  style='text-align:center;'>" + item + "</td>";
            //    Classwork += "<td style='text-align:center;'>1</td>";
            //    Classwork += "<td style='text-align:center;'>" + totalBtechFirstYearIntake + "</td>";
            //    Classwork += "<td></td>";
            //    Classwork += "<td></td>";
            //    Classwork += "<td></td>";
            //    Classwork += "<td></td>";
            //    Classwork += "<td></td>";
            //    Classwork += "</tr>";
            //    Count++;
            //}
            #endregion

            Classwork += "</table>";
            return Classwork;
        }

        public string FinalCommentsAndMembers(int? collegeId)
        {
            string Classwork = string.Empty;
            Classwork += "<span align='left'><u><b>10. Committee Findings: </b></u></span>";
            Classwork += "<br />";
            Classwork += "<table width='100%' border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'><tr><td style='text-align: center;height:100px'></td></tr></table>";
            Classwork += "<br />";
            Classwork += "<span align='left'><u><b>11. Remarks of the Committee: </b></u></span>";
            Classwork += "<br />";
            Classwork += "<table width='100%' border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'><tr><td style='text-align: center;height:100px'></td></tr></table>";
            Classwork += "<br />";
            Classwork += "<span align='left'><u><b>12. Signature of the Committee Members: </b></u></span>";
            Classwork += "<br />";
            Classwork += "<table width='100%' border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'>";
            Classwork += "<tr><td style='text-align: center;'>S.No</td>";
            Classwork += "<td  style='text-align: center;'>Name of the Member</td>";
            Classwork += "<td  style='text-align: center;'>Designation</td>";
            Classwork += "<td style='text-align: center;'>Signature with date</td>";
            Classwork += "</tr>";

            Classwork += "<tr>";
            Classwork += "<td style='text-align:center;'>1</td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "</tr>";

            Classwork += "<tr>";
            Classwork += "<td style='text-align:center;'>2</td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "</tr>";

            Classwork += "<tr>";
            Classwork += "<td style='text-align:center;'>3</td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "<td></td>";
            Classwork += "</tr>";

            //Classwork += "<tr>";
            //Classwork += "<td style='text-align:center;'>4</td>";
            //Classwork += "<td></td>";
            //Classwork += "<td></td>";
            //Classwork += "<td></td>";
            //Classwork += "</tr>";
            Classwork += "</table>";
            return Classwork;
        }

        public List<CollegeFacultyWithIntakeReport> StudentAndClasswork(int? collegeId)
        {
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            List<CollegeFacultyWithIntakeReport> intakedetailsList = new List<CollegeFacultyWithIntakeReport>();
            int[] collegeIDs = null;
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

            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();

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

                intakedetails.approvedIntake1 = SurpriseGetIntake(item.collegeId, AY1, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake2 = SurpriseGetIntake(item.collegeId, AY2, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake3 = SurpriseGetIntake(item.collegeId, AY3, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake4 = SurpriseGetIntake(item.collegeId, AY4, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetails.approvedIntake5 = SurpriseGetIntake(item.collegeId, AY5, item.specializationId, item.shiftId, 1, item.degreeID);
                intakedetailsList.Add(intakedetails);
            }



            return intakedetailsList;
        }

        private int SurpriseGetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag, int DegreeId)
        {
            int intake = 0;
            if (flag == 1 && (academicYearId == 10 || academicYearId == 9 || academicYearId == 8 || academicYearId == 3))
            {
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();

            }
            //Degree B.Tech  
            //if (DegreeId == 4)
            //{
            //    //admitted
            //    if (flag == 1 && (academicYearId == 9 || academicYearId == 8 || academicYearId == 3 || academicYearId == 2))
            //    {
            //        intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();

            //    }
            //    //else if (flag == 1 && academicYearId == 9)
            //    //{
            //    //    var inta = db.jntuh_approvedadmitted_intake.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
            //    //    if (inta != null)
            //    //    {
            //    //        intake = Convert.ToInt32(inta.proposedIntake);
            //    //    }

            //    //}
            //    else   //approved
            //    {
            //        intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            //    }
            //}
            //else
            //{
            //    //approved
            //    if (flag == 1 && academicYearId != 9)
            //    {
            //        intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();

            //    }
            //    else if (flag == 1 && academicYearId == 9)
            //    {
            //        var inta = db.jntuh_approvedadmitted_intake.FirstOrDefault(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId);
            //        if (inta != null)
            //        {
            //            intake = Convert.ToInt32(inta.proposedIntake);
            //        }

            //    }
            //    else //admitted
            //    {
            //        intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            //    }
            //}
            return intake;
        }

        public string GrantedCourses(int? collegeID)
        {
            var jntuh_academic_year = db.jntuh_academic_year.ToList();
            int presentYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY0 = jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            string StaffDetails = string.Empty;
            int count = 1;
            //StaffDetails += "<br />";
            StaffDetails += "<span align='left'><u><b>6. Affiliation Granted Courses </b></u></span><br />";
            StaffDetails += "<br />";
            StaffDetails += "<table width='100%' border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'>";
            StaffDetails += "<tr>";
            StaffDetails += "<th style='text-align:center;'>S.No</th>";
            StaffDetails += "<th style='text-align:center;'>Degree</th>";
            StaffDetails += "<th style='text-align:center;'>Department/Specialization</th>";
            StaffDetails += "<th style='text-align:center;'>Sanctioned Intake(2021-22)</th>";
            StaffDetails += "<th style='text-align:center;'>Admitted Intake(2021-22)</th>";
            StaffDetails += "<th style='text-align:center;'>Sanctioned Intake(2020-21)</th>";
            StaffDetails += "<th style='text-align:center;'>Admitted Intake(2020-21)</th>";
            StaffDetails += "</tr>";

            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            var data = intakedetailsList.GroupBy(s => s.specializationId);
            int[] deptIds = { 29, 30, 31, 32, 65, 66, 67, 68 };
            foreach (var item in intakedetailsList.Where(i => !deptIds.Contains(i.DepartmentID)))
            {
                var approvedIntake2021_22 = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == item.collegeId && i.AcademicYearId == AY1 && i.SpecializationId == item.specializationId && i.ShiftId == item.shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();
                var approvedIntake2020_21 = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == item.collegeId && i.AcademicYearId == AY0 && i.SpecializationId == item.specializationId && i.ShiftId == item.shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();
                var admittedIntake2021_22 = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == item.collegeId && i.AcademicYearId == AY1 && i.SpecializationId == item.specializationId && i.ShiftId == item.shiftId).Select(i => i.AdmittedIntake).FirstOrDefault();
                var admittedIntake2020_21 = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == item.collegeId && i.AcademicYearId == AY0 && i.SpecializationId == item.specializationId && i.ShiftId == item.shiftId).Select(i => i.AdmittedIntake).FirstOrDefault();
                StaffDetails += "<tr>";
                StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + approvedIntake2021_22 + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + admittedIntake2021_22 + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + approvedIntake2020_21 + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + admittedIntake2020_21 + "</td>";
                //StaffDetails += "<td></td><td></td><td></td><td></td>";
                StaffDetails += "</tr>";
                count++;
            }

            StaffDetails += "</table>";


            return StaffDetails;
        }

        public string SurpriseStaff(int? collegeID)
        {
            string StaffDetails = string.Empty;
            int count = 1;
            StaffDetails += "<br />";
            StaffDetails += "<span align='left'><u><b>7. Faculty Summary: </b></u></span><br />";
            StaffDetails += "<br />";
            StaffDetails += "<table border='1' style='border-collapse:collapse;border-color:black;'  cellspacing='3' cellpadding='3'>";
            StaffDetails += "<tr>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>S.No</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Degree</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Department/Specialization</th>";
            StaffDetails += "<th colspan='3' style='text-align:center;'>No. of Faculty Required</th>";
            StaffDetails += "<th colspan='3' style='text-align:center;'>No. of Faculty Present</th>";
            StaffDetails += "<th rowspan='2' style='text-align:center;'>Remarks</th>";
            StaffDetails += "</tr>";
            StaffDetails += "<tr>";
            StaffDetails += "<th style='text-align:center;'>No. of Faculty with Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>No. of Faculty without Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Total number of Faculty</th>";
            StaffDetails += "<th style='text-align:center;'>No. of Faculty with Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>No. of Faculty without Ph.D</th>";
            StaffDetails += "<th style='text-align:center;'>Total number of Faculty</th>";
            StaffDetails += "</tr>";

            List<CollegeFacultyWithIntakeReport> intakedetailsList = Get_PHD_Faculty(collegeID);

            var data = intakedetailsList.GroupBy(s => s.specializationId);




            foreach (var item in intakedetailsList.Where(i=>i.shiftId == 1).ToList())
            {
                StaffDetails += "<tr>";
                StaffDetails += "<td style='text-align:center;'>" + count + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + item.Degree + "</td>";
                if (item.shiftId == 1)
                    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "</td>";
                else if (item.shiftId == 2)
                    StaffDetails += "<td style='text-align:left;'>" + item.Specialization + "<span>(II)</span></td>";
                StaffDetails += "<td style='text-align:center;'>" + item.phdFaculty + "</td>";
                StaffDetails += "<td style='text-align:center;'>" + (item.ugFaculty + item.pgFaculty) + "</td>";

                StaffDetails += "<td style='text-align:center;'>" + item.totalFaculty + "</td>";
                StaffDetails += "<td></td><td></td><td></td><td></td>";
                StaffDetails += "</tr>";
                count++;
            }

            StaffDetails += "</table>";


            return StaffDetails;
        }

        public List<CollegeFacultyWithIntakeReport> Get_PHD_Faculty(int? collegeId)
        {

            var jntuh_specialization = db.jntuh_specialization.ToList();
            var jntuh_education_category = db.jntuh_education_category.ToList();
            var jntuh_departments = db.jntuh_department.ToList();
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

            var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new   //
                                                       //registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
                                                       //                                    && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.BASStatusOld == "Y")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new   //
                                                       {
                                                           //Departmentid = rf.DepartmentId,
                                                           RegistrationNumber = rf.RegistrationNumber.Trim(),
                                                           // Department = rf.jntuh_department.departmentName,
                                                           DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                                                           SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                                                           HighestDegreeID = rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
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
                DepartmentId = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.id).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptId).FirstOrDefault(),
                Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                rf.SpecializationId


            }).Where(e => e.Department != null).ToList();





            List<CollegeFacultyWithIntakeReport> intakedetailsList = StudentAndClasswork(collegeId);
            List<CollegeFacultyWithIntakeReport> intakedetailsList1 = new List<CollegeFacultyWithIntakeReport>();

            foreach (var item in intakedetailsList)
            {

                var intakedetails = new CollegeFacultyWithIntakeReport();
                int phdFaculty; int pgFaculty; int ugFaculty; int SpecializationpgFaculty = 0; int SpecializationphdFaculty = 0;

                intakedetails.collegeId = item.collegeId;
                intakedetails.collegeCode = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                intakedetails.collegeName = jntuh_college.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                intakedetails.Degree = item.Degree;
                intakedetails.Department = item.Department;
                intakedetails.Specialization = item.Specialization;
                intakedetails.DegreeID = item.DegreeID;
                intakedetails.specializationId = item.specializationId;
                intakedetails.DepartmentID = item.DepartmentID;
                intakedetails.shiftId = item.shiftId;

                if (item.Degree == "B.Pharmacy")
                {
                    var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department).Select(f => f.RegistrationNumber).ToArray();
                    foreach (var item1 in data)
                    {
                        DegreeFacultyRegnos.Add(item1.Trim());
                    }
                    ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                    pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                    phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                    intakedetails.Department = "Pharmacy";
                }
                else
                    if (item.Degree == "M.Pharmacy")
                    {
                        var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "M.Pharmacy").Select(f => f.RegistrationNumber).ToArray();
                        foreach (var item1 in data)
                        {
                            DegreeFacultyRegnos.Add(item1.Trim());
                        }
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "M.Pharmacy");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) &&
                                    f.Department == "M.Pharmacy");
                        phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "M.Pharmacy");

                    }

                    else if (item.Degree == "Pharm.D")
                    {
                        var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D").Select(f => f.RegistrationNumber).ToArray();
                        foreach (var item1 in data)
                        {
                            DegreeFacultyRegnos.Add(item1.Trim());
                        }

                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D");
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D";
                    }
                    else if (item.Degree == "Pharm.D PB")
                    {
                        var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D PB").Select(f => f.RegistrationNumber).ToArray();
                        foreach (var item1 in data)
                        {
                            DegreeFacultyRegnos.Add(item1.Trim());
                        }
                        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D PB");
                        pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D PB");
                        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == "Pharm.D PB");
                        //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                        intakedetails.Department = "Pharm.D PB";
                    }
                    else
                    {
                        if (item.Degree == "B.Tech")
                        {
                            // var ecedept = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department).ToList();
                            var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department).Select(f => f.RegistrationNumber).ToArray();
                            foreach (var item1 in data)
                            {
                                DegreeFacultyRegnos.Add(item1.Trim());
                            }

                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                            phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                        }
                        else if (item.Degree == "M.Tech")
                        {
                            var sss = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId).ToList();
                            var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && f.SpecializationId == item.specializationId).Select(f => f.RegistrationNumber).ToArray();
                            foreach (var item1 in data)
                            {
                                DegreeFacultyRegnos.Add(item1.Trim());
                            }
                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.SpecializationId == item.specializationId);
                            phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                        }
                        else
                        {
                            var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && f.Department == item.Department).Select(f => f.RegistrationNumber).ToArray();
                            foreach (var item1 in data)
                            {
                                DegreeFacultyRegnos.Add(item1.Trim());
                            }

                            ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                            pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                            phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                            var PhdReg = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();


                            //if (item.Degree == "B.Tech")
                            //    SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                            //else
                            //    SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                            //var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                            //SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                        }


                    }

                intakedetails.phdFaculty = phdFaculty;
                intakedetails.SpecializationsphdFaculty = SpecializationphdFaculty;
                intakedetails.SpecializationspgFaculty = SpecializationpgFaculty;
                intakedetails.pgFaculty = pgFaculty;
                intakedetails.ugFaculty = ugFaculty;
                intakedetails.totalFaculty = (ugFaculty + pgFaculty + phdFaculty);


                #region TotalFaculty In College Comment Code
                //   if(item.Degree=="B.Pharmacy")
                //   {
                //       ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree  && f.SpecializationId == item.specializationId);  
                //       pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree &&  f.SpecializationId == item.specializationId);     
                //       phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree  && f.SpecializationId == item.specializationId);
                //       //intakedetails.Department = "Pharmacy";
                //   }
                //else
                //   if (item.Degree == "M.Pharmacy")
                //   {
                //       var data = item;
                //       ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                //       pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) &&
                //                   f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);
                //       phdFaculty = jntuh_registered_faculty.Count(f => ("Ph.D" == f.HighestDegree) && f.Department == "M.Pharmacy" && f.SpecializationId == item.specializationId);

                //   }
                //   //else
                //   //    if (item.Degree == "Pharmacy")
                //   //    {
                //   //        ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department==item.Department);
                //   //        pgFaculty = jntuh_registered_faculty.Count(f => "PG" == f.HighestDegree && f.Department == item.Department);
                //   //        phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);
                //   //        //intakedetails.Department = "Pharmacy";
                //   //    }
                //   else if (item.Degree == "Pharm.D")
                //   {
                //       ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //       pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //       phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D" && f.SpecializationId == item.specializationId);
                //       //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //       intakedetails.Department = "Pharm.D";
                //   }
                //   else if (item.Degree == "Pharm.D PB")
                //   {
                //       ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //       pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //       phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == "Pharm.D PB" && f.SpecializationId == item.specializationId);
                //       //noPanOrAadhaarcount = registeredFaculty.Where(f => f.DepartmentId == pharmacyDeptId && f.PANNumber == null && f.AadhaarNumber == null && (f.isApproved == null || f.isApproved == true)).Count();
                //       intakedetails.Department = "Pharm.D PB";
                //   }
                //   else
                //   {
                //      if(item.Degree=="B.Tech")
                //      {
                //          ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                //          pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                //          phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) &&  f.Department == item.Department);
                //      }
                //      else if (item.Degree=="M.Tech")
                //      {
                //          ugFaculty=jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //          pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.SpecializationId == item.specializationId);
                //          phdFaculty=jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //      }
                //      else
                //      {
                //           ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                //       pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                //       phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                //       var PhdReg = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();


                //       if (item.Degree == "B.Tech")
                //           SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                //       else
                //           SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //       var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                //       SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));
                //      }
                //       //ugFaculty = jntuh_registered_faculty.Count(f => "UG" == f.HighestDegree && f.Department == item.Department);
                //       //pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == item.Department);
                //       //phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == item.Department);

                //       //var PhdReg = jntuh_registered_faculty.Where(f => "Ph.D" == f.HighestDegree && f.Department == item.Department).Select(f => f.RegistrationNumber).ToList();

                //       ////var phdFaculty1 = jntuh_registered_faculty.Where(f => ("Ph.D" == f.HighestDegree || "Ph.D." == f.HighestDegree || "Phd" == f.HighestDegree || "PHD" == f.HighestDegree || "Ph D" == f.HighestDegree)).ToList() ;
                //       ////if (item.Department == "MBA")
                //       ////    phdFaculty1 = phdFaculty1.Where(f => f.Department == "MBA").ToList();

                //       //// string REG=
                //       //if (item.Degree == "B.Tech")
                //       //    SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && (f.SpecializationId == item.specializationId || f.SpecializationId == null) && f.Department == item.Department);
                //       //else
                //       //    SpecializationphdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.SpecializationId == item.specializationId);
                //       //var reg = jntuh_registered_faculty.Where(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG")).Select(S => S.RegistrationNumber).ToList();
                //       //SpecializationpgFaculty = jntuh_registered_faculty.Count(f => f.SpecializationId == item.specializationId && (f.Recruitedfor == "PG" || f.Recruitedfor == "UG&PG"));

                //   }
                #endregion

                intakedetailsList1.Add(intakedetails);

            }
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
                    int pgFaculty = jntuh_registered_faculty.Count(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department);
                    var pgreg = jntuh_registered_faculty.Where(f => ("PG" == f.HighestDegree || "M.Phil" == f.HighestDegree) && f.Department == department).Select(e => e.RegistrationNumber).ToList();
                    int phdFaculty = jntuh_registered_faculty.Count(f => "Ph.D" == f.HighestDegree && f.Department == department);

                    var data = jntuh_registered_faculty.Where(f => ("UG" == f.HighestDegree || "PG" == f.HighestDegree || "M.Phil" == f.HighestDegree || "Ph.D" == f.HighestDegree) && f.Department == department).Select(f => f.RegistrationNumber).ToArray();
                    foreach (var item in data)
                    {
                        DegreeFacultyRegnos.Add(item.Trim());
                    }

                    intakedetailsList1.Add(new CollegeFacultyWithIntakeReport
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

                        approvedIntake1 = 1
                    });
                }
            }

            return intakedetailsList1;
        }

        public string CollegeDetails(int? CollegeId)
        {
            string CollegeDetails = string.Empty;
            int SNO = 1;
            List<FacultyRegistration> teachingFaculty = new List<FacultyRegistration>();

            var jntuh_departments = db.jntuh_department.ToList();
            var jntuh_specialization = db.jntuh_specialization.ToList();
            var jntuh_education_category = db.jntuh_education_category.ToList();


            var DeptNameBasedOnSpecialization = (from a in jntuh_departments
                                                 join b in jntuh_specialization on a.id equals b.departmentId
                                                 select new
                                                 {
                                                     DeptId = a.id,
                                                     DeptName = a.departmentName,
                                                     Specid = b.id
                                                 }).ToList();

            var collegeNAME = db.jntuh_college.Where(c => c.isActive == true && c.id == CollegeId).Select(c => c.collegeName).FirstOrDefault();

            var DegreeIdNameBasedOnSpecialization = (from a in jntuh_departments
                                                     join b in jntuh_specialization on a.id equals b.departmentId
                                                     join c in db.jntuh_degree on a.degreeId equals c.id
                                                     select new
                                                     {
                                                         DegreeId = c.id,
                                                         DegreeName = c.degree,
                                                         SpcializationName = b.specializationName,
                                                         Specid = b.id
                                                     }).ToList();

            //CollegeDetails += "<span align='left'><u><b>COLLEGE NAME:</b></u> " + collegeNAME + "</span>";
            //CollegeDetails += "<span>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>";
            //CollegeDetails += "<span align='center'><u><b>Date of Visit: </b></u><br /></span>";
            CollegeDetails += "<span align='left'><u><b>8. Faculty Details: </b></u></span><br />";
            CollegeDetails += "<br />";
            CollegeDetails += "<table width='100%' border='1' style='border-collapse:collapse;border-color:black;'cellpadding='3' cellspacing='0'>";
            CollegeDetails += "<tr>";
            CollegeDetails += "<td width='4%' style='text-align: center;'><b>S.No</b></td>";
            CollegeDetails += "<td width='25%' style='text-align: center;'><b>Registration ID</b></td>";
            CollegeDetails += "<td width='20%' style='text-align: center;'><b>Name of the Faculty</b></td>";
            CollegeDetails += "<td width='6%' style='text-align: center;'><b>Department</b></td>";
            CollegeDetails += "<td width='30%' style='text-align: center;'><b>PG Specialization</b></td>";
            CollegeDetails += "<td width='8%' style='text-align: center;'><b>PAN Number</b></td>";
            CollegeDetails += "<td width='7%' style='text-align: center;'><b>Present (or) Absent</b></td>";
            CollegeDetails += "</tr>";

            #region Pharmacy College Ids
            //List<int?> PharmacyIds =new List<int?>() {6,27,30,34,44,45,47,52,54,55,58,60,65,66,78,90,95,97,105,107,110,114,117,118,120,127,135,136,139,146,150,159,169,202,204,206,213,219,234,237,239,252,253,262,263,267,276,283,290,295,297,298,301,302,303,313,314,315,317,318,319,320,332,348,353,362,370,376,379,384,389,392,395,410,427,428,436,442,445,448,454};
            // List<int?> otherDeptIds =new List<int?>() { 60, 65, 66, 67, 68, 31, 32, 29, 30 };
            //bool status=false;
            //if(PharmacyIds.Contains(CollegeId))
            //{
            //    status=true;
            //}

            //List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = new List<jntuh_college_faculty_registered>();
            //string[] strRegNos = null;

            //if(status==true)
            //{
            //     jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(r => r.collegeId == CollegeId && !otherDeptIds.Contains(r.DepartmentId)).Select(r => r).ToList();
            //    strRegNos = jntuh_college_faculty_registered.Select(r => r.RegistrationNumber).ToArray();
            //}
            //else
            //{
            //     jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(r => r.collegeId == CollegeId).Select(r => r).ToList();
            //     strRegNos = jntuh_college_faculty_registered.Select(r => r.RegistrationNumber).ToArray();
            //}
            #endregion

            var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(r => r.collegeId == CollegeId).Select(r => r).ToList();
            string[] strRegNos = jntuh_college_faculty_registered.Select(r => r.RegistrationNumber).ToArray();



            string[] PrincipalRegNo = db.jntuh_college_principal_registered.Where(p => p.collegeId == CollegeId).Select(p => p.RegistrationNumber.Trim()).ToArray();
            var registered_faculty = db.jntuh_registered_faculty.Where(f => strRegNos.Contains(f.RegistrationNumber)).ToList();

            var jntuh_registered_faculty1 = registered_faculty.Select(rf => new   //
            //registeredFaculty.Where(rf => rf.type != "Adjunct" && ((rf.Absent == false) && (rf.NotQualifiedAsperAICTE == false)//&& rf.Noform16Verification == false && rf.NoForm16 == false
            //                                    && (rf.NoSCM17 == false) && (rf.PANNumber != null) && (rf.IncompleteCertificates == false) && (rf.Blacklistfaculy == false) && (rf.MultipleRegInSameCollege == false || rf.MultipleRegInSameCollege == null) && rf.NoRelevantUG == "No" && rf.NoRelevantPG == "No" && rf.NORelevantPHD == "No" && rf.NotIdentityfiedForanyProgram == false && (rf.InvalidPANNumber == false || rf.InvalidPANNumber == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) && (rf.AppliedPAN == false || rf.AppliedPAN == null) && (rf.SamePANUsedByMultipleFaculty == false || rf.SamePANUsedByMultipleFaculty == null) && (rf.BASStatusOld == "Y")) && (rf.DepartmentId != 61 || rf.DepartmentId != 27)).Select(rf => new   //
            {
                //Departmentid = rf.DepartmentId,
                RegistrationNumber = rf.RegistrationNumber.Trim(),
                FirstName = rf.FirstName,
                MiddleName = rf.MiddleName,
                LastName = rf.LastName,
                PGSpecialization = rf.PGSpecialization,
                // Department = rf.jntuh_department.departmentName,
                DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.DepartmentId).FirstOrDefault(),
                SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.SpecializationId).FirstOrDefault(),
                //HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                HighestDegreeID = rf.jntuh_registered_faculty_education.Where(r => r.educationId != 8 && r.educationId != 9).Count() != 0 ? rf.jntuh_registered_faculty_education.Where(e => e.educationId != 8 && e.educationId != 9).Select(e => e.educationId).Max() : 0,
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
                FirstName = rf.FirstName,
                MiddleName = rf.MiddleName,
                LastName = rf.LastName,
                PGSpecialization = rf.PGSpecialization,
                PANNumber = rf.PanNumber,
                Department = rf.DeptId != null ? jntuh_departments.Where(e => e.id == rf.DeptId).Select(e => e.departmentName).FirstOrDefault() : DeptNameBasedOnSpecialization.Where(e => e.Specid == rf.SpecializationId).Select(e => e.DeptName).FirstOrDefault(),
                HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber.Trim() == rf.RegistrationNumber.Trim()).Select(c => c.IdentifiedFor).FirstOrDefault(),
                rf.SpecializationId


            }).Where(e => e.Department != null).ToList();


            // var jntuh_registered_faculty_New = jntuh_registered_faculty.Where(r => DegreeFacultyRegnos.Contains(r.RegistrationNumber)).ToList();

            //if(DegreeFacultyRegnos.Count!=0 || DegreeFacultyRegnos.Count!= null)
            //{
            //    var ddd = jntuh_registered_faculty.Where(f => !DegreeFacultyRegnos.Contains(f.RegistrationNumber.Trim())).Select(f => f.RegistrationNumber).ToArray();
            //    jntuh_registered_faculty = jntuh_registered_faculty.Where(f => DegreeFacultyRegnos.Contains(f.RegistrationNumber.Trim())).ToList();

            //    var dddd = jntuh_registered_faculty.Where(f => ddd.Contains(f.RegistrationNumber)).ToList();
            //}


            int? Specializationid = 0;
            int? CollegeDepartmentId = 0;
            foreach (var item in jntuh_registered_faculty)
            {
                CollegeDepartmentId = jntuh_college_faculty_registered.Where(r => r.RegistrationNumber.Trim() == item.RegistrationNumber.Trim()).Select(r => r.DepartmentId).FirstOrDefault();
                var faculty = new FacultyRegistration();
                faculty.RegistrationNumber = item.RegistrationNumber;
                faculty.FirstName = item.FirstName;
                faculty.MiddleName = item.MiddleName;
                faculty.LastName = item.LastName;
                if (PrincipalRegNo.Contains(item.RegistrationNumber.Trim()))
                {
                    faculty.Principal = "Principal";
                }
                else
                {
                    faculty.Principal = "";
                }
                faculty.department = CollegeDepartmentId > 0 ? jntuh_departments.Where(d => d.id == CollegeDepartmentId).Select(d => d.departmentName).SingleOrDefault() : "";
                faculty.PGSpecializationName = item.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == item.PGSpecialization).Select(e => e.DegreeName + "-" + e.SpcializationName).FirstOrDefault() : "";
                // faculty.SpecializationId = item.PGSpecialization != 0 ? DegreeIdNameBasedOnSpecialization.Where(e => e.Specid == item.PGSpecialization).Select(e => e.Specid).FirstOrDefault() : "";
                // faculty.DepartmentId = item.PGSpecialization != 0 ? jntuh_specialization.Where(s=>s.jntuh_department.id): "";
                faculty.Degree = item.HighestDegree == "Ph.D" ? "Ph.D" : "";
                faculty.PANNumber = item.PANNumber;
                teachingFaculty.Add(faculty);
            }
            //teachingFaculty = teachingFaculty.OrderBy(d => d.department);

            // var data=teachingFaculty

            foreach (var item in teachingFaculty.OrderBy(d => d.department))
            {
                CollegeDetails += "<tr>";
                CollegeDetails += "<td width='4%' style='font-size: 14px; font-weight: normal;'>" + SNO + "</td>";
                CollegeDetails += "<td width='30%' style='font-size: 14px; font-weight: normal;'>" + item.RegistrationNumber + "</td>";
                if (item.Degree == "Ph.D")
                {
                    CollegeDetails += "<td width='20%' style='font-size: 14px; font-weight: bold;'>" + item.FirstName + "&nbsp;" + item.MiddleName + "&nbsp;" + item.LastName + "(&nbsp;" + item.Degree + " ) </td>";    
                }
                else
                {
                    CollegeDetails += "<td width='20%' style='font-size: 14px; font-weight: normal;'>" + item.FirstName + "&nbsp;" + item.MiddleName + "&nbsp;" + item.LastName + " </td>";
                }
                
                CollegeDetails += "<td width='6%' style='font-size: 14px; font-weight: normal;'>" + item.department + "</td>";
                CollegeDetails += "<td width='30%' style='font-size: 14px; font-weight: normal;'>" + item.PGSpecializationName + "</td>";
                CollegeDetails += "<td width='6%' style='font-size: 14px; font-weight: normal;'>" + item.PANNumber + "</td>";
                CollegeDetails += "<td width='4%' style='font-size: 14px; font-weight: normal;'></td>";
                CollegeDetails += "</tr>";
                SNO++;
            }
            CollegeDetails += "</table>";
            DegreeFacultyRegnos.Clear();
            return CollegeDetails;
        }

        public string DeficiencyCollegeLabsAnnexure(int? collegeID)
        {
            string annexure = string.Empty;

            var collegeStatus = db.jntuh_collegestatus.Where(e => e.CollegeId == collegeID).Select(e => e).FirstOrDefault();
            if (collegeStatus != null)
            {
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear)).Select(s => s.id).FirstOrDefault();
                List<FacultyVerificationController.AnonymousLabclass> collegeLabAnonymousLabclass = new List<FacultyVerificationController.AnonymousLabclass>();
                //Commented by Narayana
                //List<int> specializationIds =db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == 9 && e.proposedIntake!=0).Select(e => e.specializationId).Distinct().ToList();
                //int[] degreeids = { 2, 5, 9, 10 };
                int[] degreeids = { };
                List<int> specializationIds = db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeID && e.courseStatus != "Closure" && e.academicYearId == prAy && e.proposedIntake != 0).Select(e => e.specializationId).Distinct().ToList();
                int[] Equipmentsids = db.jntuh_college_laboratories.Where(C => C.CollegeID == collegeID).Select(C => C.EquipmentID).ToArray();
                var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && specializationIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).ToArray();

                if (DegreeIds.Contains(4))
                {
                    specializationIds.Add(39);
                }

                string CollegeAffiliationStatus = db.jntuh_college_affiliation.Where(u => u.collegeId == collegeID && u.affiliationTypeId == 7).Select(u => u.affiliationStatus).FirstOrDefault();

                if (CollegeAffiliationStatus == "Yes")
                {
                    collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => l.CollegeId == collegeID && specializationIds.Contains(l.SpecializationID)&& !degreeids.Contains(l.DegreeID) && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional")).Select(l => new FacultyVerificationController.AnonymousLabclass
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
                    if (specializationIds.Contains(33) || specializationIds.Contains(43))
                    {
                        collegeLabAnonymousLabclass =
                            db.jntuh_lab_master.AsNoTracking()
                                .Where(
                                    l =>
                                        specializationIds.Contains(l.SpecializationID) && !Equipmentsids.Contains(l.id) &&
                                        l.CollegeId == null && !degreeids.Contains(l.DegreeID) && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional"))
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
                    else
                    {
                        collegeLabAnonymousLabclass = db.jntuh_lab_master.AsNoTracking().Where(l => specializationIds.Contains(l.SpecializationID) && l.CollegeId == null && !degreeids.Contains(l.DegreeID) && l.Labcode != "PH105BS" && !l.EquipmentName.Contains("Desirable") && !l.EquipmentName.Contains("optional")).Select(l => new FacultyVerificationController.AnonymousLabclass
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
                }

                //var collegeEquipments = db.jntuh_college_laboratories_dataentry2.Where(l => l.CollegeID == collegeID).Select(l => l.EquipmentID).Distinct().ToArray();

                //var list = collegeLabMaster.Where(c => !collegeEquipments.Contains(c.EquipmentID)).Select(c => new { EquipmentID = c.id, LabCode = c.Labcode, LabName = c.LabName, EquipmentName = c.EquipmentName })
                //                           .OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                //var labDeficiencies = db.jntuh_college_laboratories_deficiency.Where(ld => ld.CollegeId == collegeID && ld.Deficiency == true).Select(ld => ld.LabCode).ToArray();

                var list1 = collegeLabAnonymousLabclass.OrderBy(c => c.LabName).ThenBy(c => c.EquipmentName).Distinct().ToList();

                #region this code written by suresh

                int?[] labequipmentIds = list1.Select(i => i.EquipmentID).ToArray();

                int[] clgequipmentIDs = db.jntuh_college_laboratories.Where(l => l.CollegeID == collegeID && labequipmentIds.Contains(l.EquipmentID)).Select(i => i.EquipmentID).ToArray();

                //list1 = list1.Where(l => !clgequipmentIDs.Contains((int)l.EquipmentID)).ToList();

                #endregion

                int[] SpecializationIDs;
                if (DegreeIds.Contains(4))
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass orderby a.Department select a.specializationId).Distinct().ToArray();
                //labs.Select(l => l.specializationId).Distinct().ToArray();
                else
                    SpecializationIDs = (from a in collegeLabAnonymousLabclass where a.specializationId != 39 orderby a.Department select a.specializationId).Distinct().ToArray();

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
                    annexure += "<td align='left'><b><u>9. No.of Laboratories with names</u></b></td>";
                    annexure += "</tr>";
                    annexure += "</table>";
                    foreach (var speclializationId in SpecializationIDs)
                    {
                        string LabNmae = "", EquipmentName = "", DepartmentName = "";
                        var specializationDetails = specializations.FirstOrDefault(s => s.id == speclializationId);
                        DepartmentName = list1.Where(l => l.specializationId == speclializationId).Select(l => l.Department).FirstOrDefault();
                        annexure += "<table width='100%' border='1' cellpadding='5' cellspacing='0'>";
                        annexure += "<tr>";
                        //annexure += "<th align='left' colspan='3'> " + specializationDetails.degree + " -" + specializationDetails.department + "-" + specializationDetails.specialization + "</th>";
                        annexure += "<th align='left' colspan='4'> " + specializationDetails.department + "</th>";
                        annexure += "</tr>";
                        annexure += "<tr>";
                        annexure += "<th align='left'>S.No</th><th align='left'>Lab Code</th><th align='left'>Lab Name</th><th align='left'>Yes (or) No</th>";
                        annexure += "</tr>";
                        int LabsCount = 0;
                        int EquipmentsCount = 0;

                        var labs = list1.Where(l => l.specializationId == speclializationId).OrderBy(e => e.year).ThenBy(e => e.Semester).GroupBy(i => i.LabCode).Select(e => new { LabCode = e.Key }).ToList();
                        int indexnow = 1;
                        foreach (var item in labs)
                        {
                            var labDetails = list1.FirstOrDefault(l => l.specializationId == speclializationId && l.LabCode == item.LabCode);
                            //LabNmae = labDetails.LabName.Trim() != null;
                            //EquipmentName = item.EquipmentName;
                            annexure += "<tr>";
                            annexure += "<td align='left'>" + indexnow + "</td><td  align='left'>" + item.LabCode + "</td><td  align='left'>" + labDetails.LabName.Trim() + "</td><td  align='left'></td>";
                            annexure += "</tr>";

                            //if (!string.IsNullOrEmpty(item.LabName))
                            //    LabsCount = 0;
                            //if (!string.IsNullOrEmpty(item.EquipmentName))
                            //    EquipmentsCount = 0;
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
                    annexure += "<td align='left'><b><u>9. No.of Laboratories with names </u></b></td>";
                    annexure += "</tr>";
                    annexure += "</table>";
                    annexure += "<table width='100%' border='1'  cellspacing='0'>";
                    annexure += "<tr><td align='center'> <b>NIL</b></td></tr>";
                    annexure += "</table>";
                }
            }
            return annexure;
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
            public int? AvailableFaculty { get; set; }
            public int approvedIntake1 { get; set; }
            public int approvedIntake2 { get; set; }
            public int approvedIntake3 { get; set; }
            public int approvedIntake4 { get; set; }
            public int approvedIntake5 { get; set; }
            public int totalIntake { get; set; }
            public int totalBtechFirstYearIntake { get; set; }
            public int dollercourseintake { get; set; }
            public int firstYearRequired { get; set; }
            public decimal requiredFaculty { get; set; }
            public decimal ArequiredFaculty { get; set; }
            public decimal SrequiredFaculty { get; set; }
            public int phdFaculty { get; set; }
            public int SpecializationsphdFaculty { get; set; }
            public int SpecializationspgFaculty { get; set; }
            public int pgFaculty { get; set; }
            public int ugFaculty { get; set; }
            public int totalFaculty { get; set; }
            public int oldtotalFaculty { get; set; }
            public int newtotalFaculty { get; set; }
            public int specializationWiseFaculty { get; set; }
            public int facultyWithoutPANAndAadhaar { get; set; }
            public int A416TotalFaculty { get; set; }
            public int form16count { get; set; }
            public int aictecount { get; set; }
            public int adjustedFaculty { get; set; }
            public string minimumRequirementMet { get; set; }

            public int DegreeID { get; set; }
            public bool isActive { get; set; }
            public DateTime? nbaFrom { get; set; }
            public DateTime? nbaTo { get; set; }
            public int NOSCM { get; set; }
            public string PHDminimumRequirementMet { get; set; }
            public int DeactivationReasionsCount { get; set; }
            public int FacultyAbsentCount { get; set; }
            public int InvalidPanCount { get; set; }
            public int incompletecerificatesCount { get; set; }
            public int adjointfacultycount { get; set; }

            public bool? deficiency { get; set; }
            public int shortage { get; set; }
            public string Note { get; set; }
            public string AffliationStatus { get; set; }
            public string collegeRandomCode { get; set; }
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

            public bool AffiliationStatus2 { get; set; }
            public bool AffiliationStatus3 { get; set; }
            public bool AffiliationStatus4 { get; set; }

            //Added this in 25-04-2017
            public int admittedIntake1 { get; set; }
            public int admittedIntake2 { get; set; }
            public int admittedIntake3 { get; set; }
            public int admittedIntake4 { get; set; }
            public int admittedIntake5 { get; set; }

            //Added 02-03-2019
            public int Proposedintake { get; set; }
            public int AICTESanctionIntake1 { get; set; }
            public int AICTESanctionIntake2 { get; set; }
            public int AICTESanctionIntake3 { get; set; }
            public int AICTESanctionIntake4 { get; set; }
            public int AICTESanctionIntake5 { get; set; }

            public int ExambranchIntake_R1 { get; set; }
            public int ExambranchIntake_R2 { get; set; }
            public int ExambranchIntake_R3 { get; set; }
            public int ExambranchIntake_R4 { get; set; }
            public int ExambranchIntake_R5 { get; set; }

            public int ExambranchIntake_L1 { get; set; }
            public int ExambranchIntake_L2 { get; set; }
            public int ExambranchIntake_L3 { get; set; }
            public int ExambranchIntake_L4 { get; set; }
            public int ExambranchIntake_L5 { get; set; }

            public int SanctionIntake1 { get; set; }
            public int SanctionIntake2 { get; set; }
            public int SanctionIntake3 { get; set; }
            public int SanctionIntake4 { get; set; }
            public int SanctionIntake5 { get; set; }
            public bool ispercentage { get; set; }
            public bool ishashcourses { get; set; }
            public bool isstarcourses { get; set; }

            public int totalAdmittedIntake { get; set; }
        }
        public class PharmacySpecilaizationList
        {
            public string PharmacyspecName { get; set; }
            public string Specialization { get; set; }
            public int PharmacyspecWiseCount { get; set; }
        }
    }
}
