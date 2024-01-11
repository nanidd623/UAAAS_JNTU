using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class CollegeWiseReportController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeSpecializations(int districtid)
        {
            string table = string.Empty;
            string district = string.Empty;

            //int[] collegeIds = db.jntuh_college.Where(c => c.id == 207).Select(c => c.id).ToArray();
            int[] collegeIds = (from c in db.jntuh_college
                                join a in db.jntuh_address on c.id equals a.collegeId
                                where a.addressTye == "COLLEGE" && c.isActive == true && a.districtId == districtid
                                orderby a.pincode
                                select c.id).ToArray();
            //.Skip(120) .Take(60)

            table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='8' style='font-size: 8px;'><b>Courses Offered</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Previous FFC Member(s)</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td><td align='center' colspan='3' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td></tr>";

            int count = 1;
            foreach (var scheduleCollegeId in collegeIds)
            {
                if (count > 0)
                {
                    district = string.Empty;

                    string scheduleCollegeCode = db.jntuh_college.Find(scheduleCollegeId).collegeCode;
                    string scheduleCollegeName = db.jntuh_college.Find(scheduleCollegeId).collegeName.ToUpper();
                    jntuh_address address = db.jntuh_address.Where(a => a.collegeId == scheduleCollegeId).Select(a => a).FirstOrDefault();
                    jntuh_college_principal_director principal = db.jntuh_college_principal_director.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).FirstOrDefault();
                    district = db.jntuh_district.Find(address.districtId).districtName;
                    //string scheduleCollegeAddress = string.Format("{0} {1} {2} {3} {4}", address.address, address.townOrCity, address.mandal, district, address.pincode);
                    string scheduleCollegeAddress = address.address;

                    if (!scheduleCollegeAddress.ToUpper().Contains(address.townOrCity.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;

                    if (!scheduleCollegeAddress.ToUpper().Contains(address.mandal.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;

                    if (!scheduleCollegeAddress.ToUpper().Contains(district.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + district;

                    scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
                    scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

                    string scheduleCollegePrincipalName = string.Empty;
                    string scheduleCollegePrincipalPhone = string.Empty;
                    string scheduleCollegePrincipalMobile = string.Empty;

                    if (principal != null)
                    {
                        scheduleCollegePrincipalName = principal.firstName + " " + principal.lastName + " " + principal.surname;
                        scheduleCollegePrincipalPhone = principal.landline + " " + (address.landline.Equals(principal.landline) ? "" : ", " + address.landline);
                        scheduleCollegePrincipalMobile = principal.mobile + " " + (address.mobile.Equals(principal.mobile) ? "" : ", " + address.mobile);
                    }

                    table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>" + count + "</b></td><td valign='top' colspan='5' style='font-size: 8px; line-height: 11px;'>" + scheduleCollegeName + ", " + scheduleCollegeAddress + "<br /><br /><b>Code : " + scheduleCollegeCode + "</b><br /><br />Prl : " + scheduleCollegePrincipalName.ToUpper() + "<br />Off : " + scheduleCollegePrincipalPhone + "<br />Cell : " + scheduleCollegePrincipalMobile + "</td>";

                    var specializations = db.jntuh_college_intake_proposed.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).ToList();
                    List<ProposedSpecialization> proposedSpecialization = new List<ProposedSpecialization>();

                    foreach (var spec in specializations)
                    {
                        ProposedSpecialization newSpec = new ProposedSpecialization();
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
                        proposedSpecialization.Add(newSpec);
                    }
                    proposedSpecialization = proposedSpecialization.OrderBy(p => p.degree).ThenBy(p => p.department).ThenBy(p => p.specialization).ThenBy(p => p.shift).ToList();
                    table += "<td valign='top' colspan='8' style='font-size: 8px;'>";
                    table += "<table border='0' cellpadding='1' cellspacing='0' width='100%'>";
                    if (proposedSpecialization.Count() > 0)
                    {
                        string fontSize = "8px";
                        string lineHeight = "12px";

                        string sFontSize = "8px";
                        string sLineHeight = "12px";

                        foreach (var spec in proposedSpecialization)
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
                                intake = spec.existing.ToString() + "+<b>" + (spec.proposed - spec.existing).ToString() + "</b>";
                            }

                            //if [proposed intake = 0] then show [existing intake (proposed intake)]
                            if (spec.proposed == 0)
                            {
                                intake = spec.existing.ToString() + "<b>(" + spec.proposed.ToString() + ")</b>";
                            }
                            string makeDegreeBold1 = string.Empty;
                            string makeDegreeBold2 = string.Empty;

                            //int flag = 0;
                            //if [existing intake = 0] then show [proposed intake]
                            if (spec.existing == 0)
                            {
                                //flag = 1;
                                intake = "<b>" + spec.proposed.ToString() + "</b>";
                                makeDegreeBold1 = "<b>";
                                makeDegreeBold2 = "</b>";
                            }

                            string degreeName = string.Empty;
                            if (spec.degree == "M.Tech" || spec.degree == "M.Pharmacy")
                            {
                                degreeName = spec.degree + " - ";
                            }

                            //if (flag == 0)
                            table += "<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + makeDegreeBold1 + degreeName + spec.specialization + makeDegreeBold2 + (spec.shift.Equals("1") ? "" : " - (Shift 2)") + "</td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'>" + intake + "</td></tr>";
                        }
                    }
                    table += "</table>";
                    table += "</td>";             

                   

                    //20-10-2014-Name(s) of the FFC Member(s) start 31/07/2014
                   int phaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

                    var auditorsList = (from c in db.jntuh_college
                                        join a in db.jntuh_address on c.id equals a.collegeId
                                        join d in db.jntuh_district on a.districtId equals d.id
                                        join s in db.jntuh_ffc_schedule on c.id equals s.collegeID
                                        join ord in db.jntuh_ffc_order on s.id equals ord.scheduleID
                                        join com in db.jntuh_ffc_committee on s.id equals com.scheduleID
                                        join aud in db.jntuh_ffc_auditor on com.auditorID equals aud.id
                                        join dis in db.jntuh_designation on aud.auditorDesignationID equals dis.id
                                        where (s.InspectionPhaseId == phaseId && c.isActive == true && a.addressTye == "College" && aud.isActive == true && dis.isActive == true && c.id == scheduleCollegeId)
                                        where (c.isActive == true && a.addressTye == "College" && d.isActive == true && aud.isActive == true && dis.isActive == true && d.id == districtid && c.id == scheduleCollegeId)
                                        group c by new
                                        {
                                            auditorName = aud.auditorName,
                                            auditorPreferredDesignation = aud.auditorPreferredDesignation,
                                            auditorPlace = aud.auditorPlace,
                                            designation = dis.designation,
                                            memberOrder = com.memberOrder,
                                            isConvenor = com.isConvenor,
                                            inspectionDate = s.inspectionDate
                                        } into g
                                        select new
                                        {
                                            auditorName = g.Key.auditorName,
                                            auditorPreferredDesignation = g.Key.auditorPreferredDesignation,
                                            auditorPlace = g.Key.auditorPlace,
                                            designation = g.Key.designation,
                                            memberOrder = g.Key.memberOrder,
                                            isConvenor = g.Key.isConvenor,
                                            inspectionDate = g.Key.inspectionDate
                                        }).ToList();


                   /* var auditorsList = (from c in db.jntuh_college
                                        join a in db.jntuh_address on c.id equals a.collegeId
                                        join d in db.jntuh_district on a.districtId equals d.id
                                        join s in db.jntuh_ffc_schedule on c.id equals s.collegeID
                                        join com in db.jntuh_ffc_committee on s.id equals com.scheduleID
                                        join aud in db.jntuh_ffc_auditor on com.auditorID equals aud.id
                                        join dis in db.jntuh_designation on aud.auditorDesignationID equals dis.id
                                        where (c.isActive == true && a.addressTye == "College" && d.isActive == true && aud.isActive == true && dis.isActive == true && d.id == districtid && c.id == scheduleCollegeId)
                                        select new
                                        {
                                            aud.auditorName,
                                            aud.auditorPreferredDesignation,
                                            aud.auditorPlace,
                                            dis.designation,
                                            com.memberOrder,
                                            com.isConvenor
                                        }).ToList();*/


                    auditorsList = auditorsList.OrderBy(a => a.memberOrder).ToList();
                    table += "<td valign='top' colspan='5' style='font-size: 8px;'>";
                    table += "<table border='0' cellpadding='1' cellspacing='0' width='100%'>";
                    int sano = 1;
                    string strdesignation = string.Empty;
                    foreach (var item in auditorsList)
                    {
                        string fontSize = "8px";
                        string lineHeight = "12px";
                        if (item.isConvenor == 1)
                        {
                            strdesignation = "- Convenor";
                        }
                        else
                        {
                            strdesignation = "- Member";
                        }

                        table += "<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + sano + " ." + item.auditorName + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + item.auditorPreferredDesignation + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + item.auditorPlace + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";text-align: right'>" + strdesignation + "</td></tr>";
                        sano++;

                    }
                    table += "</table>";
                    table += "</td>";
                   
                    //end

                    table += "<td valign='top' colspan='5' style='font-size: 8px;'>";
                    table += "</td>";

                    table += "<td valign='top' align='center' colspan='3' style='font-size: 8px;' rowspan='1'>&nbsp;</td></tr>";
                    table += "</tr>";
                }
                count++;
            }
            table += "</table>";

            string pdfPath = SaveSpecializations(table);
            string path = pdfPath.Replace("/", "\\");

            return File(path, "application/pdf", district + " - Colleges & Specializations.pdf");

            //return RedirectToAction("Index");
        }
        private string SaveSpecializations(string table)
        {
            string fullPath = string.Empty;

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A3, 10, 10, 10, 10);

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult CollegeCommittees()
        {
            //get old inspection phases
            ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                           join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                           join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                           select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().OrderByDescending(i=>i.id).ToList();

            return View("~/Views/Reports/CollegeCommittees.cshtml");
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CollegeCommittees(FormCollection fc)
        {
            string table = string.Empty;
            string district = string.Empty;           
            string strphaseId = fc["ddlPhaseId"].ToString();
            bool check = fc["chkdate"].Contains("true");            
            int phaseId = 0;
            if (strphaseId != string.Empty)
            {
                phaseId = Convert.ToInt32(strphaseId);
            }
            else
            {
                phaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            }

          
            int[] collegeIds = (from s in db.jntuh_ffc_schedule
                                join c in db.jntuh_college on s.collegeID equals c.id                                
                                where s.InspectionPhaseId == phaseId
                                //orderby c.collegeName
                                select c.id).Distinct().ToArray();
            //.Skip(120) .Take(60)

            table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            table += "<tr>";
                table += "<td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td>";
                table += "<td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td>";
                if (check == false)
                {
                    table += "<td align='center' colspan='3' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td>";
                }
            table += "</tr>";

            int count = 1;
            foreach (var scheduleCollegeId in collegeIds)
            {
                if (count > 0)
                {
                    district = string.Empty;

                    string scheduleCollegeCode = db.jntuh_college.Find(scheduleCollegeId).collegeCode;
                    string scheduleCollegeName = db.jntuh_college.Find(scheduleCollegeId).collegeName.ToUpper();
                    jntuh_address address = db.jntuh_address.Where(a => a.collegeId == scheduleCollegeId).Select(a => a).FirstOrDefault();
                    jntuh_college_principal_director principal = db.jntuh_college_principal_director.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).FirstOrDefault();
                    district = db.jntuh_district.Find(address.districtId).districtName;
                    //string scheduleCollegeAddress = string.Format("{0} {1} {2} {3} {4}", address.address, address.townOrCity, address.mandal, district, address.pincode);
                    string scheduleCollegeAddress = address.address;

                    if (!scheduleCollegeAddress.ToUpper().Contains(address.townOrCity.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;

                    if (!scheduleCollegeAddress.ToUpper().Contains(address.mandal.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;

                    if (!scheduleCollegeAddress.ToUpper().Contains(district.ToUpper()))
                        scheduleCollegeAddress = scheduleCollegeAddress + ", " + district;

                    scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
                    scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

                    string scheduleCollegePrincipalName = string.Empty;
                    string scheduleCollegePrincipalPhone = string.Empty;
                    string scheduleCollegePrincipalMobile = string.Empty;

                    if (principal != null)
                    {
                        scheduleCollegePrincipalName = principal.firstName + " " + principal.lastName + " " + principal.surname;
                        scheduleCollegePrincipalPhone = principal.landline + " " + (address.landline.Equals(principal.landline) ? "" : ", " + address.landline);
                        scheduleCollegePrincipalMobile = principal.mobile + " " + (address.mobile.Equals(principal.mobile) ? "" : ", " + address.mobile);
                    }

                    table += "<tr>";
                    table += "<td align='center' colspan='1' style='font-size: 8px; vertical-align: top;'><b>" + count + "</b></td><td valign='top' colspan='5' style='font-size: 8px; line-height: 11px;'>" + scheduleCollegeName + ", " + scheduleCollegeAddress + "<br /><br /><b>Code : " + scheduleCollegeCode + "</b><br /><br />Prl : " + scheduleCollegePrincipalName.ToUpper() + "<br />Off : " + scheduleCollegePrincipalPhone + "<br />Cell : " + scheduleCollegePrincipalMobile + "</td>";



                    var auditorsList = (from c in db.jntuh_college
                                        join a in db.jntuh_address on c.id equals a.collegeId
                                        //join d in db.jntuh_district on a.districtId equals d.id
                                        join s in db.jntuh_ffc_schedule on c.id equals s.collegeID
                                        join ord in db.jntuh_ffc_order on s.id equals ord.scheduleID
                                        join com in db.jntuh_ffc_committee on s.id equals com.scheduleID
                                        join aud in db.jntuh_ffc_auditor on com.auditorID equals aud.id
                                        join dis in db.jntuh_designation on aud.auditorDesignationID equals dis.id
                                        where (s.InspectionPhaseId == phaseId && c.isActive == true && a.addressTye == "College" && aud.isActive == true && dis.isActive == true && c.id == scheduleCollegeId)
                                        group c by new
                                        {
                                            auditorName = aud.auditorName,
                                            auditorPreferredDesignation = aud.auditorPreferredDesignation,
                                            auditorPlace = aud.auditorPlace,
                                            designation = dis.designation,
                                            memberOrder = com.memberOrder,
                                            isConvenor = com.isConvenor,
                                            inspectionDate = s.inspectionDate
                                        } into g
                                        select new
                                        {
                                            auditorName = g.Key.auditorName,
                                            auditorPreferredDesignation = g.Key.auditorPreferredDesignation,
                                            auditorPlace = g.Key.auditorPlace,
                                            designation = g.Key.designation,
                                            memberOrder = g.Key.memberOrder,
                                            isConvenor = g.Key.isConvenor,
                                            inspectionDate = g.Key.inspectionDate
                                        }).ToList();


                    auditorsList = auditorsList.OrderBy(a => a.memberOrder).ToList();
                    //auditorsList = auditorsList.ToList();

                    table += "<td valign='top' colspan='5' style='font-size: 8px;'>";
                    table += "<table border='0' cellpadding='1' cellspacing='0' width='100%'>";
                    int sano = 1;
                    string strdesignation = string.Empty;
                    string strInspectionDate = string.Empty;

                    foreach (var item in auditorsList)
                    {
                        if (sano == 1)
                        { strInspectionDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.inspectionDate.ToString()).Replace("12:00:00 AM", ""); }

                        string fontSize = "8px";
                        string lineHeight = "12px";
                        if (item.isConvenor == 1)
                        {
                            strdesignation = "- Convenor";
                        }
                        else
                        {
                            strdesignation = "- Member";
                        }

                        //table += "<tr><td align='center' style='font-size: 8px;'><b>" + item.auditorName + "</b></td></tr><tr><td align='center'  style='font-size: 8px;'><b>" + item.auditorPreferredDesignation + " " + ',' + item.auditorPlace + " </b></td></tr><tr><td align='center' style='font-size: 8px;'><b>" + item.designation + "</b></td></tr>";
                        table += "<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + sano + " ." + item.auditorName + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + item.auditorPreferredDesignation + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + item.auditorPlace + "</td></tr><tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";text-align: right'>" + strdesignation + "</td></tr>";
                        // table += "<tr><td valign='top'"+sano+"'. '" + item.auditorName + "</td></tr><tr><td valign='top'" + item.auditorPreferredDesignation + "</td></tr><tr><td valign='top'" + item.auditorPlace + "</td></tr><tr><td valign='top'" + item.designation + "</td></tr>";
                        sano++;

                    }
                    table += "</table>";
                    table += "</td>";
                    //end


                    //table += "<td valign='top' colspan='5' style='font-size: 8px;' rowspan='1'>&nbsp;";
                    //table += "</td>";
                    if (check == false)
                    {
                        table += "<td valign='top' align='center' colspan='3' style='font-size: 8px;' rowspan='1'>" + strInspectionDate + "</td>";
                    }
                    table += "</tr>";
                }
                count++;
            }
            table += "</table>";

            string pdfPath = SaveSpecializations(table);
            string path = pdfPath.Replace("/", "\\");

            var phase = db.jntuh_inspection_phase.Find(phaseId);
            var AY = db.jntuh_academic_year.Find(phase.academicYearId);
            return File(path, "application/pdf", "College_Committees_" + AY.academicYear.Replace("-","_") + "_" + phase.inspectionPhase.Replace(" ",""));

            //return RedirectToAction("CollegeCommittees");
        }
    }
}
