using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    [ErrorHandling]
    public class DistrictwiseCollegeSpecializationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult DistrictwiseCollegeSpecializations()
        {
            List<Districts> districts = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName)
                                                .Select(d => new Districts
                                                {
                                                    id = d.id,
                                                    name = d.districtName
                                                }).ToList();

            return View("~/Views/Admin/DistrictwiseCollegeSpecializations.cshtml", districts);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetCollegeSpecializations(int districtID)
        {
            string table = string.Empty;

            //int[] collegeIds = db.jntuh_college.Where(c => c.id == 207).Select(c => c.id).ToArray();
            int[] collegeIds = (from c in db.jntuh_college
                                join a in db.jntuh_address on c.id equals a.collegeId
                                where a.addressTye == "COLLEGE" && c.isActive == true && c.id != 375 && a.districtId == districtID
                                orderby a.pincode
                                select c.id).ToArray();

            table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='8' style='font-size: 8px;'><b>Courses Offered</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td><td align='center' colspan='3' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td></tr>";

            int count = 1;
            foreach (var scheduleCollegeId in collegeIds)
            {
                if (count > 0)
                {
                    string scheduleCollegeCode = db.jntuh_college.Find(scheduleCollegeId).collegeCode;
                    string scheduleCollegeName = db.jntuh_college.Find(scheduleCollegeId).collegeName;
                    jntuh_address address = db.jntuh_address.Where(a => a.collegeId == scheduleCollegeId).Select(a => a).FirstOrDefault();
                    jntuh_college_principal_director principal = db.jntuh_college_principal_director.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).FirstOrDefault();
                    string district = db.jntuh_district.Find(address.districtId).districtName;
                    string scheduleCollegeAddress = string.Format("{0} {1} {2} {3} {4}", address.address, address.townOrCity, address.mandal, district, address.pincode);
                    string scheduleCollegePrincipalName = string.Empty;
                    string scheduleCollegePrincipalPhone = string.Empty;
                    string scheduleCollegePrincipalMobile = string.Empty;

                    if (principal != null)
                    {
                        scheduleCollegePrincipalName = string.Format("{0} {1} {2}", principal.firstName, principal.lastName, principal.surname);
                        scheduleCollegePrincipalPhone = string.Format("{0}{1}", principal.landline, address.landline.Equals(principal.landline) ? "" : ", " + address.landline);
                        scheduleCollegePrincipalMobile = string.Format("{0}{1}", principal.mobile, address.mobile.Equals(principal.mobile) ? "" : ", " + address.mobile);
                    }

                    table += string.Format("<tr><td align='center' colspan='1' style='font-size: 8px;'><b>" + count + "</b></td><td valign='top' colspan='5' style='font-size: 8px; line-height: 11px;'>{0},<br />{1}<br /><br /><b>Code : {2}</b><br /><br />Prl : {3}<br />Off : {4}<br />Cell : {5}</td>", scheduleCollegeName, scheduleCollegeAddress, scheduleCollegeCode, scheduleCollegePrincipalName, scheduleCollegePrincipalPhone, scheduleCollegePrincipalMobile);
                    int academicYearId = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
                    var specializations = db.jntuh_college_intake_existing.Where(p => p.collegeId == scheduleCollegeId && p.academicYearId == academicYearId).Select(p => p).ToList();
                    List<ExistingIntakeSpecializations> existingIntakeSpecializations = new List<ExistingIntakeSpecializations>();

                    foreach (var spec in specializations)
                    {
                        ExistingIntakeSpecializations newSpec = new ExistingIntakeSpecializations();
                        newSpec.specialization = db.jntuh_specialization.Find(spec.specializationId).specializationName;
                        newSpec.shift = db.jntuh_shift.Find(spec.shiftId).shiftName;
                        int deptId = db.jntuh_specialization.Find(spec.specializationId).departmentId;
                        newSpec.department = db.jntuh_department.Find(deptId).departmentName;
                        int degreeId = db.jntuh_department.Find(deptId).degreeId;
                        newSpec.degree = db.jntuh_degree.Find(degreeId).degree;
                        int academicYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
                        int actualYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.actualYear).FirstOrDefault();
                        int prposedYear = actualYear + 1;
                        int prposedYearId = db.jntuh_academic_year.Where(ay => ay.actualYear == prposedYear).Select(ay => ay.id).FirstOrDefault();

                        newSpec.existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == scheduleCollegeId && e.academicYearId == academicYear && e.specializationId == spec.specializationId && e.shiftId == spec.shiftId).Select(e => e.approvedIntake).FirstOrDefault();
                        newSpec.proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == scheduleCollegeId && p.academicYearId == prposedYearId && p.specializationId == spec.specializationId && p.shiftId == spec.shiftId).Select(p => p.proposedIntake).FirstOrDefault();
                        existingIntakeSpecializations.Add(newSpec);
                    }
                    existingIntakeSpecializations = existingIntakeSpecializations.OrderBy(p => p.degree).ThenBy(p => p.department).ThenBy(p => p.specialization).ThenBy(p => p.shift).ToList();
                    table += "<td valign='top' colspan='8' style='font-size: 8px;'>";
                    table += "<table border='0' cellpadding='1' cellspacing='0' width='100%'>";
                    if (existingIntakeSpecializations.Count() > 0)
                    {
                        string fontSize = "8px";
                        string lineHeight = "12px";

                        string sFontSize = "8px";
                        string sLineHeight = "12px";

                        foreach (var spec in existingIntakeSpecializations)
                        {
                            string intake = string.Empty;

                            //if both are equal show only proposed intake
                            if (spec.existing == spec.proposed)
                            {
                                intake = spec.proposed.ToString();
                            }

                            //if [existing intake > proposed intake] then show [existing intake - decrease in proposed intake]
                            if (spec.existing > spec.proposed)
                            {
                                intake = spec.existing.ToString() + "-<b>" + (spec.existing - spec.proposed).ToString() + "</b>";
                            }

                            //if [existing intake < proposed intake] then show [existing intake + increase in proposed intake]
                            if (spec.existing < spec.proposed)
                            {
                                intake = spec.existing.ToString(); //+"+<b>" + (spec.proposed - spec.existing).ToString() + "</b>";
                            }

                            //if [proposed intake = 0] then show [existing intake (proposed intake)]
                            if (spec.proposed == 0)
                            {
                                intake = spec.existing.ToString() + "<b>(" + spec.proposed.ToString() + ")</b>";
                            }
                            string makeDegreeBold1 = string.Empty;
                            string makeDegreeBold2 = string.Empty;

                            int flag = 0;
                            //if [existing intake = 0] then show [proposed intake]
                            if (spec.existing == 0)
                            {
                                flag = 1;
                                intake = "<b>" + spec.proposed.ToString() + "</b>";
                                makeDegreeBold1 = "<b>";
                                makeDegreeBold2 = "</b>";
                            }

                            string degreeName = string.Empty;
                            if (spec.degree == "M.Tech" || spec.degree == "M.Pharmacy")
                            {
                                degreeName = spec.degree + " - ";
                            }
                            if (flag == 0)
                            {
                                table += string.Format("<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + makeDegreeBold1 + "{0}" + makeDegreeBold2 + "{1}</td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'>{2}</td></tr>", degreeName + spec.specialization, spec.shift.Equals("1") ? "" : " - (Shift 2)", intake);
                            }
                        }
                    }
                    table += "</table>";
                    table += "</td>";

                    table += "<td valign='top' colspan='5' style='font-size: 8px;' rowspan='1'>";
                    table += "</td>";
                    table += "<td valign='top' align='center' colspan='3' style='font-size: 8px;' rowspan='1'>&nbsp;</td></tr>";
                    table += "</tr>";
                }
                count++;
            }
            table += "</table>";

            string pdfPath = SaveSpecializations(table);
            string path = pdfPath.Replace("/", "\\");

            return File(path, "application/pdf");
        }
        private string SaveSpecializations(string table)
        {
            string fullPath = string.Empty;

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);

            string path = Server.MapPath("~/Content/PDFReports");

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            fullPath = path + "/temp/district-wise" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;
            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/Specs.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##TABLE##", table);

            //Read string contents using stream reader and convert html to parsed conent
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

            return fullPath;
        }


    }
}
