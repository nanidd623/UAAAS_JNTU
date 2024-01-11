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
using System.Text;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AuditScheduleReportController : BaseController
    {
        //Deficiency Report details

        private decimal sComputers = 0;
        private decimal sFaculty = 0;
        private string StrComputersShortageDegrees = null;
        private string StrFacultyShortageDegrees = null;

        private int academicYearId = 0;
        private int AcademicYear = 0;
        private int nextAcademicYearId = 0;
        private int LibrarycommitteobservationId = 0;
        private int[] CollegeIds;
        private int ClosureCourseId = 0;
        private int LabcommitteobservationId = 0;
        private int AcademicYearId11 = 0;
        private int AcademicYearId21 = 0;
        private int AcademicYearId31 = 0;
        private int AcademicYearId41 = 0;
        private int ComputersShortageCount = 0;
        private int FacultyShortageCount = 0;
        private int LabShortageCount = 0;
        private int LibraryShortageCount = 0;
        private int[] SubmitteCollegesId;
        private string ReportHeader = null;
        private int PrincipalShortageCount = 0;
        private string StrPrincipalShortage = null;
        private string StrLabsShortage = null;
        private string StrLibraryShortage = null;
        private int[] PermanentCollegesId;
        private int[] MBAAndMCAID;
        private int[] EmptyDegreeRatioId;
        private string[] EmptyDegrees = { "MAM", "MTM", "Pharm.D", "Pharm.D PB" };
        private string[] MBAAndMCAIDDegrees = { "MCA", "MBA" };
        private string[] StrNill = {    "-", 
                                        "- ", 
                                        "_",
                                        "--", 
                                        "---",
                                        "ACCEPTS", 
                                        "ADEQUATE",
                                        "ARE PROVIDED",
                                        "AS PER NORMS",
                                        "AVAILABLE AS PER NORMS",
                                        "AVAILABLE", 
                                        "AVAILABLE.",
                                        "AVAILABLE AND ADEQUATE",
                                        "AVERAGE SATISFACTORY",
                                        "BOOKS ARE THERE NORMS. NO SHORTAGE",
                                        "DO", 
                                        "FACILITIES ARE AVAILABLE ARE SUFFICIENT", 
                                        "GOOD",
                                        "GOOD AMBIENCE,PG LABS ARE WELL ESTABLISHED.",
                                        "GOOD AMBIANCE,ENOUGH BOOKS AVAILABLE.",
                                        "LABS ARE WELL EQUIPPED EXPERIMENTS WERE CONDUCTED PROPERLY",
                                        "LABORATORIES ARE TO",
                                        "LIBRARY IS TO LOCATED AT ONE FACE.",
                                        "NEED TO BE IMPROVED.",
                                        "NEW EDITIONS ARE TO BE ADDED.",
                                        "NEW REPUTED INTERNATIONAL JOURNAL MAY BE SUBSCRIBED",
                                        "NIL", 
                                        "NILL",
                                        "NIl",
                                        "NO COMPLAINTS",
                                        "NO DEFICIENCIES ALL ARE SUFFICIENT", 
                                        "NO DEFICIENCY ( B. TECH AGRICULTURAL COURSE WAS DROPPED )",
                                        "NO DEFICIENCY FOUND", 
                                        "NO DEFICIENCY",
                                        "NO DEFICIENCY ",
                                        "NO DEFICIENCY.",
                                        "NO DEFFICIENCY.",
                                        "NO DEFICIENCY EXCEPT FOR ONLINE JOURNALS.",
                                        "NO MAJOR COMPLAINTS",
                                        "NO SHORTAGE",
                                        "NO SHORTAGE.",
                                        "NO SPECIFIC DEFICIENCIES",
                                        "NONE", 
                                        "NOT MAJOR DEFICIENCIES",
                                        "OK",
                                        "ONE LIBRARIAN ONLY BOOKS ARE SUFFICIENT IN LIBRARY SETTING CAPACITY IS ONLY 30",
                                        "ORDER IN PLACED FOR JOURNALS",
                                        "REQUIRED NO OF UG AND PG LABS AVAILABLE",
                                        "REQUIRED LABS ARE AVAILABLE AS PER NORMS",
                                        "REQUIRED NUMBER OF LABS AVAILABLE",
                                        "REQUIRED NO OF LABS AVAILABLE AS PER NORMS",
                                        "-SATISFACTORY",
                                        "SATISFACTORY",
                                        "SATISFACTORY.",
                                        "SATISFACTION",
                                        "SUFFICIENT AVAILABLE.", 
                                        "SUFFICIENT LABS AVAILABLE",
                                        "SUFFICIENT NO DEFICIENCY",
                                        "SUFFICIENT NO OF BOOKS AND JOURNALS ARE AVAILABLE AND STUDENTS HAVE E-JOURNALS ACCESS",
                                        "SUFFICIENT",
                                        "SUFFICIENT NUMBER OF BOOKS AND BOOKS AND JOURNALS AVAILABLE AS PER NORMS",
                                        "SUFFICIENT NUMBER OF BOOKS AND JOURNALS AVAILABLE AS PER NORMS",
                                        "SUFFICIENT BOOKS AND JOURNALS AVAILABLE",
                                        "SUFFICIENT BOOKS ARE AVAILABLE",
                                        "SUFFICIENT LIBRARY BOOKS AND JOURNALS AVAILABLE",
                                        "SUFFICIENT VOLUMES ARE PROVIDED",
                                        "SUFFICIENT BOOKS AND TITLES ARE AVAILABLE",
                                        "TO BE IMPROVED",
                                        "TITLE,VOLUMES,SITTING CAPACITY IS ADEQUATE",
                                        "THERMAL ENGINEERING AND HEAT TRANSFER LABS TO BE ESTABLISHED ORDERS TO BE PLACED DETAILS ENCLOSED",
                                        "UG & PG LABS ADEQUATE",
                                        "WELL MAINTAINED",
                                        "WELL EQUIPPED LABS"
                                   };


        private uaaasDBContext db = new uaaasDBContext();
        private string DDMMYYYYspace = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        private string YYYYspace = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? PhaseId, string type, string clusterName)
        {
            List<AuditScheduleReport> schedules = new List<AuditScheduleReport>();

            int[] CollegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false).Select(e => e.collegeId).ToArray();
            IQueryable<jntuh_college> CollegeList = db.jntuh_college.Where(e => e.isActive == true && CollegeIds.Contains(e.id)).Select(c => c);

            var jntuh_ffc_schedule = db.jntuh_ffc_schedule.Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
           // var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
           // var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();
          //  var jntuh_designation = db.jntuh_designation.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(e => e).ToList();
            var jntuh_ffc_committee = db.jntuh_ffc_committee.Select(e => e).ToList();
          //  var jntuh_address = db.jntuh_address.Select(e => e).ToList();
            var jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Select(e => e).ToList();



            //if (db.jntuh_ffc_schedule.Count() > 0)
           // {
                //get old inspection phases
                ViewBag.oldInspectionPhases = (from s in db.jntuh_ffc_schedule
                                               join p in db.jntuh_inspection_phase on s.InspectionPhaseId equals p.id
                                               join a in db.jntuh_academic_year on p.academicYearId equals a.id
                                               //where p.isActive == false
                                               select new OldInspectionPhaseIds { name = a.academicYear + " (" + p.inspectionPhase + ")", id = (int)s.InspectionPhaseId }).Distinct().ToList();

                int InspectionPhaseId = 0;

                if (PhaseId == null)
                    InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
                else
                    InspectionPhaseId = (int)PhaseId;

                var listSchedules = db.jntuh_ffc_schedule.Where(s => s.InspectionPhaseId == InspectionPhaseId).OrderByDescending(s => new { s.InspectionPhaseId, s.inspectionDate }).ToList();
                if (clusterName != null)
                {
                    int?[] clusetercollegeIDs = db.college_clusters.Where(c => c.clusterName == clusterName).Select(c => c.collegeId).ToArray();
                    listSchedules = listSchedules.Where(s => clusetercollegeIDs.Contains(s.collegeID)).ToList();
                }
                foreach (var item in listSchedules)
                {
                    AuditScheduleReport newSchedule = new AuditScheduleReport();
                    newSchedule.scheduleId = item.id;
                    newSchedule.collegeId = item.collegeID == null ? 0 : (int)item.collegeID;
                    newSchedule.collegeCode = CollegeList.Where(c => c.id == newSchedule.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                    newSchedule.collegeName = CollegeList.Where(c => c.id == newSchedule.collegeId).Select(c => c.collegeName).FirstOrDefault();
                    newSchedule.auditDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.inspectionDate.ToString());
                    if (item.alternateInspectionDate != null)
                        newSchedule.alternateAuditDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.alternateInspectionDate.ToString());
                    newSchedule.orderDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.orderDate.ToString());
                    newSchedule.assignedAuditors = new List<AssignedAuditors>();
                    var listcommittee = jntuh_ffc_committee.Where(c => c.scheduleID == newSchedule.scheduleId).OrderBy(c => c.memberOrder).Select(c => c).ToList();

                    foreach (var auditor in listcommittee)
                    {
                        AssignedAuditors newAuditor = new AssignedAuditors();
                        newAuditor.auditorId = auditor.id;
                        newAuditor.auditorName = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorID).Select(a => a.auditorName).FirstOrDefault();

                        if (db.jntuh_ffc_auditor.Where(a => a.id == auditor.auditorID).Select(a => a.auditorDepartmentID).FirstOrDefault() != null)
                        {
                            newAuditor.departmentId = (int)jntuh_ffc_auditor.Where(a => a.id == auditor.auditorID).Select(a => a.auditorDepartmentID).FirstOrDefault();
                            newAuditor.deptartmentName = jntuh_department.Where(d => d.id == newAuditor.departmentId).Select(d => d.departmentName).FirstOrDefault();
                        }

                        newAuditor.auditorDesignation = string.Empty;
                        newAuditor.preferredDesignation = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorID).Select(a => a.auditorPreferredDesignation).FirstOrDefault();
                        newAuditor.isConvenor = auditor.isConvenor == 1 ? true : false;
                        newAuditor.isDeleted = false;
                        newAuditor.auditorLoad = "0";
                        newAuditor.preferredDesignation = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorID).Select(a => a.auditorPlace).FirstOrDefault();
                        newAuditor.deptartmentName = jntuh_ffc_external_auditor_groups.Where(g => g.University == newAuditor.preferredDesignation).Select(g => g.Group).FirstOrDefault();
                        newSchedule.assignedAuditors.Add(newAuditor);
                    }

                    newSchedule.isRevised = (int)item.isRevisedOrder;
                    List<int> totalOrders = db.jntuh_ffc_order.Where(o => o.scheduleID == item.id).Select(o => o.id).ToList();
                    newSchedule.totalOrdersSent = totalOrders.Count();

                    //newSchedule.isRevised = 0;
                    //jntuh_ffc_order order = db.jntuh_ffc_order.Where(o => o.scheduleID == item.id && o.inspectionDate == item.inspectionDate && o.orderDate == item.orderDate).Select(o => o).FirstOrDefault();
                    //if (order != null)
                    //{
                    //    newSchedule.isRevised = 1;
                    //}

                    var scheduleUpdatedDate = db.jntuh_ffc_schedule.Find(item.id).updatedOn;
                    var orderSentDate = db.jntuh_ffc_order.Where(o => o.scheduleID == item.id).OrderByDescending(o => o.id).Select(o => o.createdOn).FirstOrDefault();

                    newSchedule.isLastOrderSent = false;
                    jntuh_ffc_order order = db.jntuh_ffc_order.Where(o => o.scheduleID == item.id && o.inspectionDate == item.inspectionDate && o.orderDate == item.orderDate && o.isRevisedOrder == item.isRevisedOrder).OrderByDescending(o => o.id).Select(o => o).FirstOrDefault();
                    if (order != null)
                    {
                        if (scheduleUpdatedDate != null && orderSentDate != null)
                        {
                            if (scheduleUpdatedDate > orderSentDate)
                            {
                                newSchedule.isLastOrderSent = false;
                            }
                            else
                            {
                                newSchedule.isLastOrderSent = true;
                            }
                        }
                        else
                        {
                            newSchedule.isLastOrderSent = true;
                        }
                    }

                    schedules.Add(newSchedule);
                }
           // }
            if ((type == "Export" || clusterName != null) && schedules.Count() > 0)
            {
                if (clusterName == null)
                {
                    clusterName = string.Empty;
                    //int?[] clusetercollegeIDs = db.college_clusters.Where(c => c.clusterName == clusterName).Select(c => c.collegeId).ToArray();
                    // schedules = schedules.Where(s => clusetercollegeIDs.Contains(s.collegeId)).ToList();
                }

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=AuditScheduleReport " + clusterName + ".xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AuditScheduleReport.cshtml", schedules);
            }
            else
            {
                return View("~/Views/Admin/AuditScheduleReport.cshtml", schedules);
            }
        }

        [Authorize(Roles = "Admin")]
       public ActionResult Order(int id, string cmd)
       {
            string table = string.Empty;
            if (cmd == "SEND" || cmd == "PREVIEW")
            {

                var jntuh_ffc_schedule = db.jntuh_ffc_schedule.Select(e => e).ToList();
                var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
               // var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
               // var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();
               // var jntuh_designation = db.jntuh_designation.Where(d => d.isActive == true).Select(e => e).ToList();
                var jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(e => e).ToList();
                var jntuh_ffc_committee = db.jntuh_ffc_committee.Select(e => e).ToList();
                var jntuh_address = db.jntuh_address.Select(e => e).ToList();
                var jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Select(e => e).ToList();


                var inspectionDate = jntuh_ffc_schedule.Where(f=>f.id == id).Select(e=>e.inspectionDate).FirstOrDefault();
                var altInspectionDate = jntuh_ffc_schedule.Where(f => f.id == id).Select(e => e.alternateInspectionDate).FirstOrDefault();
                var orderDate = jntuh_ffc_schedule.Where(f => f.id == id).Select(e => e.orderDate).FirstOrDefault();
                int collegeId = (int)jntuh_ffc_schedule.Where(f => f.id == id).Select(e => e.collegeID).FirstOrDefault();
                string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
                int isRevised = (int)jntuh_ffc_schedule.Where(f => f.id == id).Select(e => e.isRevisedOrder).FirstOrDefault();

                List<int> sameDaySchedules = (from s1 in jntuh_ffc_schedule
                                              join c1 in jntuh_ffc_committee on s1.id equals c1.scheduleID
                                              join s2 in jntuh_ffc_schedule on s1.inspectionDate equals s2.inspectionDate
                                              join c2 in jntuh_ffc_committee on s2.id equals c2.scheduleID
                                              where s1.inspectionDate == inspectionDate && s1.isRevisedOrder == isRevised
                                              select s1.id).Distinct().ToList();

                List<int> filtered = new List<int>();
                List<Committee> prevCommittee = new List<Committee>();

                List<int?> NullAuditorCollege = jntuh_ffc_committee.Where(s => s.scheduleID == id).Select(s => s.auditorID).ToList();
                if (NullAuditorCollege.Count == 0)
                {
                    filtered.Add(id);
                }
                else
                {
                    prevCommittee = db.jntuh_ffc_committee.OrderBy(c => c.memberOrder).Where(c => c.scheduleID == id).Select(c => new Committee { auditorId = (int)c.auditorID, isConvenor = (int)c.isConvenor }).ToList();
                    filtered.Add(id);
                }

              

                foreach (var scheduleid in sameDaySchedules)
                {
                    bool sameCommittee = false;
                    List<Committee> newCommittee = new List<Committee>();
                    newCommittee = db.jntuh_ffc_committee.OrderBy(c => c.memberOrder).Where(c => c.scheduleID == scheduleid).Select(c => new Committee { auditorId = (int)c.auditorID, isConvenor = (int)c.isConvenor }).ToList();

                    if (prevCommittee.Count() > 0)
                    {
                        sameCommittee = AreEqual(prevCommittee, newCommittee);

                        if (sameCommittee)
                        {
                            if (!filtered.Contains(scheduleid))
                                filtered.Add(scheduleid);
                        }
                    }
                }

                List<int> schedulesToBeSent = new List<int>();
                schedulesToBeSent = filtered;

                List<jntuh_ffc_committee> committeeEmailsMobiles = new List<jntuh_ffc_committee>();

                int totalColleges = schedulesToBeSent.Count();

                if (totalColleges > 0)
                {
                    table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; margin: 0 auto;'";
                    table += "<tr><td align='center' colspan='3' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='2' style='font-size: 8px;'><b>Courses Offered</b></td><td align='center' colspan='2' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td><td align='center' colspan='1' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td></tr>";

                    int scheduelIndex = 0;
                    foreach (var scheduleid in schedulesToBeSent)
                    {
                        int scheduleCollegeId = (int)jntuh_ffc_schedule.Where(f => f.id == scheduleid).Select(e => e.collegeID).FirstOrDefault();
                        string scheduleCollegeCode = db.jntuh_college.Find(scheduleCollegeId).collegeCode;
                        string scheduleCollegeName = db.jntuh_college.Find(scheduleCollegeId).collegeName.ToUpper();
                        jntuh_address address = db.jntuh_address.Where(a => a.collegeId == scheduleCollegeId).Select(a => a).FirstOrDefault();
                        
                        //jntuh_college_principal_director principal = db.jntuh_college_principal_director.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).FirstOrDefault();

                        jntuh_college_principal_registered principal_old = db.jntuh_college_principal_registered.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).FirstOrDefault();
                        jntuh_college_principal_director director = new jntuh_college_principal_director();
                        jntuh_registered_faculty principal =new jntuh_registered_faculty();
                       
                        if(principal_old == null)
                        {
                            director = db.jntuh_college_principal_director.Where(s => s.collegeId == scheduleCollegeId && s.type == "DIRECTOR").Select(e => e).FirstOrDefault();
                        }
                        else{
                            principal = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == principal_old.RegistrationNumber).Select(e => e).FirstOrDefault();
                        }

                        
                        
                        string district = db.jntuh_district.Find(address.districtId).districtName;
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
                        string scheduleCollegeOfficeDetails = string.Empty;
                        string scheduleCollegePrincipalMobile = string.Empty;

                        if (principal != null)
                        {
                            scheduleCollegePrincipalName = string.Format("{0} {1} {2}", principal.FirstName, principal.LastName, principal.MiddleName);
                            scheduleCollegeOfficeDetails = string.Format("{0}", address.landline );
                            //scheduleCollegePrincipalPhone = string.Format("{0}{1}", principal.landline, address.landline.Equals(principal.landline) ? "" : ", " + address.landline);
                            scheduleCollegePrincipalMobile = string.Format("{0}{1}", address.mobile == null ? " " : address.mobile + ",", principal.Mobile);
                        }
                        else
                        {
                             scheduleCollegePrincipalName = string.Format("{0} {1} {2}",director.firstName,director.lastName,director.surname);
                            scheduleCollegeOfficeDetails = string.Format("{0}", address.landline );
                            //scheduleCollegePrincipalPhone = string.Format("{0}{1}", principal.landline, address.landline.Equals(principal.landline) ? "" : ", " + address.landline);
                            scheduleCollegePrincipalMobile = string.Format("{0}{1}", address.mobile == null ? " " : address.mobile + ",", director.mobile);
                        }
                        table += string.Format("<tr><td valign='top' colspan='3' style='font-size: 8px; line-height: 11px;'>{0},<br />{1}<br /><br /><b>Code : {2}</b><br /><br />Prl : {3}<br />Off : {4}<br />Cell : {5}</td>", scheduleCollegeName, scheduleCollegeAddress, scheduleCollegeCode, scheduleCollegePrincipalName.ToUpper(), scheduleCollegeOfficeDetails , scheduleCollegePrincipalMobile);

                        var specializations = db.jntuh_college_intake_existing.Where(p => p.collegeId == scheduleCollegeId).Select(p => p).ToList();
                        List<ProposedSpecialization> proposedSpecialization = new List<ProposedSpecialization>();

                        //foreach (var spec in specializations)
                        //{
                        //    ProposedSpecialization newSpec = new ProposedSpecialization();
                        //    newSpec.specialization = db.jntuh_specialization.Find(spec.specializationId).specializationName;
                        //    newSpec.shift = db.jntuh_shift.Find(spec.shiftId).shiftName;
                        //    int deptId = db.jntuh_specialization.Find(spec.specializationId).departmentId;
                        //    newSpec.department = db.jntuh_department.Find(deptId).departmentName;
                        //    int degreeId = db.jntuh_department.Find(deptId).degreeId;
                        //    newSpec.degree = db.jntuh_degree.Find(degreeId).degree;
                        //    int academicYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
                        //    int actualYear = db.jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.actualYear).FirstOrDefault();
                        //    int prposedYear = actualYear + 1;
                        //    int prposedYearId = db.jntuh_academic_year.Where(ay => ay.actualYear == prposedYear).Select(ay => ay.id).FirstOrDefault();

                        //    newSpec.existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == scheduleCollegeId && e.academicYearId == academicYear && e.specializationId == spec.specializationId && e.shiftId == spec.shiftId).Select(e => e.approvedIntake).FirstOrDefault();
                        //    newSpec.proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == scheduleCollegeId && p.academicYearId == prposedYearId && p.specializationId == spec.specializationId && p.shiftId == spec.shiftId).Select(p => p.proposedIntake).FirstOrDefault();
                        //    proposedSpecialization.Add(newSpec);
                        //}
                        //proposedSpecialization = proposedSpecialization.OrderBy(p => p.degree).ThenBy(p => p.department).ThenBy(p => p.specialization).ThenBy(p => p.shift).ToList();
                        table += "<td valign='top' colspan='2' style='font-size: 8px;'><b>Note: For complete details on courses offered refer section (9) in A-418 form</b></td>";
                        //table += "<td valign='top' colspan='2' style='font-size: 8px;'>";
                        //table += "<table border='0' cellpadding='1' cellspacing='0' width='100%'>";
                        //if (proposedSpecialization.Count() > 0)
                        //{
                        //    string fontSize = schedulesToBeSent.Count() > 2 ? "8px" : "8px";
                        //    string lineHeight = schedulesToBeSent.Count() > 2 ? "12px" : "12px";

                        //    fontSize = proposedSpecialization.Count() > 10 ? "8px" : "8px";
                        //    fontSize = proposedSpecialization.Count() > 20 ? "8px" : "8px";
                        //    fontSize = proposedSpecialization.Count() > 30 ? "7px" : "7px";

                        //    lineHeight = proposedSpecialization.Count() > 10 ? "12px" : "12px";
                        //    lineHeight = proposedSpecialization.Count() > 20 ? "12px" : "12px";
                        //    lineHeight = proposedSpecialization.Count() > 30 ? "11px" : "11px";

                        //    string sFontSize = schedulesToBeSent.Count() > 2 ? "8px" : "8px";
                        //    string sLineHeight = schedulesToBeSent.Count() > 2 ? "12px" : "12px";

                        //    sFontSize = proposedSpecialization.Count() > 10 ? "8px" : "8px";
                        //    sFontSize = proposedSpecialization.Count() > 20 ? "8px" : "8px";
                        //    sFontSize = proposedSpecialization.Count() > 30 ? "7px" : "7px";

                        //    sLineHeight = proposedSpecialization.Count() > 10 ? "12px" : "12px";
                        //    sLineHeight = proposedSpecialization.Count() > 20 ? "12px" : "12px";
                        //    sLineHeight = proposedSpecialization.Count() > 30 ? "11px" : "11px";

                        //    fontSize = "8px";
                        //    sFontSize = "8px";
                        //    lineHeight = "18px";
                        //    sLineHeight = "18px";

                        //    foreach (var spec in proposedSpecialization)
                        //    {
                        //        string intake = string.Empty;

                        //        //if both are equal show only proposed intake
                        //        if (spec.existing == spec.proposed)
                        //        {
                        //            intake = spec.proposed.ToString();
                        //        }

                        //        //if [existing intake > proposed intake] then show [existing intake - decrease in proposed intake]
                        //        if (spec.existing > spec.proposed)
                        //        {
                        //            // intake = spec.existing.ToString() + "-<b>" + (spec.existing - spec.proposed).ToString() + "</b>";
                        //            intake = spec.proposed.ToString();
                        //        }

                        //        //if [existing intake < proposed intake] then show [existing intake + increase in proposed intake]
                        //        if (spec.existing < spec.proposed)
                        //        {
                        //            //intake = spec.existing.ToString() + "+<b>" + (spec.proposed - spec.existing).ToString() + "</b>";
                        //            intake = spec.proposed.ToString();

                        //        }

                        //        //if [proposed intake = 0] then show [existing intake (proposed intake)]
                        //        if (spec.proposed == 0)
                        //        {
                        //            //intake = spec.existing.ToString() + "<b>(" + spec.proposed.ToString() + ")</b>";
                        //            intake = spec.proposed.ToString();
                        //        }
                        //        string makeDegreeBold1 = string.Empty;
                        //        string makeDegreeBold2 = string.Empty;

                        //        int flag = 0;
                        //        //if [existing intake = 0] then show [proposed intake]
                        //        if (spec.existing == 0)
                        //        {
                        //            flag = 1;
                        //            // intake = "<b>" + spec.proposed.ToString() + "</b>";
                        //            intake = spec.proposed.ToString();
                        //            makeDegreeBold1 = "<b>";
                        //            makeDegreeBold2 = "</b>";
                        //        }

                        //        string degreeName = string.Empty;
                        //        if (spec.degree == "M.Tech" || spec.degree == "M.Pharmacy")
                        //        {
                        //            degreeName = spec.degree + " - ";
                        //        }

                        //        //if (flag == 0)Bold
                        //        //table += string.Format("<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + makeDegreeBold1 + "{0}" + makeDegreeBold2 + "{1}</td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'>{2}</td></tr>", degreeName + spec.specialization, spec.shift.Equals("1") ? "" : " - 2", intake);                               
                        //        //table += string.Format("<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>{0}{1}</td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'>{2}</td></tr>", degreeName + spec.specialization, spec.shift.Equals("1") ? "" : " - 2", string.Empty);
                        //        table += string.Format("<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'></td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'></td></tr>");


                        //    }
                        //}
                        //table += "</table>";
                        //table += "</td>";

                        if (scheduelIndex == 0)
                        {
                            List<jntuh_ffc_committee> scheduleCommittee = db.jntuh_ffc_committee.Where(c => c.scheduleID == scheduleid).OrderBy(c => c.memberOrder).Select(c => c).ToList();
                            committeeEmailsMobiles = scheduleCommittee;

                            table += "<td valign='top' colspan='2' style='font-size: 8px;' rowspan='" + totalColleges + "'>";
                            table += "<table border='0' cellpadding='3' cellspacing='0' width='100%'>";
                            int memberIndex = 1;
                            foreach (var member in scheduleCommittee)
                            {
                                var auditor = db.jntuh_ffc_auditor.Find(member.auditorID);
                                //string strcampusplace = auditor.auditorPlace + ",Hyderabad";
                                //table += string.Format("<tr><td style='line-height: 11px;'>{0}. {1}<br />{2}, {3}</td></tr>", memberIndex, auditor.auditorName, auditor.auditorPreferredDesignation, auditor.auditorPlace);
                                string strauditorplace = string.Empty;
                                string strmobileno = string.Empty;
                                if (auditor.auditorPlace == "GOVT.POLYTECHNIC" || auditor.auditorPlace == "NITW")
                                {
                                    strauditorplace = auditor.auditorPlace;
                                }
                                else
                                {
                                    strauditorplace = auditor.auditorPlace + ", Hyderabad";
                                }
                                if (auditor.auditorMobile1 != null)
                                {
                                    strmobileno = auditor.auditorMobile1.Replace(".0000", "");
                                }
                                else
                                {
                                    strmobileno = string.Empty;
                                }

                               // table += string.Format("<tr><td style='line-height: 11px;font-size:8px;'>{0}. {1},<br />{2},<br />{3}</td></tr>", memberIndex, auditor.auditorName, strauditorplace, strmobileno);
                                table += string.Format("<tr><td style='line-height: 11px;font-size:8px;'>{0}. {1}</td></tr>", memberIndex, auditor.auditorName);
                                string auditorType = string.Empty;
                                if (member.isConvenor == 1) auditorType = "Convenor"; else auditorType = "Member";
                                //table += "<tr><td align='right'>- " + auditorType + "</td></tr>";
                                table += "<tr><td align='right'></td></tr>";
                                memberIndex++;
                            }
                            table += "</table>";
                            table += "</td>";

                            if (altInspectionDate != null)
                            {
                                table += "<td valign='top' align='center' valign='middle' colspan='1' style='font-size: 8px;' rowspan='" + totalColleges + "'>" + UAAAS.Models.Utilities.MMDDYY2DDMMYY(inspectionDate.ToString()) + "<br />&<br />" + UAAAS.Models.Utilities.MMDDYY2DDMMYY(altInspectionDate.ToString()) + "</td></tr>";
                               // table += "<td valign='top' align='center' valign='middle' colspan='1' style='font-size: 12px;' rowspan='" + totalColleges + "'><b>" + DateTime.Now.ToString("dd-MM-yyy") + "</b></td></tr>";
                            }
                            else
                            {
                                table += "<td valign='top' align='center' valign='middle' colspan='1' style='font-size: 8px;' rowspan='" + totalColleges + "'>" + UAAAS.Models.Utilities.MMDDYY2DDMMYY(inspectionDate.ToString()) + "</td></tr>";
                               // table += "<td valign='top' align='center' valign='middle' colspan='1' style='font-size: 12px;' rowspan='" + totalColleges + "'><b>" + DateTime.Now.ToString("dd-MM-yyy") + "</b></td></tr>";
                            }
                        }
                        else
                        { table += "</tr>"; }
                        scheduelIndex++;
                    }
                    //table += "<tr><td colspan='8' style='text-align: center; font-size: 8px;'><b>Note: Bold letters indicate new course /increase in intake / reduction in intake</b></td></tr>";
                   // table += "<tr><td colspan='8' style='text-align: center; font-size: 8px;'><b>Note: For complete details on courses offered refer section (9) in A-418 form</b></td></tr>";
                    table += "</table>";

                    string orderPath = string.Empty;
                    List<string> collegeDataPath = new List<string>();

                    if (cmd == "PREVIEW")
                    {
                        orderPath = SaveOrderPdf(1, table, UAAAS.Models.Utilities.MMDDYY2DDMMYY(orderDate.ToString()), isRevised, collegeCode);
                        string path = orderPath.Replace("/", "\\");

                        //return File(path, "application/pdf");

                        string orderType = string.Empty;
                        if (isRevised == 0)
                        {
                            orderType = "Order";
                        }
                        else
                        {
                            orderType = "RevisedOrder";
                        }
                        return File(path, "application/vnd.ms-word", path.Substring(path.LastIndexOf('\\') + 1));
                    }
                    else
                    {
                        orderPath = SaveOrderPdf(0, table, UAAAS.Models.Utilities.MMDDYY2DDMMYY(orderDate.ToString()), isRevised, collegeCode);
                        string path = orderPath.Replace("/", "\\");

                        //foreach (var scheduleId in schedulesToBeSent)
                        //{
                        //    int sCollegeId = (int)db.jntuh_ffc_schedule.Find(scheduleId).collegeID;
                        //    string collegePath = SaveCollegeDataPdf(0, sCollegeId);
                        //    collegePath = collegePath.Replace("/", "\\");
                        //    collegeDataPath.Add(collegePath);
                        //}

                        SendOrder(UAAAS.Models.Utilities.MMDDYY2DDMMYY(inspectionDate.ToString()), isRevised, path, collegeDataPath, committeeEmailsMobiles, schedulesToBeSent);
                    }
                }
            }
            return RedirectToAction("Index");
        }

        private string SaveOrderPdf(int preview, string table, string OrderDate, int SendingStatus, string collegeCode)
        {
            string fullPath = string.Empty;

           // Response.ClearContent();
           // Response.ClearHeaders();
           // Response.Buffer = true;
           //// string collegeCode = db.jntuh_college.Where(c => c.isActive == true && c.id == id).Select(c => c.collegeCode).FirstOrDefault();
           // Response.AddHeader("content-disposition", "attachment; filename=" + collegeCode + "- DeficiencyCorrections" + ".doc");
           // Response.ContentType = "application/vnd.ms-word ";
           // Response.Charset = string.Empty;
           // StringBuilder str = new StringBuilder();
          

           // Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);

           // pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
           // pdfDoc.SetMargins(60, 50, 60, 60);

           // int IsRevised = 0;
           // string orderType = string.Empty;

           // if (SendingStatus == 0)
           // {
           //     orderType = "Order";
           //     IsRevised = 0;
           // }
           // else
           // {
           //     orderType = "Revised Order";
           //     IsRevised = 1;
           // }
           // string path = Server.MapPath("~/Content/PDFReports/Orders/");

           // // Create a new PdfWrite object, writing the output to a MemoryStream
           // //open pdf
           // var output = new MemoryStream();
           // var writer = PdfWriter.GetInstance(pdfDoc, output);

           // if (preview == 0)
           // {
           //     fullPath = "~/Content/PDFReports" + "/Orders/" + orderType.Replace(" ", "") + "_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".doc";
           //     PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));
           // }
           // else
           // {
           //     fullPath = "~/Content/PDFReports" + "/temp/" + orderType + "_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".doc";
           //     PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));
           // }

           // pdfDoc.Open();


           // string contents = string.Empty;

           // StreamReader sr;

           // //Read file from server path
           // sr = System.IO.File.OpenText(Server.MapPath("~/Content/Order.html"));

           // //store content in the variable
           // contents = sr.ReadToEnd();
           // sr.Close();
           // if (!string.IsNullOrEmpty(OrderDate))
           //     contents = contents.Replace("##ORDER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
           // else
           //     contents = contents.Replace("##ORDER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
           // // contents = contents.Replace("##ORDER_DATE##", OrderDate);

           // contents = contents.Replace("##TABLE##", table);

           // List<IElement> parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents.ToString()), null);

           // foreach (var htmlElement in parsedHtmlElements)
           // {
           //     pdfDoc.Add((IElement)htmlElement);
           // }

           // pdfDoc.Close();

           // Response.Output.Write(contents.ToString());
           // Response.Flush();
           // Response.End();

           // return fullPath;



           // Set page size as A4


            var pdfDoc = new Document(PageSize.A4, 50, 50, 40, 40);

            int IsRevised = 0;
            string orderType = string.Empty;

            if (SendingStatus == 0)
            {
                orderType = "Order";
                IsRevised = 0;
            }
            else
            {
                orderType = "Revised Order";
                IsRevised = 1;
            }

            string path = Server.MapPath("~/Content/PDFReports/Orders");

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            if (preview == 0)
            {
                fullPath = "~/Content/PDFReports" + "/Orders/" + orderType.Replace(" ", "") + "_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));
            }
            else
            {
                fullPath = "~/Content/PDFReports" + "/temp/" + orderType + "_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));
            }

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/Order.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            if (string.IsNullOrEmpty(OrderDate))
                contents = contents.Replace("##ORDER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
            else
               // contents = contents.Replace("##ORDER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
                contents = contents.Replace("##ORDER_DATE##", OrderDate);
         
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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        //PGCourse
        [Authorize(Roles = "Admin")]
        public ActionResult CollegePGCourse(int collegeId)
        {
            string fullPath = string.Empty;

            //Set page size as A4           
            Document pdfDoc = new Document(PageSize.A4, 60, 50, 60, 60);
            // Document pdfDoc = new Document(PageSize.A4, 50, 50,50,50);
            string path = Server.MapPath("~/Content/PDFReports");
            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);
            //PageEventHelper pageEventHelper = new PageEventHelper();
            //writer.PageEvent = pageEventHelper;
            string collegeName = db.jntuh_college.Find(collegeId).collegeName;
            string path1 = path + "/CollegeData/PGCourse";
            if (!Directory.Exists(path1))
            {
                Directory.CreateDirectory(path1);
            }

            fullPath = path1 + "/A-415-PGCourse" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
            ITextEvents iTextEvents = new ITextEvents();
            iTextEvents.CollegeName = collegeName;
            iTextEvents.CollegeCode = collegeCode;
            iTextEvents.formType = "A-415";
            pdfWriter.PageEvent = iTextEvents;

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/PGCourse.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = PGCourses(collegeId, contents);
            contents = collegeIntakeProposed201415(collegeId, contents);
            contents = contents.Replace("##collegeName##", collegeName);
            contents = contents.Replace("##collegeCode##", collegeCode);

            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

            foreach (var htmlElement in parsedHtmlElements)
            {
                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
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
            return File(returnPath, "application/pdf", "A-415-PG Course-" + collegeCode + ".pdf");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegeData(int preview, int collegeId)
        {
            string pdfPath = SaveCollegeDataPdf(preview, collegeId);
            string path = pdfPath.Replace("/", "\\");

            string collegeCode = db.jntuh_college.Find(collegeId).collegeCode;
            return File(path, "application/pdf", "A-415-" + collegeCode + ".pdf");
        }

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
                fullPath = path + "/CollegeData/A-415_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
                ITextEvents iTextEvents = new ITextEvents();
                iTextEvents.CollegeName = collegeName;
                iTextEvents.CollegeCode = collegeCode;
                // iTextEvents.formType = "A-414";
                iTextEvents.formType = "A-415";
                pdfWriter.PageEvent = iTextEvents;
            }
            else
            {
                //fullPath = path + "/temp/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                // PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

                //fullPath = path + "/CollegeData/A-414_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
                fullPath = path + "/CollegeData/A-415_" + collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
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
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/A-415.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            contents = collegeAuditscheduleDate(collegeId, contents);
            contents = affiliationType(collegeId, contents);
            contents = collegeInformation(collegeId, contents);
            contents = societyInformation(collegeId, contents);
            contents = principalDirectorDetails(collegeId, contents);
            contents = chairPersonDetails(collegeId, contents);
            contents = otherCollegesAndOtherCourses(collegeId, contents);
            contents = landInformation(collegeId, contents);
            contents = administrativeLandInformation(collegeId, contents);
            contents = InstrctionalLandInformation(collegeId, contents);
            contents = collegeIntakeExisting(collegeId, contents);
            contents = collegeAcademicPerformance(collegeId, contents);
            //contents = collegeIntakeProposed(collegeId, contents);
            //contents = PGCourses(collegeId, contents);
            //contents = collegeTeachingFacultyPosition(collegeId, contents);
            ////Overall Faculty Student Ratio-- contents = collegeFacultyStudentRatio(collegeId, contents);
            //contents = CollegeOverallFacultyStudentRatio(collegeId, contents);
            //// contents = collegeFacultyMembers(collegeId, contents);
            //contents = collegeTotalFacultyMembers(collegeId, contents);

            contents = collegeTachingFacultyMembers(collegeId, contents);
            contents = collegeNonTachingFacultyMembers(collegeId, contents);
            contents = collegeTechnicalFacultyMembers(collegeId, contents);
            contents = collegeLaboratories(collegeId, contents);
            contents = collegeLibrary(collegeId, contents);
            contents = collegeComputerLab(collegeId, contents);
            contents = collegeComputers(collegeId, contents);
            contents = collegeInterBandWidth(collegeId, contents);
            contents = collegeLegalSystemSoftware(collegeId, contents);
            //contents = collegePrinters(collegeId, contents);
            contents = collegeExaminationBranch(collegeId, contents);
            //contents = collegeFeeReimbursement(collegeId, contents);
            contents = collegeGrievance(collegeId, contents);
            contents = collegeAntiRagging(collegeId, contents);
            contents = collegeWomenProtection(collegeId, contents);
            contents = collegeRTIDetails(collegeId, contents);
            contents = collegeSports(collegeId, contents);
            contents = collegeOtherDesirables(collegeId, contents);
            contents = collegeCampushostel(collegeId, contents);
            contents = collegeOperationalFunds(collegeId, contents);
            contents = collegeIncome(collegeId, contents);
            contents = collegeExpenditure(collegeId, contents);
            contents = collegeStudentpalcements(collegeId, contents);
            ////contents = collegeCommitteeMembers(collegeId, contents);
            contents = collegeEnclosures(collegeId, contents);


            //Read string contents using stream reader and convert html to parsed conent
            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            //Get each array values from parsed elements and add to the PDF document
            bool pageRotated = false;

            foreach (var htmlElement in parsedHtmlElements)
            {
                if (htmlElement.Chunks.Count >= 3)
                {
                    if (htmlElement.Chunks.Count == 4)
                    {
                        pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                        pageRotated = true;
                    }
                    else
                    {
                        if (pageRotated == true)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
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

        private void SendOrder(string scheduleDate, int SendingStatus, string orderPath, List<string> collegeDataPath, List<jntuh_ffc_committee> committee, List<int> schedulesToBeSent)
        {
            TempData["Success"] = null;
            TempData["Error"] = null;

            int IsRevised = 0;
            string orderType = string.Empty;

            if (SendingStatus == 0)
            {
                orderType = "Order";
                IsRevised = 0;
            }
            else
            {
                orderType = "Revised Order";
                IsRevised = 1;
            }

            string path = Server.MapPath("~/PDFReports");
            string strSubject = orderType + ": FFC scheduled on " + scheduleDate;

            List<string> allEmails = new List<string>();
            List<string> allMobiles = new List<string>();
            List<string> allDocuments = new List<string>();

            bool status = false;

            foreach (var strScheduleID in schedulesToBeSent)
            {
                //send emails && sms
                foreach (var member in committee)
                {
                    if (member.scheduleID == strScheduleID)
                    {
                        int IsConvenor = 0;
                        if (member.isConvenor == 1)
                        {
                            IsConvenor = 1;
                        }

                        jntuh_ffc_auditor auditor = db.jntuh_ffc_auditor.Find(member.auditorID);

                        if (!string.IsNullOrWhiteSpace(auditor.auditorEmail1))
                        {
                            string strTo = string.Empty;
                            if (!string.IsNullOrWhiteSpace(auditor.auditorEmail2))
                            {
                                strTo = auditor.auditorEmail1 + "," + auditor.auditorEmail2;
                            }
                            else
                            {
                                strTo = auditor.auditorEmail1;
                            }

                            string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
                            string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();

                            //strTo = "ramesh.bandi@csstechnergy.com" + "," + "bandiramesh.1980@gmail.com";
                            //string strCc = string.Empty; //"satish.p@csstechnergy.com";
                            //string strBcc = string.Empty; //"vijaya.lakshmi@csstechnergy.com";

                            allEmails.Add(auditor.auditorEmail1);
                            if (!string.IsNullOrWhiteSpace(auditor.auditorEmail2))
                                allEmails.Add(auditor.auditorEmail2);

                            List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();

                            orderPath = orderPath.Replace("\\", "/");
                            if (IsConvenor == 1)
                            {
                                // attach order copy
                                attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(orderPath)));
                                allDocuments.Add(orderPath);

                                // RAMESH : we are not sending college data & even empty A-414 form for the inspection year : 2014-15
                                // RAMESH : we are sending college data in the format A-414 for re-inspection A.Y: 2014-15

                                //foreach (var dataPath in collegeDataPath)
                                //{
                                //    attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(dataPath.Replace("\\", "/"))));
                                //    allDocuments.Add(dataPath.Replace("\\", "/"));
                                //}

                                //attach bill form
                                attachments.Add(new System.Net.Mail.Attachment(Server.MapPath("~/Content/PDFAttachments/FFCBillForm.docx")));
                                allDocuments.Add("~/Content/PDFAttachments/FFCBillForm.docx");
                            }
                            else
                            {
                                attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(orderPath)));
                            }

                            //Comment by Siva
                            //send emails to committee members
                            IUserMailer mailer = new UserMailer();
                           // mailer.SendOrder(strTo, strCc, strBcc, strSubject, attachments).SendAsync();

                            if (!string.IsNullOrWhiteSpace(auditor.auditorMobile1))
                            {
                                string smsOrderType = "Order ";
                                if (orderType.Equals("Revised Order"))
                                {
                                    smsOrderType = "Revised Order ";
                                }

                                string auditorsMobile = string.Empty;

                                //add Giri mobile number also

                                if (!string.IsNullOrWhiteSpace(auditor.auditorMobile2))
                                { auditorsMobile = auditor.auditorMobile1 + "," + auditor.auditorMobile2; }
                                else
                                { auditorsMobile = auditor.auditorMobile1; }

                                auditorsMobile = auditorsMobile + ",8143528998";

                                //auditorsMobile = "9493666388,7675042301";

                                allMobiles.Add(auditor.auditorMobile1);

                                //Comment by Siva
                                //send sms to committee members
                               // string message = "JNTUH: FFC visit on " + scheduleDate + ". " + smsOrderType + " copy sent to your email. - From Director, UAAC.";
                               // status = UAAAS.Models.Utilities.SendSms(auditorsMobile, message);
                            }
                        }
                    }
                }

                int collegeId = (int)db.jntuh_ffc_schedule.Find(strScheduleID).collegeID;
                string pTo = db.jntuh_college_principal_director.Where(a => a.collegeId == collegeId && a.type == "PRINCIPAL").Select(a => a.email).FirstOrDefault();
                string pCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
                string pBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();
                string pMobile = db.jntuh_address.Where(a => a.collegeId == collegeId && a.addressTye == "PRINCIPAL").Select(a => a.mobile).FirstOrDefault();

                //string pTo = "ramesh.bandi@csstechnergy.com" + "," + "bandiramesh.1980@gmail.com";
                //string pCc = string.Empty;
                //string pBcc = string.Empty;
                //string pMobile = "9493666388,7675042301";

                //Comment by Siva
                //send email to principal
                IUserMailer mailer1 = new UserMailer();
               // mailer1.SendOrderToPrincipal(pTo, pCc, pBcc, strSubject, scheduleDate).SendAsync();

                //Comment by Siva
                //send sms to principal
               // string pMessage = "JNTUH: Your college Re-Inspection will be held on " + scheduleDate + ". - From Director, UAAC.";
               // bool pStatus = UAAAS.Models.Utilities.SendSms(pMobile, pMessage);
                bool pStatus = false;
                 
                
                //insert order for every schedule id in the group
                jntuh_ffc_order newOrder = new jntuh_ffc_order();
                newOrder.scheduleID = strScheduleID;
                newOrder.emails = string.Join(",", allEmails.Select(i => i).ToArray());
                newOrder.mobiles = string.Join(",", allMobiles.Select(i => i).ToArray());
                newOrder.document = string.Join(",", allDocuments.Select(i => i).ToArray());
                newOrder.isRevisedOrder = SendingStatus;
                newOrder.orderDate = db.jntuh_ffc_schedule.Find(strScheduleID).orderDate;
                newOrder.inspectionDate = db.jntuh_ffc_schedule.Find(strScheduleID).inspectionDate;
                newOrder.isRevisedOrder = db.jntuh_ffc_schedule.Find(strScheduleID).isRevisedOrder;

                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                newOrder.createdBy = userID;
                newOrder.createdOn = DateTime.Now;
                db.jntuh_ffc_order.Add(newOrder);
                db.SaveChanges();

                if (status == true && pStatus == true)
                {
                    TempData["Success"] = "Order Sent successfully.";
                }
                else
                {
                    TempData["Error"] = "Order Sent successfully but SMS not sent.";
                }
            }
        }

        private bool AreEqual(List<Committee> x, List<Committee> y)
        {
            // same list or both are null
            if (x == y)
            {
                return true;
            }

            // one is null (but not the other)
            if (x == null || y == null)
            {
                return false;
            }

            // count differs; they are not equal
            if (x.Count != y.Count)
            {
                return false;
            }

            int sameItemCount = 0;
            foreach (var firstItem in x)
            {
                foreach (var secondItem in y)
                {
                    if (firstItem.auditorId == secondItem.auditorId && firstItem.isConvenor == secondItem.isConvenor)
                    {
                        sameItemCount++;
                    }
                }
            }

            if (sameItemCount != x.Count())
            {
                return false;
            }

            return true;
        }

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
            table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='8' style='font-size: 8px;'><b>Courses Offered</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td><td align='center' colspan='3' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td></tr>";

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

                            //if (flag == 0)
                            table += "<tr><td valign='top' style='font-size: " + fontSize + "; line-height: " + lineHeight + ";'>" + makeDegreeBold1 + degreeName + spec.specialization + makeDegreeBold2 + (spec.shift.Equals("1") ? "" : " - (Shift 2)") + "</td><td width='30%' align='right' valign='top' style='font-size: " + sFontSize + "; line-height: " + sLineHeight + ";'>" + intake + "</td></tr>";
                        }
                    }
                    table += "</table>";
                    table += "</td>";

                    //Name(s) of the FFC Member(s) start 31/07/2014
                    var auditorsList = (from c in db.jntuh_college
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
                                        }).ToList();


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
                    table += "<td valign='top' align='center' colspan='3' style='font-size: 8px;' rowspan='1'>&nbsp;</td></tr>";
                    table += "</tr>";
                }
                count++;
            }
            table += "</table>";

            string pdfPath = SaveSpecializations(table);
            string path = pdfPath.Replace("/", "\\");

            return File(path, "application/pdf", district + " - Colleges & Specializations");

            //return RedirectToAction("Index");
        }

        public ActionResult CommitteeList(int InspectionPhaseId)
        {
            string table = string.Empty;
            string district = string.Empty;

            //int[] collegeIds = db.jntuh_college.Where(c => c.id == 207).Select(c => c.id).ToArray();
            int[] collegeIds = (from s in db.jntuh_ffc_schedule
                                join c in db.jntuh_college on s.collegeID equals c.id
                                //join s in db.jntuh_ffc_schedule on c.id equals s.collegeID
                                //join a in db.jntuh_address on c.id equals a.collegeId
                                //where a.addressTye == "COLLEGE" && c.isActive == true && a.districtId == districtid
                                where s.InspectionPhaseId == InspectionPhaseId
                                //orderby c.collegeName
                                select c.id).Distinct().ToArray();
            //.Skip(120) .Take(60)

            table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            //table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='8' style='font-size: 8px;'><b>Courses Offered</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td><td align='center' colspan='3' width='15%' style='font-size: 8px;'><b>Inspection Date</b></td></tr>";
            table += "<tr><td align='center' colspan='1' style='font-size: 8px;'><b>S.No</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name of the College</b></td><td align='center' colspan='5' style='font-size: 8px;'><b>Name(s) of the FFC Member(s)</b></td></tr>";

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

                    table += "<tr><td align='center' colspan='1' style='font-size: 8px; vertical-align: top;'><b>" + count + "</b></td><td valign='top' colspan='5' style='font-size: 8px; line-height: 11px;'>" + scheduleCollegeName + ", " + scheduleCollegeAddress + "<br /><br /><b>Code : " + scheduleCollegeCode + "</b><br /><br />Prl : " + scheduleCollegePrincipalName.ToUpper() + "<br />Off : " + scheduleCollegePrincipalPhone + "<br />Cell : " + scheduleCollegePrincipalMobile + "</td>";

                    var auditorsList = (from c in db.jntuh_college
                                        join a in db.jntuh_address on c.id equals a.collegeId
                                        //join d in db.jntuh_district on a.districtId equals d.id
                                        join s in db.jntuh_ffc_schedule on c.id equals s.collegeID
                                        //join ord in db.jntuh_ffc_order on s.id equals ord.scheduleID
                                        join com in db.jntuh_ffc_committee on s.id equals com.scheduleID
                                        join aud in db.jntuh_ffc_auditor on com.auditorID equals aud.id
                                        join dis in db.jntuh_designation on aud.auditorDesignationID equals dis.id
                                        where (s.InspectionPhaseId == InspectionPhaseId && c.isActive == true && a.addressTye == "College" && aud.isActive == true && dis.isActive == true && c.id == scheduleCollegeId)
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

                    //table += "<td valign='top' align='center' colspan='3' style='font-size: 8px;' rowspan='1'>" + "" + "</td></tr>";
                    table += "</tr>";
                }
                count++;
            }
            table += "</table>";

            string pdfPath = SaveSpecializations(table);
            string path = pdfPath.Replace("/", "\\");

            return File(path, "application/pdf", "Colleges_Committees_" + InspectionPhaseId);

            //return RedirectToAction("Index");
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

        private string affiliationType(int collegeId, string contents)
        {
            string strCollegeAffiliationType = string.Empty;
            List<jntuh_college_affiliation_type> affiliationType = db.jntuh_college_affiliation_type.OrderBy(affiliation => affiliation.id)
                                                                     .Where(affiliation => affiliation.isActive == true)
                                                                     .OrderBy(affiliation => affiliation.DisplayOrder)
                                                                     .ToList();
            foreach (var item in affiliationType)
            {
                string YesOrNo = "no_b";
                int selectedId = db.jntuh_college.Where(college => college.id == collegeId)
                                                 .Select(college => college.collegeAffiliationTypeID)
                                                 .FirstOrDefault();
                if (item.id == selectedId)
                    YesOrNo = "yes_b";

                strCollegeAffiliationType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1}<br/>", YesOrNo, item.collegeAffiliationType);
            }
            contents = contents.Replace("##COLLEGE_AFFILIATIONTYPE##", strCollegeAffiliationType);

            return contents;
        }

        private string collegeAuditscheduleDate(int collegeId, string contents)
        {
            DateTime? InspectionDate = db.jntuh_ffc_schedule.Where(schedule => schedule.collegeID == collegeId).Select(schedule => schedule.inspectionDate).FirstOrDefault();
            DateTime? alternateInspectionDate = db.jntuh_ffc_schedule.Where(schedule => schedule.collegeID == collegeId).Select(schedule => schedule.alternateInspectionDate).FirstOrDefault();
            string dateOfInspection = string.Empty;
            if (InspectionDate != null && alternateInspectionDate != null)
            {
                dateOfInspection = UAAAS.Models.Utilities.MMDDYY2DDMMYY(InspectionDate.ToString()) + " & " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(alternateInspectionDate.ToString());
            }
            else
            {
                if (InspectionDate != null)
                {
                    dateOfInspection = UAAAS.Models.Utilities.MMDDYY2DDMMYY(InspectionDate.ToString());
                }
            }
            contents = contents.Replace("##AUDITSCHEDULEDATEDETAILS##", dateOfInspection);
            int presentAcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                                                        .Select(a => a.actualYear)
                                                        .FirstOrDefault();
            string academicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (presentAcademicYear + 1))
                                                        .Select(a => a.academicYear)
                                                        .FirstOrDefault();
            if (academicYear != null)
            {
                contents = contents.Replace("##GRANTACADEMICYEAR##", academicYear);
            }
            else
            {
                contents = contents.Replace("##GRANTACADEMICYEAR##", string.Empty);
            }

            return contents;
        }

        private string collegeInformation(int collegeId, string contents)
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

            string strCollegeType = string.Empty;
            List<jntuh_college_type> collegeType = db.jntuh_college_type.Where(s => s.isActive == true).ToList();
            foreach (var item in collegeType)
            {
                string YesOrNo = "no_b";
                int existCollegeTypeId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeTypeID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeTypeId)
                    YesOrNo = "yes_b";

                strCollegeType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1}&nbsp; ", YesOrNo, item.collegeType);
            }
            contents = contents.Replace("##COLLEGE_TYPE##", strCollegeType);

            string strCollegeStatus = string.Empty;
            List<jntuh_college_status> jntuh_college_status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
            foreach (var item in jntuh_college_status)
            {
                string YesOrNo = "no_b";
                int existCollegeStatusId = db.jntuh_college.Where(college => college.isActive == true && college.id == collegeId)
                                                         .Select(college => college.collegeStatusID)
                                                         .FirstOrDefault();
                if (item.id == existCollegeStatusId)
                    YesOrNo = "yes_b";

                strCollegeStatus += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp; {1}&nbsp; ", YesOrNo, item.collegeStatus);
            }
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
            string yes = "no_b";
            string no = "no_b";
            List<jntuh_affiliation_type> affiliationType = db.jntuh_affiliation_type.OrderBy(a => a.id).Where(a => a.isActive == true).ToList();
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NAAC")
                {
                    affiliationNAAC = item.affiliationType.Trim();
                    affiliationNAACId = item.id;
                }
                else
                {
                    if (item.affiliationType.Trim() == "NBA Status")
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "</br>";
                        NACId = item.id;
                    }
                    else
                    {
                        collegeAffiliationType += "<tr>";
                        collegeAffiliationType += "<td valign='top' colspan='4'>" + item.affiliationType + "</td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>";
                        collegeAffiliationType += "##AFFILIATIONTYPEIMAGE" + item.id + "##";
                        collegeAffiliationType += "<td valign='top' colspan='4'>If Yes, Period </td>";
                        collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                        collegeAffiliationType += "<td valign='top' colspan='5'>From:##AFFILIATIONTYPEFROMDATE" + item.id + "## <br/>";
                        collegeAffiliationType += "To:##AFFILIATIONTYPETODATE" + item.id + "## <br/>";
                        collegeAffiliationType += "Duration:##AFFILIATIONTYPEDURATION" + item.id + "##</td>";
                        collegeAffiliationType += "</tr>";
                        collegeAffiliationType += "</br>";
                    }
                }
            }

            List<jntuh_college_affiliation> affiliationTypeDetails = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId).ToList();
            foreach (var affiliation in affiliationTypeDetails)
            {
                if (affiliationNAACId == affiliation.affiliationTypeId)
                {
                    affiliationNAACYes = "yes_b";
                    affiliationNAACNo = "no_b";
                    if (affiliation.affiliationGrade != null)
                    {
                        affiliationNAACGrade = affiliation.affiliationGrade;
                    }
                    if (affiliation.CGPA != null)
                    {
                        affiliationNAACCGPA = affiliation.CGPA;
                    }
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        string fromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string toDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        affiliationNAACFromDate = fromDate;
                        affiliationNAACToDate = ((Convert.ToInt32(toDate.Substring(toDate.Length - 4))) - (Convert.ToInt32(fromDate.Substring(fromDate.Length - 4)))).ToString();
                    }
                }
                if (affiliation.affiliationTypeId == NACId)
                {
                    string image = string.Empty;
                    if (affiliation.affiliationFromDate != null)
                    {
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        if (affiliation.affiliationToDate != null)
                        {
                            string duration = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", ((Convert.ToInt32(duration.Substring(duration.Length - 4))) - (Convert.ToInt32(fDate.Substring(fDate.Length - 4)))).ToString());
                        }
                        else
                        {
                            collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", affiliation.affiliationDuration.ToString());
                        }
                        image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                    else if (affiliation.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", image);
                    }
                }
                else
                {
                    if (affiliation.affiliationFromDate != null && affiliation.affiliationToDate != null)
                    {
                        yes = "yes_b";
                        no = "no_b";
                        string fDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationFromDate.ToString());
                        string tDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(affiliation.affiliationToDate.ToString());
                        string duration = affiliation.affiliationDuration.ToString();
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + affiliation.affiliationTypeId + "##", fDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + affiliation.affiliationTypeId + "##", tDate);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + affiliation.affiliationTypeId + "##", duration);
                        collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + affiliation.affiliationTypeId + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                    }
                }


            }
            foreach (var item in affiliationType)
            {
                if (item.affiliationType.Trim() == "NBA Status")
                {
                    string image = string.Empty;
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", image);
                }
                else
                {
                    yes = "no_b";
                    no = "yes_b";
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEFROMDATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPETODATE" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEDURATION" + item.id + "##", string.Empty);
                    collegeAffiliationType = collegeAffiliationType.Replace("##AFFILIATIONTYPEIMAGE" + item.id + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_{2}.png' height='10' />&nbsp;{3}", yes, "Yes", no, "No</td>"));
                }
            }
            if (affiliationNAAC == "NAAC")
            {
                string image = string.Empty;
                int nackid = db.jntuh_affiliation_type.Where(at => at.affiliationType == "NAAC").Select(at => at.id).FirstOrDefault();

                var nackatype = db.jntuh_college_affiliation.Where(at => at.affiliationTypeId == nackid && at.collegeId == collegeId).Select(at => at).FirstOrDefault();
                if (nackatype != null)
                {
                    if (nackatype.affiliationFromDate != null && nackatype.affiliationToDate != null)
                    {
                        image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else if (nackatype.affiliationStatus == "Applied")
                    {
                        image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                    else
                    {
                        image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");
                    }
                }
                else
                {
                    image = string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Conferred&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Applied&nbsp;&nbsp;<br/><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Not Yet Applied</td>");

                }
                collegeAffiliationType += "<tr>";
                if (nackatype != null)
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationFromDate.ToString()).ToString() + "<br/>";
                    collegeAffiliationType += "To:&nbsp; " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(nackatype.affiliationToDate.ToString()) + "<br/>";
                    collegeAffiliationType += "Duration: " + nackatype.affiliationDuration + "<br/>";
                    collegeAffiliationType += "Grade: " + nackatype.affiliationGrade + "<br/>";
                    collegeAffiliationType += "CGPA: " + nackatype.CGPA + "</td>";
                }
                else
                {
                    collegeAffiliationType += "<td valign='top' colspan='4'>NAAC</td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>";
                    collegeAffiliationType += image;
                    collegeAffiliationType += "<td valign='top' colspan='4'>If Conferred,Period </td>";
                    collegeAffiliationType += "<td valign='top' colspan='1' align='center'>:</td>";
                    collegeAffiliationType += "<td valign='top' colspan='5'>From: <br/>";
                    collegeAffiliationType += "To:&nbsp; <br/>";
                    collegeAffiliationType += "Duration: <br/>";
                    collegeAffiliationType += "Grade:<br/>";
                    collegeAffiliationType += "CGPA: </td>";
                }

                collegeAffiliationType += "</tr>";
                collegeAffiliationType += "</br>";
            }
            contents = contents.Replace("##COLLEGE_AFFILIATIONTYPES##", collegeAffiliationType);

            #endregion

            #region from jntuh_college_degree table

            string strCollegeDegree = string.Empty;
            strCollegeDegree += "<table border='0' cellspacing='0' cellpadding='0' width='100%'><tbody><tr>";
            List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(degree => degree.isActive == true).ToList();
            int count = 0;
            foreach (var item in collegeDegree)
            {
                strCollegeDegree += "<td width='10%'>" + string.Format("{0}&nbsp; {1}", "##COLLEGEDEGREEIMAGE" + item.id + "##", item.degree) + "</td>";
                count++;
                if (count % 5 == 0)
                {
                    strCollegeDegree += "</tr><tr>";
                }
            }
            List<jntuh_college_degree> collegeDegrees = db.jntuh_college_degree.Where(degree => degree.isActive == true && degree.collegeId == collegeId).ToList();
            foreach (var degrees in collegeDegrees)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + degrees.degreeId + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
            }
            foreach (var item in collegeDegree)
            {
                strCollegeDegree = strCollegeDegree.Replace("##COLLEGEDEGREEIMAGE" + item.id + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
            }
            strCollegeDegree += "</tr></tbody></table>";
            contents = contents.Replace("##COLLEGE_DEGREE##", strCollegeDegree);
            #endregion
            return contents;
        }

        private string societyInformation(int collegeId, string contents)
        {
            #region from jntuh_college_establishment table


            jntuh_college_establishment societyInformation = db.jntuh_college_establishment.Where(society => society.collegeId == collegeId).FirstOrDefault();
            if (societyInformation != null)
            {
                contents = contents.Replace("##SocietyYear_of_Establishment##", societyInformation.societyEstablishmentYear.ToString());
                contents = contents.Replace("##SocietyRegistered_Number##", societyInformation.societyRegisterNumber.ToString());
                contents = contents.Replace("##Society_Name##", societyInformation.societyName);
                contents = contents.Replace("##SocietyYearOfEstablishment##", societyInformation.instituteEstablishedYear.ToString());
                if (societyInformation.firstApprovalDateByAICTE != null)
                {
                    contents = contents.Replace("##SocietyFirstApproval##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(societyInformation.firstApprovalDateByAICTE.ToString()));
                }
                else
                {
                    contents = contents.Replace("##SocietyFirstApproval##", DDMMYYYYspace);
                }
                if (societyInformation.firstAffiliationDateByJNTU != null)
                {
                    contents = contents.Replace("##SocietyFirstAffiliation##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(societyInformation.firstAffiliationDateByJNTU.ToString()));
                }
                else
                {
                    contents = contents.Replace("##SocietyFirstAffiliation##", DDMMYYYYspace);
                }
                contents = contents.Replace("##SocietyCommencement##", societyInformation.firstBatchCommencementYear.ToString());
            }
            else
            {
                contents = contents.Replace("##SocietyYear_of_Establishment##", YYYYspace);
                contents = contents.Replace("##SocietyRegistered_Number##", string.Empty);
                contents = contents.Replace("##Society_Name##", string.Empty);
                contents = contents.Replace("##SocietyYearOfEstablishment##", string.Empty);
                contents = contents.Replace("##SocietyFirstApproval##", DDMMYYYYspace);
                contents = contents.Replace("##SocietyFirstAffiliation##", DDMMYYYYspace);
                contents = contents.Replace("##SocietyCommencement##", string.Empty);
            }

            #endregion

            #region from jntuh_address table

            jntuh_address societyAddress = db.jntuh_address.Where(address => address.addressTye == "SOCIETY" && address.collegeId == collegeId).FirstOrDefault();
            if (societyAddress != null)
            {
                contents = contents.Replace("##SocietyAddress##", societyAddress.address);
                contents = contents.Replace("##SocietyCity/Town##", societyAddress.townOrCity);
                contents = contents.Replace("##SocietyMandal##", societyAddress.mandal);
                string societyState = db.jntuh_state.Where(state => state.id == societyAddress.stateId && state.isActive == true).Select(state => state.stateName).FirstOrDefault();
                contents = contents.Replace("##SocietyState##", societyState);
                string districtName = db.jntuh_district.Where(district => district.id == societyAddress.districtId && district.isActive == true).Select(district => district.districtName).FirstOrDefault();
                contents = contents.Replace("##SocietyDistrict##", districtName);
                contents = contents.Replace("##SocietyPinCode##", societyAddress.pincode.ToString());
                contents = contents.Replace("##SocietyFax##", societyAddress.fax);
                contents = contents.Replace("##SocietyLandline##", societyAddress.landline);
                contents = contents.Replace("##SocietyMobile##", societyAddress.mobile);
                contents = contents.Replace("##SocietyEmail##", societyAddress.email);
                contents = contents.Replace("##SocietyWebsite##", societyAddress.website);
            }
            else
            {
                contents = contents.Replace("##SocietyAddress##", string.Empty);
                contents = contents.Replace("##SocietyCity/Town##", string.Empty);
                contents = contents.Replace("##SocietyMandal##", string.Empty);
                contents = contents.Replace("##SocietyState##", string.Empty);
                contents = contents.Replace("##SocietyDistrict##", string.Empty);
                contents = contents.Replace("##SocietyPinCode##", string.Empty);
                contents = contents.Replace("##SocietyFax##", string.Empty);
                contents = contents.Replace("##SocietyLandline##", string.Empty);
                contents = contents.Replace("##SocietyMobile##", string.Empty);
                contents = contents.Replace("##SocietyEmail##", string.Empty);
                contents = contents.Replace("##SocietyWebsite##", string.Empty);
            }
            #endregion
            return contents;
        }

        private string principalDirectorDetailsOld(int collegeId, string contents)
        {
            List<string> programs = new List<string>(); // { "Engineering", "Pharmacy", "Management", "MCA" };
            List<PrincipalDirector> lstPrincipals = new List<PrincipalDirector>();

            var collegeDegrees = db.jntuh_college_degree
                                   .Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { cd, d })
                                   .Where(a => a.cd.isActive == true && a.cd.collegeId == collegeId)
                                   .OrderBy(a => a.d.degreeDisplayOrder)
                                   .Select(a => a.d.degree).Distinct().ToList();

            foreach (var degree in collegeDegrees)
            {
                if (degree.ToUpper().Equals("B.TECH") || degree.ToUpper().Equals("M.TECH"))
                {
                    if (!programs.Contains("Engineering"))
                        programs.Add("Engineering");
                }
                else if (degree.ToUpper().Equals("B.PHARMACY") || degree.ToUpper().Equals("M.PHARMACY") || degree.ToUpper().Equals("PHARM.D") || degree.ToUpper().Equals("PHARM.D PB"))
                {
                    if (!programs.Contains("Pharmacy"))
                        programs.Add("Pharmacy");
                }
                else if (degree.ToUpper().Equals("MBA") || degree.ToUpper().Equals("MAM") || degree.ToUpper().Equals("MTM"))
                {
                    if (!programs.Contains("Management"))
                        programs.Add("Management");
                }
                else if (degree.ToUpper().Equals("MCA"))
                {
                    if (!programs.Contains("MCA"))
                        programs.Add("MCA");
                }
            }

            foreach (string item in programs)
            {
                PrincipalDirector principal = new PrincipalDirector();

                int principalID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeId && e.type.Equals("PRINCIPAL") && e.programType == item).Select(e => e.id).FirstOrDefault();

                jntuh_college_principal_director _principal = db.jntuh_college_principal_director.Find(principalID);

                if (_principal != null)
                {
                    principal.id = _principal.id;
                    principal.firstName = _principal.firstName;
                    principal.lastName = _principal.lastName;
                    principal.surname = _principal.surname;
                    principal.qualificationId = _principal.qualificationId;
                    principal.phdId = _principal.phdId;
                    principal.phdFromUniversity = _principal.phdFromUniversity;
                    principal.phdYear = Convert.ToInt32(_principal.phdYear);
                    principal.departmentId = _principal.departmentId;
                    principal.dateOfAppointment = _principal.dateOfAppointment.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(_principal.dateOfAppointment.ToString()).ToString();
                    principal.isRatified = _principal.isRatified;
                    principal.ratificationPeriodFrom = _principal.ratificationPeriodFrom.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(_principal.ratificationPeriodFrom.ToString()).ToString();
                    principal.ratificationPeriodTo = _principal.ratificationPeriodTo.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(_principal.ratificationPeriodTo.ToString()).ToString();
                    principal.dateOfBirth = _principal.dateOfBirth.ToString() == null ? string.Empty : UAAAS.Models.Utilities.MMDDYY2DDMMYY(_principal.dateOfBirth.ToString()).ToString();
                    principal.fax = _principal.fax;
                    principal.landline = _principal.landline;
                    principal.mobile = _principal.mobile;
                    principal.email = _principal.email;
                    // principal.phdSubject = db.jntuh_phd_subject.Where(p => p.id == principal.phdId).Select(p => p.phdSubjectName).FirstOrDefault();
                    // principal.departmentName = db.jntuh_department.Where(d => d.id == principal.departmentId).Select(d => d.departmentName).FirstOrDefault();
                }

                //principal.programType = item;
                lstPrincipals.Add(principal);
            }

            string strPrincipal = string.Empty;
            foreach (var item in lstPrincipals)
            {
                strPrincipal += "<p style='font-size: 9px'><strong><u>Details of the Principal</u></strong> : (for <b> item.programType </b> Programs)</p><br />";
                strPrincipal += "<table border='1' cellspacing='0' cellpadding='5' width='100%' style='font-size: 9px'>";
                strPrincipal += "<tbody>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>First Name</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.firstName + "</td>";
                strPrincipal += "<td valign='top' colspan='4'>Last Name</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.lastName + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Surname</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.surname + "</td>";
                strPrincipal += "<td valign='top' colspan='4'>Qualification</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";

                string strQualification = string.Empty;
                List<jntuh_qualification> qualificationDetails = db.jntuh_qualification.ToList();
                foreach (var row in qualificationDetails)
                {
                    string YesOrNo = "no_b";
                    if (row.id == item.qualificationId)
                        YesOrNo = "yes_b";
                    strQualification += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, row.qualification);
                }

                strPrincipal += "<td valign='top' colspan='6'>" + strQualification + "</td>";

                //if (!string.IsNullOrEmpty(item.firstName))
                //{
                //    if (Convert.ToBoolean(item.qualificationId) == true)
                //    {
                //        strPrincipal += "<td valign='top' colspan='6'>" + "Doctorate" + "</td>";
                //    }
                //    else if (Convert.ToBoolean(item.qualificationId) == false)
                //    {
                //        strPrincipal += "<td valign='top' colspan='6'>" + "Non-Doctorate" + "</td>";
                //    }
                //}
                //else
                //{
                //    strPrincipal += "<td valign='top' colspan='6'>&nbsp;</td>";
                //}

                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Faculty (Ph.D in)</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";

                string strphdIn = string.Empty;
                List<jntuh_phd_subject> phdSubjects = db.jntuh_phd_subject.Where(p => p.isActive == true).ToList();
                foreach (var row in phdSubjects)
                {
                    string YesOrNo = "no_b";
                    if (row.id == item.phdId)
                        YesOrNo = "yes_b";
                    strphdIn += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, row.phdSubjectName);
                }

                //strPrincipal += "<td colspan='15' valign='top'>" + item.phdSubject + "</td>";
                strPrincipal += "<td colspan='15' valign='top' style='font-size: 8px;'>" + strphdIn + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'  style='font-size: 8px;'>Ph.D Awarded From</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.phdFromUniversity + "</td>";
                strPrincipal += "<td valign='top' colspan='4'>Year</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.phdYear + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Department </td>";
                strPrincipal += "<td valign='top' colspan='1'>: </td>";
                // strPrincipal += "<td valign='top' colspan='5'>" + item.departmentName + "</td>";
                strPrincipal += "<td valign='top' colspan='5'> item.departmentName </td>";
                strPrincipal += "<td valign='top' colspan='4'>Date of Appointment </td>";
                strPrincipal += "<td valign='top' colspan='1'>: </td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.dateOfAppointment + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Ratified by JNTUH</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                if (item.isRatified != null)
                {
                    if (Convert.ToBoolean(item.isRatified) == true)
                    {
                        //strPrincipal += "<td valign='top' colspan='5'>Yes</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>") + "</td>";
                    }
                    else if (Convert.ToBoolean(item.isRatified) == false)
                    {
                        //strPrincipal += "<td valign='top' colspan='5'>No</td>";
                        strPrincipal += "<td valign='top' colspan='5'>" + string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>") + "</td>";
                    }
                }
                else
                {
                    //strPrincipal += "<td valign='top' colspan='5'>&nbsp;</td>";
                    strPrincipal += "<td valign='top' colspan='5'>" + string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>") + "</td>";
                }

                strPrincipal += "<td valign='top' colspan='4'>Date of Ratification</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";

                if (!string.IsNullOrEmpty(item.ratificationPeriodFrom))
                {
                    strPrincipal += "<td valign='top' colspan='5'>" + UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.ratificationPeriodFrom) + " to " + UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.ratificationPeriodTo) + "</td>";
                }
                else
                {
                    strPrincipal += "<td valign='top' colspan='5'>&nbsp;</td>";
                }

                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Date of Birth</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.dateOfBirth + "</td>";
                strPrincipal += "<td valign='top' colspan='4'>Fax (+91)</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.fax + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Landline (+91)</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.landline + "</td>";
                strPrincipal += "<td valign='top' colspan='4'>Mobile (+91)</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td valign='top' colspan='5'>" + item.mobile + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "<tr>";
                strPrincipal += "<td valign='top' colspan='4'>Email</td>";
                strPrincipal += "<td valign='top' colspan='1'>:</td>";
                strPrincipal += "<td colspan='15' valign='top'>" + item.email + "</td>";
                strPrincipal += "</tr>";
                strPrincipal += "</tbody>";
                strPrincipal += "</table>";
                strPrincipal += "<br />";
            }
            contents = contents.Replace("##PRINCIPAL##", strPrincipal);

            //#region PrincipalDetails

            //jntuh_college_principal_director principalDetails = db.jntuh_college_principal_director.Where(principal => principal.collegeId == collegeId &&
            //                                                                                                          principal.type.Equals("PRINCIPAL"))
            //                                                                                      .FirstOrDefault();
            //if (principalDetails != null)
            //{
            //    contents = contents.Replace("##PrincipalFirstname##", principalDetails.firstName);
            //    contents = contents.Replace("##PrincipalLastname##", principalDetails.lastName);
            //    contents = contents.Replace("##PrincipalSurname##", principalDetails.surname);

            //    string strQualification = string.Empty;
            //    List<jntuh_qualification> qualificationDetails = db.jntuh_qualification.ToList();
            //    foreach (var item in qualificationDetails)
            //    {
            //        string YesOrNo = "no_b";
            //        if (item.id == principalDetails.qualificationId)
            //            YesOrNo = "yes_b";
            //        strQualification += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.qualification);
            //    }
            //    contents = contents.Replace("##PrincipalQualification##", strQualification);
            //    string strphdIn = string.Empty;
            //    List<jntuh_phd_subject> phdSubjects = db.jntuh_phd_subject.Where(p => p.isActive == true).ToList();
            //    foreach (var item in phdSubjects)
            //    {
            //        string YesOrNo = "no_b";
            //        if (item.id == principalDetails.phdId)
            //            YesOrNo = "yes_b";
            //        strphdIn += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.phdSubjectName);
            //    }
            //    contents = contents.Replace("##PrincialPhdIn##", strphdIn);
            //    contents = contents.Replace("##PrincipalPhdAwardedFrom##", principalDetails.phdFromUniversity);
            //    if (principalDetails.phdYear != null)
            //    {
            //        contents = contents.Replace("##PrincipalPhdAwardedYear##", principalDetails.phdYear.ToString());
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##PrincipalPhdAwardedYear##", YYYYspace);
            //    }
            //    contents = contents.Replace("##PrincipalDepartment##", db.jntuh_department.Where(department => department.id == principalDetails.departmentId &&
            //                                                                            department.isActive == true)
            //                                                       .Select(department => department.departmentName).FirstOrDefault());
            //    if (principalDetails.dateOfAppointment != null)
            //    {
            //        contents = contents.Replace("##PrincipalDateOfAppointment##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.dateOfAppointment.ToString()));
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##PrincipalDateOfAppointment##", DDMMYYYYspace);
            //    }
            //    if (principalDetails.dateOfBirth != null)
            //    {
            //        contents = contents.Replace("##PrincipalDateOfBirth##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.dateOfBirth.ToString()));
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##PrincipalDateOfBirth##", DDMMYYYYspace);
            //    }
            //    contents = contents.Replace("##PrincipalFax##", principalDetails.fax);
            //    contents = contents.Replace("##PrincipalLandLinen##", principalDetails.landline);
            //    contents = contents.Replace("##PrincipalMobile##", principalDetails.mobile);
            //    contents = contents.Replace("##PrincipalEmail##", principalDetails.email);

            //    if (principalDetails.isRatified == true)
            //    {
            //        contents = contents.Replace("##RATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##RATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
            //    }
            //    if (principalDetails.ratificationPeriodFrom != null)
            //    {
            //        contents = contents.Replace("##PrincipalDateOfRatificationFrom##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.ratificationPeriodFrom.ToString()));
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##PrincipalDateOfRatificationFrom##", DDMMYYYYspace);
            //    }
            //    if (principalDetails.ratificationPeriodTo != null)
            //    {
            //        contents = contents.Replace("##PrincipalDateOfRatificationTo##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(principalDetails.ratificationPeriodTo.ToString()));
            //    }
            //    else
            //    {
            //        contents = contents.Replace("##PrincipalDateOfRatificationTo##", DDMMYYYYspace);
            //    }
            //}
            //else
            //{
            //    string strQualification = string.Empty;
            //    List<jntuh_qualification> qualificationDetails = db.jntuh_qualification.ToList();
            //    foreach (var item in qualificationDetails)
            //    {
            //        strQualification += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0}&nbsp;", item.qualification);
            //    }
            //    contents = contents.Replace("##PrincipalQualification##", strQualification);
            //    string strphdIn = string.Empty;
            //    List<jntuh_phd_subject> phdSubjects = db.jntuh_phd_subject.Where(p => p.isActive == true).ToList();
            //    foreach (var item in phdSubjects)
            //    {
            //        strphdIn += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0}&nbsp;", item.phdSubjectName);
            //    }
            //    contents = contents.Replace("##PrincialPhdIn##", strphdIn);
            //    contents = contents.Replace("##PrincipalFirstname##", string.Empty);
            //    contents = contents.Replace("##PrincipalLastname##", string.Empty);
            //    contents = contents.Replace("##PrincipalSurname##", string.Empty);
            //    contents = contents.Replace("##PrincipalPhdAwardedFrom##", string.Empty);
            //    contents = contents.Replace("##PrincipalPhdAwardedYear##", YYYYspace);
            //    contents = contents.Replace("##PrincipalDepartment##", string.Empty);
            //    contents = contents.Replace("##PrincipalDateOfAppointment##", DDMMYYYYspace);
            //    contents = contents.Replace("##PrincipalDateOfBirth##", DDMMYYYYspace);
            //    contents = contents.Replace("##PrincipalFax##", string.Empty);
            //    contents = contents.Replace("##PrincipalLandLinen##", string.Empty);
            //    contents = contents.Replace("##PrincipalMobile##", string.Empty);
            //    contents = contents.Replace("##PrincipalEmail##", string.Empty);
            //    contents = contents.Replace("##RATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</td>"));
            //    contents = contents.Replace("##PrincipalDateOfRatificationFrom##", DDMMYYYYspace);
            //    contents = contents.Replace("##PrincipalDateOfRatificationTo##", DDMMYYYYspace);
            //}

            //#endregion

            #region Director Details

            jntuh_college_principal_director directorDetails = db.jntuh_college_principal_director.Where(principal => principal.collegeId == collegeId &&
                                                                                                                      principal.type.Equals("DIRECTOR"))
                                                                                                  .FirstOrDefault();
            if (directorDetails != null)
            {
                contents = contents.Replace("##DirectorFirstname##", directorDetails.firstName);
                contents = contents.Replace("##DirectorLastname##", directorDetails.lastName);
                contents = contents.Replace("##DirectorSurname##", directorDetails.surname);
                string strQualification = string.Empty;
                List<jntuh_qualification> qualificationDetails = db.jntuh_qualification.ToList();
                foreach (var item in qualificationDetails)
                {
                    string YesOrNo = "no_b";

                    if (item.id == directorDetails.qualificationId)
                        YesOrNo = "yes_b";

                    strQualification += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.qualification);
                }
                contents = contents.Replace("##DirectoQualification##", strQualification);
                if (directorDetails.dateOfAppointment != null)
                {
                    contents = contents.Replace("##DirectorDateOfAppoinment##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(directorDetails.dateOfAppointment.ToString()));
                }
                else
                {
                    contents = contents.Replace("##DirectorDateOfAppoinment##", DDMMYYYYspace);
                }
                if (directorDetails.dateOfBirth != null)
                {
                    contents = contents.Replace("##DirectorDateOfBirtht##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(directorDetails.dateOfBirth.ToString()));
                }
                else
                {
                    contents = contents.Replace("##DirectorDateOfBirtht##", DDMMYYYYspace);
                }
                contents = contents.Replace("##DirectorFax##", directorDetails.fax);
                contents = contents.Replace("##DirectorLandline##", directorDetails.landline);
                contents = contents.Replace("##DirectorMobile##", directorDetails.mobile);
                contents = contents.Replace("##DirectorEmail##", directorDetails.email);
                string strphdIn = string.Empty;
                List<jntuh_phd_subject> phdSubjects = db.jntuh_phd_subject.Where(p => p.isActive == true).ToList();
                foreach (var item in phdSubjects)
                {
                    string YesOrNo = "no_b";
                    if (item.id == directorDetails.phdId)
                        YesOrNo = "yes_b";
                    strphdIn += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.phdSubjectName);
                }
                contents = contents.Replace("##DirectorPhdIn##", strphdIn);

                contents = contents.Replace("##DirectorPhdAwardedFrom##", directorDetails.phdFromUniversity);
                if (directorDetails.phdYear != null)
                {
                    contents = contents.Replace("##DirectorPhdAwardedYear##", directorDetails.phdYear.ToString());
                }
                else
                {
                    contents = contents.Replace("##DirectorPhdAwardedYear##", YYYYspace);
                }

                contents = contents.Replace("##DirectorDepartment##", db.jntuh_department.Where(department => department.id == directorDetails.departmentId &&
                                                                                        department.isActive == true)
                                                                   .Select(department => department.departmentName).FirstOrDefault());
                if (directorDetails.ratificationPeriodFrom != null)
                {
                    contents = contents.Replace("##DirectorDateOfRatificationFrom##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(directorDetails.ratificationPeriodFrom.ToString()));
                }
                else
                {
                    contents = contents.Replace("##DirectorDateOfRatificationFrom##", DDMMYYYYspace);
                }
                if (directorDetails.ratificationPeriodTo != null)
                {
                    contents = contents.Replace("##DirectorDateOfRatificationTo##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(directorDetails.ratificationPeriodTo.ToString()));
                }
                else
                {
                    contents = contents.Replace("##DirectorDateOfRatificationTo##", DDMMYYYYspace);
                }
                if (directorDetails.isRatified == true)
                {
                    contents = contents.Replace("##DirectorRATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                }
                else
                {
                    contents = contents.Replace("##DirectorRATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                }
            }
            else
            {
                string strQualification = string.Empty;
                List<jntuh_qualification> qualificationDetails = db.jntuh_qualification.ToList();
                foreach (var item in qualificationDetails)
                {
                    strQualification += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0}&nbsp;", item.qualification);
                }
                contents = contents.Replace("##DirectoQualification##", strQualification);
                string strphdIn = string.Empty;
                List<jntuh_phd_subject> phdSubjects = db.jntuh_phd_subject.Where(p => p.isActive == true).ToList();
                foreach (var item in phdSubjects)
                {
                    strphdIn += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0}&nbsp;", item.phdSubjectName);
                }
                contents = contents.Replace("##DirectorPhdIn##", strphdIn);
                contents = contents.Replace("##DirectorFirstname##", string.Empty);
                contents = contents.Replace("##DirectorLastname##", string.Empty);
                contents = contents.Replace("##DirectorSurname##", string.Empty);
                contents = contents.Replace("##DirectorDateOfAppoinment##", DDMMYYYYspace);
                contents = contents.Replace("##DirectorDateOfBirtht##", DDMMYYYYspace);
                contents = contents.Replace("##DirectorFax##", string.Empty);
                contents = contents.Replace("##DirectorLandline##", string.Empty);
                contents = contents.Replace("##DirectorMobile##", string.Empty);
                contents = contents.Replace("##DirectorEmail##", string.Empty);
                contents = contents.Replace("##DirectorPhdAwardedFrom##", string.Empty);
                contents = contents.Replace("##DirectorPhdAwardedYear##", YYYYspace);
                contents = contents.Replace("##DirectorDepartment##", string.Empty);
                contents = contents.Replace("##DirectorRATIFICATIONIMAGE##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</td>"));
                contents = contents.Replace("##DirectorDateOfRatificationFrom##", DDMMYYYYspace);
                contents = contents.Replace("##DirectorDateOfRatificationTo##", DDMMYYYYspace);
            }

            #endregion

            return contents;
        }

        public string principalDirectorDetails(int collegeId, string contents)
        {
            int directorID = db.jntuh_college_principal_director.Where(e => e.collegeId == collegeId && e.type.Equals("DIRECTOR")).Select(e => e.id).FirstOrDefault();

            jntuh_college_principal_director director = db.jntuh_college_principal_director.Find(directorID);

            string strPrincipal = string.Empty;
            ////Principal Details
            var regNo = db.jntuh_college_principal_registered.Where(r => r.collegeId == collegeId).Select(r => r.RegistrationNumber).FirstOrDefault();
            if (!string.IsNullOrEmpty(regNo))
            {
                var PrincipalDetails = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regNo).Select(r => r).FirstOrDefault();
                var education = db.jntuh_registered_faculty_education.Where(e => e.facultyId == PrincipalDetails.id).OrderByDescending(e => e.id).Select(e => e).FirstOrDefault();
                if (PrincipalDetails != null)
                {
                    strPrincipal += "<p><strong><u>Details of Principal</u></strong></p><br />";
                    strPrincipal += "<table border='1' cellspacing='0' cellpadding='5'>";
                    strPrincipal += "<tbody>";
                    strPrincipal += "<tr>";
                    strPrincipal += "<td valign='top' colspan='4'>Registration Number</td>";
                    strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                    strPrincipal += "<td colspan='15' valign='top'>" + PrincipalDetails.RegistrationNumber + "</td>";
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
                        string strPrincipalPhoto = "<img src='http://localhost:49713/Content/Upload/Faculty/Photos/" + PrincipalDetails.Photo + "'" + " align='center'  width='80' height='50' />";
                        strPrincipal += "<td colspan='15' valign='top' style='height: 50px;'>" + strPrincipalPhoto + "</td>";
                        //strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'></td>";
                    }
                    else
                    {
                        strPrincipal += "<td colspan='5' valign='top' style='height: 50px;'></td>";
                    }
                    strPrincipal += "</tr>";
                    strPrincipal += "</tbody>";
                    strPrincipal += "</table>";
                    strPrincipal += "<br />";

                }

            }
            else
            {
                strPrincipal += "<p><strong><u>Details of Principal:</u></strong></p> (PRINCIPAL DETAILS ARE NOT UPLOADED)<br /><br />";
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
                strPrincipal += "<td colspan='5' valign='top'>Yes</td>";

                strPrincipal += "<td valign='top' colspan='4'>Photo</td>";
                strPrincipal += "<td valign='top' colspan='1' align='center'>:</td>";
                strPrincipal += "<td colspan='5' valign='top'></td>";
                strPrincipal += "</tr>";

                strPrincipal += "</tbody>";
                strPrincipal += "</table>";
                strPrincipal += "<br />";
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
                    strDirectorQualification = "<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Doctorate<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Non-Doctorate";
                }
                else
                {
                    strDirectorQualification = "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Doctorate<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Non-Doctorate";
                }
                //strPhdSubjects
                List<jntuh_phd_subject> jntuh_phd_subject = db.jntuh_phd_subject.Where(p => p.isActive == true).Select(p => p).ToList();
                if (jntuh_phd_subject != null)
                {
                    foreach (var item in jntuh_phd_subject)
                    {
                        string yesORno = "no_b";
                        if (director.phdId != null)
                        {
                            if (director.phdId == item.id)
                            {
                                yesORno = "yes_b";
                            }
                            strDirectorPhdSubjects += "<img src='http://localhost:49713/Content/Images/checkbox_" + yesORno + ".png' height='10' />&nbsp;" + item.phdSubjectName;
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
                jntuh_department department = db.jntuh_department.Where(d => d.id == director.departmentId && d.isActive == true).Select(d => d).FirstOrDefault();
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
                    string strDirectorPhoto = "<img src='http://localhost:49713/Content/Upload/PrincipalDirectorPhotos/" + director.photo + "'" + " align='center'  width='80' height='50' />";
                    contents = contents.Replace("##DirectorPhoto##", strDirectorPhoto);
                    //contents = contents.Replace("##DirectorPhoto##", string.Empty);
                }
                else
                {
                    contents = contents.Replace("##DirectorPhoto##", string.Empty);
                }

            }
            else
            {
                string strDirectorQualification = string.Empty;
                string dateOfAppointment = string.Empty;
                string dateOfBirth = string.Empty;
                contents = contents.Replace("##DirectorTitle##", "<p><strong><u>Details of Director:</u></strong></p> (DIRECTOR DETAILS ARE NOT UPLOADED)<br />");
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

                strDirectorQualification = "<img src='http://localhost:49713/Content/Images/checkbox_no.png' height='10' />&nbsp;Doctorate<img src='http://localhost:49713/Content/Images/checkbox_no.png' height='10' />&nbsp;Non-Doctorate";

                contents = contents.Replace("##DirectorQualification##", strDirectorQualification);

                contents = contents.Replace("##DirectorPhdSubjects##", string.Empty);
                contents = contents.Replace("##DirectorPhDAwardedFrom##", string.Empty);
                contents = contents.Replace("##DirectorYear##", string.Empty);
                contents = contents.Replace("##DirectorDepartment##", string.Empty);
                contents = contents.Replace("##DirectorPhoto##", string.Empty);
            }


            return contents;
        }

        private string chairPersonDetails(int collegeId, string contents)
        {
            #region from jntuh_college_chairperson table

            jntuh_college_chairperson chairpersonDetails = db.jntuh_college_chairperson.Where(chairPerson => chairPerson.collegeId == collegeId).FirstOrDefault();
            if (chairpersonDetails != null)
            {
                contents = contents.Replace("##ChairPersonFirstname##", chairpersonDetails.firstName);
                contents = contents.Replace("##ChairPersonLastname##", chairpersonDetails.lastName);
                contents = contents.Replace("##ChairPersonSurname##", chairpersonDetails.surname);
                contents = contents.Replace("##ChairPersonDesignation##", db.jntuh_chairperson_designation.Where(designation => designation.id == chairpersonDetails.designationId).Select(designation => designation.designationName).FirstOrDefault());
            }
            else
            {
                contents = contents.Replace("##ChairPersonFirstname##", string.Empty);
                contents = contents.Replace("##ChairPersonLastname##", string.Empty);
                contents = contents.Replace("##ChairPersonSurname##", string.Empty);
                contents = contents.Replace("##ChairPersonDesignation##", string.Empty);
            }

            #endregion

            #region from jntuh_college_chairperson table

            jntuh_address addressDetails = db.jntuh_address.Where(address => address.collegeId == collegeId && address.addressTye == "SECRETARY").FirstOrDefault();
            if (addressDetails != null)
            {
                contents = contents.Replace("##ChairAddress##", addressDetails.address);
                contents = contents.Replace("##ChairPersonCityOrTown##", addressDetails.townOrCity);
                contents = contents.Replace("##ChairPersonMandal##", addressDetails.mandal);
                contents = contents.Replace("##ChairPersonDistrict##", db.jntuh_district.Where(district => district.id == addressDetails.districtId).Select(district => district.districtName).FirstOrDefault());
                contents = contents.Replace("##ChairPersonState##", db.jntuh_state.Where(state => state.id == addressDetails.stateId).Select(state => state.stateName).FirstOrDefault());
                contents = contents.Replace("##ChairPersonPinCode##", addressDetails.pincode.ToString());
                contents = contents.Replace("##ChairPersonFax##", addressDetails.fax);
                contents = contents.Replace("##ChairPersonLandline##", addressDetails.landline);
                contents = contents.Replace("##ChairPersonMobile##", addressDetails.mobile);
                contents = contents.Replace("##ChairPersonEmail##", addressDetails.email);
            }
            else
            {
                contents = contents.Replace("##ChairAddress##", string.Empty);
                contents = contents.Replace("##ChairPersonCityOrTown##", string.Empty);
                contents = contents.Replace("##ChairPersonMandal##", string.Empty);
                contents = contents.Replace("##ChairPersonDistrict##", string.Empty);
                contents = contents.Replace("##ChairPersonState##", string.Empty);
                contents = contents.Replace("##ChairPersonPinCode##", string.Empty);
                contents = contents.Replace("##ChairPersonFax##", string.Empty);
                contents = contents.Replace("##ChairPersonLandline##", string.Empty);
                contents = contents.Replace("##ChairPersonMobile##", string.Empty);
                contents = contents.Replace("##ChairPersonEmail##", string.Empty);
            }

            #endregion

            return contents;
        }

        private string otherCollegesAndOtherCourses(int collegeId, string contents)
        {
            #region from jntuh_society_other_colleges table

            List<jntuh_society_other_colleges> otherColleges = db.jntuh_society_other_colleges.Where(otherCollege => otherCollege.collegeId == collegeId).ToList();

            string societyOtherCollege = string.Empty;
            if (otherColleges.Count > 5)
            {
                int count = 1;
                string universityName = string.Empty;
                contents = contents.Replace("##OtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                foreach (var item in otherColleges)
                {
                    societyOtherCollege += "<td colspan='1' valign='top'>" + count + "</td>";
                    societyOtherCollege += "<td colspan='5' valign='top' height='80px'>" + item.collegeName + "</td>";

                    universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                    if (universityName.Trim() == "Other")
                    {
                        universityName = db.jntuh_society_other_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.otherUniversityName).FirstOrDefault();
                    }
                    societyOtherCollege += "<td colspan='3' valign='top'>&nbsp;</td>";
                    societyOtherCollege += "<td colspan='3' valign='top'>" + universityName + "</td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    societyOtherCollege += "<td colspan='1' valign='top'>" + i + "</td>";
                    societyOtherCollege += "<td colspan='5' valign='top' height='80px'>##OTHERCOLLEGESCOLLEGENAME" + i + "##</td>";
                    societyOtherCollege += "<td colspan='3' valign='top'>&nbsp;</td>";
                    societyOtherCollege += "<td colspan='3' valign='top'>##OTHERCOLLEGESUNIVERSITYNAME" + i + "##</td>";
                }

                if (otherColleges.Count == 0)
                {
                    contents = contents.Replace("##OtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                }
                else
                {
                    int count = 1;
                    string universityName = string.Empty;
                    contents = contents.Replace("##OtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                    foreach (var item in otherColleges)
                    {
                        societyOtherCollege = societyOtherCollege.Replace("##OTHERCOLLEGESCOLLEGENAME" + count + "##", item.collegeName);
                        universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                        if (universityName.Trim() == "Other")
                        {
                            universityName = db.jntuh_society_other_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.otherUniversityName).FirstOrDefault();
                        }
                        societyOtherCollege = societyOtherCollege.Replace("##OTHERCOLLEGESUNIVERSITYNAME" + count + "##", universityName);
                        count++;
                    }

                }
                for (int i = 1; i <= 5; i++)
                {
                    societyOtherCollege = societyOtherCollege.Replace("##OTHERCOLLEGESCOLLEGENAME" + i + "##", "<br /> " + string.Empty);
                    societyOtherCollege = societyOtherCollege.Replace("##OTHERCOLLEGESUNIVERSITYNAME" + i + "##", "<br /> " + string.Empty);
                }
            }
            contents = contents.Replace("##SOCIETYOTHERCOLLEGESDETAILS##", societyOtherCollege);
            #endregion

            #region from jntuh_college_other_university_courses table

            string societyOtherCourse = string.Empty;
            List<jntuh_college_other_university_courses> otherCourses = db.jntuh_college_other_university_courses.Where(otherCourse => otherCourse.collegeId == collegeId).ToList();
            if (otherCourses.Count > 5)
            {
                int count = 1;
                string universityName = string.Empty;
                contents = contents.Replace("##OtherCourseIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                foreach (var item in otherCourses)
                {
                    societyOtherCourse += "<td colspan='1' valign='top'>" + count + "</td>";
                    societyOtherCourse += "<td colspan='6' valign='top' height='80px'>" + item.courseName + "</td>";
                    universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                    if (universityName.Trim() == "Other")
                    {
                        universityName = db.jntuh_college_other_university_courses.Where(u => u.id == item.id).Select(u => u.otherUniversityName).FirstOrDefault();
                    }
                    societyOtherCourse += "<td colspan='3' valign='top'>" + universityName + "</td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    societyOtherCourse += "<td colspan='1' valign='top'>" + i + "</td>";
                    societyOtherCourse += "<td colspan='6' valign='top' height='80px'>##OTHERCOURSESCOLLEGENAME" + i + "##</td>";
                    societyOtherCourse += "<td colspan='3' valign='top'>##OTHERCOURSESUNIVERSITYNAME" + i + "##</td>";
                }

                if (otherCourses.Count == 0)
                {
                    contents = contents.Replace("##OtherCourseIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                }
                else
                {
                    int count = 1;
                    string universityName = string.Empty;
                    contents = contents.Replace("##OtherCourseIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                    foreach (var item in otherCourses)
                    {
                        societyOtherCourse = societyOtherCourse.Replace("##OTHERCOURSESCOLLEGENAME" + count + "##", item.courseName);
                        universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                        if (universityName.Trim() == "Other")
                        {
                            universityName = db.jntuh_college_other_university_courses.Where(u => u.id == item.id).Select(u => u.otherUniversityName).FirstOrDefault();
                        }
                        societyOtherCourse = societyOtherCourse.Replace("##OTHERCOURSESUNIVERSITYNAME" + count + "##", universityName);
                        count++;
                    }
                }
                for (int i = 1; i <= 5; i++)
                {
                    societyOtherCourse = societyOtherCourse.Replace("##OTHERCOURSESCOLLEGENAME" + i + "##", "<br /> " + string.Empty);
                    societyOtherCourse = societyOtherCourse.Replace("##OTHERCOURSESUNIVERSITYNAME" + i + "##", "<br /> " + string.Empty);
                }
            }
            contents = contents.Replace("##SOCIETYOTHERCOURSESDETAILS##", societyOtherCourse);

            #endregion

            #region from jntuh_society_other_locations_colleges table

            List<jntuh_society_other_locations_colleges> societyOtherColleges = db.jntuh_society_other_locations_colleges.Where(otherCollege => otherCollege.collegeId == collegeId).ToList();

            string sameSocietyOtherCollege = string.Empty;
            if (societyOtherColleges.Count > 5)
            {
                int count = 1;
                string universityName = string.Empty;
                string location = string.Empty;
                int? year = null;
                contents = contents.Replace("##SocietyOtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                foreach (var item in societyOtherColleges)
                {
                    sameSocietyOtherCollege += "<td colspan='1' valign='top'>" + count + "</td>";
                    sameSocietyOtherCollege += "<td colspan='4' valign='top' height='80px'>" + item.collegeName + "</td>";

                    universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                    if (universityName.Trim() == "Other")
                    {
                        universityName = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.otherUniversityName).FirstOrDefault();
                    }
                    year = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.yearOfEstablishment).FirstOrDefault();
                    sameSocietyOtherCollege += "<td colspan='2' valign='top'>" + year + "</td>";

                    location = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.collegeLocation).FirstOrDefault();
                    sameSocietyOtherCollege += "<td colspan='3' valign='top'>" + location + "</td>";
                    sameSocietyOtherCollege += "<td colspan='2' valign='top'>" + universityName + "</td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    sameSocietyOtherCollege += "<td colspan='1' valign='top'>" + i + "</td>";
                    sameSocietyOtherCollege += "<td colspan='4' valign='top' height='80px'>##SOTHERCOLLEGESCOLLEGENAME" + i + "##</td>";
                    sameSocietyOtherCollege += "<td colspan='2' valign='top'>##SOTHERCOLLEGESCOLLEGEYEAR" + i + "##</td>";
                    sameSocietyOtherCollege += "<td colspan='3' valign='top'>##SOTHERCOLLEGESCOLLEGELOCATION" + i + "##</td>";
                    sameSocietyOtherCollege += "<td colspan='2' valign='top'>##SOTHERCOLLEGESUNIVERSITYNAME" + i + "##</td>";
                }

                if (societyOtherColleges.Count == 0)
                {
                    contents = contents.Replace("##SocietyOtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                }
                else
                {
                    int count = 1;
                    string universityName = string.Empty;
                    string location = string.Empty;
                    int? year = null;
                    contents = contents.Replace("##SocietyOtherCollegeIMAGES##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));
                    foreach (var item in societyOtherColleges)
                    {
                        sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGENAME" + count + "##", item.collegeName);
                        universityName = db.jntuh_university.Where(university => university.id == item.affiliatedUniversityId).Select(university => university.universityName).FirstOrDefault();
                        if (universityName.Trim() == "Other")
                        {
                            universityName = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.otherUniversityName).FirstOrDefault();
                        }
                        year = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.yearOfEstablishment).FirstOrDefault();
                        sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGEYEAR" + count + "##", year.ToString());

                        location = db.jntuh_society_other_locations_colleges.Where(u => u.id == item.id && u.collegeId == collegeId).Select(u => u.collegeLocation).FirstOrDefault();
                        sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGELOCATION" + count + "##", location);
                        sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESUNIVERSITYNAME" + count + "##", universityName);
                        count++;
                    }

                }
                for (int i = 1; i <= 5; i++)
                {
                    sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGENAME" + i + "##", "<br /> " + string.Empty);
                    sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGEYEAR" + i + "##", "<br /> " + string.Empty);
                    sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESCOLLEGELOCATION" + i + "##", "<br /> " + string.Empty);
                    sameSocietyOtherCollege = sameSocietyOtherCollege.Replace("##SOTHERCOLLEGESUNIVERSITYNAME" + i + "##", "<br /> " + string.Empty);
                }
            }
            contents = contents.Replace("##SAMESOCIETYOTHERCOLLEGESDETAILS##", sameSocietyOtherCollege);
            #endregion
            return contents;
        }

        private string landInformation(int collegeId, string contents)
        {
            #region from jntuh_college_land table

            jntuh_college_land landDetails = db.jntuh_college_land.Where(land => land.collegeId == collegeId).FirstOrDefault();
            if (landDetails != null)
            {
                contents = contents.Replace("##LAND_Total_Land_Area##", landDetails.areaInAcres.ToString());

                string landType = string.Empty;
                List<jntuh_land_type> landTypeDetails = db.jntuh_land_type.Where(land => land.isActive == true).ToList();
                if (landTypeDetails.Count != 0)
                {
                    foreach (var item in landTypeDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.landTypeID)
                        {
                            YesOrNo = "yes_b";
                        }
                        landType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landType);
                    }
                }
                contents = contents.Replace("##LAND_Land_Type##", landType);

                string landRegistrationType = string.Empty;
                List<jntuh_land_registration_type> landRegistrationTypeDetails = db.jntuh_land_registration_type.Where(land => land.isActive == true).ToList();
                if (landRegistrationTypeDetails.Count != 0)
                {
                    foreach (var item in landRegistrationTypeDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.landRegistrationTypeId)
                        {
                            YesOrNo = "yes_b";
                        }
                        landRegistrationType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landRegistrationType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Land_Registration_Type##", landRegistrationType);

                string landCategory = string.Empty;
                List<jntuh_land_category> landCategoryDetails = db.jntuh_land_category.Where(land => land.isActive == true).ToList();
                if (landCategoryDetails.Count != 0)
                {
                    foreach (var item in landCategoryDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.landCategoryId)
                        {
                            YesOrNo = "yes_b";
                        }
                        landCategory += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landCategory + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Land_Category##", landCategory);
                contents = contents.Replace("##LAND_Conversion_Issued_by##", landDetails.conversionCertificateIssuedBy);
                if (landDetails.conversionCertificateIssuedDate != null)
                {
                    contents = contents.Replace("##LAND_Conversion_Issued_Date##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(landDetails.conversionCertificateIssuedDate.ToString()));
                }
                else
                {
                    contents = contents.Replace("##LAND_Conversion_Issued_Date##", DDMMYYYYspace);
                }
                contents = contents.Replace("##LAND_Conversion_Issued_Purpose##", landDetails.conversionCertificateIssuedPurpose);
                contents = contents.Replace("##LAND_proposed_Issued_by##", landDetails.buildingPlanIssuedBy);
                if (landDetails.buildingPlanIssuedDate != null)
                {
                    contents = contents.Replace("##LAND_proposed_Issued_Date##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(landDetails.buildingPlanIssuedDate.ToString()));
                }
                else
                {
                    contents = contents.Replace("##LAND_proposed_Issued_Date##", DDMMYYYYspace);
                }
                contents = contents.Replace("##LAND_institution_Issued_by##", landDetails.masterPlanIssuedBy);
                if (landDetails.masterPlanIssuedDate != null)
                {
                    contents = contents.Replace("##LAND_institution_Issued_Date##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(landDetails.masterPlanIssuedDate.ToString()));
                }
                else
                {
                    contents = contents.Replace("##LAND_institution_Issued_Date##", DDMMYYYYspace);
                }
                if (landDetails.compoundWall == true)
                {
                    contents = contents.Replace("##LAND_CompoundWallFencing##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }
                else
                {
                    contents = contents.Replace("##LAND_CompoundWallFencing##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }

                string approachRoad = string.Empty;
                List<jntuh_approach_road> approachRoadDetails = db.jntuh_approach_road.Where(road => road.isActive == true).ToList();
                if (approachRoadDetails.Count != 0)
                {
                    foreach (var item in approachRoadDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.approachRoadId)
                        {
                            YesOrNo = "yes_b";
                        }
                        approachRoad += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.approachRoadType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Approach_Road##", approachRoad);
                string powerSupply = string.Empty;
                List<jntuh_facility_status> facilityStatusDetails = db.jntuh_facility_status.Where(facility => facility.isActive == true).ToList();
                if (facilityStatusDetails.Count != 0)
                {
                    foreach (var item in facilityStatusDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.powerSupplyId)
                        {
                            YesOrNo = "yes_b";
                        }
                        powerSupply += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.facilityStatus + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Power_Supply##", powerSupply);
                string waterSupply = string.Empty;
                List<jntuh_facility_status> facilityDetails = db.jntuh_facility_status.Where(facility => facility.isActive == true).ToList();
                if (facilityDetails.Count != 0)
                {
                    foreach (var item in facilityDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.WaterSupplyId)
                        {
                            YesOrNo = "yes_b";
                        }
                        waterSupply += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.facilityStatus + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Water_Supply##", waterSupply);
                string drinkingWater = string.Empty;
                List<jntuh_water_type> drinkingWaterDetails = db.jntuh_water_type.Where(facility => facility.isActive == true).ToList();
                if (drinkingWaterDetails.Count != 0)
                {
                    foreach (var item in drinkingWaterDetails)
                    {
                        string YesOrNo = "no_b";
                        if (item.id == landDetails.drinkingWaterId)
                        {
                            YesOrNo = "yes_b";
                        }
                        drinkingWater += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.waterType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Drinking_Water##", drinkingWater);

                if (landDetails.IsPurifiedWater == true)
                {
                    contents = contents.Replace("##LAND_IS_Purified_Water##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }
                else
                {
                    contents = contents.Replace("##LAND_IS_Purified_Water##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }

                contents = contents.Replace("##LAND_Potable_Water##", landDetails.potableWaterPerDay.ToString());
            }
            else
            {
                contents = contents.Replace("##LAND_Total_Land_Area##", string.Empty);
                string landType = string.Empty;
                List<jntuh_land_type> landTypeDetails = db.jntuh_land_type.Where(land => land.isActive == true).ToList();
                if (landTypeDetails.Count != 0)
                {
                    foreach (var item in landTypeDetails)
                    {
                        string YesOrNo = "no_b";
                        landType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Land_Type##", landType);

                string landRegistrationType = string.Empty;
                List<jntuh_land_registration_type> landRegistrationTypeDetails = db.jntuh_land_registration_type.Where(land => land.isActive == true).ToList();
                if (landRegistrationTypeDetails.Count != 0)
                {
                    foreach (var item in landRegistrationTypeDetails)
                    {
                        string YesOrNo = "no_b";
                        landRegistrationType += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landRegistrationType);
                    }
                }
                contents = contents.Replace("##LAND_Land_Registration_Type##", landRegistrationType);

                string landCategory = string.Empty;
                List<jntuh_land_category> landCategoryDetails = db.jntuh_land_category.Where(land => land.isActive == true).ToList();
                if (landCategoryDetails.Count != 0)
                {
                    foreach (var item in landCategoryDetails)
                    {
                        string YesOrNo = "no_b";
                        landCategory += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.landCategory + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Land_Category##", landCategory);
                contents = contents.Replace("##LAND_Conversion_Issued_by##", string.Empty);
                contents = contents.Replace("##LAND_Conversion_Issued_Date##", DDMMYYYYspace);
                contents = contents.Replace("##LAND_Conversion_Issued_Purpose##", string.Empty);
                contents = contents.Replace("##LAND_proposed_Issued_by##", string.Empty);
                contents = contents.Replace("##LAND_proposed_Issued_Date##", DDMMYYYYspace);
                contents = contents.Replace("##LAND_institution_Issued_by##", string.Empty);
                contents = contents.Replace("##LAND_institution_Issued_Date##", DDMMYYYYspace);
                contents = contents.Replace("##LAND_CompoundWallFencing##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No</td>"));


                string approachRoad = string.Empty;
                List<jntuh_approach_road> approachRoadDetails = db.jntuh_approach_road.Where(road => road.isActive == true).ToList();
                if (approachRoadDetails.Count != 0)
                {
                    foreach (var item in approachRoadDetails)
                    {
                        string YesOrNo = "no_b";
                        approachRoad += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.approachRoadType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Approach_Road##", approachRoad);
                string powerSupply = string.Empty;
                List<jntuh_facility_status> facilityStatusDetails = db.jntuh_facility_status.Where(facility => facility.isActive == true).ToList();
                if (facilityStatusDetails.Count != 0)
                {
                    foreach (var item in facilityStatusDetails)
                    {
                        string YesOrNo = "no_b";
                        powerSupply += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.facilityStatus + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Power_Supply##", powerSupply);
                string waterSupply = string.Empty;
                List<jntuh_facility_status> facilityDetails = db.jntuh_facility_status.Where(facility => facility.isActive == true).ToList();
                if (facilityDetails.Count != 0)
                {
                    foreach (var item in facilityDetails)
                    {
                        string YesOrNo = "no_b";
                        waterSupply += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.facilityStatus + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Water_Supply##", waterSupply);
                string drinkingWater = string.Empty;
                List<jntuh_water_type> drinkingWaterDetails = db.jntuh_water_type.Where(facility => facility.isActive == true).ToList();
                if (drinkingWaterDetails.Count != 0)
                {
                    foreach (var item in drinkingWaterDetails)
                    {
                        string YesOrNo = "no_b";
                        drinkingWater += string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />&nbsp;{1}&nbsp;", YesOrNo, item.waterType + "&nbsp; &nbsp; &nbsp;");
                    }
                }
                contents = contents.Replace("##LAND_Drinking_Water##", drinkingWater);

                contents = contents.Replace("##LAND_IS_Purified_Water##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No"));

                contents = contents.Replace("##LAND_Potable_Water##", string.Empty);
            }

            #endregion

            #region from jntuh_college_land_registration table

            string lanRegistrationInformation = string.Empty;

            List<jntuh_college_land_registration> landRegistrationDetails = db.jntuh_college_land_registration.Where(land => land.collegeId == collegeId).ToList();

            if (landRegistrationDetails.Count > 5)
            {
                int i = 1;
                foreach (var item in landRegistrationDetails)
                {
                    lanRegistrationInformation += "<td colspan='1'>" + i + "</td>";
                    lanRegistrationInformation += "<td colspan='4'>" + UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.landRegistraionDate.ToString()) + "</td>";
                    lanRegistrationInformation += "<td colspan='2'>" + item.landAreaInAcres.ToString() + "</td>";
                    lanRegistrationInformation += "<td colspan='3'>" + item.landDocumentNumber + "</td>";
                    lanRegistrationInformation += "<td colspan='4'>" + item.landSurveyNumber + "</td>";
                    lanRegistrationInformation += "<td colspan='5' valign='top'>" + item.landLocation + "</td>";
                    i++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    lanRegistrationInformation += "<td colspan='1'>" + i + "</td>";
                    lanRegistrationInformation += "<td colspan='4'>##LAND_LAND_REGISTRATION_DATE(DD/MM/YYYY)" + i + "##</td>";
                    lanRegistrationInformation += "<td colspan='2'>##LAND_LAND_REGISTRATION_AREA_IN_ACRES" + i + "##</td>";
                    lanRegistrationInformation += "<td colspan='3'>##LAND_LAND_DOCUMENT_NUMBER" + i + "##</td>";
                    lanRegistrationInformation += "<td colspan='4'>##LAND_SURVEY_NUMBER" + i + "##</td>";
                    lanRegistrationInformation += "<td colspan='5' valign='top'>##OLAND_LOCATION_OR_VILLAGE" + i + "##</td>";
                }

                if (landRegistrationDetails.Count != 0)
                {
                    int count = 1;
                    foreach (var item in landRegistrationDetails)
                    {
                        lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_REGISTRATION_DATE(DD/MM/YYYY)" + count + "##", UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.landRegistraionDate.ToString()));
                        lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_REGISTRATION_AREA_IN_ACRES" + count + "##", item.landAreaInAcres.ToString());
                        lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_DOCUMENT_NUMBER" + count + "##", item.landDocumentNumber);
                        lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_SURVEY_NUMBER" + count + "##", item.landSurveyNumber);
                        lanRegistrationInformation = lanRegistrationInformation.Replace("##OLAND_LOCATION_OR_VILLAGE" + count + "##", item.landLocation);
                        count++;
                    }

                }
                for (int i = 1; i <= 5; i++)
                {
                    lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_REGISTRATION_DATE(DD/MM/YYYY)" + i + "##", string.Empty);
                    lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_REGISTRATION_AREA_IN_ACRES" + i + "##", string.Empty);
                    lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_LAND_DOCUMENT_NUMBER" + i + "##", string.Empty);
                    lanRegistrationInformation = lanRegistrationInformation.Replace("##LAND_SURVEY_NUMBER" + i + "##", string.Empty);
                    lanRegistrationInformation = lanRegistrationInformation.Replace("##OLAND_LOCATION_OR_VILLAGE" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##LAND_LANDREGISTRATIONINFORMATION##", lanRegistrationInformation);
            #endregion

            return contents;
        }

        private string administrativeLandInformation(int collegeId, string contents)
        {
            decimal totalArea = 0;
            string adminLand = string.Empty;
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
                                     }).Where(g => g.availableRooms != null).ToList();
            foreach (var item in land)
            {
                adminLand += "<td colspan='3' width='24%'><p>" + item.requirementType.ToString() + "</p></td>";
                if (item.jntuh_program_type != null)
                {
                    adminLand += "<td colspan='2' width='18%'>" + item.jntuh_program_type.programType.ToString() + "</td>";
                }
                else
                {
                    adminLand += "<td colspan='2' width='18%'>&nbsp;</td>";
                }
                adminLand += "<td colspan='1' width='9%' align='right'>" + Convert.ToInt32(item.availableRooms) + "</td>";
                //adminLand += "<td colspan='1' width='9%'>&nbsp;</td>";
                adminLand += "<td colspan='1' width='9%' align='right'>" + item.availableArea + "</td>";
                // adminLand += "<td colspan='1' width='9%'>&nbsp;</td>";

                if (item.availableArea != null)
                {
                    totalArea += (decimal)item.availableArea;
                }
            }
            //adminLand += "<tr>";
            //strAdministrativeLandDetails += "<td width='42%'></td>";
            adminLand += "<td colspan='6' align='right'><b>Total</b></td>";
            adminLand += "<td width='9%' align='right'>" + totalArea + "</td>";
            //adminLand += "</tr>";
            contents = contents.Replace("##ADMINISTRATIVE_LAND_DETAILS##", adminLand);
            return contents;
        }

        private string InstrctionalLandInformation(int collegeId, string contents)
        {
            string instructionalLand = string.Empty;
            string programType = string.Empty;
            decimal? availableRooms = 0;
            decimal? availableArea = 0;
            int existId = 0;
            decimal totalArea = 0;

            Nullable<int>[] list = db.jntuh_area_requirement.OrderBy(area => area.programId)
                                                            .Where(area => area.areaType == "INSTRUCTIONAL" && area.isActive == true)
                                                            .Select(area => area.programId).Distinct().ToArray();
            List<string> degreeNames = db.jntuh_college_degree.Join(db.jntuh_degree, cd => cd.degreeId, d => d.id, (cd, d) => new { d.degree, cd.collegeId, cd.isActive }).Where(cd => cd.collegeId == collegeId && cd.isActive == true).Select(d => d.degree).ToList();

            foreach (var item in list)
            {
                string program = db.jntuh_program_type.Where(p => p.id == item).Select(p => p.programType).FirstOrDefault();

                if (degreeNames.Contains(program))
                {
                    if (string.Equals(program.ToUpper(), "M.PHARMACY") || string.Equals(program.ToUpper(), "M.TECH"))
                    {
                        var collegeDegreeSpecializations = db.jntuh_college_intake_proposed
                                                         .Join(db.jntuh_specialization, prop => prop.specializationId, spec => spec.id, (prop, spec) => new { prop, spec })
                                                         .Join(db.jntuh_department, a => a.spec.departmentId, dep => dep.id, (a, dep) => new { a, dep })
                                                         .Join(db.jntuh_degree, b => b.dep.degreeId, d => d.id, (b, d) => new { b, d })
                                                         .Where(s => s.b.a.spec.isActive == true && s.b.a.prop.collegeId == collegeId && s.d.degree == program)
                                                         .OrderBy(s => s.b.a.spec.specializationName)
                                                         .Select(s => new { Text = s.b.a.spec.specializationName, Value = s.b.a.spec.id }).ToList();

                        collegeDegreeSpecializations = collegeDegreeSpecializations.GroupBy(s => s.Text).Select(s => new { Text = s.First().Text, Value = s.First().Value }).ToList();

                        foreach (var specialization in collegeDegreeSpecializations)
                        {
                            int[] requirementId = db.jntuh_area_requirement.OrderBy(area => area.programId)
                                                                           .Where(area => area.isActive == true &&
                                                                                          area.areaType == "INSTRUCTIONAL" &&
                                                                                          area.programId == item)
                                                                           .Select(area => area.id)
                                                                           .ToArray();

                            if (existId == 0)
                            {
                                instructionalLand += "<tr><td width='28%'><p><strong>" + program + " (" + specialization.Text + ")</strong></p></td>";
                                instructionalLand += "<td width='10%'><p align='center'>Available Rooms</p></td>";
                                // instructionalLand += "<td width='9%'><p align='center'><strong>Committee Findings (Rooms)</strong></p></td>";
                                instructionalLand += "<td width='10%'><p align='center'>Available Area</p></td></tr>";
                                //instructionalLand += "<td width='9%'><p align='center'><strong>Committee Findings (Area)</strong></p></td></tr>";
                                existId++;
                            }
                            else
                            {
                                instructionalLand += "<tr><td colspan='5' style='width: 200%'><p><strong>" + program + " (" + specialization.Text + ")</strong></p></td></tr>";
                            }

                            foreach (var id in requirementId)
                            {
                                programType = db.jntuh_area_requirement.Where(a => a.id == id).Select(a => a.requirementType).FirstOrDefault();
                                availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == id && a.specializationID == specialization.Value).Select(a => a.availableRooms).FirstOrDefault();
                                availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == id && a.specializationID == specialization.Value).Select(a => a.availableArea).FirstOrDefault();
                                if (availableRooms != null && availableRooms.ToString() != string.Empty)
                                {
                                    instructionalLand += "<tr><td width='28%'><p>" + programType + "</p></td>";
                                    instructionalLand += "<td width='10%' align='right'>" + Convert.ToInt32(availableRooms).ToString() + "</td>";
                                    // instructionalLand += "<td width='10%'>&nbsp;</td>";
                                    instructionalLand += "<td width='10%' align='right'>" + availableArea.ToString() + "</td></tr>";
                                    // instructionalLand += "<td width='10%'>&nbsp;</td></tr>";
                                }
                                if (availableArea != null)
                                {
                                    totalArea += (decimal)availableArea;
                                }
                            }
                        }
                    }
                    else
                    {
                        int[] requirementId = db.jntuh_area_requirement.OrderBy(area => area.programId)
                                                                       .Where(area => area.isActive == true &&
                                                                                      area.areaType == "INSTRUCTIONAL" &&
                                                                                      area.programId == item)
                                                                       .Select(area => area.id)
                                                                       .ToArray();

                        if (existId == 0)
                        {
                            instructionalLand += "<tr><td width='28%'><p><strong>" + program + "</strong></p></td>";
                            instructionalLand += "<td width='10%'><p align='center'>Available Rooms</p></td>";
                            // instructionalLand += "<td width='9%'><p align='center'><strong>Committee Findings (Rooms)</strong></p></td>";
                            instructionalLand += "<td width='10%'><p align='center'>Available Area</p></td></tr>";
                            //instructionalLand += "<td width='9%'><p align='center'><strong>Committee Findings (Area)</strong></p></td></tr>";
                            existId++;
                        }
                        else
                        {
                            instructionalLand += "<tr><td colspan='5' style='width: 200%'><p><strong>" + program + "</strong></p></td></tr>";
                        }

                        foreach (var id in requirementId)
                        {
                            programType = db.jntuh_area_requirement.Where(a => a.id == id).Select(a => a.requirementType).FirstOrDefault();
                            availableRooms = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == id).Select(a => a.availableRooms).FirstOrDefault();
                            availableArea = db.jntuh_college_area.Where(a => a.collegeId == collegeId && a.areaRequirementId == id).Select(a => a.availableArea).FirstOrDefault();
                            if (availableRooms != null && availableRooms.ToString() != string.Empty)
                            {
                                instructionalLand += "<tr><td width='28%'><p>" + programType + "</p></td>";
                                instructionalLand += "<td width='10%' align='right'>" + Convert.ToInt32(availableRooms).ToString() + "</td>";
                                //instructionalLand += "<td width='10%'>&nbsp;</td>";
                                instructionalLand += "<td width='10%' align='right'>" + availableArea.ToString() + "</td></tr>";
                                //  instructionalLand += "<td width='10%'>&nbsp;</td></tr>";
                            }
                            if (availableArea != null)
                            {
                                totalArea += (decimal)availableArea;
                            }
                        }
                    }
                }
            }

            instructionalLand += "<tr>";
            instructionalLand += "<td colspan='2' align='right'><b>Total</b></td>";
            instructionalLand += "<td width='10%' align='right'>" + totalArea + "</td>";
            instructionalLand += "</tr>";

            contents = contents.Replace("##INSTRUCTIONAL_LAND_DETAILS##", instructionalLand);
            return contents;
        }

        private string collegeIntakeExistingOld(int collegeId, string contents)
        {
            int count = 1;
            // int proposedIntake201415 = 0;
            //int totalproposedIntake201415 = 0;
            int approved1 = 0;
            int admitted1 = 0;
            int approved2 = 0;
            int admitted2 = 0;
            int approved3 = 0;
            int admitted3 = 0;
            int approved4 = 0;
            int admitted4 = 0;
            int approved5 = 0;
            int admitted5 = 0;
            string intakeExisting = string.Empty;
            string academicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            // string presentYear201415 = String.Format("{0}-{1}", (actualYear+1).ToString(), (actualYear + 2).ToString().Substring(2, 2));

            string firstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
            string secondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            string thirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            string fourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            string fifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AY201415 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR11##", fifthYear);
            contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR22##", fourthYear);
            contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR33##", thirdYear);
            contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR44##", secondYear);
            contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR55##", firstYear);
            //contents = contents.Replace("##EXISTINGINTAKEACADEMICYEAR201415##", presentYear201415);

            List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId).ToList();

            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();

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
                newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeIntakeExisting.Add(newIntake);
            }

            collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

            //19/11/2014
            /*int statuuscodeID=db.jntuh_course_affiliation_status.Where(s=>s.courseAffiliationStatusCode=="N").Select(s=>s.id).FirstOrDefault();
            var ProposedListnotinExisting = db.jntuh_college_intake_proposed.Where(p => p.courseAffiliationStatusCodeId == statuuscodeID && p.collegeId==collegeId).Select(p => p).ToList();
            if (ProposedListnotinExisting.Count() > 0)
            {
                foreach (var item in ProposedListnotinExisting)
                {
                    CollegeIntakeExisting newIntake = new CollegeIntakeExisting();
                    newIntake.id = item.id;
                    newIntake.collegeId = item.collegeId;
                    newIntake.academicYearId = item.academicYearId;
                    newIntake.shiftId = item.shiftId;
                    newIntake.isActive = item.isActive;
                    newIntake.nbaFrom = null;
                    newIntake.nbaTo = null;
                    newIntake.specializationId = item.specializationId;
                    newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                    newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                    newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                    newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                    newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                    newIntake.shiftId = item.shiftId;
                    newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                    collegeIntakeExisting.Add(newIntake);
                }
            }*/
            //

            collegeIntakeExisting = collegeIntakeExisting.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            if (collegeIntakeExisting.Count > 10)
            {
                foreach (var item in collegeIntakeExisting)
                {
                    //proposedIntake201415 = 0;
                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                    //proposedIntake201415 = db.jntuh_college_intake_proposed.Where(i => i.collegeId == collegeId && i.academicYearId == AY201415 && i.specializationId ==item.specializationId && i.shiftId ==item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();

                    item.approvedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);

                    item.approvedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);

                    item.approvedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);

                    item.approvedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);

                    item.approvedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);


                    // totalproposedIntake201415 += Convert.ToInt32(proposedIntake201415);
                    approved1 += Convert.ToInt32(item.approvedIntake1);
                    approved2 += Convert.ToInt32(item.approvedIntake2);
                    approved3 += Convert.ToInt32(item.approvedIntake3);
                    approved4 += Convert.ToInt32(item.approvedIntake4);
                    approved5 += Convert.ToInt32(item.approvedIntake5);
                    admitted1 += Convert.ToInt32(item.admittedIntake1);
                    admitted2 += Convert.ToInt32(item.admittedIntake2);
                    admitted3 += Convert.ToInt32(item.admittedIntake3);
                    admitted4 += Convert.ToInt32(item.admittedIntake4);
                    admitted5 += Convert.ToInt32(item.admittedIntake5);

                    intakeExisting += "<td colspan='1' width='28'><p align='center'>" + count + "</p></td>";
                    intakeExisting += "<td colspan='3' style='font-size: 9px;' width='56'>" + item.Degree + "</td>";
                    intakeExisting += "<td colspan='4' style='font-size: 9px;' width='63'>" + item.Department + "</td>";
                    intakeExisting += "<td colspan='4' style='font-size: 9px;' width='200'>" + item.Specialization + "</td>";
                    intakeExisting += "<td colspan='1' width='42'>" + item.Shift + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.approvedIntake5.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.admittedIntake5.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.approvedIntake4.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.admittedIntake4.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.approvedIntake3.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.admittedIntake3.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.approvedIntake2.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.admittedIntake2.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='42'>" + item.approvedIntake1.ToString() + "</td>";
                    intakeExisting += "<td colspan='1' width='50'>" + item.admittedIntake1.ToString() + "</td>";
                    //2014-15
                    // intakeExisting += "<td colspan='1' width='42'>" + proposedIntake201415.ToString() + "</td>";
                    //intakeExisting += "<td colspan='1' width='50'></td>";

                    intakeExisting += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                    intakeExisting += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 10; i++)
                {
                    intakeExisting += "<td colspan='1' width='28'><p align='center'>" + i + "</p></td>";
                    intakeExisting += "<td colspan='3' style='font-size: 9px;' width='56'>##EXISTINGDEGREE" + i + "##</td>";
                    intakeExisting += "<td colspan='4' style='font-size: 9px;' width='63'>##EXISTINGDEPARTMENT" + i + "##</td>";
                    intakeExisting += "<td colspan='4' style='font-size: 9px;' width='200'>##EXISTINGSPECIALIZATION" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='42'>##EXISTINGSHIFT" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED5" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED5" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED4" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED4" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED3" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED3" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED2" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED2" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED1" + i + "##</td>";
                    intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED1" + i + "##</td>";
                    //2014-15
                    // intakeExisting += "<td colspan='1' width='50'>##EXISTINGAPPROVED201415" + i + "##</td>";
                    // intakeExisting += "<td colspan='1' width='50'>##EXISTINGADMITTED201415" + i + "##</td>";

                    intakeExisting += "<td colspan='2' style='font-size: 8px;' width='50'>##EXISTINGPERIODFROM" + i + "##</td>";
                    intakeExisting += "<td colspan='2' style='font-size: 8px;' width='50'>##EXISTINGPERIODTO" + i + "##</td>";
                }

                foreach (var item in collegeIntakeExisting)
                {
                    //proposedIntake201415 = 0;
                    if (item.nbaFrom != null)
                        item.nbaFromDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaFrom.ToString());
                    if (item.nbaTo != null)
                        item.nbaToDate = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.nbaTo.ToString());

                    //proposedIntake201415 = db.jntuh_college_intake_proposed.Where(i => i.collegeId == collegeId && i.academicYearId == AY201415 && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.proposedIntake).FirstOrDefault();

                    item.approvedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 1);
                    item.admittedIntake1 = GetIntake(collegeId, AY1, item.specializationId, item.shiftId, 0);

                    item.approvedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 1);
                    item.admittedIntake2 = GetIntake(collegeId, AY2, item.specializationId, item.shiftId, 0);

                    item.approvedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 1);
                    item.admittedIntake3 = GetIntake(collegeId, AY3, item.specializationId, item.shiftId, 0);

                    item.approvedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 1);
                    item.admittedIntake4 = GetIntake(collegeId, AY4, item.specializationId, item.shiftId, 0);

                    item.approvedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 1);
                    item.admittedIntake5 = GetIntake(collegeId, AY5, item.specializationId, item.shiftId, 0);

                    //totalproposedIntake201415 += Convert.ToInt32(proposedIntake201415);

                    approved1 += Convert.ToInt32(item.approvedIntake1);
                    approved2 += Convert.ToInt32(item.approvedIntake2);
                    approved3 += Convert.ToInt32(item.approvedIntake3);
                    approved4 += Convert.ToInt32(item.approvedIntake4);
                    approved5 += Convert.ToInt32(item.approvedIntake5);
                    admitted1 += Convert.ToInt32(item.admittedIntake1);
                    admitted2 += Convert.ToInt32(item.admittedIntake2);
                    admitted3 += Convert.ToInt32(item.admittedIntake3);
                    admitted4 += Convert.ToInt32(item.admittedIntake4);
                    admitted5 += Convert.ToInt32(item.admittedIntake5);

                    intakeExisting = intakeExisting.Replace("##EXISTINGDEGREE" + count + "##", item.Degree);
                    intakeExisting = intakeExisting.Replace("##EXISTINGDEPARTMENT" + count + "##", item.Department);
                    intakeExisting = intakeExisting.Replace("##EXISTINGSPECIALIZATION" + count + "##", item.Specialization);
                    intakeExisting = intakeExisting.Replace("##EXISTINGSHIFT" + count + "##", item.Shift);
                    //2014-15
                    //intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED201415" + count + "##", proposedIntake201415.ToString());
                    //intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED201415" + count + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED1" + count + "##", item.approvedIntake1.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED1" + count + "##", item.admittedIntake1.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED2" + count + "##", item.approvedIntake2.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED2" + count + "##", item.admittedIntake2.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED3" + count + "##", item.approvedIntake3.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED3" + count + "##", item.admittedIntake3.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED4" + count + "##", item.approvedIntake4.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED4" + count + "##", item.admittedIntake4.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED5" + count + "##", item.approvedIntake5.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED5" + count + "##", item.admittedIntake5.ToString());
                    intakeExisting = intakeExisting.Replace("##EXISTINGPERIODFROM" + count + "##", item.nbaFromDate);
                    intakeExisting = intakeExisting.Replace("##EXISTINGPERIODTO" + count + "##", item.nbaToDate);
                    count++;
                }

                for (int i = 1; i <= 10; i++)
                {
                    intakeExisting = intakeExisting.Replace("##EXISTINGDEGREE" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGDEPARTMENT" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGSPECIALIZATION" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGSHIFT" + i + "##", string.Empty);
                    //2014-15
                    // intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED201415" + i + "##", string.Empty);
                    // intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED201415" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED1" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED1" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED2" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED2" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED3" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED3" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED4" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED4" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGAPPROVED5" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGADMITTED5" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGPERIODFROM" + i + "##", string.Empty);
                    intakeExisting = intakeExisting.Replace("##EXISTINGPERIODTO" + i + "##", string.Empty);
                }
            }
            string total = ((admitted1 + admitted2 + admitted3 + admitted4 + admitted5).ToString()) + "/" + ((approved1 + approved2 + approved3 + approved4 + approved5).ToString());

            //2014-15
            //contents = contents.Replace("##EXISTINGAPPROVED201415##", totalproposedIntake201415.ToString());
            //contents = contents.Replace("##EXISTINGADMITTED201415##", string.Empty);

            contents = contents.Replace("##EXISTINGAPPROVED55##", approved1.ToString());
            contents = contents.Replace("##EXISTINGAPPROVED44##", approved2.ToString());
            contents = contents.Replace("##EXISTINGAPPROVED33##", approved3.ToString());
            contents = contents.Replace("##EXISTINGAPPROVED22##", approved4.ToString());
            contents = contents.Replace("##EXISTINGAPPROVED11##", approved5.ToString());
            contents = contents.Replace("##EXISTINGADMITTED55##", admitted1.ToString());
            contents = contents.Replace("##EXISTINGADMITTED44##", admitted2.ToString());
            contents = contents.Replace("##EXISTINGADMITTED33##", admitted3.ToString());
            contents = contents.Replace("##EXISTINGADMITTED22##", admitted4.ToString());
            contents = contents.Replace("##EXISTINGADMITTED11##", admitted5.ToString());
            contents = contents.Replace("##EXISTINGTOTALADMITTEDTOTALAPPROVED##", total);
            contents = contents.Replace("##COLLEGEINTAKEEXISTINGINFORMATION##", intakeExisting);
            return contents;
        }

        public string collegeIntakeExisting(int collegeId, string contents)
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

            ViewBag.AcademicYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.academicYear).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            string FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            string SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            string ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            string FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            string FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

            int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
            int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
            int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
            int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
            int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

            int PAY = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();

            strExistingIntakeDetails += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 8px;'>";
            strExistingIntakeDetails += "<tbody>";
            strExistingIntakeDetails += "<tr>";
            strExistingIntakeDetails += "<td width='28' rowspan='3' colspan='1'><p align='center'>S.No</p></td>";
            strExistingIntakeDetails += "<td width='56' rowspan='3' colspan='3'><p align='left'>Degree</p><p align='left'>*</p></td>";
            strExistingIntakeDetails += "<td width='63' rowspan='3' colspan='4'><p align='left'>Department</p><p align='left'>**</p></td>";
            strExistingIntakeDetails += "<td width='200' rowspan='3' colspan='4'><p align='left'>Specialization</p><p align='left'>***</p></td>";
            strExistingIntakeDetails += "<td width='42' rowspan='3' colspan='1' style='font-size: 9px; line-height: 10px;'><p align='center'>Shift</p><p align='center'>#</p></td>";
            strExistingIntakeDetails += "<td width='500' colspan='10'><p align='center'>Sanctioned & Actual Admitted Intake as per Academic Year</p></td>";
            strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>AI</p><p align='left'></p></td>";
            strExistingIntakeDetails += "<td  rowspan='3' colspan='1'><p align='left'>PI</p><p align='left'></p></td>";
            strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center'>NBA accreditation Period (if exists)</p></td>";
            strExistingIntakeDetails += "<td width='200' rowspan='3' colspan='2'><p align='left' style='font-size: 7px;'>Committee Findings</td></tr>";
            strExistingIntakeDetails += "<tr><td width='100' colspan='2'><p align='center'>" + FifthYear + "</p></td>";
            strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FourthYear + "</p></td>";
            strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + ThirdYear + "</p></td>";
            strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + SecondYear + "</p></td>";
            strExistingIntakeDetails += "<td width='100' colspan='2'><p align='center'>" + FirstYear + "</p></td>";


            strExistingIntakeDetails += "<td width='100' colspan='4' valign='top'><p align='center' style='font-size: 7px;'>(DD/MM/YYYY)</p></td>";
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
            var jntuh_specialization = db.jntuh_specialization;
            var jntuh_department = db.jntuh_department;
            var jntuh_degree = db.jntuh_degree;
            var jntuh_shift = db.jntuh_shift;
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
                newIntake.Specialization = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newIntake.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newIntake.Department = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newIntake.degreeID = jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newIntake.Degree = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
                newIntake.degreeDisplayOrder = jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newIntake.shiftId = item.shiftId;
                newIntake.Shift = jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
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
                item.ApprovedIntake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == PAY && i.specializationId == item.specializationId && i.shiftId == item.shiftId).Select(i => i.approvedIntake).FirstOrDefault();
                if (item.ApprovedIntake != null)
                    totalPAYApproved += (int)item.ApprovedIntake;
                //totalPAYAdmited += item.admittedIntake;

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
                strExistingIntakeDetails += "<td colspan='1'>" + item.ApprovedIntake + "</td>";
                strExistingIntakeDetails += "<td colspan='1'>" + item.ProposedIntake + "</td>";
                strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaFromDate + "</td>";
                strExistingIntakeDetails += "<td colspan='2' style='font-size: 8px;' width='50'>" + item.nbaToDate + "</td>";
                strExistingIntakeDetails += "<td colspan='2'></td>";
                strExistingIntakeDetails += "</tr>";
                if (item.Degree == "Pharm.D PB")//6
                {
                    totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4 + item.admittedIntake5;
                    totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5;
                }
                else if (item.Degree == "MAM" || item.Degree == "MTM" || item.Degree == "Pharm.D")//5
                {
                    totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3 + item.admittedIntake4;
                    totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4;
                }
                else if (item.Degree == "B.Tech" || item.Degree == "B.Pharmacy")//4
                {
                    totalAdmited += item.admittedIntake1 + item.admittedIntake2 + item.admittedIntake3;
                    totalApproved += item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3;
                }
                else if (item.Degree == "MCA")//3
                {
                    totalAdmited += item.admittedIntake1 + item.admittedIntake2;
                    totalApproved += item.approvedIntake1 + item.approvedIntake2;
                }
                else if (item.Degree == "M.Tech" || item.Degree == "M.Pharmacy" || item.Degree == "MBA") //2
                {
                    totalAdmited += item.admittedIntake1;
                    totalApproved += item.approvedIntake1;
                }
            }
            // totalAdmited += totalAdmittedIntake1 + totalAdmittedIntake2 + totalAdmittedIntake3 + totalAdmittedIntake4 + totalAdmittedIntake5 + totalPAYAdmited;
            // totalApproved += totalApprovedIntake1 + totalApprovedIntake2 + totalApprovedIntake3 + totalApprovedIntake4 + totalApprovedIntake5 + totalPAYApproved;

            strExistingIntakeDetails += "<tr><td width='337' colspan='13'><p align='right'>Total =</p></td><td width='50' colspan='1' align='center'>" + totalApprovedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake5 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake4 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake3 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake2 + "</td><td width='50' colspan='1' align='center'>" + totalApprovedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalAdmittedIntake1 + "</td><td width='50' colspan='1' align='center'>" + totalPAYApproved + "</td><td width='50' colspan='1' align='center'>" + totalPAYProposed + "</td><td width='50' colspan='2' valign='top' align='center'></td><td width='50' colspan='2' valign='top' align='center'></td><td colspan='2' valign='top' align='center'></td></tr>";
            strExistingIntakeDetails += "<tr><td colspan='15'><p align='right'>Total Admitted / Total Sanctioned =</p></td><td colspan='14' width='600'>" + totalAdmited + '/' + totalApproved + "</td><td colspan='2' valign='top' align='center'></td></tr>";
            strExistingIntakeDetails += "</tbody></table>";
            contents = contents.Replace("##ExistingIntakeDetails##", strExistingIntakeDetails);

            return contents;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;

            if (flag == 1)
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
            }
            else
            {
                intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
            }
            return intake;
        }

        private string collegeAcademicPerformance(int collegeId, string contents)
        {
            int count = 1;
            string academicPerformace = string.Empty;
            int degreeTypeId = 0;
            string degreeType = string.Empty;
            int appread1 = 0;
            int appread2 = 0;
            int appread3 = 0;
            int appread4 = 0;
            int passed1 = 0;
            int passed2 = 0;
            int passed3 = 0;
            int passed4 = 0;
            decimal percentage1 = 0;
            decimal percentage2 = 0;
            decimal percentage3 = 0;
            decimal percentage4 = 0;
            int ugAppeared1 = 0;
            int ugAppeared2 = 0;
            int ugAppeared3 = 0;
            int ugAppeared4 = 0;
            int pgAppreared1 = 0;
            int pgAppreared2 = 0;
            int pgAppreared3 = 0;
            int ugPassed1 = 0;
            int ugPassed2 = 0;
            int ugPassed3 = 0;
            int ugPassed4 = 0;
            int pgPassed1 = 0;
            int pgPassed2 = 0;
            int pgPassed3 = 0;
            decimal UGpercentage1 = 0;
            decimal UGpercentage2 = 0;
            decimal UGpercentage3 = 0;
            decimal UGpercentage4 = 0;
            decimal PGpercentage1 = 0;
            decimal PGpercentage2 = 0;
            decimal PGpercentage3 = 0;

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

            int AYID = db.jntuh_academic_year.Where(a => a.actualYear == actualYear).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_academic_performance> performance = db.jntuh_college_academic_performance.Where(i => i.collegeId == collegeId).ToList();

            List<CollegeAcademicPerformance> collegeAcademicPerformance = new List<CollegeAcademicPerformance>();

            foreach (var item in performance)
            {
                CollegeAcademicPerformance newPerformance = new CollegeAcademicPerformance();
                newPerformance.id = item.id;
                newPerformance.collegeId = item.collegeId;
                newPerformance.academicYearId = item.academicYearId;
                newPerformance.shiftId = item.shiftId;
                newPerformance.isActive = item.isActive;
                newPerformance.specializationId = item.specializationId;
                newPerformance.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPerformance.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPerformance.Department = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newPerformance.degreeID = db.jntuh_department.Where(d => d.id == newPerformance.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newPerformance.Degree = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degree).FirstOrDefault();
                newPerformance.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPerformance.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newPerformance.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                collegeAcademicPerformance.Add(newPerformance);
            }

            collegeAcademicPerformance = collegeAcademicPerformance.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            collegeAcademicPerformance = collegeAcademicPerformance.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            if (collegeAcademicPerformance.Count > 10)
            {
                foreach (var item in collegeAcademicPerformance)
                {
                    item.appearedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 1));
                    item.passedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 2));
                    item.passPercentage1 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 3));

                    item.appearedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 1));
                    item.passedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 2));
                    item.passPercentage2 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 3));

                    item.appearedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 1));
                    item.passedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 2));
                    item.passPercentage3 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 3));

                    item.appearedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 1));
                    item.passedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 2));
                    item.passPercentage4 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 3));

                    appread1 += item.appearedStudents1;
                    appread2 += item.appearedStudents2;
                    appread3 += item.appearedStudents3;
                    appread4 += item.appearedStudents4;
                    passed1 += item.passedStudents1;
                    passed2 += item.passedStudents2;
                    passed3 += item.passedStudents3;
                    passed4 += item.passedStudents4;

                    academicPerformace += "<td colspan='2' width='40'><p align='center'>" + count + "</p></td>";
                    academicPerformace += "<td colspan='3' style='font-size: 9px;' width='78' align='left'>" + item.Degree + "</td>";
                    academicPerformace += "<td colspan='4' style='font-size: 9px;' width='59' align='left'>" + item.Department + "</td>";
                    academicPerformace += "<td colspan='5' style='font-size: 9px;' width='148' align='left'>" + item.Specialization + "</td>";
                    academicPerformace += "<td colspan='2' width='42' align='center'>" + item.Shift + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.appearedStudents4 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.passedStudents4 + "</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>" + item.passPercentage4 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.appearedStudents3 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.passedStudents3 + "</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>" + item.passPercentage3 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.appearedStudents2 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.passedStudents2 + "</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>" + item.passPercentage2 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.appearedStudents1 + "</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>" + item.passedStudents1 + "</td>";
                    academicPerformace += "<td colspan='2' width='49' align='center'>" + item.passPercentage1 + "</td>";

                    count++;

                    degreeTypeId = db.jntuh_degree.Where(degree => degree.id == item.degreeID).Select(degree => degree.degreeTypeId).FirstOrDefault();

                    degreeType = db.jntuh_degree_type.Where(degree => degree.id == degreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();

                    if (degreeType.Trim().ToUpper() == "UG")
                    {
                        ugAppeared1 += item.appearedStudents1;
                        ugAppeared2 += item.appearedStudents2;
                        ugAppeared3 += item.appearedStudents3;
                        ugAppeared4 += item.appearedStudents4;

                        ugPassed1 += item.passedStudents1;
                        ugPassed2 += item.passedStudents2;
                        ugPassed3 += item.passedStudents3;
                        ugPassed4 += item.passedStudents4;
                        //ugPercentage1 += item.passPercentage1;
                        //ugPercentage2 += item.passPercentage2;
                        //ugPercentage3 += item.passPercentage3;
                        //ugPercentage4 += item.passPercentage4;
                    }
                    if (degreeType.Trim().ToUpper() == "PG")
                    {
                        decimal duration = db.jntuh_degree.Where(d => d.id == item.degreeID).Select(d => d.degreeDuration).FirstOrDefault();
                        if (duration == 2)
                        {
                            pgAppreared1 += item.appearedStudents1;
                            pgAppreared2 += item.appearedStudents2;
                            pgPassed1 += item.passedStudents1;
                            pgPassed2 += item.passedStudents2;
                        }
                        if (duration == 3)
                        {
                            pgAppreared1 += item.appearedStudents1;
                            pgAppreared2 += item.appearedStudents2;
                            pgAppreared3 += item.appearedStudents3;
                            pgPassed1 += item.passedStudents1;
                            pgPassed2 += item.passedStudents2;
                            pgPassed3 += item.passedStudents3;
                        }
                        //pgPercentage1 += item.passPercentage1;
                        //pgPercentage2 += item.passPercentage2;
                        //pgPercentage3 += item.passPercentage3;
                    }

                }
            }
            else
            {
                for (int i = 1; i <= 10; i++)
                {
                    academicPerformace += "<td colspan='2' width='40'><p align='center'>" + i + "</p></td>";
                    academicPerformace += "<td colspan='3' style='font-size: 9px;' width='78' align='left'>##ACADEMICDEGREE" + i + "##</td>";
                    academicPerformace += "<td colspan='4' style='font-size: 9px;' width='59' align='left'>##ACADEMICDEPARTMENT" + i + "##</td>";
                    academicPerformace += "<td colspan='5' style='font-size: 9px;' width='148' align='left'>##ACADEMICSPECIALIZATION" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='42' align='center'>##ACADEMICSHIFT" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##FOURTHAPPEARED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##FOURTHPASSED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>##FOURTHPERCENTAGE" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##THIRDAPPEARED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##THIRDPASSED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>##THIRDPERCENTAGE" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##SECONDAPPEARED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##SECONDPASSED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='48' align='center'>##SECONDPERCENTAGE" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##FIRSTAPPEARED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='60' align='center'>##FIRSTPASSED" + i + "##</td>";
                    academicPerformace += "<td colspan='2' width='49' align='center'>##FIRSTPERCENTAGE" + i + "##</td>";
                }

                foreach (var item in collegeAcademicPerformance)
                {
                    item.appearedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 1));
                    item.passedStudents1 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 2));
                    item.passPercentage1 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 1, 3));

                    item.appearedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 1));
                    item.passedStudents2 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 2));
                    item.passPercentage2 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 2, 3));

                    item.appearedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 1));
                    item.passedStudents3 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 2));
                    item.passPercentage3 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 3, 3));

                    item.appearedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 1));
                    item.passedStudents4 = Convert.ToInt32(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 2));
                    item.passPercentage4 = Convert.ToDecimal(GetDetails(collegeId, AYID, item.specializationId, item.shiftId, 4, 3));

                    appread1 += item.appearedStudents1;
                    appread2 += item.appearedStudents2;
                    appread3 += item.appearedStudents3;
                    appread4 += item.appearedStudents4;
                    passed1 += item.passedStudents1;
                    passed2 += item.passedStudents2;
                    passed3 += item.passedStudents3;
                    passed4 += item.passedStudents4;
                    //percentage1 += item.passPercentage1;
                    //percentage2 += item.passPercentage2;
                    //percentage3 += item.passPercentage3;
                    //percentage4 += item.passPercentage4;

                    academicPerformace = academicPerformace.Replace("##ACADEMICDEGREE" + count + "##", item.Degree);
                    academicPerformace = academicPerformace.Replace("##ACADEMICDEPARTMENT" + count + "##", item.Department);
                    academicPerformace = academicPerformace.Replace("##ACADEMICSPECIALIZATION" + count + "##", item.Specialization);
                    academicPerformace = academicPerformace.Replace("##ACADEMICSHIFT" + count + "##", item.Shift);
                    academicPerformace = academicPerformace.Replace("##FOURTHAPPEARED" + count + "##", item.appearedStudents4.ToString());
                    academicPerformace = academicPerformace.Replace("##FOURTHPASSED" + count + "##", item.passedStudents4.ToString());
                    academicPerformace = academicPerformace.Replace("##FOURTHPERCENTAGE" + count + "##", item.passPercentage4.ToString());
                    academicPerformace = academicPerformace.Replace("##THIRDAPPEARED" + count + "##", item.appearedStudents3.ToString());
                    academicPerformace = academicPerformace.Replace("##THIRDPASSED" + count + "##", item.passedStudents3.ToString());
                    academicPerformace = academicPerformace.Replace("##THIRDPERCENTAGE" + count + "##", item.passPercentage3.ToString());
                    academicPerformace = academicPerformace.Replace("##SECONDAPPEARED" + count + "##", item.appearedStudents2.ToString());
                    academicPerformace = academicPerformace.Replace("##SECONDPASSED" + count + "##", item.passedStudents2.ToString());
                    academicPerformace = academicPerformace.Replace("##SECONDPERCENTAGE" + count + "##", item.passPercentage2.ToString());
                    academicPerformace = academicPerformace.Replace("##FIRSTAPPEARED" + count + "##", item.appearedStudents1.ToString());
                    academicPerformace = academicPerformace.Replace("##FIRSTPASSED" + count + "##", item.passedStudents1.ToString());
                    academicPerformace = academicPerformace.Replace("##FIRSTPERCENTAGE" + count + "##", item.passPercentage1.ToString());
                    count++;

                    degreeTypeId = db.jntuh_degree.Where(degree => degree.id == item.degreeID).Select(degree => degree.degreeTypeId).FirstOrDefault();

                    degreeType = db.jntuh_degree_type.Where(degree => degree.id == degreeTypeId).Select(degree => degree.degreeType).FirstOrDefault();

                    if (degreeType.Trim().ToUpper() == "UG")
                    {
                        ugAppeared1 += item.appearedStudents1;
                        ugAppeared2 += item.appearedStudents2;
                        ugAppeared3 += item.appearedStudents3;
                        ugAppeared4 += item.appearedStudents4;

                        ugPassed1 += item.passedStudents1;
                        ugPassed2 += item.passedStudents2;
                        ugPassed3 += item.passedStudents3;
                        ugPassed4 += item.passedStudents4;
                        //ugPercentage1 += item.passPercentage1;
                        //ugPercentage2 += item.passPercentage2;
                        //ugPercentage3 += item.passPercentage3;
                        //ugPercentage4 += item.passPercentage4;
                    }
                    if (degreeType.Trim().ToUpper() == "PG")
                    {
                        decimal duration = db.jntuh_degree.Where(d => d.id == item.degreeID).Select(d => d.degreeDuration).FirstOrDefault();
                        if (duration == 2)
                        {
                            pgAppreared1 += item.appearedStudents1;
                            pgAppreared2 += item.appearedStudents2;
                            pgPassed1 += item.passedStudents1;
                            pgPassed2 += item.passedStudents2;
                        }
                        if (duration == 3)
                        {
                            pgAppreared1 += item.appearedStudents1;
                            pgAppreared2 += item.appearedStudents2;
                            pgAppreared3 += item.appearedStudents3;
                            pgPassed1 += item.passedStudents1;
                            pgPassed2 += item.passedStudents2;
                            pgPassed3 += item.passedStudents3;
                        }

                        //pgPercentage1 += item.passPercentage1;
                        //pgPercentage2 += item.passPercentage2;
                        //pgPercentage3 += item.passPercentage3;
                    }

                }

                for (int i = 1; i <= 10; i++)
                {
                    academicPerformace = academicPerformace.Replace("##ACADEMICDEGREE" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##ACADEMICDEPARTMENT" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##ACADEMICSPECIALIZATION" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##ACADEMICSHIFT" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FOURTHAPPEARED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FOURTHPASSED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FOURTHPERCENTAGE" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##THIRDAPPEARED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##THIRDPASSED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##THIRDPERCENTAGE" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##SECONDAPPEARED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##SECONDPASSED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##SECONDPERCENTAGE" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FIRSTAPPEARED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FIRSTPASSED" + i + "##", string.Empty);
                    academicPerformace = academicPerformace.Replace("##FIRSTPERCENTAGE" + i + "##", string.Empty);
                }
            }
            if (passed1 != 0 && appread1 != 0)
            {
                percentage1 = (Convert.ToDecimal(passed1) / Convert.ToDecimal(appread1)) * 100;
                percentage1 = Math.Round(percentage1, 2);
            }
            if (passed2 != 0 && appread2 != 0)
            {
                percentage2 = (Convert.ToDecimal(passed2) / Convert.ToDecimal(appread2)) * 100;
                percentage2 = Math.Round(percentage2, 2);
            }
            if (passed3 != 0 && appread3 != 0)
            {
                percentage3 = (Convert.ToDecimal(passed3) / Convert.ToDecimal(appread3)) * 100;
                percentage3 = Math.Round(percentage3, 2);
            }
            if (passed4 != 0 && appread4 != 0)
            {
                percentage4 = (Convert.ToDecimal(passed4) / Convert.ToDecimal(appread4)) * 100;
                percentage4 = Math.Round(percentage4, 2);
            }
            if (ugPassed1 != 0 && ugAppeared1 != 0)
            {
                UGpercentage1 = (Convert.ToDecimal(ugPassed1) / Convert.ToDecimal(ugAppeared1)) * 100;
                UGpercentage1 = Math.Round(UGpercentage1, 2);
            }
            if (ugPassed2 != 0 && ugAppeared2 != 0)
            {
                UGpercentage2 = (Convert.ToDecimal(ugPassed2) / Convert.ToDecimal(ugAppeared2)) * 100;
                UGpercentage2 = Math.Round(UGpercentage2, 2);
            }
            if (ugPassed3 != 0 && ugAppeared3 != 0)
            {
                UGpercentage3 = (Convert.ToDecimal(ugPassed3) / Convert.ToDecimal(ugAppeared3)) * 100;
                UGpercentage3 = Math.Round(UGpercentage3, 2);
            }
            if (ugPassed4 != 0 && ugAppeared4 != 0)
            {
                UGpercentage4 = (Convert.ToDecimal(ugPassed4) / Convert.ToDecimal(ugAppeared4)) * 100;
                UGpercentage4 = Math.Round(UGpercentage4, 2);
            }
            if (pgPassed1 != 0 && pgAppreared1 != 0)
            {
                PGpercentage1 = (Convert.ToDecimal(pgPassed1) / Convert.ToDecimal(pgAppreared1)) * 100;
                PGpercentage1 = Math.Round(PGpercentage1, 2);
            }
            if (pgPassed2 != 0 && pgAppreared2 != 0)
            {
                PGpercentage2 = (Convert.ToDecimal(pgPassed2) / Convert.ToDecimal(pgAppreared2)) * 100;
                PGpercentage2 = Math.Round(PGpercentage2, 2);
            }
            if (pgPassed3 != 0 && pgAppreared3 != 0)
            {
                PGpercentage3 = (Convert.ToDecimal(pgPassed3) / Convert.ToDecimal(pgAppreared3)) * 100;
                PGpercentage3 = Math.Round(PGpercentage3, 2);
            }

            contents = contents.Replace("##ACADEMICAPPREAD4##", appread4.ToString());
            contents = contents.Replace("##ACADEMICAPPREAD3##", appread3.ToString());
            contents = contents.Replace("##ACADEMICAPPREAD2##", appread2.ToString());
            contents = contents.Replace("##ACADEMICAPPREAD1##", appread1.ToString());
            contents = contents.Replace("##ACADEMICPASSED4##", passed4.ToString());
            contents = contents.Replace("##ACADEMICPASSED3##", passed3.ToString());
            contents = contents.Replace("##ACADEMICPASSED2##", passed2.ToString());
            contents = contents.Replace("##ACADEMICPASSED1##", passed1.ToString());
            contents = contents.Replace("##ACADEMICPERCENTAGE4##", percentage4.ToString());
            contents = contents.Replace("##ACADEMICPERCENTAGE3##", percentage3.ToString());
            contents = contents.Replace("##ACADEMICPERCENTAGE2##", percentage2.ToString());
            contents = contents.Replace("##ACADEMICPERCENTAGE1##", percentage1.ToString());
            //contents = contents.Replace("##ACADEMICFIRSTYEARUGPERCENTAGE##", ugPercentage1.ToString());
            //contents = contents.Replace("##ACADEMICFIRSTYEARPGPERCENTAGE##", pgPercentage1.ToString());
            //contents = contents.Replace("##ACADEMICSECONDYEARUGPERCENTAGE##", ugPercentage2.ToString());
            //contents = contents.Replace("##ACADEMICTHIRDYEARUGPERCENTAGE##", ugPercentage3.ToString());
            //contents = contents.Replace("##ACADEMICFOURTHYEARUGPERCENTAGE##", ugPercentage4.ToString());
            //contents = contents.Replace("##ACADEMICSECONDPGPERCENTAGE##", pgPercentage2.ToString());
            //contents = contents.Replace("##ACADEMICTHIRDYEARPGPERCENTAGE##", pgPercentage3.ToString());
            contents = contents.Replace("##ACADEMICFIRSTYEARUGPERCENTAGE##", UGpercentage1.ToString());
            contents = contents.Replace("##ACADEMICFIRSTYEARPGPERCENTAGE##", PGpercentage1.ToString());
            contents = contents.Replace("##ACADEMICSECONDYEARUGPERCENTAGE##", UGpercentage2.ToString());
            contents = contents.Replace("##ACADEMICTHIRDYEARUGPERCENTAGE##", UGpercentage3.ToString());
            contents = contents.Replace("##ACADEMICFOURTHYEARUGPERCENTAGE##", UGpercentage4.ToString());
            contents = contents.Replace("##ACADEMICSECONDPGPERCENTAGE##", PGpercentage2.ToString());
            contents = contents.Replace("##ACADEMICTHIRDYEARPGPERCENTAGE##", PGpercentage3.ToString());
            contents = contents.Replace("##ACADEMICPERFORMANCEINFORMATION##", academicPerformace);
            return contents;
        }

        private string GetDetails(int collegeId, int academicYearId, int specializationId, int shiftId, int yearInDegree, int flag)
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

        private string collegeIntakeProposed(int collegeId, string contents)
        {
            string intakeProposed = string.Empty;
            int count = 1;
            int totalIntakeProposed = 0;
            int totalIntakeExist = 0;
            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.collegeId = item.collegeId;
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                newProposed.ExistingIntake = db.jntuh_college_intake_existing.Where(ei => ei.specializationId == item.specializationId &&
                                                                             ei.shiftId == item.shiftId &&
                                                                             ei.collegeId == collegeId &&
                                                                             ei.academicYearId == academicYearId)
                                                                        .Select(ei => ei.approvedIntake).FirstOrDefault();
                collegeIntakeProposedList.Add(newProposed);
            }

            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            if (collegeIntakeProposedList.Count > 15)
            {
                foreach (var item in collegeIntakeProposedList)
                {
                    string rowBoldStart = string.Empty;
                    string rowBoldEnd = string.Empty;

                    string intakeBoldStart = string.Empty;
                    string intakeBoldEnd = string.Empty;

                    if (item.ExistingIntake == 0)
                    {
                        rowBoldStart = "<b>";
                        rowBoldEnd = "</b>";
                    }

                    if (item.ExistingIntake > item.proposedIntake || item.ExistingIntake < item.proposedIntake || item.proposedIntake == 0)
                    {
                        intakeBoldStart = "<b>";
                        intakeBoldEnd = "</b>";
                    }

                    intakeProposed += "<td colspan='2' width='45'><p align='center'>" + rowBoldStart + count + rowBoldEnd + "</p></td>";
                    intakeProposed += "<td colspan='3' style='font-size: 9px;' width='96'>" + rowBoldStart + item.Degree + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='4' style='font-size: 9px;' width='74'>" + rowBoldStart + item.Department + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='7' style='font-size: 9px;' width='196'>" + rowBoldStart + item.Specialization + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='2' width='41' align='center'>" + rowBoldStart + item.Shift + rowBoldEnd + "</td>";
                    //intakeProposed += "<td colspan='3' width='71' align='center'>" + rowBoldStart + item.CourseAffiliationStatusCode + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='3' width='78' align='center'>" + rowBoldStart + item.ExistingIntake.ToString() + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='3' width='90' align='center'>" + rowBoldStart + intakeBoldStart + item.proposedIntake.ToString() + intakeBoldEnd + rowBoldEnd + "</td>";
                    totalIntakeProposed += item.proposedIntake;

                    totalIntakeExist += item.ExistingIntake;

                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 15; i++)
                {
                    intakeProposed += "<td colspan='2' width='45'><p align='center'>" + i + "</p></td>";
                    intakeProposed += "<td colspan='3' style='font-size: 9px;' width='96'>##COLLEGEPROPOSEDINTAKEDEGREE" + i + "##</td>";
                    intakeProposed += "<td colspan='4' style='font-size: 9px;' width='74'>##COLLEGEPROPOSEDINTAKEDEPARTMENT" + i + "##</td>";
                    intakeProposed += "<td colspan='7' style='font-size: 9px;' width='196'>##COLLEGEPROPOSEDINTAKESPECIALIZATION" + i + "##</td>";
                    intakeProposed += "<td colspan='2' width='41' align='center'>##COLLEGEPROPOSEDINTAKESHIFT" + i + "##</td>";
                    //intakeProposed += "<td colspan='3' width='71' align='center'>##COLLEGEPROPOSEDINTAKECOURSESTSATUS" + i + "##</td>";
                    intakeProposed += "<td colspan='3' width='78' align='center'>##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + i + "##</td>";
                    intakeProposed += "<td colspan='3' width='90' align='center'>##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + i + "##</td>";
                }

                foreach (var item in collegeIntakeProposedList)
                {
                    //if (item.ExistingIntake != 0)
                    //{
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEGREE" + count + "##", item.Degree);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEPARTMENT" + count + "##", item.Department);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESPECIALIZATION" + count + "##", item.Specialization);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESHIFT" + count + "##", item.Shift);
                    //intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKECOURSESTSATUS" + count + "##", item.CourseAffiliationStatusCode);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + count + "##", item.ExistingIntake.ToString());
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + count + "##", item.proposedIntake.ToString());
                    totalIntakeProposed += item.proposedIntake;
                    //if (item.proposedIntake > item.ExistingIntake)
                    //{
                    //    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + count + "##", item.ExistingIntake.ToString());
                    //    totalIntakeProposed += item.ExistingIntake;
                    //}
                    //else
                    //{
                    //    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + count + "##", item.proposedIntake.ToString());
                    //    totalIntakeProposed += item.proposedIntake;
                    //}
                    totalIntakeExist += item.ExistingIntake;

                    count++;
                    //}
                }

                for (int i = 1; i <= 15; i++)
                {
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEGREE" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEPARTMENT" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESPECIALIZATION" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESHIFT" + i + "##", string.Empty);
                    //intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKECOURSESTSATUS" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + i + "##", string.Empty);
                }

            }
            contents = contents.Replace("##COLLEGEINTAKEPROPOSEDINFORMATION##", intakeProposed);
            contents = contents.Replace("##COLLEGETOTALINTAKEEXISTING##", totalIntakeExist.ToString());
            contents = contents.Replace("##COLLEGETOTALINTAKEPROPOSED##", totalIntakeProposed.ToString());
            return contents;
        }

        public string PGCourses(int collegeId, string contents)
        {
            List<jntuh_college_pgcourses> pgCourses = db.jntuh_college_pgcourses.Where(p => p.collegeId == collegeId && p.isActive == true).OrderBy(p => new { p.jntuh_department.jntuh_degree.degreeDisplayOrder, p.jntuh_specialization.specializationName, p.shiftId }).ToList();

            string pg_courses = string.Empty;

            int courseno = 1;

            foreach (var course in pgCourses)
            {
                var PGCourseDetails = db.jntuh_college_pgcourses.Where(p => p.isActive == true && p.id == course.id).Select(p => p).FirstOrDefault();

                CollegePGCourse collegePGCourse = new CollegePGCourse();

                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);



                if (PGCourseDetails != null)
                {
                    collegePGCourse.id = PGCourseDetails.id;
                    collegePGCourse.collegeId = PGCourseDetails.collegeId;
                    collegePGCourse.jntuh_college = db.jntuh_college.Where(c => c.isActive == true && c.id == PGCourseDetails.collegeId).Select(c => c).FirstOrDefault();
                    var degreeDetails = (from s in db.jntuh_specialization
                                         join de in db.jntuh_department on s.departmentId equals de.id
                                         join d in db.jntuh_degree on de.degreeId equals d.id
                                         where (s.isActive == true && de.isActive == true && d.isActive == true && s.id == PGCourseDetails.specializationId && de.id == PGCourseDetails.departmentId)
                                         select new
                                         {
                                             d.id,
                                             d.degree
                                         }).FirstOrDefault();
                    collegePGCourse.degree = degreeDetails.degree;
                    collegePGCourse.departmentId = PGCourseDetails.departmentId;
                    collegePGCourse.jntuh_department = db.jntuh_department.Where(d => d.isActive == true && d.id == PGCourseDetails.departmentId).Select(d => d).FirstOrDefault();
                    collegePGCourse.specializationId = PGCourseDetails.specializationId;
                    collegePGCourse.jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true && s.id == PGCourseDetails.specializationId).Select(s => s).FirstOrDefault();
                    collegePGCourse.shiftId = PGCourseDetails.shiftId;
                    collegePGCourse.intake = PGCourseDetails.intake;
                    collegePGCourse.professors = PGCourseDetails.professors;
                    collegePGCourse.associateProfessors = PGCourseDetails.associateProfessors;
                    collegePGCourse.assistantProfessors = PGCourseDetails.assistantProfessors;
                    collegePGCourse.UGFacultyStudentRatio = PGCourseDetails.UGFacultyStudentRatio;
                    collegePGCourse.isActive = true;
                    collegePGCourse.createdOn = DateTime.Now;
                    collegePGCourse.createdBy = userID;
                    collegePGCourse.type = PGCourseDetails.Type;
                    collegePGCourse.PGFaculty = PGCourseFacultyList(PGCourseDetails.id);

                    if (collegePGCourse.shiftId == 1)
                    {
                        pg_courses += "<b style='font-size: 9px; text-transform: uppercase;'>" + courseno + ". " + collegePGCourse.degree + " - " + collegePGCourse.jntuh_specialization.specializationName + "</b><br /><br />";
                    }
                    else
                    {
                        pg_courses += "<b style='font-size: 9px; text-transform: uppercase;'>" + courseno + ". " + collegePGCourse.degree + " - " + collegePGCourse.jntuh_specialization.specializationName + " - " + collegePGCourse.shiftId + "</b><br /><br />";
                    }

                    //pg_courses += "<table border='1' cellpadding='5' cellspacing='0'  style='font-size: 9px;'>";                   
                    //pg_courses += "<tr><th colspan='3'>Name of the College</th><td colspan='5'>" + collegePGCourse.jntuh_college.collegeName + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>College Code</th><td colspan='5'>" + collegePGCourse.jntuh_college.collegeCode + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Degree</th><td colspan='5'>" + collegePGCourse.degree + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Department</th><td colspan='5'>" + collegePGCourse.jntuh_department.departmentName + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Name of the PG Programme</th><td colspan='5'>" + collegePGCourse.jntuh_specialization.specializationName + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Program Type [ New / Existing ]</th><td colspan='5'>" + collegePGCourse.type + " Program </td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Shift</th><td colspan='5'>" + collegePGCourse.shiftId + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>Intake</th><td colspan='5'>" + collegePGCourse.intake + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>No of Professors</th><td colspan='5'>" + collegePGCourse.professors + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>No of Associate Professors</th><td colspan='5'>" + collegePGCourse.associateProfessors + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>No of Assistant Professors</th><td colspan='5'>" + collegePGCourse.assistantProfessors + "</td></tr>";
                    //pg_courses += "<tr><th colspan='3'>UG Faculty/Student Ratio of the Department</th><td colspan='5'>" + collegePGCourse.UGFacultyStudentRatio + "</td></tr>";
                    //pg_courses += "</table>";

                    //pg_courses += "<b style='font-size: 10px; text-transform: uppercase;'>Details of PG faculty</b><br /><br />";
                    //pg_courses += "<table border='1' cellpadding='4' cellspacing='0' style='font-size: 9px;'>";
                    //pg_courses += "<tr><th colspan='1' align='center'>S.No</th><th colspan='6'>Name of the Faculty</th><th colspan='4'>Designation</th><th colspan='4'>Qualification</th><th colspan='4'>Specialization</th></tr>";
                    pg_courses += "<table border='1' cellpadding='5' cellspacing='0'  style='font-size: 9px;'>";
                    pg_courses += "<tr><th colspan='2'>Intake</th><th colspan='3'>No of Professors</th><th colspan='3'>No of Associate Professors</th><th colspan='3'>No of Assistant Professors</th><th colspan='4' style='font-size: 8px;'>UG Faculty/Student Ratio of the Department</th></tr>";
                    pg_courses += "<tr><td colspan='2'>" + collegePGCourse.intake + "</td><td colspan='3'>" + collegePGCourse.professors + "</td><td colspan='3'>" + collegePGCourse.associateProfessors + "</td><td colspan='3'>" + collegePGCourse.assistantProfessors + "</td><td colspan='5'>" + collegePGCourse.UGFacultyStudentRatio + "</td></tr>";
                    pg_courses += "</table>";

                    pg_courses += "<b style='font-size: 9px; text-transform: uppercase;'>Details of PG faculty</b><br /><br />";
                    pg_courses += "<table border='1' cellpadding='4' cellspacing='0' style='font-size: 9px;'>";
                    pg_courses += "<tr><th colspan='2' align='center'>S. No</th><th colspan='5'>Name of the Faculty</th><th colspan='3'>Designation</th><th colspan='5'>Qualification</th><th colspan='5'>Specialization</th></tr>";
                    int sno = 1;
                    foreach (var item in collegePGCourse.PGFaculty)
                    {
                        pg_courses += "<tr>";
                        pg_courses += "<td colspan='2' align='center'>" + (sno++) + "</td>";
                        pg_courses += "<td colspan='5' style='font-size: 8px;'>" + item.facultyName + "</td>";
                        pg_courses += "<td colspan='3' style='font-size: 8px;'>" + item.designation + "</td>";
                        pg_courses += "<td colspan='5'>";
                        pg_courses += "<table border='0' cellpadding='2' cellspacing='0' style='font-size: 9px;'><tr><td colspan='2' style='font-size: 8px;'><b>UG</b></td><td colspan='1'>-</td><td colspan='5' style='font-size: 8px;'>" + item.UG + "</td></tr><tr><td colspan='2' style='font-size: 8px;'><b>PG</b></td><td colspan='1'>-</td><td colspan='5' style='font-size: 8px;'>" + item.PG + "</td></tr><tr><td colspan='2' style='font-size: 8px;'><b>Ph.D</b></td><td colspan='1'>-</td><td colspan='5' style='font-size: 8px;'>" + item.Phd + "</td></tr></table>";
                        pg_courses += "</td>";
                        pg_courses += "<td colspan='5'>";
                        pg_courses += "<table border='0' cellpadding='2' cellspacing='0' style='font-size: 9px;'><tr><td colspan='1'>-</td><td colspan='6' style='font-size: 8px;'>" + item.UGSpecialization + "</td></tr><tr><td colspan='1'>-</td><td colspan='6' style='font-size: 8px;'>" + item.PGSpecialization + "</td></tr><tr><td colspan='1'>-</td><td colspan='6' style='font-size: 8px;'>" + item.PhdSpecialization + "</td></tr></table>";
                        pg_courses += "</td>";
                        pg_courses += "</tr>";
                    }
                    pg_courses += "</table>";
                    pg_courses += "<br />";
                    courseno++;
                }
            }

            contents = contents.Replace("##PGCOURSES##", pg_courses);

            return contents;
        }

        private List<CollegePGCourseFaculty> PGCourseFacultyList(int id)
        {
            List<CollegePGCourseFaculty> collegePGCourseFaculty = new List<CollegePGCourseFaculty>();

            var PGCourseFacultyList = db.jntuh_college_pgcourse_faculty.Where(pf => pf.isActive == true && pf.courseId == id).ToList();
            if (PGCourseFacultyList.Count() > 0)
            {
                foreach (var item in PGCourseFacultyList)
                {
                    CollegePGCourseFaculty pgcoursefaculty = new CollegePGCourseFaculty();
                    pgcoursefaculty.courseId = item.courseId;
                    pgcoursefaculty.facultyName = item.facultyName;
                    pgcoursefaculty.designation = item.designation;
                    pgcoursefaculty.UG = item.UG;
                    pgcoursefaculty.PG = item.PG;
                    pgcoursefaculty.Phd = item.Phd;
                    pgcoursefaculty.UGSpecialization = item.UGSpecialization;
                    pgcoursefaculty.PGSpecialization = item.PGSpecialization;
                    pgcoursefaculty.PhdSpecialization = item.PhdSpecialization;
                    collegePGCourseFaculty.Add(pgcoursefaculty);
                }
            }

            return collegePGCourseFaculty;
        }


        private string collegeNonTachingFacultyMembers(int collegeId, string contents)
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

            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
            collegeFaculty += "<td colspan='5'><p p align='left'>Faculty Name</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Qualification </p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Experience</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Date of Appointment</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Scale of Pay</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Net Salary</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Bank Name</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Branch Name</p></td>";
            //collegeFaculty += "<td colspan='1'><p align='center'>Ratified</p></td>";
            //collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
            collegeFaculty += "</tr>";

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
                designation = db.jntuh_designation.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
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
                collegeFaculty += "<td colspan='5'><p p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";
                //collegeFaculty += "<td colspan='3'><p align='left'>" + department + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPayScale + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.netSalary + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBankName + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBranchName + "</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>" + ratified + "</p></td>";                
                //if (!string.IsNullOrEmpty(item.facultyPhoto))
                //{                    
                //   // string strFacultyPhoto = "<img src='http://112.133.193.228:75'" + item.facultyPhoto+ "'" + " align='center' height='50' />";
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
        private string collegeTechnicalFacultyMembers(int collegeId, string contents)
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


            contents = contents.Replace("##COLLEGETechnicalFaculty##", collegeFaculty);
            return contents;
        }

        private string collegeTachingFacultyMembers(int collegeId, string contents)
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
            string[] strRegNoS = db.jntuh_college_faculty_registered.Where(rf => rf.collegeId == collegeId).Select(rf => rf.RegistrationNumber).ToArray();
            var facultyList = db.jntuh_registered_faculty.Where(f => strRegNoS.Contains(f.RegistrationNumber)).OrderBy(faculty => faculty.DepartmentId)
                                     .ThenBy(faculty => faculty.DesignationId)
                                     .ThenBy(faculty => faculty.FirstName).Select(f => f).ToList();

            var DeptIDs = facultyList.Where(a => a.DepartmentId != null).Select(d => d.DepartmentId).Distinct().ToList();

            var type = facultyList.Where(f => f.type == "NewFaculty").ToList();
            if (DeptIDs.Count() > 0)
            {
                foreach (var deptId in DeptIDs)
                {
                    collegeFaculty += TeachingFaculty(facultyList.Where(g => g.DepartmentId == deptId).ToList());
                }
            }

            if (type.Count() > 0)
            {
                collegeFaculty += TeachingFaculty(facultyList.Where(g => g.DepartmentId == null).ToList());
            }
            //contents += TeachingFaculty(null, facultyList.ToList());
            contents = contents.Replace("##COLLEGETeachingFaculty##", collegeFaculty);

            return contents;
        }

        private string TechnicalFaculty(List<jntuh_college_faculty> facultyList, int teachingFacultyTypeId)
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

            collegeFaculty += "<strong><u>" + department + " Faculty</u></strong> <br /> <br />";
            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
            collegeFaculty += "<td colspan='5'><p p align='left'>Faculty Name</p></td>";
            collegeFaculty += "<td colspan='1'><p align='center'>Gender</p></td>";
            //collegeFaculty += "<td colspan='3'><p align='left'>Department</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Qualification </p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Experience</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Date of Appointment</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Scale of Pay</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Net Salary</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Bank Name</p></td>";
            //collegeFaculty += "<td colspan='2'><p align='center'>Branch Name</p></td>";
            //collegeFaculty += "<td colspan='1'><p align='center'>Ratified</p></td>";
            //collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
            collegeFaculty += "</tr>";

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
                department = db.jntuh_department.Where(d => d.id == item.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                designation = db.jntuh_designation.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
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
                collegeFaculty += "<td colspan='5'><p p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + gender + "</p></td>";
                //collegeFaculty += "<td colspan='3'><p align='left'>" + department + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.facultyPayScale + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.grossSalary + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.netSalary + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBankName + "</p></td>";
                //collegeFaculty += "<td colspan='2'><p align='center'>" + item.salaryBranchName + "</p></td>";
                //collegeFaculty += "<td colspan='1'><p align='center'>" + ratified + "</p></td>";               
                //if (!string.IsNullOrEmpty(item.facultyPhoto))
                //{
                //    //string strFacultyPhoto = "<img src='http://112.133.193.228:75'" + item.facultyPhoto + "'" + " align='center' height='50' />";
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
            collegeFaculty += "</table> <br /> <br />";
            return collegeFaculty;
        }

        private string TeachingFaculty(List<jntuh_registered_faculty> facultyList)
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
            int? deptid = facultyList.FirstOrDefault().DepartmentId;
            if (deptid == null)
            {
                department = "New";
            }
            else
            {
                department = db.jntuh_department.Where(d => d.id == deptid).FirstOrDefault().departmentName;
            }
            collegeFaculty += "<strong><u>" + department + " Faculty</u></strong> <br /> <br />";
            collegeFaculty += "<table border='1' cellspacing='0' cellpadding='5' style='font-size: 9px;'>";
            collegeFaculty += "<tbody>";
            collegeFaculty += "<tr>";
            collegeFaculty += "<td colspan='1'><p align='center'>SNo</p></td>";
            collegeFaculty += "<td colspan='5'><p p align='left'>Faculty Name</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gender</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Designation</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Qualification </p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Specilization</p></td>";
            collegeFaculty += "<td colspan='3'><p align='left'>Identified for UG/PG/UG and PG</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Date of Appointment</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>EXPerience</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Gross Salary</p></td>";
            collegeFaculty += "<td colspan='4'><p align='center'>Registration Number</p></td>";
            collegeFaculty += "<td colspan='3'><p align='center'>Photo</p></td>";
            collegeFaculty += "<td colspan='2'><p align='center'>Ratified as P/ Assoc. P/Asst.p</p></td>";
            collegeFaculty += "</tr>";

            foreach (var item in facultyList)
            {
                if (item.GenderId == 1)
                {
                    gender = "M";
                }
                else
                {
                    gender = "F";
                }

                designation = db.jntuh_designation.Where(d => d.id == item.DesignationId).Select(d => d.designation).FirstOrDefault();
                if (item.DateOfAppointment != null)
                {
                    dateOfAppointment = UAAAS.Models.Utilities.MMDDYY2DDMMYY(item.DateOfAppointment.ToString());
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
                collegeFaculty += "<tr>";
                collegeFaculty += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                collegeFaculty += "<td colspan='5'><p p align='left'>" + (item.FirstName + " " + item.MiddleName + " " + item.LastName).ToUpper() + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + gender + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + designation + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + qualification + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'>" + strSpecialization + "</p></td>";
                collegeFaculty += "<td colspan='3'><p align='left'> </p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + dateOfAppointment + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.TotalExperience.ToString().Replace(".00", "") + "</p></td>";
                collegeFaculty += "<td colspan='2'><p align='center'>" + item.grosssalary + "</p></td>";
                collegeFaculty += "<td colspan='4'><p align='center'>" + item.RegistrationNumber + "</p></td>";
                if (!string.IsNullOrEmpty(item.Photo))
                {
                    string imgPath = "http://112.133.193.228:75/Content/Upload/Faculty/Photos/" + item.Photo;
                    string strFacultyPhoto = "<img src='" + imgPath + "'" + " align='center' height='50' />";
                    collegeFaculty += "<td colspan='3'><p align='center'>" + strFacultyPhoto + "</p></td>";
                    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                }
                else
                {
                    collegeFaculty += "<td colspan='3'><p align='center'></p></td>";
                }
                collegeFaculty += "<td colspan='2'><p align='center'>" + ratified + "</p></td>";
                collegeFaculty += "</tr>";
                count++;
            }
            collegeFaculty += "</tbody>";
            collegeFaculty += "</table> <br /> <br />";
            return collegeFaculty;
        }

        private string collegeLaboratories(int collegeId, string contents)
        {
            string strLaboratories = string.Empty;
            int count = 1;
            List<jntuh_college_lab> labs = db.jntuh_college_lab.Where(l => l.collegeId == collegeId).ToList();

            List<Laboratories> laboratories = new List<Laboratories>();

            foreach (var item in labs)
            {
                Laboratories newLab = new Laboratories();
                newLab.id = item.id;
                newLab.collegeId = item.collegeId;
                newLab.labName = item.labName;
                newLab.totalExperiments = item.totalExperiments;
                newLab.labFloorArea = item.labFloorArea;
                newLab.specializationId = item.specializationId;
                newLab.shiftId = item.shiftId;
                newLab.specializationName = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newLab.departmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newLab.department = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.departmentName).FirstOrDefault();
                newLab.degreeId = db.jntuh_department.Where(d => d.id == newLab.departmentId).Select(d => d.degreeId).FirstOrDefault();
                newLab.degree = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degree).FirstOrDefault();
                newLab.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newLab.degreeId).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newLab.shiftName = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newLab.year = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
                newLab.yearInDegreeId = item.yearInDegreeId;
                newLab.IsShared = (bool)item.isShared;
                laboratories.Add(newLab);
            }
            laboratories = laboratories.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specializationName).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.year).ToList();
            if (laboratories.Count > 15)
            {
                foreach (var item in laboratories)
                {
                    strLaboratories += "<td colspan='2' width='33'><p align='center'>" + count + "</p></td>";
                    strLaboratories += "<td colspan='3' style='font-size: 9px;' width='64'>" + item.degree + "</td>";
                    strLaboratories += "<td colspan='5' style='font-size: 9px;' width='72'>" + item.department + "</td>";
                    strLaboratories += "<td colspan='6' style='font-size: 9px;' width='264'>" + item.specializationName + "</td>";
                    strLaboratories += "<td colspan='2' width='60' align='center'>" + item.shiftName + "</td>";
                    strLaboratories += "<td colspan='2' width='48' align='center'>" + item.year + "</td>";
                    strLaboratories += "<td colspan='9' width='264'>" + item.labName + "</td>";
                    strLaboratories += "<td colspan='3' align='center'>" + (item.IsShared.Equals(true) ? "Yes" : "No") + "</td>";
                    strLaboratories += "<td colspan='3' width='84'>" + item.totalExperiments.ToString() + "</td>";
                    strLaboratories += "<td colspan='3' width='84'>&nbsp;</td>";
                    strLaboratories += "<td colspan='3' width='78'>" + item.labFloorArea.ToString() + "</td>";
                    strLaboratories += "<td colspan='3' width='84'>&nbsp;</td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 15; i++)
                {
                    strLaboratories += "<td colspan='2' width='33'><p align='center'>" + i + "</p></td>";
                    strLaboratories += "<td colspan='3' style='font-size: 9px;' width='64'>##COLLEGELABSDEGREE" + i + "##</td>";
                    strLaboratories += "<td colspan='5' style='font-size: 9px;' width='72'>##COLLEGELABSDEPARTMENT" + i + "##</td>";
                    strLaboratories += "<td colspan='6' style='font-size: 9px;' width='264'>##COLLEGELABSSPECIALIZATION" + i + "##</td>";
                    strLaboratories += "<td colspan='2' width='60'>##COLLEGELABSSHIFT" + i + "##</td>";
                    strLaboratories += "<td colspan='2' width='48'>##COLLEGELABSYEAR" + i + "##</td>";
                    strLaboratories += "<td colspan='9' width='264'>##COLLEGELABSNAME" + i + "##</td>";
                    strLaboratories += "<td colspan='3' >##ISSHARED" + i + "##</td>";
                    strLaboratories += "<td colspan='3' width='84'>##COLLEGELABSEXPERIMENTS" + i + "##</td>";
                    strLaboratories += "<td colspan='3' width='84'>&nbsp;</td>";
                    strLaboratories += "<td colspan='3' width='78'>##COLLEGELABSFLOORAREA" + i + "##</td>";
                    strLaboratories += "<td colspan='3' width='84'>&nbsp;</td>";
                }
                foreach (var item in laboratories)
                {
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSDEGREE" + count + "##", item.degree);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSDEPARTMENT" + count + "##", item.department);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSSPECIALIZATION" + count + "##", item.specializationName);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSSHIFT" + count + "##", item.shiftName);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSYEAR" + count + "##", item.year);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSNAME" + count + "##", item.labName);
                    strLaboratories = strLaboratories.Replace("##ISSHARED" + count + "##", (item.IsShared.Equals(true) ? "Yes" : "No"));
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSEXPERIMENTS" + count + "##", item.totalExperiments.ToString());
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSFLOORAREA" + count + "##", item.labFloorArea.ToString());
                    count++;
                }
                for (int i = 1; i <= 15; i++)
                {
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSDEGREE" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSDEPARTMENT" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSSPECIALIZATION" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSSHIFT" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSYEAR" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSNAME" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##ISSHARED" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSEXPERIMENTS" + i + "##", string.Empty);
                    strLaboratories = strLaboratories.Replace("##COLLEGELABSFLOORAREA" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##COLLEGELABORATORIESINFORMATION##", strLaboratories);
            return contents;
        }

        private string collegeLibrary(int collegeId, string contents)
        {
            jntuh_college_library libaryInformation = db.jntuh_college_library.Where(library => library.collegeId == collegeId).FirstOrDefault();
            if (libaryInformation != null)
            {
                contents = contents.Replace("##LIBRARYINFORAMATIONNAMEOFLIBRARIAN##", libaryInformation.librarianName);
                contents = contents.Replace("##LIBRARYINFORAMATIONQUALIFICATION##", libaryInformation.librarianQualifications);
                contents = contents.Replace("##LIBRARYINFORAMATIONPHONENUMBER##", libaryInformation.libraryPhoneNumber);
                contents = contents.Replace("##LIBRARYINFORAMATIONSUPPORTINGSTAFF##", libaryInformation.totalSupportingStaff.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONTITLES##", libaryInformation.totalTitles.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONVOLUMES##", libaryInformation.totalVolumes.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONJOURNALS##", libaryInformation.totalNationalJournals.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONNATIONALJOURNALS##", libaryInformation.totalInternationalJournals.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONEJOURNALS##", libaryInformation.totalEJournals.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONSEATINGCAPACITY##", libaryInformation.librarySeatingCapacity.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONHOURSFROM##", libaryInformation.libraryWorkingHoursFrom.ToString());
                contents = contents.Replace("##LIBRARYINFORAMATIONHOURSTO##", libaryInformation.libraryWorkingHoursTo.ToString());
            }
            else
            {
                contents = contents.Replace("##LIBRARYINFORAMATIONNAMEOFLIBRARIAN##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONQUALIFICATION##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONPHONENUMBER##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONSUPPORTINGSTAFF##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONTITLES##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONVOLUMES##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONJOURNALS##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONNATIONALJOURNALS##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONEJOURNALS##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONSEATINGCAPACITY##", string.Empty);
                contents = contents.Replace("##LIBRARYINFORAMATIONHOURSFROM##", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                contents = contents.Replace("##LIBRARYINFORAMATIONHOURSTO##", "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
            }

            string libraryInformation = string.Empty;
            List<LibraryDetails> libraryDetails = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder)
                                                                 .Where(degree => degree.isActive == true)
                                                                 .Select(degree => new LibraryDetails
                                                                 {
                                                                     degreeId = degree.id,
                                                                     degree = degree.degree,
                                                                     totalTitles = null,
                                                                     totalVolumes = null,
                                                                     totalNationalJournals = null,
                                                                     totalInternationalJournals = null,
                                                                     totalEJournals = null
                                                                 }).ToList();
            foreach (var item in libraryDetails)
            {
                item.totalTitles = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary.totalTitles)
                                                                   .FirstOrDefault();
                item.totalVolumes = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary.totalVolumes)
                                                                   .FirstOrDefault();
                item.totalNationalJournals = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary.totalNationalJournals)
                                                                   .FirstOrDefault();
                item.totalInternationalJournals = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary.totalInternationalJournals)
                                                                   .FirstOrDefault();
                item.totalEJournals = db.jntuh_college_library_details.Where(collegeLibrary => collegeLibrary.collegeId == collegeId &&
                                                                          collegeLibrary.degreeId == item.degreeId)
                                                                   .Select(collegeLibrary => collegeLibrary.totalEJournals)
                                                                   .FirstOrDefault();
                libraryInformation += "<td colspan='2' style='font-size: 8px;'><p align='left'>" + item.degree + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.totalTitles + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.totalVolumes + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.totalNationalJournals + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.totalInternationalJournals + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>" + item.totalEJournals + "</p></td>";
                libraryInformation += "<td colspan='1' style='font-size: 8px;'><p align='center'>&nbsp;</p></td>";
            }
            contents = contents.Replace("##COLLEGELIBRARYINFORAMTIONBOOKSDETAILS##", libraryInformation);
            return contents;
        }

        private string collegeComputers(int collegeId, string contents)
        {
            string computerStudentRatio = string.Empty;
            List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(d => d.isActive == true).ToList();

            List<ComputerStudentRatioDetails> computerStudentDetails = new List<ComputerStudentRatioDetails>();

            foreach (var item in collegeDegree)
            {
                ComputerStudentRatioDetails details = new ComputerStudentRatioDetails();
                details.id = 0;
                details.degreeId = item.id;
                details.degree = item.degree;
                details.totalIntake = GetIntake(item.id, collegeId);
                details.availableComputers = db.jntuh_college_computer_student_ratio.Where(computerStudenRatio => computerStudenRatio.collegeId == collegeId &&
                                                                                        computerStudenRatio.degreeId == item.id)
                                                                                  .Select(computerStudenRatio => computerStudenRatio.availableComputers)
                                                                                  .FirstOrDefault();
                computerStudentDetails.Add(details);
            }

            foreach (var item in computerStudentDetails)
            {
                string norms = db.jntuh_computer_student_ratio_norms.Where(n => n.degreeId == item.degreeId).Select(n => n.Norms).FirstOrDefault();

                computerStudentRatio += "<td colspan='3'><p align='left'>" + item.degree + "</p></td>";
                computerStudentRatio += "<td colspan='3'><p align='center'>" + item.totalIntake + "</td>";
                computerStudentRatio += "<td colspan='3'><p align='center'>" + norms + "</td>";
                computerStudentRatio += "<td colspan='3'><p align='center'>" + item.availableComputers + "</td>";
                computerStudentRatio += "<td colspan='3'><p align='center'>&nbsp;</td>";
                computerStudentRatio += "<td colspan='3'><p align='center'>&nbsp;</td>";
            }
            contents = contents.Replace("##COLLEGECOMPUTERSTUDENTRATIONINFORMATION##", computerStudentRatio);
            return contents;
        }

        private int GetIntake(int degreeId, int collegeId)
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
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId).Select(e => e.shiftId).ToArray();
                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
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

        private string collegeInterBandWidth(int collegeId, string contents)
        {
            string internetBandWidth = string.Empty;
            List<jntuh_degree> collegeDegree = db.jntuh_degree.OrderBy(degree => degree.degreeDisplayOrder).Where(d => d.isActive == true).ToList();

            List<InternetBandwidthDetails> internetDetails = new List<InternetBandwidthDetails>();

            foreach (var item in collegeDegree)
            {
                InternetBandwidthDetails details = new InternetBandwidthDetails();
                details.id = 0;
                details.degreeId = item.id;
                details.degree = db.jntuh_degree.Where(d => d.isActive == true && d.id == item.id).Select(d => d.degree).FirstOrDefault();
                details.totalIntake = GetIntake(item.id, collegeId);
                details.availableInternetSpeed = db.jntuh_college_internet_bandwidth.Where(internetbandwidth => internetbandwidth.collegeId == collegeId &&
                                                                                        internetbandwidth.degreeId == item.id)
                                                                                 .Select(internetbandwidth => internetbandwidth.availableInternetSpeed)
                                                                                 .FirstOrDefault();
                internetDetails.Add(details);
            }
            foreach (var item in internetDetails)
            {
                string norms = db.jntuh_internet_bandwidth_norms.Where(n => n.degreeId == item.degreeId).Select(n => n.Norms).FirstOrDefault();

                internetBandWidth += "<td  colspan='3'><p align='left'>" + item.degree + "</p></td>";
                internetBandWidth += "<td  colspan='3'><p align='center'>" + item.totalIntake + "</td>";
                internetBandWidth += "<td  colspan='4'><p align='center'>" + norms + "</td>";
                internetBandWidth += "<td  colspan='3'><p align='center'>" + item.availableInternetSpeed + "</td>";
                internetBandWidth += "<td  colspan='3'><p align='center'>&nbsp;</td>";
                internetBandWidth += "<td  colspan='3'><p align='center'>&nbsp;</td>";
            }
            contents = contents.Replace("##COLLEGEINTERNETBANDWIDTHINFORAMATION##", internetBandWidth);
            return contents;
        }

        private string collegeLegalSystemSoftware(int collegeId, string contents)
        {
            string legalSystemSoftware = string.Empty;
            List<CollegeLegalSoftwarDetails> legalSoftwarDetails = (from CollegeDegree in db.jntuh_degree
                                                                    orderby CollegeDegree.degreeDisplayOrder
                                                                    where (CollegeDegree.isActive == true)
                                                                    select new CollegeLegalSoftwarDetails
                                                                    {
                                                                        degreeId = CollegeDegree.id,
                                                                        degree = CollegeDegree.degree,
                                                                        availableApplicationSoftware = 0,
                                                                        availableSystemSoftware = 0
                                                                    }).ToList();
            foreach (var item in legalSoftwarDetails)
            {
                item.availableApplicationSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == collegeId &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableApplicationSoftware)
                                                                                   .FirstOrDefault();
                item.availableSystemSoftware = db.jntuh_college_legal_software.Where(legalSoftware => legalSoftware.collegeId == collegeId &&
                                                                                          legalSoftware.degreeId == item.degreeId)
                                                                                   .Select(legalSoftware => legalSoftware.availableSystemSoftware)
                                                                                   .FirstOrDefault();
            }

            foreach (var item in legalSoftwarDetails)
            {
                var norms = db.jntuh_software_norms.Where(n => n.degreeId == item.degreeId).Select(n => n).FirstOrDefault();

                legalSystemSoftware += "<td><p align='left'>" + item.degree + "</p></td>";
                legalSystemSoftware += "<td>" + norms.SystemSoftware + "</td>";
                legalSystemSoftware += "<td>" + norms.ApplicationSoftware + "</td>";
                legalSystemSoftware += "<td>" + item.availableSystemSoftware + "</td>";
                legalSystemSoftware += "<td>" + item.availableApplicationSoftware + "</td>";
                legalSystemSoftware += "<td>&nbsp;</td>";
                legalSystemSoftware += "<td>&nbsp;</td>";
            }
            contents = contents.Replace("##COLLEGELEGALSYSTEMSOFTWAREDETAILS##", legalSystemSoftware);
            return contents;
        }

        //private string collegePrinters(int collegeId, string contents)
        //{
        //    string collegePrinters = string.Empty;
        //    List<CollegePrinterDetails> PrinterDetails = (from CollegeDegree in db.jntuh_degree
        //                                                  orderby CollegeDegree.degreeDisplayOrder
        //                                                  where (CollegeDegree.isActive == true)
        //                                                  select new CollegePrinterDetails
        //                                                  {
        //                                                      degreeId = CollegeDegree.id,
        //                                                      degree = CollegeDegree.degree,
        //                                                      availableComputers = 0,
        //                                                      availablePrinters = 0
        //                                                  }).ToList();
        //    foreach (var item in PrinterDetails)
        //    {
        //        item.availableComputers = db.jntuh_college_computer_student_ratio.Where(availableComputers => availableComputers.collegeId == collegeId &&
        //                                                                                availableComputers.degreeId == item.degreeId)
        //                                                                         .Select(availableComputers => availableComputers.availableComputers)
        //                                                                         .FirstOrDefault();
        //        item.availablePrinters = db.jntuh_college_computer_lab_printers.Where(availablePrinters => availablePrinters.collegeId == collegeId &&
        //                                                                              availablePrinters.degreeId == item.degreeId)
        //                                                                       .Select(availablePrinters => availablePrinters.availablePrinters)
        //                                                                       .FirstOrDefault();

        //        var norms = db.jntuh_printers_norms.Where(n => n.degreeId == item.degreeId).Select(n => n.Norms).FirstOrDefault();

        //        collegePrinters += "<td><p align='left'>" + item.degree + "</p></td>";
        //        collegePrinters += "<td>" + item.availableComputers + "</td>";
        //        collegePrinters += "<td>" + norms + "</td>";
        //        collegePrinters += "<td valign='top'>" + item.availablePrinters + "</td>";
        //        collegePrinters += "<td>&nbsp;</td>";
        //        collegePrinters += "<td>&nbsp;</td>";
        //    }

        //    contents = contents.Replace("##COLLEGEPRINTERSDETAILS##", collegePrinters);
        //    return contents;
        //}

        private string collegeExaminationBranch(int collegeId, string contents)
        {
            #region from jntuh_college_examination_branch table
            jntuh_college_examination_branch examinationBranchDetails = db.jntuh_college_examination_branch.Where(examinationBranch => examinationBranch.collegeId == collegeId).FirstOrDefault();
            if (examinationBranchDetails != null)
            {
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHEXIST##", "<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No");
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHAREA##", examinationBranchDetails.examinationBranchArea.ToString());
                if (examinationBranchDetails.isConfidenatialRoomExists == true)
                {
                    contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHCONFIDENTIALROOM##", "<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No");
                }
                else
                {
                    contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHCONFIDENTIALROOM##", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No");
                }
                if (examinationBranchDetails.isAdjacentPrincipalRoom == true)
                {
                    contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHADJACENTTOTHERPRINCIPAL##", "<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No");
                }
                else
                {
                    contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHADJACENTTOTHERPRINCIPAL##", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No");
                }


            }
            else
            {
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHEXIST##", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No");
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHAREA##", string.Empty);
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHCONFIDENTIALROOM##", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No");
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHADJACENTTOTHERPRINCIPAL##", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No");
            }
            #endregion

            #region from jntuh_college_examination_branch_security table
            jntuh_college_examination_branch_security examinationBranchSecurityDetails = db.jntuh_college_examination_branch_security.Where(security => security.collegeId == collegeId).FirstOrDefault();
            if (examinationBranchSecurityDetails != null)
            {
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES1##", examinationBranchSecurityDetails.securityMesearesTaken1);
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES2##", examinationBranchSecurityDetails.securityMesearesTaken2);
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES3##", examinationBranchSecurityDetails.securityMesearesTaken3);
            }
            else
            {
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES1##", string.Empty);
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES2##", string.Empty);
                contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHMEASURES3##", string.Empty);
            }
            #endregion

            #region from jntuh_college_examination_branch_staff table
            string examinationBranchStaff = string.Empty;
            int count = 1;
            List<jntuh_college_examination_branch_staff> examinationBranchStaffDetails = db.jntuh_college_examination_branch_staff.Where(staff => staff.collegeId == collegeId).ToList();
            if (examinationBranchStaffDetails.Count() > 5)
            {
                foreach (var item in examinationBranchStaffDetails)
                {
                    examinationBranchStaff += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    examinationBranchStaff += "<td colspan='5'>" + item.staffName + "</td>";
                    examinationBranchStaff += "<td colspan='3'>" + item.staffDesignation + "</td>";
                    if (item.isTeachingStaff == true)
                    {
                        examinationBranchStaff += "<td colspan='3'>Teaching</td>";
                    }
                    else
                    {
                        examinationBranchStaff += "<td colspan='3'>Non-Teaching</td>";
                    }
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    examinationBranchStaff += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    examinationBranchStaff += "<td colspan='5'>##EXAMINATIONBRANCHSTAFFNAME" + i + "##</td>";
                    examinationBranchStaff += "<td colspan='3'>##EXAMINATIONBRANCHSTAFFDESIGNATION" + i + "##</td>";
                    examinationBranchStaff += "<td colspan='3'>##EXAMINATIONBRANCHSTAFFTEACHINGORNONTEACHING" + i + "##</td>";
                }
                foreach (var item in examinationBranchStaffDetails)
                {
                    examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFNAME" + count + "##", item.staffName);
                    examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFDESIGNATION" + count + "##", item.staffDesignation);
                    if (item.isTeachingStaff == true)
                    {
                        examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFTEACHINGORNONTEACHING" + count + "##", "Teaching");
                    }
                    else
                    {
                        examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFTEACHINGORNONTEACHING" + count + "##", "Non-Teaching");
                    }
                    count++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFNAME" + i + "##", string.Empty);
                    examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFDESIGNATION" + i + "##", string.Empty);
                    examinationBranchStaff = examinationBranchStaff.Replace("##EXAMINATIONBRANCHSTAFFTEACHINGORNONTEACHING" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHEXAMINATIONBRANCHSTAFF##", examinationBranchStaff);
            #endregion

            #region from jntuh_college_examination_branch_edep table
            string collegeEDEP = string.Empty;
            int collegeEDEPCount = 1;
            List<CollegeEDEPDetails> examinationBanchEDEPDetails = db.jntuh_edep_equipment.Where(equipment => equipment.isActive == true)
                                                                                 .Select(equipment => new CollegeEDEPDetails
                                                                                 {
                                                                                     EDEPEquipmentId = equipment.id,
                                                                                     EDEPEquipment = equipment.equipmentName,
                                                                                     ActualValue = string.Empty,
                                                                                 }).ToList();
            foreach (var item in examinationBanchEDEPDetails)
            {
                item.ActualValue = db.jntuh_college_examination_branch_edep.Where(e => e.collegeId == collegeId && e.EDEPEquipmentId == item.EDEPEquipmentId).Select(e => e.ActualValue).FirstOrDefault();

                collegeEDEP += "<td colspan='1' width='57'><p align='center'>" + collegeEDEPCount + "</p></td>";
                collegeEDEP += "<td colspan='7' width='468'><p>" + item.EDEPEquipment + "</p></td>";
                collegeEDEP += "<td colspan='2' width='82'><p align='center'>" + item.ActualValue + "</p></td>";
                collegeEDEP += "<td colspan='2' width='82'>&nbsp;</td>";
                collegeEDEPCount++;
            }

            contents = contents.Replace("##COLLEGEEXAMINATIONBRANCHEDEPEQUIPMENT##", collegeEDEP);
            #endregion

            return contents;
        }

        //private string collegeFeeReimbursement(int collegeId, string contents)
        //{
        //    string feeReimbersement = string.Empty;
        //    int count = 1;
        //    int withoutReimbersementSeatsTotal = 0;
        //    decimal withoutReimbersementFeeTotal = 0;
        //    int withReimbersementSeatsTotal = 0;
        //    decimal withReimbersementFeeTotal = 0;
        //    int NRISeatsTotal = 0;
        //    decimal NRIFeeTotal = 0;
        //    int PIOSeatsTotal = 0;
        //    decimal PIOFeeTotal = 0;
        //    string academicYear = db.jntuh_academic_year.Where(a => a.isActive == true &&
        //                                                              a.isPresentAcademicYear == true)
        //                                                  .Select(a => a.academicYear).FirstOrDefault();
        //    List<jntuh_college_fee_reimbursement> reimbursement = db.jntuh_college_fee_reimbursement.Where(p => p.collegeId == collegeId).ToList();
        //    List<CollegeFeeReimbursement> collegeFeeReimbursement = new List<CollegeFeeReimbursement>();

        //    foreach (var item in reimbursement)
        //    {
        //        CollegeFeeReimbursement newReimbursement = new CollegeFeeReimbursement();
        //        newReimbursement.id = item.id;
        //        newReimbursement.collegeId = item.collegeId;
        //        newReimbursement.academicYearId = item.academicYearId;
        //        newReimbursement.specializationId = item.specializationId;
        //        newReimbursement.shiftId = item.shiftId;
        //        newReimbursement.yearInDegreeId = item.yearInDegreeId;
        //        newReimbursement.seatsWithoutReimbursement = item.seatsWithoutReimbursement;
        //        newReimbursement.feeWithoutReimbursement = item.feeWithoutReimbursement;
        //        newReimbursement.seatsWithReimbursement = item.seatsWithReimbursement;
        //        newReimbursement.feeWithReimbursement = item.feeWithReimbursement;
        //        newReimbursement.NRISeats = item.NRISeats;
        //        newReimbursement.totalNRIFee = item.totalNRIFee;
        //        newReimbursement.PIOSeats = item.PIOSeats;
        //        newReimbursement.totalPIOFee = item.totalPIOFee;
        //        newReimbursement.yearInDegree = db.jntuh_year_in_degree.Where(y => y.id == item.yearInDegreeId).Select(y => y.yearInDegree).FirstOrDefault();
        //        newReimbursement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //        newReimbursement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //        newReimbursement.department = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.departmentName).FirstOrDefault();
        //        newReimbursement.degreeID = db.jntuh_department.Where(d => d.id == newReimbursement.departmentID).Select(d => d.degreeId).FirstOrDefault();
        //        newReimbursement.degree = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degree).FirstOrDefault();
        //        newReimbursement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newReimbursement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //        newReimbursement.shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //        collegeFeeReimbursement.Add(newReimbursement);
        //        withoutReimbersementSeatsTotal += item.seatsWithoutReimbursement;
        //        withoutReimbersementFeeTotal += item.feeWithoutReimbursement;
        //        withReimbersementSeatsTotal += item.seatsWithReimbursement;
        //        withReimbersementFeeTotal += item.feeWithReimbursement;
        //        NRISeatsTotal += item.NRISeats;
        //        NRIFeeTotal += item.totalNRIFee;
        //        PIOSeatsTotal += item.PIOSeats;
        //        PIOFeeTotal += item.totalPIOFee;
        //    }
        //    collegeFeeReimbursement = collegeFeeReimbursement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ThenBy(ei => ei.shiftId).ThenBy(ei => ei.yearInDegree).ToList();
        //    if (reimbursement.Count() > 10)
        //    {
        //        foreach (var item in collegeFeeReimbursement)
        //        {
        //            feeReimbersement += "<td colspan='1'><p align='center'>" + count + "</p></td>";
        //            feeReimbersement += "<td colspan='2'><p>" + item.degree + "</p></td>";
        //            feeReimbersement += "<td colspan='2'><p>" + item.department + "</p></td>";
        //            feeReimbersement += "<td colspan='4'><p>" + item.specialization + "</p></td>";
        //            feeReimbersement += "<td colspan='1'><p>" + item.shift + "</p></td>";
        //            feeReimbersement += "<td colspan='1'><p>" + item.yearInDegree + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>" + item.seatsWithoutReimbursement + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>" + item.feeWithoutReimbursement + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>" + item.seatsWithReimbursement + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>" + item.feeWithReimbursement + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>" + item.NRISeats + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>" + item.totalNRIFee + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>" + item.PIOSeats + "</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>" + item.totalPIOFee + "</p></td>";
        //            withoutReimbersementSeatsTotal += item.seatsWithoutReimbursement;
        //            withoutReimbersementFeeTotal += item.feeWithoutReimbursement;
        //            withReimbersementSeatsTotal += item.seatsWithReimbursement;
        //            withReimbersementFeeTotal += item.feeWithReimbursement;
        //            NRISeatsTotal += item.NRISeats;
        //            NRIFeeTotal += item.totalNRIFee;
        //            PIOSeatsTotal += item.PIOSeats;
        //            PIOFeeTotal += item.totalPIOFee;
        //            count++;
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 1; i <= 10; i++)
        //        {
        //            feeReimbersement += "<td colspan='1'><p align='center'>" + i + "</p></td>";
        //            feeReimbersement += "<td colspan='2'><p>##FEEREIMBERSEMENTDEGREE" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2'><p>##FEEREIMBERSEMENTDEPARTMENT" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='4'><p>##FEEREIMBERSEMENTSPECIALIZATION" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='1' align='center'><p>##FEEREIMBERSEMENTSHIFT" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='1' align='center'><p>##FEEREIMBERSEMENTYEARINDEGREE" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENT" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENTTOTAL" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENT" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENTTOTAL" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>##FEEREIMBERSEMENTMANAGEMENTNRISEATS" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>##FEEREIMBERSEMENTMANAGEMENTNRISEATSTOTAL" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='center'><p style='font-size:8px'>##FEEREIMBERSEMENTMANAGEMENTPIOSEATS" + i + "##</p></td>";
        //            feeReimbersement += "<td colspan='2' align='right'><p style='font-size:8px'>##FEEREIMBERSEMENTMANAGEMENTPIOSEATSTOTAL" + i + "##</p></td>";
        //        }

        //        foreach (var item in collegeFeeReimbursement)
        //        {
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTDEGREE" + count + "##", item.degree);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTDEPARTMENT" + count + "##", item.department);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTSPECIALIZATION" + count + "##", item.specialization);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTSHIFT" + count + "##", item.shift);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTYEARINDEGREE" + count + "##", item.yearInDegree);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENT" + count + "##", item.seatsWithoutReimbursement.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENTTOTAL" + count + "##", item.feeWithoutReimbursement.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENT" + count + "##", item.seatsWithReimbursement.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENTTOTAL" + count + "##", item.feeWithReimbursement.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTNRISEATS" + count + "##", item.NRISeats.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTNRISEATSTOTAL" + count + "##", item.totalNRIFee.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTPIOSEATS" + count + "##", item.PIOSeats.ToString());
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTPIOSEATSTOTAL" + count + "##", item.totalPIOFee.ToString());
        //            count++;
        //        }

        //        for (int i = 1; i <= 10; i++)
        //        {
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTDEGREE" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTDEPARTMENT" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTSPECIALIZATION" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTSHIFT" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTYEARINDEGREE" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENT" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHOUTREIMNERSEMENTTOTAL" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENT" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTCONVENERWITHREIMNERSEMENTTOTAL" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTNRISEATS" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTNRISEATSTOTAL" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTPIOSEATS" + i + "##", string.Empty);
        //            feeReimbersement = feeReimbersement.Replace("##FEEREIMBERSEMENTMANAGEMENTPIOSEATSTOTAL" + i + "##", string.Empty);
        //        }

        //    }
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTINFORMATION##", feeReimbersement);
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTACADEMICYEAR##", academicYear);
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTWITHOUTREIMBERSEMENTTOTAL##", withoutReimbersementSeatsTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTWITHOUTREIMBERSEMENTFEETOTAL##", withoutReimbersementFeeTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTWITHREIMBERSEMENTTOTAL##", withReimbersementSeatsTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTWITHREIMBERSEMENTFEETOTAL##", withReimbersementFeeTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTNRISEATSTOTAL##", NRISeatsTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTNRISEATSFEETOTAL##", NRIFeeTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTPIOSEATSTOTAL##", PIOSeatsTotal.ToString());
        //    contents = contents.Replace("##COLLEGEFEEREIMBERSEMENTPIOSEATSFEETOTAL##", PIOFeeTotal.ToString());
        //    return contents;
        //}

        private string collegeGrievance(int collegeId, string contents)
        {
            #region from jntuh_college_grievance_committee table

            string grievanceCommittee = string.Empty;
            int committeeCount = 1;
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
                                                           }).ToList();

            if (committee.Count > 5)
            {
                foreach (var item in committee)
                {
                    grievanceCommittee += "<td colspan='1'><p align='center'>" + committeeCount + "</p></td>";
                    grievanceCommittee += "<td colspan='4'><p>" + item.memberName + "</p></td>";
                    grievanceCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    grievanceCommittee += "<td colspan='4'><p>" + item.designationName + "</p></td>";

                    committeeCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    grievanceCommittee += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    grievanceCommittee += "<td colspan='4'><p>##GRIEVANCECOMMITTEENAME" + i + "##</p></td>";
                    grievanceCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    grievanceCommittee += "<td colspan='4'><p>##GRIEVANCECOMMITTEEDESIGNATION" + i + "##</p></td>";

                }
                foreach (var item in committee)
                {
                    grievanceCommittee = grievanceCommittee.Replace("##GRIEVANCECOMMITTEEDESIGNATION" + committeeCount + "##", item.designationName);
                    grievanceCommittee = grievanceCommittee.Replace("##GRIEVANCECOMMITTEENAME" + committeeCount + "##", item.memberName);
                    committeeCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    grievanceCommittee = grievanceCommittee.Replace("##GRIEVANCECOMMITTEEDESIGNATION" + i + "##", string.Empty);
                    grievanceCommittee = grievanceCommittee.Replace("##GRIEVANCECOMMITTEENAME" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##GRIEVANCEREDRESSALCOMMITTEE##", grievanceCommittee);
            #endregion

            #region from jntuh_college_grievance_complaints table

            string grievanceComplaints = string.Empty;
            int complaintsCount = 1;
            List<GrievanceRedressalComplaints> complaints = db.jntuh_college_grievance_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                              new GrievanceRedressalComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken
                                              }).ToList();
            if (complaints.Count > 5)
            {
                foreach (var item in complaints)
                {
                    grievanceComplaints += "<td colspan='1'><p align='center'>" + complaintsCount + "</p></td>";
                    grievanceComplaints += "<td colspan='6'><p>" + item.complaintReceived + "</p></td>";
                    grievanceComplaints += "<td colspan='3'><p>" + item.actionsTaken + "</p></td>";
                    complaintsCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    grievanceComplaints += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    grievanceComplaints += "<td colspan='6'><p>##GRIEVANCECOMPLAINTCOMPLAINTRECIEVED" + i + "##</p></td>";
                    grievanceComplaints += "<td colspan='3'><p>##GRIEVANCECOMPLAINTACTIONTAKEN" + i + "##</p></td>";
                }
                foreach (var item in complaints)
                {
                    grievanceComplaints = grievanceComplaints.Replace("##GRIEVANCECOMPLAINTCOMPLAINTRECIEVED" + complaintsCount + "##", item.complaintReceived);
                    grievanceComplaints = grievanceComplaints.Replace("##GRIEVANCECOMPLAINTACTIONTAKEN" + complaintsCount + "##", item.actionsTaken);
                    complaintsCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    grievanceComplaints = grievanceComplaints.Replace("##GRIEVANCECOMPLAINTCOMPLAINTRECIEVED" + i + "##", string.Empty);
                    grievanceComplaints = grievanceComplaints.Replace("##GRIEVANCECOMPLAINTACTIONTAKEN" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##GRIEVANCEREDRESSALCOMPLAINTS##", grievanceComplaints);
            contents = contents.Replace("##GRIEVANCETOTALCOMPLAINTSRECIEVED##", complaints.Count().ToString());
            #endregion
            return contents;
        }

        private string collegeAntiRagging(int collegeId, string contents)
        {
            #region from jntuh_college_antiragging_committee table
            string antiRaggingCommittee = string.Empty;
            int antiRaggingCommitteeCount = 1;
            List<AntiRaggingCommittee> committee = (from ac in db.jntuh_college_antiragging_committee
                                                    join d in db.jntuh_grc_designation on ac.memberDesignation equals d.id
                                                    join ad in db.jntuh_designation on ac.actualDesignation equals ad.id
                                                    where ac.collegeId == collegeId
                                                    select new AntiRaggingCommittee
                                                    {
                                                        id = ac.id,
                                                        collegeId = ac.collegeId,
                                                        memberDesignation = ac.memberDesignation,
                                                        memberName = ac.memberName,
                                                        designationName = d.Designation
                                                    }).ToList();
            if (committee.Count() > 5)
            {
                foreach (var item in committee)
                {
                    antiRaggingCommittee += "<td colspan='1'><p align='center'>" + antiRaggingCommitteeCount + "</p></td>";
                    antiRaggingCommittee += "<td colspan='4'><p>" + item.memberName + "</p></td>";
                    antiRaggingCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    antiRaggingCommittee += "<td colspan='4'><p>" + item.designationName + "</p></td>";

                    antiRaggingCommitteeCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    antiRaggingCommittee += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    antiRaggingCommittee += "<td colspan='4'><p>##ANTIRAGGINGCOMMITTEENAME" + i + "##</p></td>";
                    antiRaggingCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    antiRaggingCommittee += "<td colspan='4'><p>##ANTIRAGGINGCOMMITTEEDESIGNATION" + i + "##</p></td>";

                }
                foreach (var item in committee)
                {
                    antiRaggingCommittee = antiRaggingCommittee.Replace("##ANTIRAGGINGCOMMITTEEDESIGNATION" + antiRaggingCommitteeCount + "##", item.designationName);
                    antiRaggingCommittee = antiRaggingCommittee.Replace("##ANTIRAGGINGCOMMITTEENAME" + antiRaggingCommitteeCount + "##", item.memberName);
                    antiRaggingCommitteeCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    antiRaggingCommittee = antiRaggingCommittee.Replace("##ANTIRAGGINGCOMMITTEEDESIGNATION" + i + "##", string.Empty);
                    antiRaggingCommittee = antiRaggingCommittee.Replace("##ANTIRAGGINGCOMMITTEENAME" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##COLLEGEANTIRAGGINGCOMMITTEEINFORMATION##", antiRaggingCommittee);
            #endregion

            #region from jntuh_college_antiragging_complaints table
            string antiraggingComplaints = string.Empty;
            int antiraggingComplaintsCount = 1;
            List<AntiRaggingComplaints> complaints = db.jntuh_college_antiragging_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                              new AntiRaggingComplaints
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  complaintReceived = a.complaintReceived,
                                                  actionsTaken = a.actionsTaken
                                              }).ToList();
            if (complaints.Count > 5)
            {
                foreach (var item in complaints)
                {
                    antiraggingComplaints += "<td colspan='1'><p align='center'>" + antiraggingComplaintsCount + "</p></td>";
                    antiraggingComplaints += "<td colspan='6'><p>" + item.complaintReceived + "</p></td>";
                    antiraggingComplaints += "<td colspan='3'><p>" + item.actionsTaken + "</p></td>";
                    antiraggingComplaintsCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    antiraggingComplaints += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    antiraggingComplaints += "<td colspan='6'><p>##ANTIRAGGINGCOMPLAINTCOMPLAINTRECIEVED" + i + "##</p></td>";
                    antiraggingComplaints += "<td colspan='3'><p>##ANTIRAGGINGCOMPLAINTACTIONTAKEN" + i + "##</p></td>";
                }
                foreach (var item in complaints)
                {
                    antiraggingComplaints = antiraggingComplaints.Replace("##ANTIRAGGINGCOMPLAINTCOMPLAINTRECIEVED" + antiraggingComplaintsCount + "##", item.complaintReceived);
                    antiraggingComplaints = antiraggingComplaints.Replace("##ANTIRAGGINGCOMPLAINTACTIONTAKEN" + antiraggingComplaintsCount + "##", item.actionsTaken);
                    antiraggingComplaintsCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    antiraggingComplaints = antiraggingComplaints.Replace("##ANTIRAGGINGCOMPLAINTCOMPLAINTRECIEVED" + i + "##", string.Empty);
                    antiraggingComplaints = antiraggingComplaints.Replace("##ANTIRAGGINGCOMPLAINTACTIONTAKEN" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##COLLEGEANTIRAGGINGCOMPLAINTSINFORMATION##", antiraggingComplaints);
            contents = contents.Replace("##COLLEGEANTIRAGGINGCOMPLAINTSINFORMATIONTOTAL##", complaints.Count().ToString());
            #endregion
            return contents;
        }

        private string collegeWomenProtection(int collegeId, string contents)
        {
            #region Women Protection Committee

            string womenProtectionCommittee = string.Empty;

            int womenProtectionCellCount = 1;
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
                                                             }).ToList();
            if (womenProtectionCell.Count() > 5)
            {
                foreach (var item in womenProtectionCell)
                {
                    womenProtectionCommittee += "<td colspan='1'><p align='center'>" + womenProtectionCellCount + "</p></td>";
                    womenProtectionCommittee += "<td colspan='4'><p>" + item.memberName + "</p></td>";
                    womenProtectionCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    womenProtectionCommittee += "<td colspan='4'><p>" + item.designationName + "</p></td>";
                    womenProtectionCellCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    womenProtectionCommittee += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    womenProtectionCommittee += "<td colspan='4'><p>##NAME" + i + "##</p></td>";
                    womenProtectionCommittee += "<td colspan='3'><p>&nbsp;</p></td>";
                    womenProtectionCommittee += "<td colspan='4'><p>##DESIGNATION" + i + "##</p></td>";

                }
                foreach (var item in womenProtectionCell)
                {
                    womenProtectionCommittee = womenProtectionCommittee.Replace("##DESIGNATION" + womenProtectionCellCount + "##", item.designationName);
                    womenProtectionCommittee = womenProtectionCommittee.Replace("##NAME" + womenProtectionCellCount + "##", item.memberName);
                    womenProtectionCellCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    womenProtectionCommittee = womenProtectionCommittee.Replace("##DESIGNATION" + i + "##", string.Empty);
                    womenProtectionCommittee = womenProtectionCommittee.Replace("##NAME" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##COLLEGEWOMENPROTECTIONCOMMITTEEINFORMATION##", womenProtectionCommittee);

            #endregion

            #region Women Protection Complaints
            string womenProtectionComplaints = string.Empty;


            int womenProtectionComplaintsCount = 1;
            List<WomenProtectionCellComplaints> complaints = db.jntuh_college_women_protection_cell_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                            new WomenProtectionCellComplaints
                                            {
                                                id = a.id,
                                                collegeId = a.collegeId,
                                                complaintReceived = a.complaintReceived,
                                                actionsTaken = a.actionsTaken,
                                            }).ToList();
            if (complaints.Count > 5)
            {
                foreach (var item in complaints)
                {
                    womenProtectionComplaints += "<td colspan='1'><p align='center'>" + womenProtectionComplaintsCount + "</p></td>";
                    womenProtectionComplaints += "<td colspan='6'><p>" + item.complaintReceived + "</p></td>";
                    womenProtectionComplaints += "<td colspan='3'><p>" + item.actionsTaken + "</p></td>";
                    womenProtectionComplaintsCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    womenProtectionComplaints += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    womenProtectionComplaints += "<td colspan='6'><p>##WOMENPROTECTIONCOMPLAINTCOMPLAINTRECIEVED" + i + "##</p></td>";
                    womenProtectionComplaints += "<td colspan='3'><p>##WOMENPROTECTIONCOMPLAINTACTIONTAKEN" + i + "##</p></td>";
                }
                foreach (var item in complaints)
                {
                    womenProtectionComplaints = womenProtectionComplaints.Replace("##WOMENPROTECTIONCOMPLAINTCOMPLAINTRECIEVED" + womenProtectionComplaintsCount + "##", item.complaintReceived);
                    womenProtectionComplaints = womenProtectionComplaints.Replace("##WOMENPROTECTIONCOMPLAINTACTIONTAKEN" + womenProtectionComplaintsCount + "##", item.actionsTaken);
                    womenProtectionComplaintsCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    womenProtectionComplaints = womenProtectionComplaints.Replace("##WOMENPROTECTIONCOMPLAINTCOMPLAINTRECIEVED" + i + "##", string.Empty);
                    womenProtectionComplaints = womenProtectionComplaints.Replace("##WOMENPROTECTIONCOMPLAINTACTIONTAKEN" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##COLLEGEWOMENPROTECTIONCOMPLAINTSINFORMATION##", womenProtectionComplaints);
            contents = contents.Replace("##COLLEGEWOMENPROTECTIONCOMPLAINTSINFORMATIONTOTAL##", complaints.Count().ToString());
            #endregion
            return contents;
        }

        private string collegeRTIDetails(int collegeId, string contents)
        {
            #region RTIDetails Committee
            string RTIDetailsString = string.Empty;
            int RTIDetailsCount = 1;


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
                                           }).ToList();
            if (rtiDetails.Count() > 5)
            {
                foreach (var item in rtiDetails)
                {
                    RTIDetailsString += "<td colspan='1'><p align='center'>" + RTIDetailsCount + "</p></td>";
                    RTIDetailsString += "<td colspan='4'><p>" + item.memberName + "</p></td>";
                    RTIDetailsString += "<td colspan='3'><p>&nbsp;</p></td>";
                    RTIDetailsString += "<td colspan='4'><p>" + item.designationName + "</p></td>";
                    RTIDetailsCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    RTIDetailsString += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    RTIDetailsString += "<td colspan='4'><p>##NAME" + i + "##</p></td>";
                    RTIDetailsString += "<td colspan='3'><p>&nbsp;</p></td>";
                    RTIDetailsString += "<td colspan='4'><p>##DESIGNATION" + i + "##</p></td>";

                }
                foreach (var item in rtiDetails)
                {
                    RTIDetailsString = RTIDetailsString.Replace("##DESIGNATION" + RTIDetailsCount + "##", item.designationName);
                    RTIDetailsString = RTIDetailsString.Replace("##NAME" + RTIDetailsCount + "##", item.memberName);
                    RTIDetailsCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    RTIDetailsString = RTIDetailsString.Replace("##DESIGNATION" + i + "##", string.Empty);
                    RTIDetailsString = RTIDetailsString.Replace("##NAME" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##COLLEGERTIDETAILSINFORMATION##", RTIDetailsString);

            #endregion

            #region RTIDetails Compliaints

            string RTIDetailsComplaints = string.Empty;
            int RTIComplaintsCount = 1;

            List<RTIComplaints> rtiComplaints = db.jntuh_college_rti_complaints.Where(a => a.collegeId == collegeId).Select(a =>
                                               new RTIComplaints
                                               {
                                                   id = a.id,
                                                   collegeId = a.collegeId,
                                                   complaintReceived = a.complaintReceived,
                                                   actionsTaken = a.actionsTaken,
                                               }).ToList();
            if (rtiComplaints.Count > 5)
            {
                foreach (var item in rtiComplaints)
                {
                    RTIDetailsComplaints += "<td colspan='1'><p align='center'>" + RTIComplaintsCount + "</p></td>";
                    RTIDetailsComplaints += "<td colspan='6'><p>" + item.complaintReceived + "</p></td>";
                    RTIDetailsComplaints += "<td colspan='3'><p>" + item.actionsTaken + "</p></td>";
                    RTIComplaintsCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    RTIDetailsComplaints += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    RTIDetailsComplaints += "<td colspan='6'><p>##RTICOMPLAINTCOMPLAINTRECIEVED" + i + "##</p></td>";
                    RTIDetailsComplaints += "<td colspan='3'><p>##RTICOMPLAINTACTIONTAKEN" + i + "##</p></td>";
                }
                foreach (var item in rtiComplaints)
                {
                    RTIDetailsComplaints = RTIDetailsComplaints.Replace("##RTICOMPLAINTCOMPLAINTRECIEVED" + RTIComplaintsCount + "##", item.complaintReceived);
                    RTIDetailsComplaints = RTIDetailsComplaints.Replace("##RTICOMPLAINTACTIONTAKEN" + RTIComplaintsCount + "##", item.actionsTaken);
                    RTIComplaintsCount++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    RTIDetailsComplaints = RTIDetailsComplaints.Replace("##RTICOMPLAINTCOMPLAINTRECIEVED" + i + "##", string.Empty);
                    RTIDetailsComplaints = RTIDetailsComplaints.Replace("##RTICOMPLAINTACTIONTAKEN" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##COLLEGERTIDETAILSCOMPLAINTSINFORMATION##", RTIDetailsComplaints);
            contents = contents.Replace("##COLLEGERTIDETAILSCOMPLAINTSINFORMATIONTOTAL##", rtiComplaints.Count().ToString());

            #endregion

            return contents;
        }

        private string collegeSports(int collegeId, string contents)
        {
            #region indoorGames

            string indoorGames = string.Empty;
            int indoorGamesCount = 1;
            int indoorGamesId = 1;
            indoorGames += "<table cellpadding='2'><tr><td colspan='1'></td>";

            List<jntuh_college_sports> indoorSports = db.jntuh_college_sports.Where(sports => sports.collegeId == collegeId && sports.sportsTypeId == indoorGamesId).ToList();
            if (indoorSports.Count() > 9)
            {
                foreach (var item in indoorSports)
                {
                    indoorGames += "<td colspan='9' style='font-size:9px; text-transform: uppercase;'>" + indoorGamesCount + ". " + item.sportsFacility + "</td>";
                    if (indoorGamesCount % 3 == 0)
                    {
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "</tr><tr><td colspan='1'></td>";
                    }
                    indoorGamesCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 9; i++)
                {

                    indoorGames += "<td colspan='9' style='font-size:9px'>" + i + ". ##SPORTSFACILITYINDOOR" + i + "##</td>";
                    if (i % 3 == 0)
                    {
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "<td colspan='1'></td>";
                        indoorGames += "</tr><tr><td colspan='1'></td>";
                    }
                }
                foreach (var item in indoorSports)
                {
                    indoorGames = indoorGames.Replace("##SPORTSFACILITYINDOOR" + indoorGamesCount + "##", item.sportsFacility.ToUpper());
                    indoorGamesCount++;
                }
                for (int i = 1; i <= 9; i++)
                {
                    indoorGames = indoorGames.Replace("##SPORTSFACILITYINDOOR" + i + "##", string.Empty);
                }
            }
            indoorGames += "</tr></table>";
            contents = contents.Replace("##COLLEGESPORTSINDOORGAMES##  ", indoorGames);

            #endregion

            #region outdoorGames

            string outdoorGames = string.Empty;
            int outdoorGamesCount = 1;
            outdoorGames += "<table cellpadding='2'><tr><td colspan='1'></td>";
            int outdoorGamesId = 2;
            List<jntuh_college_sports> outdoorSports = db.jntuh_college_sports.Where(sports => sports.collegeId == collegeId && sports.sportsTypeId == outdoorGamesId).ToList();
            if (outdoorSports.Count() > 9)
            {
                foreach (var item in outdoorSports)
                {
                    outdoorGames += "<td colspan='9' style='font-size:9px; text-transform: uppercase;'>" + outdoorGamesCount + ". " + item.sportsFacility + "</td>";
                    if (outdoorGamesCount % 3 == 0)
                    {
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "</tr><tr><td colspan='1'></td>";
                    }
                    outdoorGamesCount++;
                }
            }
            else
            {
                for (int i = 1; i <= 9; i++)
                {
                    outdoorGames += "<td colspan='9' style='font-size:9px'>" + i + ". ##SPORTSFACILITYOUTDOOR" + i + "##</td>";
                    if (i % 3 == 0)
                    {
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "<td colspan='1'></td>";
                        outdoorGames += "</tr><tr><td colspan='1'></td>";
                    }
                }

                foreach (var item in outdoorSports)
                {
                    outdoorGames = outdoorGames.Replace("##SPORTSFACILITYOUTDOOR" + outdoorGamesCount + "##", item.sportsFacility.ToUpper());
                    outdoorGamesCount++;
                }
                for (int i = 1; i <= 9; i++)
                {
                    outdoorGames = outdoorGames.Replace("##SPORTSFACILITYOUTDOOR" + i + "##", string.Empty);
                }
            }
            outdoorGames += "</tr></table>";
            contents = contents.Replace("##COLLEGESPORTSOUTDOORGAMES##", outdoorGames);

            #endregion

            #region from jntuh_college_desirable_others table

            jntuh_college_desirable_others desirablesOthers = db.jntuh_college_desirable_others.Where(sports => sports.collegeId == collegeId).FirstOrDefault();

            if (desirablesOthers != null)
            {
                string strPlayGroundType = string.Empty;
                List<PlayGroundTypeModel> playGroundType = playGroundTypes.ToList();
                foreach (var item in playGroundType)
                {
                    strPlayGroundType += string.Format("{0}&nbsp; {1}&nbsp;", "##COLLEGEPLAYGROUNDTYPE" + item.id + "##", item.Name);
                }

                string[] selectedPlayGroundType = desirablesOthers.playgroundType.Split('|').ToArray();
                int groundType = 0;
                foreach (var item in selectedPlayGroundType)
                {
                    if (item != string.Empty)
                    {
                        groundType = Convert.ToInt32(item);
                        strPlayGroundType = strPlayGroundType.Replace("##COLLEGEPLAYGROUNDTYPE" + groundType + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
                    }
                }
                int groundTypeCount = 1;
                foreach (var item in playGroundType)
                {
                    strPlayGroundType = strPlayGroundType.Replace("##COLLEGEPLAYGROUNDTYPE" + groundTypeCount + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
                    groundTypeCount++;
                }

                string strModeofTransport = string.Empty;
                List<ModeOfTransportModel> modeofTransportType = transportMode.ToList();
                foreach (var item in modeofTransportType)
                {
                    strModeofTransport += string.Format("{0}&nbsp; {1}&nbsp;", "##COLLEGEMODEOFTRANSPORTTYPE" + item.id + "##", item.Name);
                }

                string[] selectedTransportType = desirablesOthers.modeOfTransport.Split('|').ToArray();
                int transportType = 0;
                foreach (var item in selectedTransportType)
                {
                    if (item != string.Empty)
                    {
                        transportType = Convert.ToInt32(item);
                        strModeofTransport = strModeofTransport.Replace("##COLLEGEMODEOFTRANSPORTTYPE" + transportType + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
                    }
                }
                int transportTypeCount = 1;
                foreach (var item in modeofTransportType)
                {
                    strModeofTransport = strModeofTransport.Replace("##COLLEGEMODEOFTRANSPORTTYPE" + transportTypeCount + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
                    transportTypeCount++;
                }

                string strModeofPayment = string.Empty;
                List<ModeOfPaymentModel> modeofPayment = paymentMode.ToList();
                foreach (var item in modeofPayment)
                {
                    strModeofPayment += string.Format("{0}&nbsp; {1}", "##COLLEGEMODEOFPAYMENTTYPE" + item.id + "##", item.Name);
                }

                string[] selectedPaymentMode = desirablesOthers.modeOfPayment.Split('|').ToArray();
                int paymentType = 0;
                foreach (var item in selectedPaymentMode)
                {
                    if (item != string.Empty)
                    {
                        paymentType = Convert.ToInt32(item);
                        strModeofPayment = strModeofPayment.Replace("##COLLEGEMODEOFPAYMENTTYPE" + paymentType + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "yes_b"));
                    }
                }
                int paymentTypeCount = 1;
                foreach (var item in modeofPayment)
                {
                    strModeofPayment = strModeofPayment.Replace("##COLLEGEMODEOFPAYMENTTYPE" + paymentTypeCount + "##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_{0}.png' height='10' />", "no_b"));
                    paymentTypeCount++;
                }

                contents = contents.Replace("##COLLEGESPORTSNUMBEROFBUSSES##", desirablesOthers.numberOfBus.ToString());
                contents = contents.Replace("##COLLEGESPORTSNUMBEROFOTHERVEHICLES##", desirablesOthers.numberOfOtherVehicles.ToString());
                contents = contents.Replace("##COLLEGESPORTSMODEOFPAYMENTSALARY##", strModeofPayment);
                contents = contents.Replace("##COLLEGESPORTSMODEOFTRANSPORTINSTITUTE##", strModeofTransport);
                contents = contents.Replace("##COLLEGESPORTSPLAYGROUNDTYPE##", strPlayGroundType);
                contents = contents.Replace("##COLLEGESPORTSNUMBEROFPLAYGROUNDS##", desirablesOthers.totalPlaygrounds.ToString());
            }
            else
            {
                contents = contents.Replace("##COLLEGESPORTSNUMBEROFBUSSES##", string.Empty);
                contents = contents.Replace("##COLLEGESPORTSNUMBEROFOTHERVEHICLES##", string.Empty);
                contents = contents.Replace("##COLLEGESPORTSMODEOFPAYMENTSALARY##", string.Empty);
                contents = contents.Replace("##COLLEGESPORTSMODEOFTRANSPORTINSTITUTE##", string.Empty);
                contents = contents.Replace("##COLLEGESPORTSPLAYGROUNDTYPE##", string.Empty);
                contents = contents.Replace("##COLLEGESPORTSNUMBEROFPLAYGROUNDS##", string.Empty);
            }

            #endregion
            contents = contents.Replace("##COLLEGEEXTRATOTALAREASQM##", "&nbsp;");
            return contents;
        }

        #region sports
        public PlayGroundTypeModel[] playGroundTypes = new[]
            {
                new PlayGroundTypeModel { id = "1", Name = "Square" },
                new PlayGroundTypeModel { id = "2", Name = "Rectangle" },
                new PlayGroundTypeModel { id = "3", Name = "Round" },
                new PlayGroundTypeModel { id = "4", Name = "Oval" },
                new PlayGroundTypeModel { id = "5", Name = "Cricket" },
                new PlayGroundTypeModel { id = "6", Name = "Other" }
            };

        public ModeOfTransportModel[] transportMode = new[]
            {
                new ModeOfTransportModel { id = "1", Name = "College Transport" },
                new ModeOfTransportModel { id = "2", Name = "Public Transport" },
                new ModeOfTransportModel { id = "3", Name = "Other" }
            };

        public ModeOfPaymentModel[] paymentMode = new[]
            {
                new ModeOfPaymentModel { id = "1", Name = "Cash" },
                new ModeOfPaymentModel { id = "2", Name = "Cheque" },
                new ModeOfPaymentModel { id = "3", Name = "Bank Transfer" },
                new ModeOfPaymentModel { id = "4", Name = "Other" }
            };
        #endregion

        private string collegeOtherDesirables(int collegeId, string contents)
        {
            string otherDesirables = string.Empty;
            List<OtherDesirableRequirements> otherDesirableRequiremetns = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == false)
                                                                           join d in db.jntuh_college_desirable_requirement.Where(d => d.collegeId == collegeId) on r.id equals d.requirementTypeID into de
                                                                           from desirables in de.DefaultIfEmpty()
                                                                           select new OtherDesirableRequirements
                                                                           {
                                                                               requirementId = (int?)desirables.requirementTypeID,
                                                                               requirementType = r.requirementType,
                                                                               isSelected = desirables.isAvaiable == true ? "true" : "false",
                                                                               governingBodymeetings = (int)desirables.governingBodyMeetings
                                                                           }).ToList();
            if (otherDesirableRequiremetns.Count() > 0)
            {
                foreach (var item in otherDesirableRequiremetns)
                {
                    otherDesirables += "<tr><td colspan='8'><p>" + item.requirementType + "</p></td>";

                    if (item.requirementType == "No. of Governing Body meetings held in the past one academic year")
                    {
                        if (item.governingBodymeetings == 2)
                        {
                            otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;NIL &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;One or more</p></td>";
                        }
                        else
                        {
                            if (item.isSelected == "true")
                            {
                                otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;NIL &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One or more</p></td>";
                            }
                            else
                            {
                                otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;NIL &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;One &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;One or more</p></td>";
                            }
                        }
                    }
                    else
                    {
                        if (item.requirementId == null)
                        {
                            otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</p></td>";
                        }
                        else
                        {
                            if (item.isSelected == "true")
                            {
                                otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</p></td>";
                            }
                            else
                            {
                                otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No</p></td>";
                            }
                        }
                        otherDesirables += "</tr>";
                    }
                }
            }
            else
            {
                string[] otherDesirableslist = db.jntuh_desirable_requirement_type.Where(hostel => hostel.isActive == true && hostel.isHostelRequirement == false)
                                                                           .Select(hostel => hostel.requirementType).ToArray();
                foreach (var item in otherDesirableslist)
                {
                    otherDesirables += "<tr><td colspan='8'><p>" + item + "</p></td>";
                    otherDesirables += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</p></td>";
                    otherDesirables += "</tr>";
                }
            }
            contents = contents.Replace("##COLLEGESPORTSOTHERDESIRABLEREQUIREMENTS##", otherDesirables);
            return contents;
        }

        private string collegeCampushostel(int collegeId, string contents)
        {
            string hostelRequirement = string.Empty;
            List<HostelRequirements> hostelRequirements = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == true)
                                                           join d in db.jntuh_college_hostel_maintenance.Where(d => d.collegeId == collegeId) on r.id equals d.requirementTypeID into de
                                                           from desirables in de.DefaultIfEmpty()
                                                           select new HostelRequirements
                                                           {
                                                               requirementId = (int?)desirables.requirementTypeID,
                                                               requirementType = r.requirementType,
                                                               isSelected = desirables.isAvaiable == true ? "true" : "false"
                                                           }).ToList();

            //List<HostelRequirements> hostelRequirements = (from r in db.jntuh_desirable_requirement_type.Where(r => r.isActive == true && r.isHostelRequirement == true)
            //                                               select new HostelRequirements
            //                                               {
            //                                                   requirementId = r.id,
            //                                                   requirementType = r.requirementType,
            //                                                   isSelected = "false"
            //                                               }).ToList();
            //foreach (var item in hostelRequirements)
            //{
            //    int existinfId = db.jntuh_college_hostel_maintenance.Where(d => d.collegeId == collegeId && d.requirementTypeID == item.id).Select(d => d.id).FirstOrDefault();

            //    if (existinfId > 0)
            //    {
            //        item.isSelected = "true";
            //    }
            //}

            if (hostelRequirements.Count() > 0)
            {
                foreach (var item in hostelRequirements)
                {
                    hostelRequirement += "<tr><td colspan='8' style='font-size: 9px;' width='547'><p>" + item.requirementType + "</p></td>";
                    if (item.requirementId == null)
                    {
                        hostelRequirement += "<td colspan='2'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes &nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</p></td>";
                    }
                    else
                    {
                        if (item.isSelected == "true")
                        {
                            hostelRequirement += "<td colspan='2' style='font-size: 9px; width='165'><p><img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;Yes&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No</p></td>";
                        }
                        else
                        {
                            hostelRequirement += "<td colspan='2' style='font-size: 9px; width='165'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No</p></td>";
                        }
                    }
                    hostelRequirement += "</tr>";
                }
            }
            else
            {
                string[] hostelFacility = db.jntuh_desirable_requirement_type.Where(hostel => hostel.isActive == true && hostel.isHostelRequirement == true)
                                                                           .Select(hostel => hostel.requirementType).ToArray();
                foreach (var item in hostelFacility)
                {
                    hostelRequirement += "<tr><td colspan='8' style='font-size: 9px;' width='547'><p>" + item + "</p></td>";
                    hostelRequirement += "<td colspan='2' style='font-size: 9px; width='165'><p><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;No</p></td>";
                    hostelRequirement += "</tr>";
                }
            }
            contents = contents.Replace("##COLLEGEHOSTELMAINTAINCEREQUIREMENTS##", hostelRequirement);
            return contents;
        }

        private string collegeOperationalFunds(int collegeId, string contents)
        {
            string strOperationalFunds = string.Empty;
            int count = 1;
            List<OperationalFunds> operationalFunds = db.jntuh_college_funds.Where(a => a.collegeId == collegeId).Select(a =>
                                              new OperationalFunds
                                              {
                                                  id = a.id,
                                                  collegeId = a.collegeId,
                                                  bankName = a.bankName,
                                                  bankBranch = a.bankBranch,
                                                  bankAddress = a.bankAddress,
                                                  bankAccountNumber = a.bankAccountNumber,
                                                  FDR = a.FDR,
                                                  cashBalance = a.cashBalance,
                                                  total = a.cashBalance + a.FDR
                                              }).ToList();

            if (operationalFunds.Count > 5)
            {
                foreach (var item in operationalFunds)
                {
                    strOperationalFunds += "<td colspan='1'><p align='center'>" + count + "</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>" + item.bankName + "</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>" + item.bankAddress + "</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>" + item.bankAccountNumber + "</p></td>";
                    strOperationalFunds += "<td colspan='2' style='font-size: 8px;'><p>" + item.cashBalance + "</p></td>";
                    strOperationalFunds += "<td colspan='2' style='font-size: 8px;'><p>" + item.FDR + "</p></td>";
                    strOperationalFunds += "<td colspan='2'><p>" + item.total + "</p></td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 5; i++)
                {
                    strOperationalFunds += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>##OPERATIONALFUNDSBANKNAME" + i + "##</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>##OPERATIONALFUNDSBANKADDRESS" + i + "##</p></td>";
                    strOperationalFunds += "<td colspan='4'><p>##OPERATIONALFUNDSACCNUM" + i + "##</p></td>";
                    strOperationalFunds += "<td colspan='2' style='font-size: 8px;' align='right'><p>##OPERATIONALFUNDSCASHBAL" + i + "##</p></td>";
                    strOperationalFunds += "<td colspan='2' style='font-size: 8px;' align='right'><p>##OPERATIONALFUNDSFDR" + i + "##</p></td>";
                    strOperationalFunds += "<td colspan='2' style='font-size: 8px;' align='right'><p>##OPERATIONALFUNDSTOTAL" + i + "##</p></td>";
                }
                foreach (var item in operationalFunds)
                {
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSBANKNAME" + count + "##", item.bankName);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSBANKADDRESS" + count + "##", item.bankAddress);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSACCNUM" + count + "##", item.bankAccountNumber);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSCASHBAL" + count + "##", item.cashBalance.ToString());
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSFDR" + count + "##", item.FDR.ToString());
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSTOTAL" + count + "##", item.total.ToString());
                    count++;
                }
                for (int i = 1; i <= 5; i++)
                {
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSBANKNAME" + i + "##", string.Empty);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSBANKADDRESS" + i + "##", string.Empty);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSACCNUM" + i + "##", string.Empty);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSCASHBAL" + i + "##", string.Empty);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSFDR" + i + "##", string.Empty);
                    strOperationalFunds = strOperationalFunds.Replace("##OPERATIONALFUNDSTOTAL" + i + "##", string.Empty);
                }
            }
            contents = contents.Replace("##COLLEGEOPERATIONALFUNDSINFORMATION##", strOperationalFunds);
            return contents;
        }

        private string collegeIncome(int collegeId, string contents)
        {
            string strCollegeIncome = string.Empty;
            int count = 1;
            decimal total = 0;
            List<CollegeIncome> incomeType = db.jntuh_college_income_type.Where(income => income.isActive == true)
                                                                             .Select(income => new CollegeIncome
                                                                             {
                                                                                 incomeTypeID = income.id,
                                                                                 incomeType = income.sourceOfIncome,
                                                                                 incomeAmount = 0,
                                                                             }).ToList();
            foreach (var item in incomeType)
            {
                item.incomeAmount = db.jntuh_college_income.Where(collegeIncome => collegeIncome.collegeId == collegeId &&
                                                                collegeIncome.incomeTypeID == item.incomeTypeID)
                                                         .Select(collegeIncome => collegeIncome.incomeAmount).FirstOrDefault();
                strCollegeIncome += "<td colspan='1' width='112'><p>" + count + "</p></td>";
                strCollegeIncome += "<td colspan='6' width='112'><p>" + item.incomeType + "</p></td>";
                strCollegeIncome += "<td colspan='3' width='112' align='right'><p>" + item.incomeAmount + "</p></td>";
                total += item.incomeAmount;
                count++;
            }
            contents = contents.Replace("##COLLEGEINCOMEINFORMATION##", strCollegeIncome);
            contents = contents.Replace("##COLLEGEINCOMEINFORMATIONTOTAL##", total.ToString());
            return contents;
        }

        private string collegeExpenditure(int collegeId, string contents)
        {
            string strCollegeExpenditure = string.Empty;
            int count = 1;
            decimal total = 0;
            List<CollegeExpenditure> collegeExpenditure = db.jntuh_college_expenditure_type.Where(expenditureType => expenditureType.isActive == true)
                                                                                           .Select(expenditureType => new CollegeExpenditure
                                                                                           {
                                                                                               expenditureTypeID = expenditureType.id,
                                                                                               expenditure = expenditureType.expenditure,
                                                                                               expenditureAmount = 0
                                                                                           }).ToList();
            foreach (var item in collegeExpenditure)
            {
                item.expenditureAmount = db.jntuh_college_expenditure.Where(e => e.collegeId == collegeId && e.expenditureTypeID == item.expenditureTypeID)
                                                                     .Select(e => e.expenditureAmount).FirstOrDefault();
                strCollegeExpenditure += "<td colspan='1' width='112'><p>" + count + "</p></td>";
                strCollegeExpenditure += "<td colspan='6' width='112'><p>" + item.expenditure + "</p></td>";
                strCollegeExpenditure += "<td colspan='3' width='112' align='right'><p>" + item.expenditureAmount + "</p></td>";
                total += item.expenditureAmount;
                count++;
            }
            contents = contents.Replace("##COLLEGEEXPENDITUREINFORMATION##", strCollegeExpenditure);
            contents = contents.Replace("##COLLEGEEXPENDITUREINFORMATIONTOTAL##", total.ToString());
            return contents;
        }

        private string collegeStudentpalcements(int collegeId, string contents)
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

            foreach (var item in placements)
            {
                CollegePlacement newPlacement = new CollegePlacement();
                newPlacement.id = item.id;
                newPlacement.collegeId = item.collegeId;
                newPlacement.academicYearId = item.academicYearId;
                newPlacement.specializationId = item.specializationId;
                newPlacement.specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newPlacement.departmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newPlacement.department = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.departmentName).FirstOrDefault();
                newPlacement.degreeID = db.jntuh_department.Where(d => d.id == newPlacement.departmentID).Select(d => d.degreeId).FirstOrDefault();
                newPlacement.degree = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degree).FirstOrDefault();
                newPlacement.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newPlacement.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                collegePlacement.Add(newPlacement);
            }

            collegePlacement = collegePlacement.AsEnumerable().GroupBy(p => p.specializationId).Select(p => p.First()).ToList();
            collegePlacement = collegePlacement.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.department).ThenBy(ei => ei.specialization).ToList();

            foreach (var item in collegePlacement)
            {
                item.totalStudentsPassed1 = GetStudents(collegeId, AY1, item.specializationId, 1);
                item.totalStudentsPlaced1 = GetStudents(collegeId, AY1, item.specializationId, 0);

                item.totalStudentsPassed2 = GetStudents(collegeId, AY2, item.specializationId, 1);
                item.totalStudentsPlaced2 = GetStudents(collegeId, AY2, item.specializationId, 0);

                item.totalStudentsPassed3 = GetStudents(collegeId, AY3, item.specializationId, 1);
                item.totalStudentsPlaced3 = GetStudents(collegeId, AY3, item.specializationId, 0);
            }
            collegePlacement = collegePlacement.OrderBy(p => p.degree).ToList();
            if (collegePlacement.Count() > 10)
            {
                foreach (var item in collegePlacement)
                {
                    item.totalStudentsPassed1 = GetStudents(collegeId, AY1, item.specializationId, 1);
                    item.totalStudentsPlaced1 = GetStudents(collegeId, AY1, item.specializationId, 0);

                    item.totalStudentsPassed2 = GetStudents(collegeId, AY2, item.specializationId, 1);
                    item.totalStudentsPlaced2 = GetStudents(collegeId, AY2, item.specializationId, 0);

                    item.totalStudentsPassed3 = GetStudents(collegeId, AY3, item.specializationId, 1);
                    item.totalStudentsPlaced3 = GetStudents(collegeId, AY3, item.specializationId, 0);
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
                    strCollegePlacement += "<td><p>" + item.totalStudentsPassed3 + "</p></td>";
                    strCollegePlacement += "<td><p>" + item.totalStudentsPlaced3 + "</p></td>";
                    strCollegePlacement += "<td><p>" + item.totalStudentsPassed2 + "</p></td>";
                    strCollegePlacement += "<td><p>" + item.totalStudentsPlaced2 + "</p></td>";
                    strCollegePlacement += "<td><p>" + item.totalStudentsPassed2 + "</p></td>";
                    strCollegePlacement += "<td><p>" + item.totalStudentsPlaced2 + "</p></td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 10; i++)
                {
                    strCollegePlacement += "<td colspan='1'><p align='center'>" + i + "</p></td>";
                    strCollegePlacement += "<td colspan='2' style='font-size:9px'><p>##COLLEGEPLACEMENTDEGREE" + i + "##</p></td>";
                    strCollegePlacement += "<td colspan='2' style='font-size:9px'><p>##COLLEGEPLACEMENTDEPARTMENT" + i + "##</p></td>";
                    strCollegePlacement += "<td colspan='3' style='font-size:9px'><p>##COLLEGEPLACEMENTSPECIALIZATION" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPASSEDTHREE" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPLASEDTHREE" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPASSEDTWO" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPLASEDTWO" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPASSEDONE" + i + "##</p></td>";
                    strCollegePlacement += "<td align='center'><p>##COLLEGEPLACEMENTSTUDENTSPLASEDONE" + i + "##</p></td>";

                }
                foreach (var item in collegePlacement)
                {
                    item.totalStudentsPassed1 = GetStudents(collegeId, AY1, item.specializationId, 1);
                    item.totalStudentsPlaced1 = GetStudents(collegeId, AY1, item.specializationId, 0);

                    item.totalStudentsPassed2 = GetStudents(collegeId, AY2, item.specializationId, 1);
                    item.totalStudentsPlaced2 = GetStudents(collegeId, AY2, item.specializationId, 0);

                    item.totalStudentsPassed3 = GetStudents(collegeId, AY3, item.specializationId, 1);
                    item.totalStudentsPlaced3 = GetStudents(collegeId, AY3, item.specializationId, 0);

                    totalpassed1 += item.totalStudentsPassed1;
                    totalplaced1 += item.totalStudentsPlaced1;
                    totalpassed2 += item.totalStudentsPassed2;
                    totalplaced2 += item.totalStudentsPlaced2;
                    totalpassed3 += item.totalStudentsPassed3;
                    totalplaced3 += item.totalStudentsPlaced3;

                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTDEGREE" + count + "##", item.degree);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTDEPARTMENT" + count + "##", item.department);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSPECIALIZATION" + count + "##", item.specialization);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDONE" + count + "##", item.totalStudentsPassed1.ToString());
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDONE" + count + "##", item.totalStudentsPlaced1.ToString());
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDTWO" + count + "##", item.totalStudentsPassed2.ToString());
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDTWO" + count + "##", item.totalStudentsPlaced2.ToString());
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDTHREE" + count + "##", item.totalStudentsPassed3.ToString());
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDTHREE" + count + "##", item.totalStudentsPlaced3.ToString());
                    count++;
                }
                for (int i = 1; i <= 10; i++)
                {
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTDEGREE" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTDEPARTMENT" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSPECIALIZATION" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDONE" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDONE" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDTWO" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDTWO" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPASSEDTHREE" + i + "##", string.Empty);
                    strCollegePlacement = strCollegePlacement.Replace("##COLLEGEPLACEMENTSTUDENTSPLASEDTHREE" + i + "##", string.Empty);
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

        private int? GetStudents(int collegeId, int academicYearId, int specializationId, int flag)
        {
            int? student = 0;

            if (flag == 1)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
            else
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
            return student == null ? (int?)null : Convert.ToInt32(student);
        }

        private string collegeComputerLab(int collegeId, string contents)
        {
            jntuh_college_computer_lab computetLabDetails = db.jntuh_college_computer_lab.Where(c => c.collegeId == collegeId).FirstOrDefault();
            if (computetLabDetails != null)
            {
                if (computetLabDetails.printersAvailability == true)
                {
                    contents = contents.Replace("##COLLEGECOMPUTERSLABPRINTERSAVAILABILITYINFORMATION##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }
                else
                {
                    contents = contents.Replace("##COLLEGECOMPUTERSLABPRINTERSAVAILABILITYINFORMATION##", string.Format("<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;{0} &nbsp;&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_yes_b.png' height='10' />&nbsp;{1}", "Yes", "No"));
                }
                contents = contents.Replace("##COLLEGECOMPUTERSLABWORKINFHOURSFROM##", computetLabDetails.labWorkingHoursFrom.ToString(@"hh\:mm"));
                contents = contents.Replace("##COLLEGECOMPUTERSLABWORKINFHOURSTO##", computetLabDetails.labWorkingHoursTo.ToString(@"hh\:mm"));
                contents = contents.Replace("##COLLEGEINTERNETWORKINFHOURSFROM##", computetLabDetails.internetAccessibilityFrom.ToString(@"hh\:mm"));
                contents = contents.Replace("##COLLEGEINTERNETWORKINFHOURSTO##", computetLabDetails.internetAccessibilityTo.ToString(@"hh\:mm"));

            }
            else
            {
                contents = contents.Replace("##COLLEGECOMPUTERSLABPRINTERSAVAILABILITYINFORMATION##", string.Format("{0}&nbsp;Yes {1}&nbsp;No", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />", "<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />"));

                contents = contents.Replace("##COLLEGECOMPUTERSLABWORKINFHOURSFROM##", "____________");
                contents = contents.Replace("##COLLEGECOMPUTERSLABWORKINFHOURSTO##", "____________");
                contents = contents.Replace("##COLLEGEINTERNETWORKINFHOURSFROM##", "____________");
                contents = contents.Replace("##COLLEGEINTERNETWORKINFHOURSTO##", "____________");
                //contents = contents.Replace("##COLLEGECOMPUTERSLABPRINTERSAVAILABILITYINFORMATION##", "____________");
            }
            return contents;
        }

        private string collegeTeachingFacultyPosition(int collegeId, string contents)
        {
            int[] fId;
            string teachingFacultyPosition = string.Empty;
            int ProfessorDesignationId = 0;
            int assistentProfessorDesignationId = 0;
            int associateProfessorDesignationId = 0;
            int designationId = 0;
            int ProfessorCount = 0;
            int assistentProfessorCount = 0;
            int associateProfessorCount = 0;
            int[] facultyId;
            bool professorRatified;
            bool assistantProfessorRatified;
            bool associateProfessorRatified;
            int ProfessorRatifiedCount = 0;
            int assistentProfessorRatifiedCount = 0;
            int associateProfessorRatifiedCount = 0;
            int count = 1;
            int total = 0;
            int TotalIntake = 0;
            int TotalProfessorsCount = 0;
            int TotalAssistantProfessorsCount = 0;
            int TotalAssociateProfessorsCount = 0;
            int TotalRatifiedCount = 0;
            int facultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            ProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Professor").Select(d => d.id).FirstOrDefault();
            assistentProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Assistant Professor").Select(d => d.id).FirstOrDefault();
            associateProfessorDesignationId = db.jntuh_designation.Where(d => d.designation == "Associate Professor").Select(d => d.id).FirstOrDefault();
            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.id).FirstOrDefault();

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId && p.isActive == true).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.collegeId = item.collegeId;
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                collegeIntakeProposedList.Add(newProposed);
            }
            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
            facultyId = db.jntuh_college_faculty.Where(f => f.collegeId == collegeId && f.facultyTypeId == facultyTypeId).Select(f => f.id).ToArray();

            //HCode1
            var degree = db.jntuh_college_degree.Where(cd => cd.collegeId == collegeId && cd.jntuh_degree.degree == "B.Tech").Select(cd => cd).FirstOrDefault();
            if (degree != null)
            {
                count = count + 1;
                int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
                int Humanitiesintake = db.jntuh_college_intake_proposed.Where(pi => pi.collegeId == collegeId
                                                                    && pi.jntuh_specialization.jntuh_department.jntuh_degree.degree == "B.Tech"
                                                                    && pi.academicYearId == AY1).Select(pi => pi.proposedIntake).Sum();

                teachingFacultyPosition += "<td colspan='2' width='45'><p align='center'>" + 1 + "</p></td>";
                teachingFacultyPosition += "<td colspan='3' style='font-size: 9px;' width='96'>B.Tech</td>";
                teachingFacultyPosition += "<td colspan='4' style='font-size: 9px;' width='74'>Humanities</td>";
                teachingFacultyPosition += "<td colspan='5' style='font-size: 9px;' width='196'>Humanities</td>";
                teachingFacultyPosition += "<td width='41' colspan='2' align='center'>1</td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + Humanitiesintake + "</td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'></td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'></td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'></td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'></td>";
                teachingFacultyPosition += "<td width='90' colspan='2' align='center'>&nbsp;</td>";
            }
            //
            if (proposed.Count > 15)
            {

                foreach (var item in collegeIntakeProposedList)
                {
                    ProfessorCount = 0;
                    assistentProfessorCount = 0;
                    associateProfessorCount = 0;
                    ProfessorRatifiedCount = 0;
                    assistentProfessorRatifiedCount = 0;
                    associateProfessorRatifiedCount = 0;

                    fId = db.jntuh_faculty_subjects.Where(s => facultyId.Contains(s.facultyId) && s.specializationId == item.specializationId &&
                                                                   s.shiftId == item.shiftId)
                                                        .Select(s => s.facultyId)
                                                        .Distinct()
                                                        .ToArray();
                    foreach (var id in fId)
                    {
                        designationId = db.jntuh_college_faculty.Where(f => f.id == id).Select(d => d.facultyDesignationId).FirstOrDefault();

                        if (designationId == ProfessorDesignationId)
                        {
                            ProfessorCount++;
                            professorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (professorRatified == true)
                            {
                                ProfessorRatifiedCount++;
                            }
                        }
                        if (designationId == assistentProfessorDesignationId)
                        {
                            assistentProfessorCount++;
                            assistantProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (assistantProfessorRatified == true)
                            {
                                assistentProfessorRatifiedCount++;
                            }
                        }
                        if (designationId == associateProfessorDesignationId)
                        {
                            associateProfessorCount++;
                            associateProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (associateProfessorRatified == true)
                            {
                                associateProfessorRatifiedCount++;
                            }
                        }
                    }

                    total = ProfessorRatifiedCount + assistentProfessorRatifiedCount + associateProfessorRatifiedCount;
                    teachingFacultyPosition += "<td colspan='2' width='45'><p align='center'>" + count + "</p></td>";
                    teachingFacultyPosition += "<td colspan='3' style='font-size: 9px;' width='96'>" + item.Degree + "</td>";
                    teachingFacultyPosition += "<td colspan='4' style='font-size: 9px;' width='74'>" + item.Department + "</td>";
                    teachingFacultyPosition += "<td colspan='5' style='font-size: 9px;' width='196'>" + item.Specialization + "</td>";
                    teachingFacultyPosition += "<td width='41' colspan='2' align='center'>" + item.Shift + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + FacultyTotalIntake(item.degreeID, item.specializationId, item.shiftId, collegeId).ToString() + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + ProfessorCount.ToString() + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + associateProfessorCount.ToString() + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + assistentProfessorCount.ToString() + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>" + total.ToString() + "</td>";
                    teachingFacultyPosition += "<td width='90' colspan='2' align='center'>&nbsp;</td>";
                    count++;
                    TotalIntake += FacultyTotalIntake(item.degreeID, item.specializationId, item.shiftId, collegeId);
                    TotalProfessorsCount += ProfessorCount;
                    TotalAssociateProfessorsCount += associateProfessorCount;
                    TotalAssistantProfessorsCount += assistentProfessorCount;
                    TotalRatifiedCount += total;
                }
            }
            else
            {
                for (int i = 1; i <= 15; i++)
                {
                    teachingFacultyPosition += "<td colspan='2' width='45'><p align='center'>" + i + "</p></td>";
                    teachingFacultyPosition += "<td colspan='3' style='font-size: 9px;' width='96'>##COLLEGEFACULTYDEGREE" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='4' style='font-size: 9px;' width='74'>##COLLEGEFACULTYDEPARTMENT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='5' style='font-size: 9px;' width='196'>##COLLEGEFACULTYSPECIALIZATION" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='41' align='center'>##COLLEGEFACULTYSHIFT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>##COLLEGEFACULTYPROPOSEDINTAKE" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>##COLLEGEFACULTYPROFESSORCOUNT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>##COLLEGEFACULTYASSOCIATEPROFESSORCOUNT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>##COLLEGEFACULTYASSISTANTPROFESSORCOUNT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>##COLLEGEFACULTYTOTALCOUNT" + i + "##</td>";
                    teachingFacultyPosition += "<td colspan='2' width='90' align='center'>&nbsp;</td>";
                }

                foreach (var item in collegeIntakeProposedList)
                {
                    ProfessorCount = 0;
                    assistentProfessorCount = 0;
                    associateProfessorCount = 0;
                    ProfessorRatifiedCount = 0;
                    assistentProfessorRatifiedCount = 0;
                    associateProfessorRatifiedCount = 0;

                    fId = db.jntuh_faculty_subjects.Where(s => facultyId.Contains(s.facultyId) && s.specializationId == item.specializationId &&
                                                                   s.shiftId == item.shiftId)
                                                        .Select(s => s.facultyId)
                                                        .Distinct()
                                                        .ToArray();
                    foreach (var id in fId)
                    {
                        designationId = db.jntuh_college_faculty.Where(f => f.id == id).Select(d => d.facultyDesignationId).FirstOrDefault();

                        if (designationId == ProfessorDesignationId)
                        {
                            ProfessorCount++;
                            professorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (professorRatified == true)
                            {
                                ProfessorRatifiedCount++;
                            }
                        }
                        if (designationId == assistentProfessorDesignationId)
                        {
                            assistentProfessorCount++;
                            assistantProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (assistantProfessorRatified == true)
                            {
                                assistentProfessorRatifiedCount++;
                            }
                        }
                        if (designationId == associateProfessorDesignationId)
                        {
                            associateProfessorCount++;
                            associateProfessorRatified = db.jntuh_college_faculty.Where(r => r.id == id).Select(r => r.isFacultyRatifiedByJNTU).FirstOrDefault();
                            if (associateProfessorRatified == true)
                            {
                                associateProfessorRatifiedCount++;
                            }
                        }
                    }
                    total = ProfessorRatifiedCount + assistentProfessorRatifiedCount + associateProfessorRatifiedCount;
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYDEGREE" + count + "##", item.Degree);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYDEPARTMENT" + count + "##", item.Department);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYSPECIALIZATION" + count + "##", item.Specialization);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYSHIFT" + count + "##", item.Shift);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYPROPOSEDINTAKE" + count + "##", FacultyTotalIntake(item.degreeID, item.specializationId, item.shiftId, collegeId).ToString());
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYPROFESSORCOUNT" + count + "##", ProfessorCount.ToString());
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYASSOCIATEPROFESSORCOUNT" + count + "##", associateProfessorCount.ToString());
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYASSISTANTPROFESSORCOUNT" + count + "##", assistentProfessorCount.ToString());
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYTOTALCOUNT" + count + "##", total.ToString());
                    count++;
                    TotalIntake += FacultyTotalIntake(item.degreeID, item.specializationId, item.shiftId, collegeId);
                    TotalProfessorsCount += ProfessorCount;
                    TotalAssociateProfessorsCount += associateProfessorCount;
                    TotalAssistantProfessorsCount += assistentProfessorCount;
                    TotalRatifiedCount += total;
                }

                for (int i = 1; i <= 15; i++)
                {
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYDEGREE" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYDEPARTMENT" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYSPECIALIZATION" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYSHIFT" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYPROPOSEDINTAKE" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYPROFESSORCOUNT" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYASSOCIATEPROFESSORCOUNT" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYASSISTANTPROFESSORCOUNT" + i + "##", string.Empty);
                    teachingFacultyPosition = teachingFacultyPosition.Replace("##COLLEGEFACULTYTOTALCOUNT" + i + "##", string.Empty);
                }

            }
            contents = contents.Replace("##COLLEGETEACHINGFACULTYTYPEPOSITIONDEPARTMENTWISEINFORMATION##", teachingFacultyPosition);
            contents = contents.Replace("##COLLEGENEWTOTALINTAKE##", TotalIntake.ToString());
            contents = contents.Replace("##COLLEGENEWPROFESSORSCOUNT##", TotalProfessorsCount.ToString());
            contents = contents.Replace("##COLLEGENEWASSOCIATEPROFESSORSCOUNT##", TotalAssociateProfessorsCount.ToString());
            contents = contents.Replace("##COLLEGENEWASSISTANTPROFESSORSCOUNT##", TotalAssistantProfessorsCount.ToString());
            contents = contents.Replace("##COLLEGENEWRATIFIEDCOUNT##", TotalRatifiedCount.ToString());
            return contents;
        }

        private int FacultyTotalIntake(int degreeId, int specializationId, int shiftId, int collegeId)
        {
            int totalIntake = 0;
            int duration = Convert.ToInt32(db.jntuh_degree.Where(d => d.id == degreeId).Select(d => d.degreeDuration).FirstOrDefault());
            int presentAcademicYearId = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.id == presentAcademicYearId).Select(a => a.actualYear).FirstOrDefault();
            int AcademicYearId1 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId2 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 1)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId3 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 2)).Select(a => a.id).FirstOrDefault();
            int AcademicYearId4 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (actualYear - 3)).Select(a => a.id).FirstOrDefault();

            int totalIntake1 = 0;
            int totalIntake2 = 0;
            int totalIntake3 = 0;
            int totalIntake4 = 0;
            int totalIntake5 = 0;

            totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.proposedIntake).FirstOrDefault();
            totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == shiftId).Select(a => a.approvedIntake).FirstOrDefault();
            int[] specializationsId = (from d in db.jntuh_college_degree
                                       join de in db.jntuh_department on d.degreeId equals de.degreeId
                                       join s in db.jntuh_specialization on de.id equals s.departmentId
                                       join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
                                       where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
                                       select ProposedIntakeExisting.specializationId).Distinct().ToArray();
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
            return totalIntake;
        }

        //private string collegeFacultyStudentRatio(int collegeId, string contents)
        //{
        //    string facultyStudentRatio = string.Empty;
        //    int degreeId = 0;
        //    string degree = string.Empty;
        //    int totalIntake = 0;
        //    int totalFaculty = 0;
        //    List<jntuh_degree> collegeDegree = db.jntuh_degree.Where(d => d.isActive == true).OrderBy(d => d.degreeDisplayOrder).ToList();
        //    foreach (var item in collegeDegree)
        //    {
        //        degreeId = item.id;
        //        degree = item.degree;
        //        totalIntake = GetIntake(item.id, collegeId);
        //        totalFaculty = getTotalFaculty(item.id, collegeId);
        //        facultyStudentRatio += "<td width='112' colspan='2'><p align='left'>" + item.degree + "</p></td>";
        //        facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>" + totalIntake + "</td>";
        //        facultyStudentRatio += "<td width='98' colspan='2'><p align='center'>" + totalFaculty + "</td>";
        //        facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>&nbsp;</td>";
        //        facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>&nbsp;</td>";
        //    }
        //    contents = contents.Replace("##COLLEGEFACULTYSTUDENTRATIONDETAILSINFORMATION##", facultyStudentRatio);
        //    return contents;
        //}

        //private int getTotalFaculty(int degreeId, int collegeId)
        //{
        //    int totalFaculty = 0;
        //    int[] fId;
        //    int otherDesignationId = db.jntuh_designation.Where(d => d.designation == "Other").Select(d => d.id).FirstOrDefault();
        //    int facultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
        //    int[] facultyId = db.jntuh_college_faculty
        //                        .Where(f => f.collegeId == collegeId &&
        //                                    f.facultyTypeId == facultyTypeId &&
        //                                    f.facultyDesignationId != otherDesignationId)
        //                        .Select(f => f.id)
        //                        .ToArray();
        //    int[] specializationsId = (from d in db.jntuh_college_degree
        //                               join de in db.jntuh_department on d.degreeId equals de.degreeId
        //                               join s in db.jntuh_specialization on de.id equals s.departmentId
        //                               join ProposedIntakeExisting in db.jntuh_college_intake_proposed on s.id equals ProposedIntakeExisting.specializationId
        //                               where (d.degreeId == degreeId && d.isActive == true && d.collegeId == collegeId && ProposedIntakeExisting.collegeId == collegeId)
        //                               select ProposedIntakeExisting.specializationId).Distinct().ToArray();
        //    foreach (var item in specializationsId)
        //    {
        //        int[] shiftId = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == item).Select(e => e.shiftId).ToArray();
        //        foreach (var sId in shiftId)
        //        {
        //            fId = db.jntuh_faculty_subjects.Where(s => facultyId.Contains(s.facultyId) && s.specializationId == item &&
        //                                                           s.shiftId == sId)
        //                                                .Select(s => s.facultyId)
        //                                                .Distinct()
        //                                                .ToArray();
        //            totalFaculty += fId.Count();
        //        }
        //    }
        //    return totalFaculty;
        //}

        private string collegeFacultyStudentRatio(int collegeId, string contents)
        {
            string facultyStudentRatio = string.Empty;
            int degreeId = 0;
            string degree = string.Empty;
            int totalIntake = 0;
            int totalFaculty = 0;
            int otherDesignationId = db.jntuh_designation.Where(d => d.designation == "Other").Select(d => d.id).FirstOrDefault();
            int facultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            int[] facultyId = db.jntuh_college_faculty
                                .Where(f => f.collegeId == collegeId &&
                                            f.facultyTypeId == facultyTypeId &&
                                            f.facultyDesignationId != otherDesignationId)
                                .Select(f => f.id)
                                .ToArray();
            List<jntuh_degree> collegeDegree = db.jntuh_degree.Where(d => d.isActive == true).OrderBy(d => d.degreeDisplayOrder).ToList();
            foreach (var item in collegeDegree)
            {
                degreeId = item.id;
                degree = item.degree;
                totalIntake = GetIntake(item.id, collegeId);
                totalFaculty = getTotalFaculty(item.id, collegeId, facultyId);
                facultyStudentRatio += "<td width='112' colspan='2'><p align='left'>" + item.degree + "</p></td>";
                facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>" + totalIntake + "</td>";
                facultyStudentRatio += "<td width='98' colspan='2'><p align='center'>" + totalFaculty + "</td>";
                facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>&nbsp;</td>";
                facultyStudentRatio += "<td width='98' colspan='3'><p align='center'>&nbsp;</td>";
            }
            contents = contents.Replace("##COLLEGEFACULTYSTUDENTRATIONDETAILSINFORMATION##", facultyStudentRatio);
            return contents;
        }

        private int getTotalFaculty(int degreeId, int collegeId, int[] facultyId)
        {
            int totalFaculty = 0;

            int[] facultyDegreeCount = (from fs in db.jntuh_faculty_subjects
                                        join e in db.jntuh_college_intake_proposed on new { fs.specializationId, fs.shiftId } equals new { e.specializationId, e.shiftId }
                                        join s in db.jntuh_specialization on e.specializationId equals s.id
                                        join dp in db.jntuh_department on s.departmentId equals dp.id
                                        join d in db.jntuh_college_degree on dp.degreeId equals d.degreeId
                                        where (facultyId.Contains(fs.facultyId) &&
                                               d.degreeId == degreeId &&
                                               d.collegeId == collegeId &&
                                               d.isActive == true &&
                                               e.collegeId == collegeId &&
                                               e.isActive == true)
                                        select fs.facultyId).Distinct().ToArray();
            totalFaculty = facultyDegreeCount.Count();
            return totalFaculty;
        }

        private string collegeCommitteeMembers(int collegeId, string contents)
        {
            string collegeCommitteeMember = string.Empty;
            int count = 1;
            string ffcDesignation = string.Empty;
            int auditScheduleId = db.jntuh_ffc_schedule.Where(schedule => schedule.collegeID == collegeId).Select(schedule => schedule.id).FirstOrDefault();
            if (auditScheduleId != 0)
            {
                List<jntuh_ffc_committee> ffcCommittee = db.jntuh_ffc_committee.OrderByDescending(committee => committee.isConvenor).Where(committee => committee.scheduleID == auditScheduleId).ToList();
                if (ffcCommittee.Count > 4)
                {
                    foreach (var item in ffcCommittee)
                    {
                        collegeCommitteeMember += "<td colspan='1' width='63'><p align='center'>" + count + "</p></td>";

                        jntuh_ffc_auditor ffcAuditor = db.jntuh_ffc_auditor.Where(auditor => auditor.id == item.auditorID).FirstOrDefault();
                        ffcDesignation = db.jntuh_designation.Where(designation => designation.id == ffcAuditor.auditorDesignationID).Select(designation => designation.designation).FirstOrDefault();
                        if (ffcAuditor != null)
                        {
                            if (item.isConvenor == 0)
                            {
                                collegeCommitteeMember += "<td colspan='4' width='245'><p>" + ffcAuditor.auditorName + " (Member)</p></td>";
                                collegeCommitteeMember += "<td colspan='2' width='120'><p align='center'>" + ffcDesignation + "</p></td>";
                            }
                            else
                            {
                                collegeCommitteeMember += "<td colspan='4' width='245'><p>" + ffcAuditor.auditorName + " (Convenor)</p></td>";
                                collegeCommitteeMember += "<td colspan='2' width='120'><p align='center'>" + ffcDesignation + "</p></td>";
                            }
                        }
                        collegeCommitteeMember += "<td colspan='3' width='218'>&nbsp;</td>";
                        count++;
                    }
                }
                else
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        collegeCommitteeMember += "<td colspan='1' width='63'><p align='center'>" + i + "</p></td>";
                        collegeCommitteeMember += "<td colspan='4' width='245'><p>##FFCCOMMITTEETEAMMEMBERNAME" + i + "##</p></td>";
                        collegeCommitteeMember += "<td colspan='2' width='120'><p align='center'>##FFCCOMMITTEETEAMMEMBERDESIGNATION" + i + "##</p></td>";
                        collegeCommitteeMember += "<td colspan='3' width='218'>&nbsp;</td>";
                    }
                    foreach (var item in ffcCommittee)
                    {
                        jntuh_ffc_auditor ffcAuditor = db.jntuh_ffc_auditor.Where(auditor => auditor.id == item.auditorID).FirstOrDefault();
                        ffcDesignation = db.jntuh_designation.Where(designation => designation.id == ffcAuditor.auditorDesignationID).Select(designation => designation.designation).FirstOrDefault();
                        if (ffcAuditor != null)
                        {
                            if (item.isConvenor == 0)
                            {
                                collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERNAME" + count + "##", ffcAuditor.auditorName + " (Member)");
                                collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERDESIGNATION" + count + "##", ffcDesignation);
                            }
                            else
                            {
                                collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERNAME" + count + "##", ffcAuditor.auditorName + " (Convener)");
                                collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERDESIGNATION" + count + "##", ffcDesignation);
                            }
                        }
                        count++;
                    }
                    for (int i = 1; i <= 4; i++)
                    {
                        collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERNAME" + i + "##", string.Empty);
                        collegeCommitteeMember = collegeCommitteeMember.Replace("##FFCCOMMITTEETEAMMEMBERDESIGNATION" + i + "##", string.Empty);
                    }
                }
            }
            contents = contents.Replace("##AUDITCOMMITTEEMEMBERSINFORMATION##", collegeCommitteeMember);
            return contents;
        }

        private string collegeEnclosures(int collegeId, string contents)
        {
            string collegeEnclosures = string.Empty;

            var enclosures = db.jntuh_enclosures.Where(d => d.isActive == true).Select(d => d).ToList();

            collegeEnclosures += "<tr>";
            collegeEnclosures += "<td colspan='1'><p align='center'>S.No</p></td>";
            collegeEnclosures += "<td colspan='12' align='left'><p>Document Name</p></td>";
            collegeEnclosures += "<td colspan='3'><p align='center'>Enclosed</p></td>";
            collegeEnclosures += "</tr>";

            int count = 1;
            foreach (var item in enclosures)
            {
                collegeEnclosures += "<tr>";
                collegeEnclosures += "<td colspan='1' align='center'><p align='center'>" + count + "</p></td>";
                collegeEnclosures += "<td colspan='12' align='left'><p>" + item.documentName + "</p></td>";
                collegeEnclosures += "<td colspan='3' align='center'><p align='center'><img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;Yes&nbsp;<img src='http://localhost:49713/Content/Images/checkbox_no_b.png' height='10' />&nbsp;No&nbsp;</p></td>";
                collegeEnclosures += "</tr>";

                count++;
            }

            contents = contents.Replace("##COLLEGEENCLOSURES##", collegeEnclosures);
            return contents;
        }

        private string collegeFacultyMembers(int collegeId, string contents)
        {
            string collegeFaculty = string.Empty;
            int count = 1;
            string gender = string.Empty;
            string category = string.Empty;
            string department = string.Empty;
            string designation = string.Empty;
            int qualificationId = 0;
            string qualification = string.Empty;
            string dateOfAppointment = string.Empty;
            string teachingType = string.Empty;
            int teachingFacultyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            string ratified = string.Empty;
            List<jntuh_college_faculty> facultyList = db.jntuh_college_faculty.OrderBy(faculty => faculty.facultyTypeId)
                                                                              .Where(faculty => faculty.collegeId == collegeId)
                                                                              .ToList();
            facultyList = facultyList.OrderBy(faculty => faculty.facultyDepartmentId).ToList();
            facultyList = facultyList.OrderBy(faculty => faculty.facultyDesignationId).ToList();
            if (facultyList.Count() > 20)
            {
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
                    category = db.jntuh_faculty_category.Where(f => f.id == item.facultyCategoryId).Select(f => f.facultyCategory).FirstOrDefault();
                    department = db.jntuh_department.Where(d => d.id == item.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    designation = db.jntuh_designation.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
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
                    qualificationId = db.jntuh_faculty_education.OrderByDescending(education => education.educationId)
                                                              .Where(education => education.facultyId == item.id)
                                                              .Select(education => education.educationId)
                                                              .FirstOrDefault();
                    qualification = db.jntuh_education_category.Where(education => education.id == qualificationId).Select(education => education.educationCategoryName).FirstOrDefault();
                    teachingType = db.jntuh_faculty_type.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty += "<td colspan='1' width='37'><p align='center'>" + count + "</p></td>";
                    collegeFaculty += "<td colspan='2' width='155'><p p align='left'>" + item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='30'><p align='center'>" + gender + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='71'><p align='center'>" + category + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='83'><p align='center'>" + department + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='84'><p align='center'>" + designation + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='90'><p align='center'>" + qualification + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='78'><p align='center'>" + item.facultyPreviousExperience + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='90'><p align='center'>" + dateOfAppointment + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='138'><p align='center'>" + item.facultyPayScale + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='99'><p align='center'>" + teachingType + "</p></td>";
                    collegeFaculty += "<td colspan='1' width='65'><p align='center'>" + ratified + "</p></td>";
                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 20; i++)
                {
                    collegeFaculty += "<td colspan='1' width='37'><p align='center'>" + i + "</p></td>";
                    collegeFaculty += "<td colspan='2' width='155'><p p align='left'>##FACULTYINFORMATIONNAME" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='30'><p align='center'>##FACULTYINFORMATIONGENDER" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='71'><p align='center'>##FACULTYINFORMATIONCATEGORY" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='83'><p align='center'>##FACULTYINFORMATIONDEPARTMENT" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='84'><p align='center'>##FACULTYINFORMATIONDESIGNATION" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='90'><p align='center'>##FACULTYINFORMATIONQUALIFICATION" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='78'><p align='center'>##FACULTYINFORMATIONEXPERIENCE" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='90'><p align='center'>##FACULTYINFORMATIONDATEOFAPPOINTMENT" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='138'><p align='center'>##FACULTYINFORMATIONPAYSCALE" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='99'><p align='center'>##FACULTYINFORMATIONTEACHINGTYPE" + i + "##</p></td>";
                    collegeFaculty += "<td colspan='1' width='65'><p align='center'>##FACULTYINFORMATIONRATIFIED" + i + "##</p></td>";
                }
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
                    category = db.jntuh_faculty_category.Where(f => f.id == item.facultyCategoryId).Select(f => f.facultyCategory).FirstOrDefault();
                    department = db.jntuh_department.Where(d => d.id == item.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                    designation = db.jntuh_designation.Where(d => d.id == item.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
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
                    qualificationId = db.jntuh_faculty_education.OrderByDescending(education => education.educationId)
                                                              .Where(education => education.facultyId == item.id)
                                                              .Select(education => education.educationId)
                                                              .FirstOrDefault();
                    qualification = db.jntuh_education_category.Where(education => education.id == qualificationId).Select(education => education.educationCategoryName).FirstOrDefault();
                    teachingType = db.jntuh_faculty_type.Where(f => f.id == item.facultyTypeId).Select(f => f.facultyType).FirstOrDefault();
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONNAME" + count + "##", item.facultyFirstName + " " + item.facultyLastName + " " + item.facultySurname);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONGENDER" + count + "##", gender);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONCATEGORY" + count + "##", category);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDEPARTMENT" + count + "##", department);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDESIGNATION" + count + "##", designation);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONQUALIFICATION" + count + "##", qualification);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONEXPERIENCE" + count + "##", item.facultyPreviousExperience.ToString());
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDATEOFAPPOINTMENT" + count + "##", dateOfAppointment);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONPAYSCALE" + count + "##", item.facultyPayScale);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONTEACHINGTYPE" + count + "##", teachingType);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONRATIFIED" + count + "##", ratified);
                    count++;
                }
                for (int i = 1; i <= 20; i++)
                {
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONNAME" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONGENDER" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONCATEGORY" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDEPARTMENT" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDESIGNATION" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONQUALIFICATION" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONEXPERIENCE" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONDATEOFAPPOINTMENT" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONPAYSCALE" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONTEACHINGTYPE" + i + "##", string.Empty);
                    collegeFaculty = collegeFaculty.Replace("##FACULTYINFORMATIONRATIFIED" + i + "##", string.Empty);
                }
            }

            contents = contents.Replace("##COLLEGECONSOLIDATEDFACULTYINFORMATION##", collegeFaculty);
            return contents;
        }

        private string collegeTotalFacultyMembers(int collegeId, string contents)
        {
            int teachingFacultyCount = 0;
            int NonTeachingFacultyCount = 0;
            int TechnicalFacultyCount = 0;
            int teachingFalutyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Teaching").Select(f => f.id).FirstOrDefault();
            int NonTeachingFalutyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Non-Teaching").Select(f => f.id).FirstOrDefault();
            int technicalFalutyTypeId = db.jntuh_faculty_type.Where(f => f.facultyType == "Technical").Select(f => f.id).FirstOrDefault();
            List<jntuh_college_faculty> teachingFacultyDetails = db.jntuh_college_faculty.Where(f => f.collegeId == collegeId && f.facultyTypeId == teachingFalutyTypeId).ToList();
            List<jntuh_college_faculty> NonTeachingFacultyDetails = db.jntuh_college_faculty.Where(f => f.collegeId == collegeId && f.facultyTypeId == NonTeachingFalutyTypeId).ToList();
            List<jntuh_college_faculty> technicalFacultyDetails = db.jntuh_college_faculty.Where(f => f.collegeId == collegeId && f.facultyTypeId == technicalFalutyTypeId).ToList();
            teachingFacultyCount = teachingFacultyDetails.Count();
            NonTeachingFacultyCount = NonTeachingFacultyDetails.Count();
            TechnicalFacultyCount = technicalFacultyDetails.Count();
            contents = contents.Replace("##COLLEGESTAFFPOSITIONTEACHING##", teachingFacultyCount.ToString());
            contents = contents.Replace("##COLLEGESTAFFPOSITIONNONTEACHING##", NonTeachingFacultyCount.ToString());
            contents = contents.Replace("##COLLEGESTAFFPOSITIONTECHNICAL##", TechnicalFacultyCount.ToString());
            return contents;
        }
        //Commented on 14-06-2018 by Narayana Reddy
        //private string CollegeOverallFacultyStudentRatio(int collegeId, string contents)
        //{
        //    string strcollegeOverallFacultyStudentRatio = string.Empty;

        //    strcollegeOverallFacultyStudentRatio += "<table border='1' cellspacing='0' cellpadding='4' width='100%' style='font-size: 9px'>";
        //    strcollegeOverallFacultyStudentRatio += "<tr>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='4'><p align='left'>Program</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='5'><p align='left'>Branch / Specialization Name</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='12'><p align='center'>Total Student Sanctioned Strength</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>Committee Findings (Total Student Sanctioned Strength)</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>Total Faculty Required</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>Committee Findings (Total Faculty)</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='6'><p align='center'>Faculty Student Ratio Available (As per Sanctioned Strength. <b>Required B.Tech=1:15, M.Tech=1:12, MBA/MCA=1:15</b>)</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>Ratified Faculty</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>Committe Findings (Ratified Faculty)</p></td>";
        //    strcollegeOverallFacultyStudentRatio += "</tr>";
        //    strcollegeOverallFacultyStudentRatio += "</table>";
        //    strcollegeOverallFacultyStudentRatio += "<span><br /></span>";

        //    int presentYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    int AY1 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear + 1)).Select(a => a.id).FirstOrDefault();
        //    int AY2 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear)).Select(a => a.id).FirstOrDefault();
        //    int AY3 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 1)).Select(a => a.id).FirstOrDefault();
        //    int AY4 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 2)).Select(a => a.id).FirstOrDefault();
        //    int AY5 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 3)).Select(a => a.id).FirstOrDefault();
        //    int AY6 = db.jntuh_academic_year.Where(a => a.actualYear == (presentYear - 4)).Select(a => a.id).FirstOrDefault();

        //    List<jntuh_college_overall_faculty_studentratio> intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == collegeId).ToList();

        //    List<CollegeOverallFacultyStudentRatio> collegeFacultyStudentRatio = new List<CollegeOverallFacultyStudentRatio>();

        //    foreach (var item in intake)
        //    {
        //        CollegeOverallFacultyStudentRatio newIntake = new CollegeOverallFacultyStudentRatio();
        //        newIntake.id = item.id;
        //        newIntake.collegeId = item.collegeId;
        //        newIntake.academicYearId = item.academicYearId;
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.isActive = item.isActive;
        //        newIntake.totalFaculty = item.totalFaculty;
        //        newIntake.ratifiedFaculty = item.ratifiedFaculty;
        //        newIntake.specializationId = item.specializationId;
        //        newIntake.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
        //        newIntake.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //        newIntake.Department = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
        //        newIntake.degreeID = db.jntuh_department.Where(d => d.id == newIntake.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
        //        newIntake.Degree = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degree).FirstOrDefault();
        //        newIntake.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newIntake.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
        //        newIntake.shiftId = item.shiftId;
        //        newIntake.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
        //        collegeFacultyStudentRatio.Add(newIntake);
        //    }

        //    collegeFacultyStudentRatio = collegeFacultyStudentRatio.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
        //    foreach (var item in collegeFacultyStudentRatio)
        //    {

        //        item.totalFaculty = item.totalFaculty;
        //        item.ratifiedFaculty = item.ratifiedFaculty;
        //        item.approvedIntake1 = GetCollegeFacultyStudentRatioIntake(collegeId, AY1, item.specializationId, item.shiftId);
        //        item.approvedIntake2 = GetCollegeFacultyStudentRatioIntake(collegeId, AY2, item.specializationId, item.shiftId);
        //        item.approvedIntake3 = GetCollegeFacultyStudentRatioIntake(collegeId, AY3, item.specializationId, item.shiftId);
        //        item.approvedIntake4 = GetCollegeFacultyStudentRatioIntake(collegeId, AY4, item.specializationId, item.shiftId);
        //        item.approvedIntake5 = GetCollegeFacultyStudentRatioIntake(collegeId, AY5, item.specializationId, item.shiftId);
        //        item.approvedIntake6 = GetCollegeFacultyStudentRatioIntake(collegeId, AY6, item.specializationId, item.shiftId);

        //    }
        //    collegeFacultyStudentRatio = collegeFacultyStudentRatio.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();
        //    List<string> degree = collegeFacultyStudentRatio.Select(d => d.Degree).Distinct().ToList();
        //    var facultystudentrationorms = db.jntuh_faculty_student_ratio_norms.Where(n => n.isActive == true).Select(n => n).ToList();
        //    int fsnorm = 0;
        //    int count = 0;
        //    int btechSummary = 0;
        //    foreach (var degreename in degree)
        //    {
        //        if (degreename.ToUpper().Equals("B.TECH"))
        //        {
        //            foreach (var item in collegeFacultyStudentRatio)
        //            {
        //                if (degreename == item.Degree)
        //                {
        //                    var FYUG = db.jntuh_college_overall_faculty_studentratio.Where(s => s.collegeId == item.collegeId && s.specializationId == 39).Select(s => s).FirstOrDefault();
        //                    int duration = db.jntuh_degree.Where(d => d.id == item.degreeID).Select(d => (int)d.degreeDuration).FirstOrDefault();
        //                    if (btechSummary == 0 && count == 0 && FYUG != null)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<table border='1' cellspacing='0' cellpadding='4' width='100%' style='font-size: 9px'>";
        //                        strcollegeOverallFacultyStudentRatio += "<tr>";

        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='4'><p align='left'>First Year UG (Engg.)</p></td>";
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='5'><p align='left'>All Branches</p></td>";
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='12'><p align='center'>" + FYUG.sanctionedIntake + "</p></td>";
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";

        //                        int totalRequiredFaculty = FYUG.sanctionedIntake / 15;
        //                        int facultyStudentRatio = 0;
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>" + totalRequiredFaculty + "</p></td>";
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";
        //                        if (totalRequiredFaculty != 0)
        //                        {
        //                            facultyStudentRatio = FYUG.sanctionedIntake / totalRequiredFaculty;
        //                        }
        //                        if (facultyStudentRatio == 0)
        //                        {
        //                            strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'> 0 : " + facultyStudentRatio + "</p></td>";
        //                        }
        //                        else
        //                        {
        //                            strcollegeOverallFacultyStudentRatio += "<td colspan='6'><p align='center'> 1 : " + facultyStudentRatio + "</p></td>";

        //                        }
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>" + FYUG.ratifiedFaculty + "</p></td>";
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";

        //                        btechSummary = 1;
        //                        strcollegeOverallFacultyStudentRatio += "</tr>";
        //                        strcollegeOverallFacultyStudentRatio += "</table>";
        //                        strcollegeOverallFacultyStudentRatio += "<span><br /></span>";
        //                    }
        //                }
        //            }

        //        }

        //        strcollegeOverallFacultyStudentRatio += "<table border='1' cellspacing='0' cellpadding='4' width='100%' style='font-size: 9px'>";
        //        foreach (var item in collegeFacultyStudentRatio)
        //        {
        //            if (degreename == item.Degree && item.Specialization != "Humanities")
        //            {
        //                int duration = db.jntuh_degree.Where(d => d.id == item.degreeID).Select(d => (int)d.degreeDuration).FirstOrDefault();
        //                int totalRequiredFaculty = 0;
        //                int facultyStudentRatio = 0;
        //                decimal colspan = 0;
        //                //2,3,4,5,6
        //                if (duration == 2)
        //                {
        //                    colspan = 6;
        //                }
        //                if (duration == 3)
        //                {
        //                    colspan = 4;
        //                }
        //                if (duration == 4)
        //                {
        //                    if (item.Degree == "B.Tech")
        //                    {
        //                        colspan = 4;
        //                    }
        //                    else
        //                    {
        //                        colspan = 3;
        //                    }

        //                }

        //                if (duration == 5)
        //                {
        //                    colspan = 2;
        //                }
        //                if (duration == 6)
        //                {
        //                    colspan = 2;
        //                }

        //                strcollegeOverallFacultyStudentRatio += "<tr>";
        //                if (count == 0)
        //                {
        //                    strcollegeOverallFacultyStudentRatio += "<td colspan='4' rowspan='" + collegeFacultyStudentRatio.Count() + "'><p align='left'>" + degreename + "</p></td>";
        //                }
        //                if (item.shiftId == 1)
        //                {
        //                    strcollegeOverallFacultyStudentRatio += "<td colspan='5'><p align='left'>" + item.Specialization + "</p></td>";
        //                }
        //                else
        //                {
        //                    strcollegeOverallFacultyStudentRatio += "<td colspan='5'><p align='left'>" + item.Specialization + "-" + item.shiftId + "</p></td>";
        //                }
        //                if (degreename.ToUpper().Equals("B.TECH"))
        //                {
        //                    if (duration >= 1)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake2 + "</p></td>";
        //                    }
        //                    if (duration >= 2)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake3 + "</p></td>";
        //                    }
        //                    if (duration >= 3)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake4 + "</p></td>";
        //                    }

        //                }
        //                else
        //                {
        //                    if (duration >= 1)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake1 + "</p></td>";
        //                    }
        //                    if (duration >= 2)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake2 + "</p></td>";
        //                    }
        //                    if (duration >= 3)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake3 + "</p></td>";
        //                    }
        //                    if (duration >= 4)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake4 + "</p></td>";
        //                    }
        //                    if (duration >= 5)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='2'><p align='center'>" + item.approvedIntake5 + "</p></td>";
        //                    }
        //                    if (duration >= 6)
        //                    {
        //                        strcollegeOverallFacultyStudentRatio += "<td colspan='" + colspan + "'><p align='center'>" + item.approvedIntake6 + "</p></td>";
        //                    }

        //                }

        //                strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";
        //                fsnorm = Convert.ToInt32((facultystudentrationorms.Where(f => f.degreeId == item.degreeID).Select(f => f.Norms).FirstOrDefault()).Split(':')[1]);
        //                if (item.Degree == "B.Tech")
        //                {
        //                    totalRequiredFaculty = (item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6) / fsnorm;
        //                    if (totalRequiredFaculty != 0)
        //                    {
        //                        facultyStudentRatio = (item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6) / totalRequiredFaculty;
        //                    }
        //                }
        //                else
        //                {
        //                    totalRequiredFaculty = (item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6) / fsnorm;
        //                    if (totalRequiredFaculty != 0)
        //                    {
        //                        facultyStudentRatio = (item.approvedIntake1 + item.approvedIntake2 + item.approvedIntake3 + item.approvedIntake4 + item.approvedIntake5 + item.approvedIntake6) / totalRequiredFaculty;
        //                    }
        //                }

        //                strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>" + totalRequiredFaculty + "</p></td>";
        //                strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";
        //                if (facultyStudentRatio == 0)
        //                {
        //                    strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'> 0 : " + facultyStudentRatio + "</p></td>";
        //                }
        //                else
        //                {
        //                    strcollegeOverallFacultyStudentRatio += "<td colspan='6'><p align='center'> 1 : " + facultyStudentRatio + "</p></td>";
        //                }
        //                strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'>" + item.ratifiedFaculty + "</p></td>";
        //                strcollegeOverallFacultyStudentRatio += "<td colspan='3'><p align='center'></p></td>";

        //                strcollegeOverallFacultyStudentRatio += "</tr>";
        //                count++;
        //            }
        //        }
        //        strcollegeOverallFacultyStudentRatio += "</table>";
        //        strcollegeOverallFacultyStudentRatio += "<span><br /></span>";
        //        count = 0;
        //    }
        //    contents = contents.Replace("##COLLEGEOVERALLFACULTYSTUDENTRATIO##", strcollegeOverallFacultyStudentRatio);
        //    return contents;
        //}

        //private int GetCollegeFacultyStudentRatioIntake(int collegeId, int academicYearId, int specializationId, int shiftId)
        //{
        //    int intake = 0;
        //    intake = db.jntuh_college_overall_faculty_studentratio.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.sanctionedIntake).FirstOrDefault();
        //    return intake;
        //}



        //deficiencies
        private void GetIds(int collegeID)
        {
            //Get  MAM, MTM ,Pharm.D,Pharm.D PB Ids
            EmptyDegreeRatioId = db.jntuh_degree
                                 .Where(d => d.isActive == true && EmptyDegrees.Contains(d.degree.Trim()))
                                 .Select(d => d.id).ToArray();

            //Get MBA and MCA Ids
            MBAAndMCAID = db.jntuh_degree
                            .Where(d => d.isActive == true && MBAAndMCAIDDegrees.Contains(d.degree.Trim()))
                            .Select(d => d.id).ToArray();

            //Get Colsure Course Id
            ClosureCourseId = db.jntuh_course_affiliation_status
                                .Where(c => c.courseAffiliationStatusCode.Trim() == "C")
                                .Select(c => c.id)
                                .FirstOrDefault();
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
            AcademicYearId11 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear + 1)).Select(a => a.id).FirstOrDefault();
            AcademicYearId21 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 1)).Select(a => a.id).FirstOrDefault();
            AcademicYearId31 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 2)).Select(a => a.id).FirstOrDefault();
            AcademicYearId41 = db.jntuh_academic_year.Where(a => a.isActive == true && a.actualYear == (AcademicYear - 3)).Select(a => a.id).FirstOrDefault();

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            //get current user Id
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            //get current user CollegeId
            //int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            DateTime now = Convert.ToDateTime("08/14/2014 16:00:00");
            //Get Data entry Submitted colleges
            SubmitteCollegesId = db.jntuh_dataentry_allotment
                //.Where(d => d.isVerified == true)
                                   .Where(d => d.isCompleted == true && d.InspectionPhaseId == InspectionPhaseId && d.collegeID == collegeID) // && d.updatedOn >= now 
                                   .Select(d => d.collegeID)
                                   .ToArray();
            //Get all data entry submitted colleges 
            CollegeIds = db.jntuh_college
                         .Where(c => c.isActive == true &&
                                     SubmitteCollegesId.Contains(c.id) &&
                                     c.isPermant == false && c.isNew != true &&
                                     c.isClosed == false)
                         .OrderBy(c => c.collegeName.ToUpper().Trim())
                         .Select(c => c.id)
                         .ToArray();

            //Get lab Committe observationId
            LibrarycommitteobservationId = db.jntuh_committee_observations
                                             .Where(c => c.isActive == true &&
                                                    c.observationType == "Library")
                                             .Select(c => c.id)
                                             .FirstOrDefault();
            //Get lab Committe observationId
            LabcommitteobservationId = db.jntuh_committee_observations
                                             .Where(c => c.isActive == true &&
                                                    c.observationType == "Labs")
                                             .Select(c => c.id)
                                             .FirstOrDefault();
        }

        private List<UGWithDeficiency> DeficiencyList(int? CollegeId, string cmd)
        {
            int Count = 0;
            List<UGWithDeficiency> UGWithDeficiencyList = new List<UGWithDeficiency>();
            if ((cmd == "All" || cmd == "Export") && CollegeId != null)
            {
                Count++;
                UGWithDeficiencyList = db.jntuh_college
                                         .Where(college => college.isActive == true &&
                                                           college.id == CollegeId)
                                         .Select(college => new UGWithDeficiency
                                         {
                                             CollegeId = college.id,
                                             CollegeCode = college.collegeCode,
                                             CollegeName = college.collegeName.Trim(),
                                         }).ToList();
                //set file name as collegeCode
                if (UGWithDeficiencyList != null)
                {
                    ReportHeader = UGWithDeficiencyList.Select(c => c.CollegeCode).FirstOrDefault() + "_Deficiencies.xls";
                }
            }

            if (cmd == "All")
            {
                ReportHeader = "All_Colleges_Deficiencies.xls";
            }
            else//if user selects with deficiency colleges Then get collegeIds of with deficiency colleges
            {
                ReportHeader = "Colleges_With_Deficiencies.xls";
            }

            //if user all colleges data of either all permanent or with deficiency or without deficiency colleges then get data of that colleges 
            if (CollegeId == null && Count == 0)
            {
                if (cmd == "All")
                {
                    //Get all data entry submitted colleges 
                    //get current user Id
                    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                    //get current user CollegeId
                    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

                    CollegeIds = db.jntuh_college
                                 .Where(c => c.isActive == true && c.isNew != true &&
                                             SubmitteCollegesId.Contains(c.id)
                                             )
                                 .OrderBy(c => c.collegeName.ToUpper().Trim())
                                 .Select(c => c.id)
                                 .ToArray();
                }

                UGWithDeficiencyList = db.jntuh_college
                                         .Where(college => college.isActive == true && college.isNew != true &&
                                             CollegeIds.Contains(college.id))
                                         .Select(college => new UGWithDeficiency
                                         {
                                             CollegeId = college.id,
                                             CollegeCode = college.collegeCode,
                                             CollegeName = college.collegeName.Trim()
                                         })
                                         .ToList();
            }
            List<UGWithDeficiency> DeficiencyList = new List<UGWithDeficiency>();
            if (cmd == "All")
            {
                foreach (var item in UGWithDeficiencyList)
                {
                    sComputers = 0;
                    sFaculty = 0;
                    LabShortageCount = 0;
                    ComputersShortageCount = 0;
                    PrincipalShortageCount = 0;
                    FacultyShortageCount = 0;
                    LibraryShortageCount = 0;
                    StrLabsShortage = null;
                    StrPrincipalShortage = null;
                    StrLibraryShortage = null;
                    item.LabsShortage = GetLabsShortage(item.CollegeId);
                    item.ComputersShortage = GetComputersShortageList(item.CollegeId);
                    item.LibraryShortage = GetLibraryShortage(item.CollegeId);
                    item.OverallPoints = GetOverallPoints(item.CollegeId);
                    item.NBAStatus = GetNBAStatus(item.CollegeId);
                    item.NAACStatus = GetNAACStatus(item.CollegeId);
                    item.PrincipalGrade = GetPrincipalRatified(item.CollegeId);
                    item.FacultyShortage = GetUGFacultyShortageList(item.CollegeId);
                    item.CollegeSpecializations = GetUGWithDeficiencyCollegeSpecializations(item.CollegeId);
                    item.Number = GetFacultyNumber(item.CollegeId);
                    item.Percentage = GetFacultyPercentage(item.CollegeId);
                    item.CollegeAddress = CollegeAddress(item.CollegeId);
                    item.UGObservations = GetUGObservationsList(item.CollegeId);
                    item.Establishyear = CollegeEstablishYear(item.CollegeId);

                    //if (PermanentCollegesId.Contains(item.CollegeId))
                    //{ item.IsPermanentCollege = "Permanent College"; }
                    //else
                    //{ item.IsPermanentCollege = ""; }

                    DeficiencyList.Add(item);
                }
            }
            else//for with deficiency colleges report (get labs,library,computers,faculty,principal,ratified faculty  deficiencies and specialization details, college establishment details)
            {

            }

            //set order by college name for all colleges
            DeficiencyList = DeficiencyList.OrderBy(c => c.CollegeName).ToList();

            //assign colleges data to viewbag
            ViewBag.CollegeSpecializations = DeficiencyList;

            //assign colleges count to viewbag
            ViewBag.Count = DeficiencyList.Count();

            //return data of all colleges
            return DeficiencyList;
        }

        //Get labs shortage
        private string GetLabsShortage(int collegeId)
        {
            int ObservationId = 0;
            string collegeLabsShortage = null;
            //get ObservationId of corresponding collegeId
            ObservationId = db.jntuh_college_committee_observations
                                    .Where(c => c.collegeId == collegeId &&
                                                c.observationTypeId == LabcommitteobservationId)
                                    .Select(c => c.id)
                                    .FirstOrDefault();
            //if ObservationId is not equal to zero then display 
            if (ObservationId != 0)
            {
                collegeLabsShortage = db.jntuh_college_committee_observations.Where(o => o.id == ObservationId && !StrNill.Contains(o.observations.ToUpper().Trim()))
                    .Select(o => o.observations).FirstOrDefault();
                //if observations is match with any string in the StrNill array then display it as NIL                      
                if (collegeLabsShortage == null)
                {
                    collegeLabsShortage = "NIL";
                }
                else//if observations is does not match with any string in the StrNill array then display it * and write remarks as observations     
                {
                    LabShortageCount++;
                    StrLabsShortage = collegeLabsShortage;
                    collegeLabsShortage = "*";
                }
            }
            else//if ObservationId is not equal to zero then display NIL
            {
                collegeLabsShortage = "NIL";
            }
            return collegeLabsShortage;
        }
        private List<string> GetComputersShortageList(int collegeId)
        {
            List<string> UGCompuetrsShortageList = new List<string>();
            string Existingratio = string.Empty;
            int ActualRatioValue = 0;
            decimal TotalIntake = 0;
            decimal CalculatedRatio = 0;
            decimal Deficiency = 0;
            decimal TotalComputers = 0;
            //get degreeIds based on collegeId
            int[] AllDegreeIds = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where (cd.isActive == true && cd.collegeId == collegeId && d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();
            //get degreeId doesnot contains mba,mca,mam,mtm,pharm d,pharmdpb
            int[] DegreeIds = (from d in AllDegreeIds
                               where !MBAAndMCAID.Contains(d) && !EmptyDegreeRatioId.Contains(d)
                               select d).ToArray();
            //get mba,mca, degreeids
            int[] MBAAndMCAIds = (from d in AllDegreeIds
                                  where MBAAndMCAID.Contains(d)
                                  select d).ToArray();
            //get mam,mtm,pharm d,pharmdpb degreeIds
            //int[] EmptyRatioDegreeIds = (from d in AllDegreeIds
            //                             where EmptyDegreeRatioId.Contains(d)
            //                             select d).ToArray();
            string DegreeName = string.Empty;
            if (DegreeIds.Count() > 0)
            {
                foreach (var item in DegreeIds)
                {
                    DegreeName = string.Empty;
                    TotalIntake = 0;
                    CalculatedRatio = 0;
                    TotalComputers = 0;
                    Existingratio = (db.jntuh_computer_student_ratio_norms
                                         .Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms)
                                         .FirstOrDefault()).Split(':')[1];
                    ActualRatioValue = Convert.ToInt32(Existingratio);
                    //TotalIntake = GetIntakeForComputers(item, collegeId);
                    //Commented on 10-Sep-2014 : Ramesh
                    //TotalIntake = db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
                    TotalIntake = GetDegreeWiseIntake(item, collegeId);
                    TotalComputers = db.jntuh_college_computer_student_ratio
                                       .Where(c => c.collegeId == collegeId &&
                                                   c.degreeId == item)
                                       .Select(c => c.availableComputers)
                                       .FirstOrDefault();
                    if (TotalIntake != 0 && TotalComputers != 0)
                    {
                        CalculatedRatio = TotalIntake / TotalComputers;
                        if (ActualRatioValue != 0 && CalculatedRatio != 0)
                        {
                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                    .Select(d => d.degree)
                                                    .FirstOrDefault();
                            if (ActualRatioValue < decimal.Round(CalculatedRatio))
                            {
                                ////With out Ratio
                                //ComputersShortageCount++;
                                //DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));

                                //With Ratio
                                Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                                StrComputersShortageDegrees += DegreeName + ", ";
                                ComputersShortageCount++;
                                DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                                if (Deficiency > 0)
                                    sComputers += decimal.Round(Deficiency);
                            }
                            else
                            {
                                ////With out Ratio
                                //DegreeName = DegreeName + "- NIL";

                                //With Ratio
                                DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }

                    //if (TotalIntake != 0 && TotalComputers == 0)
                    if (TotalComputers == 0 || (TotalIntake == 0 && TotalComputers != 0))
                    {
                        ////With out Ratio
                        //DegreeName = db.jntuh_degree.Where(d => d.id == item)
                        //                           .Select(d => d.degree).FirstOrDefault();
                        //ComputersShortageCount++;
                        //DegreeName = DegreeName + "- No Computers";

                        //With Ratio
                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                   .Select(d => d.degree).FirstOrDefault();
                        Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                        ComputersShortageCount++;
                        StrComputersShortageDegrees += DegreeName + ", ";
                        //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                        DegreeName = DegreeName + "- No Computers" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                        if (Deficiency > 0)
                            sComputers += decimal.Round(Deficiency);
                    }

                    UGCompuetrsShortageList.Add(DegreeName);
                }
            }
            if (MBAAndMCAIds.Count() > 0)
            {
                DegreeName = string.Empty;
                TotalIntake = 0;
                CalculatedRatio = 0;
                TotalComputers = 0;
                ActualRatioValue = 0;
                foreach (var item in MBAAndMCAIds)
                {
                    Existingratio = (db.jntuh_computer_student_ratio_norms
                                         .Where(c => c.isActive == true && c.degreeId == item)
                                         .Select(c => c.Norms)
                                         .FirstOrDefault()).Split(':')[1];
                    ActualRatioValue += Convert.ToInt32(Existingratio);
                    //TotalIntake += GetIntakeForComputers(item, collegeId);
                    //Commented on 10-Sep-2014 : Ramesh
                    //TotalIntake += db.jntuh_college_faculty_student_ratio.Where(f => f.collegeId == collegeId && f.degreeId == item).Select(f => f.totalIntake).FirstOrDefault();
                    TotalIntake += GetDegreeWiseIntake(item, collegeId);
                    TotalComputers += db.jntuh_college_computer_student_ratio
                                       .Where(c => c.collegeId == collegeId &&
                                                   c.degreeId == item)
                                       .Select(c => c.availableComputers)
                                       .FirstOrDefault();
                }
                if (TotalIntake != 0 && TotalComputers != 0)
                {
                    CalculatedRatio = TotalIntake / TotalComputers;
                    if (ActualRatioValue != 0 && CalculatedRatio != 0)
                    {
                        DegreeName = "MBA/MCA";
                        if (ActualRatioValue < decimal.Round(CalculatedRatio))
                        {
                            ////With out Ratio
                            //ComputersShortageCount++;
                            //DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));

                            //With  Ratio
                            ComputersShortageCount++;
                            Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                            StrComputersShortageDegrees += DegreeName + ", ";
                            DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                            if (Deficiency > 0)
                                sComputers += decimal.Round(Deficiency);
                        }
                        else
                        {
                            ////With out Ratio
                            //DegreeName = DegreeName + "- NIL";

                            //With Ratio
                            DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(CalculatedRatio, 0, MidpointRounding.AwayFromZero));
                        }
                    }
                }

                //if (TotalIntake != 0 && TotalComputers == 0)
                if (TotalComputers == 0 || (TotalIntake == 0 && TotalComputers != 0))
                {
                    DegreeName = "MBA/MCA";
                    ////With out Ratio
                    //ComputersShortageCount++;
                    //DegreeName = DegreeName + "- No Computers";

                    //With Ratio
                    Deficiency = (TotalIntake - (TotalComputers * ActualRatioValue)) / ActualRatioValue;
                    ComputersShortageCount++;
                    StrComputersShortageDegrees += DegreeName + ", ";
                    //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                    DegreeName = DegreeName + "- No Computers" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                    if (Deficiency > 0)
                        sComputers += decimal.Round(Deficiency);
                }

                UGCompuetrsShortageList.Add(DegreeName);
            }

            return UGCompuetrsShortageList;
        }

        private int GetDegreeWiseIntake(int degreeId, int collegeId)
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
            //int[] specializations = specializationsId.Distinct().ToArray();
            foreach (var specializationId in specializationsId)
            {
                int totalIntake1 = 0;
                int totalIntake2 = 0;
                int totalIntake3 = 0;
                int totalIntake4 = 0;
                int totalIntake5 = 0;
                int[] shiftId1 = db.jntuh_college_intake_proposed.Where(e => e.collegeId == collegeId && e.specializationId == specializationId && e.academicYearId == AcademicYearId1).Select(e => e.shiftId).ToArray();
                foreach (var sId1 in shiftId1)
                {
                    totalIntake1 += db.jntuh_college_intake_proposed.Where(e => e.academicYearId == AcademicYearId1 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.proposedIntake).FirstOrDefault();
                    totalIntake2 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == presentAcademicYearId && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake3 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId2 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake4 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId3 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
                    totalIntake5 += db.jntuh_college_intake_existing.Where(e => e.academicYearId == AcademicYearId4 && e.collegeId == collegeId && e.specializationId == specializationId && e.shiftId == sId1).Select(a => a.approvedIntake).FirstOrDefault();
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

        private string GetLibraryShortage(int collegeId)
        {
            int ObservationId = 0;
            string collegeLibraryShortage = null;
            ObservationId = db.jntuh_college_committee_observations
                                   .Where(c => c.collegeId == collegeId &&
                                               c.observationTypeId == LibrarycommitteobservationId)
                                   .Select(c => c.id)
                                   .FirstOrDefault();
            if (ObservationId != 0)
            {
                collegeLibraryShortage = db.jntuh_college_committee_observations
                                           .Where(o => o.id == ObservationId &&
                                                       !StrNill.Contains(o.observations.ToUpper().Trim()))
                                           .Select(o => o.observations).FirstOrDefault();
                if (collegeLibraryShortage == null)
                {
                    collegeLibraryShortage = "NIL";
                }
                else
                {
                    LibraryShortageCount++;
                    StrLibraryShortage = collegeLibraryShortage;
                    collegeLibraryShortage = "*";
                }
            }
            else
            {
                collegeLibraryShortage = "NIL";
            }
            return collegeLibraryShortage;
        }

        private string GetPrincipalRatified(int collegeId)
        {
            string PrincipalGrade = string.Empty;
            var PrincipalDetails = db.jntuh_college_principal_director
                                     .Where(p => p.collegeId == collegeId && p.type == "PRINCIPAL")
                                     .Select(p => new
                                     {
                                         Id = p.id,
                                         Ratified = p.isRatified,
                                     })
                                     .FirstOrDefault();
            if (PrincipalDetails != null)
            {
                if (PrincipalDetails.Ratified == true)
                {
                    PrincipalGrade = "A";
                }
                else
                {
                    PrincipalShortageCount++;
                    StrPrincipalShortage = "Principal is not ratified";
                    PrincipalGrade = "B";
                }
            }
            else
            {
                PrincipalShortageCount++;
                StrPrincipalShortage = "No Principal";
                PrincipalGrade = "*";
            }
            return PrincipalGrade;
        }
        private List<string> GetUGFacultyShortageList(int CollegeId)
        {
            string DegreeName = string.Empty;
            decimal Caluculatedvalue = 0;
            decimal FacultyIntake = 0;
            decimal TotalFaculty = 0;
            int ActualRatioValue = 0;
            decimal Deficiency;
            List<string> UGFacultyShortageList = new List<string>();
            int[] AllDegreeIds = (from cd in db.jntuh_college_degree
                                  join d in db.jntuh_degree on cd.degreeId equals d.id
                                  where (cd.isActive == true && cd.collegeId == CollegeId && d.isActive == true)
                                  orderby d.degreeDisplayOrder
                                  select cd.degreeId).ToArray();
            //get degreeId doesnot contains mba,mca,mam,mtm,pharm d,pharmdpb
            int[] DegreeIds = (from d in AllDegreeIds
                               where !MBAAndMCAID.Contains(d) && !EmptyDegreeRatioId.Contains(d)
                               select d).ToArray();
            //get mba,mca, degreeids
            int[] MBAAndMCAIds = (from d in AllDegreeIds
                                  where MBAAndMCAID.Contains(d)
                                  select d).ToArray();
            //get mam,mtm,pharm d,pharmdpb degreeIds
            //int[] EmptyRatioDegreeIds = (from d in AllDegreeIds
            //                             where EmptyDegreeRatioId.Contains(d)
            //                             select d).ToArray();

            if (DegreeIds.Count() > 0)
            {
                foreach (var item in DegreeIds)
                {
                    DegreeName = string.Empty;
                    Caluculatedvalue = 0;
                    ActualRatioValue = Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
                                   .Where(f => f.isActive == true && f.degreeId == item)
                                   .Select(f => f.Norms)
                                  .FirstOrDefault()).Split(':')[1]);

                    //Commented on 10-Sep-2014 : Ramesh
                    //FacultyIntake = db.jntuh_college_faculty_student_ratio
                    //                       .Where(f => f.collegeId == CollegeId &&
                    //                                   f.degreeId == item)
                    //                       .Select(f => f.totalIntake)
                    //                       .FirstOrDefault();
                    FacultyIntake = GetDegreeWiseIntake(item, CollegeId);

                    TotalFaculty = db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalFaculty)
                                           .FirstOrDefault();
                    if (FacultyIntake != 0 && TotalFaculty != 0)
                    {
                        Caluculatedvalue = FacultyIntake / TotalFaculty;
                        if (ActualRatioValue != 0 && Caluculatedvalue != 0)
                        {
                            DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                        .Select(d => d.degree).FirstOrDefault();
                            if (ActualRatioValue < decimal.Round(Caluculatedvalue))
                            {
                                ////with out Ratio
                                //FacultyShortageCount++;
                                //DegreeName = DegreeName + "- 1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));

                                //With Ratio
                                Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                                FacultyShortageCount++;
                                StrFacultyShortageDegrees += DegreeName + ", ";
                                //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                                DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                                if (Deficiency > 0)
                                    sFaculty += decimal.Round(Deficiency);
                            }
                            else
                            {
                                ////With out Ratio
                                //DegreeName = DegreeName + "- NIL";

                                //With Ratio
                                DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
                            }
                        }
                    }

                    //if (FacultyIntake != 0 && TotalFaculty == 0)
                    if (TotalFaculty == 0)
                    {
                        DegreeName = db.jntuh_degree.Where(d => d.id == item)
                                                    .Select(d => d.degree).FirstOrDefault();

                        ////With out Ratio
                        //FacultyShortageCount++;
                        //DegreeName = DegreeName + "- No Faculty";

                        //With  Ratio
                        Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                        FacultyShortageCount++;
                        StrFacultyShortageDegrees += DegreeName + ", ";
                        //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                        DegreeName = DegreeName + "- No Faculty" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                        if (Deficiency > 0)
                            sFaculty += decimal.Round(Deficiency);
                    }

                    UGFacultyShortageList.Add(DegreeName);
                }
            }
            if (MBAAndMCAIds.Count() > 0)
            {
                DegreeName = string.Empty;
                FacultyIntake = 0;
                TotalFaculty = 0;
                Caluculatedvalue = 0;
                ActualRatioValue = 0;
                foreach (var item in MBAAndMCAIds)
                {
                    ActualRatioValue += Convert.ToInt32((db.jntuh_faculty_student_ratio_norms
                                   .Where(f => f.isActive == true && f.degreeId == item)
                                   .Select(f => f.Norms)
                                  .FirstOrDefault()).Split(':')[1]);
                    //Commented on 10-Sep-2014 : Ramesh
                    //FacultyIntake += db.jntuh_college_faculty_student_ratio
                    //                       .Where(f => f.collegeId == CollegeId &&
                    //                                   f.degreeId == item)
                    //                       .Select(f => f.totalIntake)
                    //                       .FirstOrDefault();
                    FacultyIntake += GetDegreeWiseIntake(item, CollegeId);

                    TotalFaculty += db.jntuh_college_faculty_student_ratio
                                           .Where(f => f.collegeId == CollegeId &&
                                                       f.degreeId == item)
                                           .Select(f => f.totalFaculty)
                                           .FirstOrDefault();
                }
                if (FacultyIntake != 0 && TotalFaculty != 0)
                {
                    Caluculatedvalue = FacultyIntake / TotalFaculty;
                    if (ActualRatioValue != 0 && Caluculatedvalue != 0)
                    {
                        DegreeName = "MBA/MCA";
                        if (ActualRatioValue < decimal.Round(Caluculatedvalue))
                        {
                            ////With out Ratio
                            //FacultyShortageCount++;
                            //DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));

                            //With Ratio
                            FacultyShortageCount++;
                            StrFacultyShortageDegrees += DegreeName + ", ";
                            Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                            //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                            DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero)) + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                            if (Deficiency > 0)
                                sFaculty += decimal.Round(Deficiency);
                        }
                        else
                        {
                            ////With out Ratio
                            //DegreeName = DegreeName + "-NIL";

                            //With Ratio
                            DegreeName = DegreeName + "-1:" + Convert.ToString(Math.Round(Caluculatedvalue, 0, MidpointRounding.AwayFromZero));
                        }
                    }
                }

                //if (FacultyIntake != 0 && TotalFaculty == 0)
                if (TotalFaculty == 0)
                {
                    DegreeName = "MBA/MCA";

                    ////With out Ratio
                    //FacultyShortageCount++;
                    //DegreeName = DegreeName + "- No Faculty";

                    //With Ratio
                    Deficiency = (FacultyIntake - (TotalFaculty * ActualRatioValue)) / ActualRatioValue;
                    FacultyShortageCount++;
                    StrFacultyShortageDegrees += DegreeName + ", ";
                    //Modified Date:01/07/2014, Repalace * inplace of Deficiency
                    DegreeName = DegreeName + "- No Faculty" + "(" + Convert.ToString(decimal.Round(Deficiency)) + ")";

                    if (Deficiency > 0)
                        sFaculty += decimal.Round(Deficiency);

                }

                UGFacultyShortageList.Add(DegreeName);
            }

            return UGFacultyShortageList;
        }

        private string GetOverallPoints(int collegeId)
        {
            int overallpoints = 0;
            int apoints = 0;
            int ipoints = 0;

            apoints = db.jntuh_college_academic_performance_points.Where(a => a.collegeId == collegeId && a.pointsObtained != null).Select(a => (int?)a.pointsObtained).Sum() ?? 0;
            ipoints = db.jntuh_college_infrastructure_parameters.Where(a => a.collegeId == collegeId && a.pointsObtained != null).Select(a => (int?)a.pointsObtained).Sum() ?? 0;

            overallpoints = apoints + ipoints;

            return overallpoints.ToString();
        }

        private string GetNBAStatus(int collegeId)
        {
            string NBAStatus = "";
            if (db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 8) != null)
            {
                var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 8).Select(a => a).FirstOrDefault();
                if (Status != null)
                {
                    NBAStatus = Status.affiliationStatus;

                    //    NBAStatus = "YES";

                }
                else
                {
                    NBAStatus = "Not Yet Applied";
                }
            }
            return NBAStatus;
        }

        private string GetNAACStatus(int collegeId)
        {
            string NAACStatus = "";
            var Status = db.jntuh_college_affiliation.Where(a => a.collegeId == collegeId && a.affiliationTypeId == 2).Select(a => a).FirstOrDefault();
            if (Status != null)
            {
                NAACStatus = Status.affiliationStatus;

                //NAACStatus = "YES";
            }
            else
            {
                NAACStatus = "Not Yet Applied";
            }
            return NAACStatus;
        }

        private string GetFacultyNumber(int CollegeId)
        {
            string FacultyNum;
            int facultyRatifiedSum = 0;
            int facultySum = 0;
            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.ratified)
                                        .ToArray();
            int[] facultyCount = db.jntuh_college_faculty_student_ratio
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.totalFaculty)
                                        .ToArray();
            if (facultyRatifiedCount.Count() != 0)
            {
                facultyRatifiedSum = facultyRatifiedCount.Sum();
            }
            if (facultyCount.Count() != 0)
            {
                facultySum = facultyCount.Sum();
            }
            if (facultyRatifiedSum != 0 && facultySum != 0)
            {
                FacultyNum = Convert.ToString(facultyRatifiedSum) + "/" + Convert.ToString(facultySum) + ".";
            }
            else if (facultyRatifiedSum != 0)
            {
                FacultyNum = Convert.ToString(facultyRatifiedSum) + "/" + "0" + ".";
            }
            else if (facultySum != 0)
            {
                FacultyNum = "0" + "/" + Convert.ToString(facultySum) + ".";
            }
            else
            {
                FacultyNum = "0" + "/" + "0" + ".";
            }
            return FacultyNum;
        }

        private decimal GetFacultyPercentage(int CollegeId)
        {
            decimal Percentage = 0;
            decimal facultyRatifiedSum = 0;
            decimal facultySum = 0;
            int[] facultyRatifiedCount = db.jntuh_college_teaching_faculty_position
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.ratified)
                                        .ToArray();
            int[] facultyCount = db.jntuh_college_faculty_student_ratio
                                        .Where(f => f.collegeId == CollegeId)
                                        .Select(f => f.totalFaculty)
                                        .ToArray();
            if (facultyRatifiedCount.Count() != 0)
            {
                facultyRatifiedSum = facultyRatifiedCount.Sum();
            }
            if (facultyCount.Count() != 0)
            {
                facultySum = facultyCount.Sum();
            }
            if (facultyRatifiedSum != 0 && facultySum != 0)
            {
                Percentage = (facultyRatifiedSum / facultySum) * 100;
                Percentage = Math.Round(Percentage, 2);
            }
            return Percentage;
        }

        private List<string> GetUGObservationsList(int CollegeId)
        {
            List<string> UGObservations = new List<string>();
            if (LabShortageCount > 0 && StrLabsShortage != null)
            {
                UGObservations.Add(StrLabsShortage);
            }
            if (ComputersShortageCount > 0)
            {
                UGObservations.Add("Shortage of computers (" + sComputers + ")");
            }
            if (PrincipalShortageCount > 0 && StrPrincipalShortage != null)
            {
                UGObservations.Add(StrPrincipalShortage);
            }
            if (FacultyShortageCount > 0)
            {
                UGObservations.Add("Shortage of faculty (" + sFaculty + ")");
            }
            if (LibraryShortageCount > 0 && StrLibraryShortage != null)
            {
                UGObservations.Add(StrLibraryShortage);
            }
            return UGObservations;
        }

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
                Address += CollegeAddress.townOrCity + ", ";
            }
            if (CollegeAddress.mandal != null || CollegeAddress.mandal != string.Empty)
            {
                Address += CollegeAddress.mandal + ", ";
            }
            if (CollegeAddress.districtId != 0)
            {
                District = db.jntuh_district
                             .Where(d => d.id == CollegeAddress.districtId)
                             .Select(d => d.districtName)
                             .FirstOrDefault();
                Address += District + ", ";
            }
            if (CollegeAddress.pincode != 0)
            {
                Address += CollegeAddress.pincode.ToString();
            }
            return Address;
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

        //Binding Specializations data 

        private List<UGWithDeficiencyCollegeSpecializations> GetUGWithDeficiencyCollegeSpecializations(int CollegeId)
        {
            string adminMBA = string.Empty;
            string NewSpecialization = string.Empty;
            string NewIntake = string.Empty;

            List<UGWithDeficiencyCollegeSpecializations> UGWithDeficiencyCollegeSpecializations = new List<UGWithDeficiencyCollegeSpecializations>();

            List<UGWithDeficiencySpecializations> SpecializationList = (from p in db.jntuh_college_intake_proposed
                                                                        join s in db.jntuh_specialization on p.specializationId equals s.id
                                                                        join d in db.jntuh_department on s.departmentId equals d.id
                                                                        join de in db.jntuh_degree on d.degreeId equals de.id
                                                                        join dt in db.jntuh_degree_type on de.degreeTypeId equals dt.id
                                                                        where (p.collegeId == CollegeId &&
                                                                               p.academicYearId == nextAcademicYearId &&
                                                                            // p.courseAffiliationStatusCodeId != ClosureCourseId &&
                                                                               p.isActive == true)
                                                                        //orderby p.specializationId
                                                                        orderby de.degreeDisplayOrder
                                                                        select new UGWithDeficiencySpecializations
                                                                        {
                                                                            CollegeId = CollegeId,
                                                                            SpecializationId = p.specializationId,
                                                                            ShiftId = p.shiftId,
                                                                            ProposedIntake = p.proposedIntake,
                                                                            CourseAffiliationStatusCodeId = p.courseAffiliationStatusCodeId,
                                                                            DegreeType = dt.degreeType,
                                                                            DegreeName = de.degree,
                                                                            SpecializationName = s.specializationName,
                                                                            DegreeId = de.id
                                                                        }).ToList();
            foreach (var item in SpecializationList)
            {
                UGWithDeficiencyCollegeSpecializations UGWithDeficiencySpecializations = new UGWithDeficiencyCollegeSpecializations();
                if (item.DegreeType != null)
                {
                    NewIntake = string.Empty;
                    NewSpecialization = string.Empty;
                    if (item.DegreeType.ToUpper().Trim() == "PG")
                    {
                        //if degree is MBA then show degreeName as mba and add all mba specializations proposed intake 
                        if (item.DegreeName.ToUpper().Trim() == "MBA" && adminMBA != "MBA")
                        {
                            adminMBA = "MBA";
                            NewSpecialization = item.DegreeName;
                            //display proposed intake bold based on existing intake
                            NewIntake = GetMBAIntake(item.CollegeId, item.DegreeId);
                        }
                        //if degree is mtech or mpharmacy then display specialization name as degreename-specializationname-shiftname
                        if (item.DegreeName.ToUpper().Trim() == "M.TECH" || item.DegreeName.ToUpper().Trim() == "M.PHARMACY")
                        {
                            //display proposed intake bold based on CourseAffiliationStatusCodeId
                            NewIntake = GetIntake1(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            //if specialization consists of shift id then dispaly shift anme
                            NewSpecialization = item.ShiftId == 1 ? item.DegreeName + "-" + item.SpecializationName : item.DegreeName + "-" + item.SpecializationName + "-" + "(II Shift)";
                        }
                        //if degree is not equal to mtech or mpharmacy then display specialization name as specializationname-shiftname
                        if (item.DegreeName.ToUpper().Trim() != "M.TECH" && item.DegreeName.ToUpper().Trim() != "M.PHARMACY" && item.DegreeName.ToUpper().Trim() != "MBA")
                        {
                            NewIntake = GetIntake1(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                            NewSpecialization = item.ShiftId == 1 ? item.SpecializationName : item.SpecializationName + "-" + "(II Shift)";
                        }
                    }
                    else//for Ug degrees
                    {
                        NewIntake = GetIntake1(item.CollegeId, item.SpecializationId, item.ShiftId, item.CourseAffiliationStatusCodeId, item.ProposedIntake);
                        NewSpecialization = item.ShiftId == 1 ? item.SpecializationName : item.SpecializationName + "-" + "(II Shift)";
                    }
                    //if intake and specialization is exists then add to list
                    if (NewIntake != string.Empty && NewSpecialization != string.Empty)
                    {
                        UGWithDeficiencySpecializations.Specialization = NewSpecialization;
                        UGWithDeficiencySpecializations.Intake = NewIntake;
                        int existingIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == CollegeId && ex.specializationId == item.SpecializationId && ex.shiftId == item.ShiftId).Select(ex => ex.approvedIntake).FirstOrDefault();
                        UGWithDeficiencySpecializations.ExistingIntake = existingIntake;
                        UGWithDeficiencyCollegeSpecializations.Add(UGWithDeficiencySpecializations);
                    }
                }
            }
            return UGWithDeficiencyCollegeSpecializations;
        }

        //Binding MBA degree intake
        private string GetMBAIntake(int collegeId, int degreeId)
        {
            int existingIntake = 0;
            int proposedIntake = 0;
            string intake = string.Empty;
            //get all specializations for corresponding degree
            int[] specializationId = (from department in db.jntuh_department
                                      join specialization in db.jntuh_specialization on department.id equals specialization.departmentId
                                      where department.degreeId == degreeId
                                      select specialization.id).ToArray();
            foreach (var item in specializationId)
            {
                //get shifts
                int[] shift = db.jntuh_shift.Where(s => s.isActive == true).Select(s => s.id).ToArray();
                foreach (var shiftId in shift)
                {
                    //get proposed intake id 
                    int id = db.jntuh_college_intake_proposed.Where(ei => ei.collegeId == collegeId &&
                                                                          ei.specializationId == item &&
                                                                          ei.academicYearId == nextAcademicYearId &&
                                                                          ei.shiftId == shiftId &&
                                                                          ei.isActive == true
                        //&& ei.courseAffiliationStatusCodeId != ClosureCourseId
                                                                          )
                                                             .Select(ei => ei.id)
                                                             .FirstOrDefault();
                    //if proposed intake id is exists then get existing intake 
                    if (id > 0)
                    {
                        //add proposed intake for all specializations under mba degree
                        proposedIntake += db.jntuh_college_intake_proposed
                                            .Where(ei => ei.id == id)
                                            .Select(ei => ei.proposedIntake).FirstOrDefault();
                        //add existing intake for all specializations under mba degree
                        existingIntake += db.jntuh_college_intake_existing
                                            .Where(ei => ei.collegeId == collegeId &&
                                                         ei.specializationId == item &&
                                                         ei.academicYearId == academicYearId &&
                                                         ei.shiftId == shiftId)
                                           .Select(ei => ei.approvedIntake)
                                           .FirstOrDefault();
                    }

                }
            }
            if (proposedIntake != 0)
            {
                //if exising and proposed intake is same then dispaly this specialization and intake normal based on $ symbol inside view
                if (existingIntake == proposedIntake)
                {
                    intake = proposedIntake.ToString() + "$";
                }
                //if exising is zero then dispaly this specialization and intake bold letters based on # symbol inside view
                else if (existingIntake == 0)
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else//if proposed intake is greater or less than of existing intake then dispaly this specialization normal and intake bold letters based on * symbol inside view
                {
                    intake = proposedIntake.ToString() + "*";
                }
            }
            else
            {
                intake = proposedIntake.ToString() + "*";
            }
            return intake;
        }

        //get intake for coresponding degree
        private string GetIntake1(int collegeId, int specializationId, int shiftId, int courseAffiliationStatusCodeId, int proposedIntake)
        {
            string intake = string.Empty;
            string courseAffiliationStatusCode = string.Empty;
            courseAffiliationStatusCode = (db.jntuh_course_affiliation_status.Where(status => status.id == courseAffiliationStatusCodeId).Select(status => status.courseAffiliationStatusCode).FirstOrDefault()).ToUpper().Trim();
            if (proposedIntake != 0)
            {
                //if courseAffiliationStatusCode is 'S' then dispaly this specialization and intake normal based on $ symbol inside view
                if (courseAffiliationStatusCode == "S")
                {
                    intake = proposedIntake.ToString() + "$";
                }
                //if courseAffiliationStatusCode is 'N' then dispaly this specialization and intake bold based on # symbol inside view
                else if (courseAffiliationStatusCode == "N")
                {
                    intake = proposedIntake.ToString() + "#";
                }
                else//dispaly this specialization normal and intake bold letters based on * symbol inside view
                {
                    intake = proposedIntake.ToString() + "*";

                    //int existingIntake = db.jntuh_college_intake_existing.Where(ex => ex.collegeId == collegeId && ex.specializationId == specializationId && ex.shiftId == shiftId).Select(ex => ex.approvedIntake).FirstOrDefault();

                    ////if [existing intake > proposed intake] then show [existing intake - decrease in proposed intake]
                    //if (existingIntake > proposedIntake)
                    //{
                    //    intake = existingIntake.ToString() + "-<span style='font-weight: bold'>" + (existingIntake - proposedIntake).ToString() + "</span>" + "*";
                    //}

                    ////if [existing intake < proposed intake] then show [existing intake + increase in proposed intake]
                    //if (existingIntake < proposedIntake)
                    //{
                    //    intake = existingIntake.ToString() + "+<span style='font-weight: bold'>" + (proposedIntake - existingIntake).ToString() + "</span>" + "*";
                    //}

                }
            }
            else
            {
                intake = proposedIntake.ToString() + "*";
            }
            return intake;
        }

        //deficiency letter formats

        [Authorize(Roles = "Admin")]
        public ActionResult SendDefficiencyLetters(int collegeId)
        {
            int count = 0;

            List<int> collegeIds = db.jntuh_college.Where(c => c.collegeCode != "WL").Select(c => c.id).ToList();

            //foreach (var collegeid in collegeIds)
            //{
            string code = db.jntuh_college.Where(c => c.id == collegeId).Select(c => c.collegeCode).FirstOrDefault().ToUpper();

                string path = SaveCollegeDefficiencyLetterPdf(code);

                //string strTo = db.jntuh_college_principal_director.Where(a => a.collegeId == collegeid && a.type == "PRINCIPAL").Select(a => a.email).FirstOrDefault();
                //string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
                //string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();

                string strTo = "ramesh.bandi@csstechnergy.com";
                string strCc = string.Empty;
                string strBcc = string.Empty;

                if (!string.IsNullOrEmpty(strTo))
                {
                    List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                    attachments.Add(new System.Net.Mail.Attachment(path));

                    //send emails to committee members
                    IUserMailer mailer = new UserMailer();
                    //mailer.SendDeficiencyLetter(strTo, strCc, strBcc, "JNTUH: " + code + " Deficiency Letter", attachments).SendAsync();

                    //string pMobile = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == "PRINCIPAL").Select(a => a.mobile).FirstOrDefault();
                    string pMobile = "8978889891";

                    //send sms to committee members
                    string message = "JNTUH: " + code + " deficiency letter has been sent to your email. - From Director, UAAC.";
                    //UAAAS.Models.Utilities.SendSms(pMobile, message);

                    //college_undertaking def = db.college_undertaking.Where(u => u.collegeId == collegeid).FirstOrDefault();

                    //def.EmailSent = true;

                    //db.Entry(def).State = EntityState.Modified;
                    //db.SaveChanges();
                    count++;
                }
                else
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
            //}

            ViewBag.SentCount = count;

            return View();
        }

        private string SaveCollegeDefficiencyLetterPdf(string collegeCode)
        {
            string fullPath = string.Empty;

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            string path = Server.MapPath("~/Content/PDFReports/DefficiencyLetters/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegeCode + "-DeficiencyLetter.pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/AffiliationProceedings.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##COLLEGE_CODE##", collegeCode);

            int collegeid = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.id).FirstOrDefault();
            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeid).Select(a => a).FirstOrDefault();
            string district = string.Empty;
            if (address != null)
            {
                district = db.jntuh_district.Find(address.districtId).districtName;
            }
            string scheduleCollegeAddress = string.Empty;

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();

            if (address != null)
            {
                scheduleCollegeAddress = collegeName + ",<br />" + address.address;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;
                scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            }

            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
            string list = string.Empty;

            //GetIds(collegeid);

            ////Binding all permanent deficiency colleges data to table
            //List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "All");
            //int i = 1;
            //list = "<ol>";
            //if (UGWithDeficiencies != null)
            //{
            //    foreach (var item in UGWithDeficiencies)
            //    {
            //        foreach (var def in item.UGObservations)
            //        {
            //            list = list + "<li>" + def + "</li>";
            //        }
            //    }
            //}

            //if (list.Contains("some"))
            //    list = list.Replace("some", "required");

            //if (list.Contains("Some"))
            //    list = list.Replace("Some", "Required");

            //list = list + "</ol>";
            //contents = contents.Replace("##DEFICIENCIES##", list);

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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        //[Authorize(Roles = "Admin")]
        //public ActionResult SendCommonDefficiencyLetters()
        //{
        //    List<string> colleges = db.college_undertaking.Join(db.jntuh_college, u => u.collegeId, c => c.id, (u, c) => new { c.collegeCode, u.EmailSent, u.inspectionPhaseId }).Where(c => c.EmailSent == false && c.inspectionPhaseId == 1).Select(c => c.collegeCode).ToList();

        //    int count = 0;
        //    if (colleges != null)
        //    {
        //        foreach (string code in colleges)
        //        {
        //            int collegeid = db.jntuh_college.Where(c => c.collegeCode == code).Select(c => c.id).FirstOrDefault();

        //            string path = SaveCommonDefficiencyLetterPdf(code);

        //            string strTo = db.jntuh_college_principal_director.Where(a => a.collegeId == collegeid && a.type == "PRINCIPAL").Select(a => a.email).FirstOrDefault();
        //            string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
        //            string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();

        //            //string strTo = "ramesh.bandi@csstechnergy.com";
        //            //string strCc = string.Empty;
        //            //string strBcc = string.Empty;

        //            if (!string.IsNullOrEmpty(strTo))
        //            {
        //                List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
        //                attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(path)));

        //                //send emails to committee members
        //                IUserMailer mailer = new UserMailer();
        //                //mailer.SendCommonDeficiencyLetter(strTo, strCc, strBcc, "JNTUH: Grounds for reduction in intake for admissions", attachments).SendAsync();

        //                string pMobile = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == "PRINCIPAL").Select(a => a.mobile).FirstOrDefault();
        //                //string pMobile = string.Empty;
        //                //pMobile = "9493666388";

        //                //send sms to committee members
        //                string message = "JNTUH: College deficiency letter has been sent to your email. - From Director, UAAC.";
        //                //UAAAS.Models.Utilities.SendSms(pMobile, message);

        //                college_undertaking def = db.college_undertaking.Where(u => u.collegeId == collegeid).FirstOrDefault();

        //                def.EmailSent = true;

        //                db.Entry(def).State = EntityState.Modified;
        //                //db.SaveChanges();
        //                count++;
        //            }
        //            else
        //            {
        //                if (System.IO.File.Exists(path))
        //                {
        //                    System.IO.File.Delete(path);
        //                }
        //            }
        //        }
        //    }

        //    ViewBag.SentCount = count;

        //    return View();
        //}

        private string SaveCommonDefficiencyLetterPdf(string collegeCode)
        {
            string fullPath = string.Empty;

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 30, 30, 20, 20);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            fullPath = "~/Content/PDFReports/DefficiencyLetters/Common_" + collegeCode + "_" + InspectionPhaseId + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/CommonDeficiencyLetter.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##COLLEGE_CODE##", collegeCode);

            int collegeid = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.id).FirstOrDefault();
            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeid).Select(a => a).FirstOrDefault();
            string district = db.jntuh_district.Find(address.districtId).districtName;
            string scheduleCollegeAddress = string.Empty;

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();
            scheduleCollegeAddress = collegeName + ",<br />" + address.address;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.townOrCity.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.mandal.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;

            if (!scheduleCollegeAddress.ToUpper().Contains(district.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;

            scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));
            string list = string.Empty;
            //Get all Ids(college,academicYear,Library,Lab)
            GetIds(collegeid);

            //ViewBag.Individualcollege = "Individualcollege";

            //Binding all permanent deficiency colleges data to table
            //List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "All");
            //int i = 1;
            //list = "<ol>";
            //if (UGWithDeficiencies != null)
            //{
            //    foreach (var item in UGWithDeficiencies)
            //    {
            //        foreach (var def in item.UGObservations)
            //        {
            //            list = list + "<li>" + def + "</li>";
            //        }
            //    }
            //}

            //if (list.Contains("some"))
            //    list = list.Replace("some", "required");

            //if (list.Contains("Some"))
            //    list = list.Replace("Some", "Required");

            //list = list + "</ol>";
            //contents = contents.Replace("##DEFICIENCIES##", list);

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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SendCourtDefficiencyLetters()
        {
            List<int?> colleges = db.college_circulars.Where(c => c.circularName == "CourtLetter1").Select(c => c.collegeId).ToList();

            int count = 0;

            if (colleges != null)
            {
                foreach (int cid in colleges)
                {
                    int collegeid = db.jntuh_college.Where(c => c.id == cid).Select(c => c.id).FirstOrDefault();

                    string path = SaveCourtDefficiencyLetter(cid);

                    string strTo = db.jntuh_college_principal_director.Where(a => a.collegeId == collegeid && a.type == "PRINCIPAL").Select(a => a.email).FirstOrDefault();
                    string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
                    string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();

                    //string strTo = "ramesh.bandi@csstechnergy.com";
                    //string strCc = string.Empty;
                    //string strBcc = string.Empty;

                    if (!string.IsNullOrEmpty(strTo))
                    {
                        List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                        attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(path)));

                        //send emails to committee members
                        IUserMailer mailer = new UserMailer();
                        //mailer.SendDeficiencyLetter(strTo, strCc, strBcc, "JNTUH: College Deficiency Letter", attachments).SendAsync();

                        string pMobile = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == "PRINCIPAL").Select(a => a.mobile).FirstOrDefault();
                        //string pMobile = string.Empty;
                        //pMobile = "9493666388";

                        //send sms to committee members
                        string message = "JNTUH: College deficiency letter has been sent to your email. - From Director, UAAC.";
                        //UAAAS.Models.Utilities.SendSms(pMobile, message);

                        //college_undertaking def = db.college_undertaking.Where(u => u.collegeId == collegeid).FirstOrDefault();

                        //def.EmailSent = true;

                        //db.Entry(def).State = EntityState.Modified;
                        //db.SaveChanges();
                        count++;
                    }
                    else
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
            }

            ViewBag.SentCount = count;

            return View();
        }

        private string SaveCourtDefficiencyLetter(int collegeid)
        {
            string collegeCode = db.jntuh_college.Where(c => c.id == collegeid).Select(c => c.collegeCode).FirstOrDefault();

            string fullPath = string.Empty;

            //Set page size as A4
            //var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);
            var pdfDoc = new Document(PageSize.A4, 40, 40, 40, 40);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            //if (!Directory.Exists("~/Content/PDFReports/CourtDefficiencyLetters/"))
            //{
            //    Directory.CreateDirectory("~/Content/PDFReports/CourtDefficiencyLetters/");
            //}

            int letterNumber = 1;

            fullPath = "~/Content/PDFReports/CourtDefficiencyLetters/" + collegeCode + "_IP" + InspectionPhaseId + "_LN" + letterNumber + ".pdf";

            if (System.IO.File.Exists(Server.MapPath(fullPath)))
            {
                System.IO.File.Delete(Server.MapPath(fullPath));
            }

            PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/CourtDeficiencyLetter.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##COLLEGE_CODE##", collegeCode);


            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeid).Select(a => a).FirstOrDefault();
            string district = db.jntuh_district.Find(address.districtId).districtName;
            string scheduleCollegeAddress = string.Empty;

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();
            scheduleCollegeAddress = collegeName + ",<br />" + address.address;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.townOrCity.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.mandal.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;

            if (!scheduleCollegeAddress.ToUpper().Contains(district.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;

            scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

            string list = string.Empty;

            //Get all Ids(college,academicYear,Library,Lab)
            GetIds(collegeid);

            //Binding all permanent deficiency colleges data to table
            List<UGWithDeficiency> UGWithDeficiencies = DeficiencyList(null, "All");
            //int i = 1;
            list = "<ol>";
            if (UGWithDeficiencies != null)
            {
                foreach (var item in UGWithDeficiencies)
                {
                    foreach (var def in item.UGObservations)
                    {
                        list = list + "<li>" + def + "</li>";
                    }
                }
            }

            if (list.Contains("some"))
                list = list.Replace("some", "required");

            if (list.Contains("Some"))
                list = list.Replace("Some", "Required");

            list = list + "</ol>";

            /* string labs = GetLabsShortage(collegeid);
            List<string> computers = GetComputersShortageList(collegeid);
            string principalGrade = GetPrincipalRatified(collegeid);
            List<string> faculty = GetUGFacultyShortageList(collegeid);
            string Number = GetFacultyNumber(collegeid);
            string Percentage = GetFacultyPercentage(collegeid).ToString();
            string Library = GetLibraryShortage(collegeid);
            string Points = GetOverallPoints(collegeid);
            string NBA = GetNBAStatus(collegeid);
            string NAAC = GetNAACStatus(collegeid);




            string sComp = string.Empty;

            if (computers != null)
            {
                foreach (var ComptersShortage in computers)
                {
                    sComp += ComptersShortage + "<br />";
                }
            }

            string sfaculty = string.Empty;

            if (faculty != null)
            {
                foreach (var facultyShortage in faculty)
                {
                    sfaculty += facultyShortage + "<br />";
                }
            }

            string table = string.Empty;
           table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            table += "<tr><td align='center' colspan='3' style='font-size: 8px;'><b>Labs (shortage)</b></td><td align='center'  colspan='4' style='font-size: 8px;'><b>Computers (shortage)</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Principal</b></td><td align='center'  colspan='4'  style='font-size: 8px;'><b>Faculty (shortage)</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Ratified Number</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Ratified %</b></td><td align='center'  colspan='3'  style='font-size: 8px;'><b>Library (shortage)</b></td><td align='center'  colspan='3'  style='font-size: 8px;'><b>Overall Points</b></td><td align='center'  colspan='2'  style='font-size: 8px;'><b>NBA</b></td><td align='center' colspan='2'  style='font-size: 8px;'><b>NAAC</b></td></tr>";
            table += "<tr><td align='center' colspan='3' style='font-size: 8px;'>" + labs + "</td><td align='center' colspan='4'  style='font-size: 8px;'>" + sComp + "</td><td align='center' colspan='3'   style='font-size: 8px;'>" + principalGrade + "</td><td align='center' colspan='4'  style='font-size: 8px;'>" + sfaculty + "</td><td align='center' colspan='3'  style='font-size: 8px;'>" + Number + "</td><td align='center' colspan='3' style='font-size: 8px;'>" + Percentage + "</td><td align='center' colspan='3' style='font-size: 8px;'>" + Library + "</td><td align='center' colspan='3'  style='font-size: 8px;'>" + Points + "/100</td><td align='center' colspan='2'  style='font-size: 8px;'>" + NBA + "</td><td align='center' colspan='2'  style='font-size: 8px;'>" + NAAC + "</td></tr>";
            table += "</table>";

            string table = string.Empty;
            table += "<table border='1' cellpadding='3' cellspacing='0' width='100%' style='font-family: Times New Roman; font-size: 8px; font-weight: normal; background-color: gray; margin: 0 auto;'";
            table += "<tr><td align='center' colspan='3' style='font-size: 8px;'><b>Labs (shortage)</b></td><td align='center'  colspan='4' style='font-size: 8px;'><b>Computers (shortage)</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Principal</b></td><td align='center'  colspan='4'  style='font-size: 8px;'><b>Faculty (shortage)</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Ratified Number</b></td><td align='center'  colspan='3' style='font-size: 8px;'><b>Ratified %</b></td><td align='center'  colspan='3'  style='font-size: 8px;'><b>Library (shortage)</b></td><td align='center'  colspan='3'  style='font-size: 8px;'><b>Overall Points</b></td><td align='center'  colspan='2'  style='font-size: 8px;'><b>NBA</b></td><td align='center' colspan='2'  style='font-size: 8px;'><b>NAAC</b></td><td align='center' colspan='4'  style='font-size: 8px;><b>Remarks</b></td></tr>";
            table += "<tr><td align='center' colspan='3' style='font-size: 8px;'>" + labs + "</td><td align='center' colspan='4'  style='font-size: 8px;'>" + sComp + "</td><td align='center' colspan='3'   style='font-size: 8px;'>" + principalGrade + "</td><td align='center' colspan='4'  style='font-size: 8px;'>" + sfaculty + "</td><td align='center' colspan='3'  style='font-size: 8px;'>" + Number + "</td><td align='center' colspan='3' style='font-size: 8px;'>" + Percentage + "</td><td align='center' colspan='3' style='font-size: 8px;'>" + Library + "</td><td align='center' colspan='3'  style='font-size: 8px;'>" + Points + "/100</td><td align='center' colspan='2'  style='font-size: 8px;'>" + NBA + "</td><td align='center' colspan='2'  style='font-size: 8px;'>" + NAAC + "</td><td align='center' colspan='4'  style='font-size: 8px;'>" + list + "</td></tr>";
            table += "</table>";*/

            //Exelformat Start
            /*  string table = string.Empty;
              table += "<table border='1' style='width: 100%'>";
              table += "<tr  style='border: solid 1px #ccc;'><td colspan='14' style='text-align: right; font-size: 5px;'><u><b>Computers Norms</b></u>: B.Tech-1:4, B.Pharmacy-1:6, M.Tech-1:2, M.Pharmacy-1:6, MBA-1:2, MCA-1:2, MAM-1:2, MTM-1:2, Pharm.D-1:6, Pharm.D PB-1:6 &nbsp;&nbsp;&nbsp;<u><b>Principal</b></u>: A = Quallified Principal, B = Not Qualified Principal &nbsp;&nbsp;&nbsp;<u><b>Faculty Norms</b></u>: B.Tech-1:15, B.Pharmacy-1:15, M.Tech-1:12, M.Pharmacy-1:12, MBA-1:15, MCA-1:15, MAM-1:12, MTM-1:12, Pharm.D-1:12, Pharm.D PB-1:12 &nbsp;&nbsp;&nbsp;</td></tr>";

              table += "<thead><tr>";
              table += "<th style='text-align: center; background-color: yellow; width: 3%' rowspan='1'>S.No</th>";
              table += "<th rowspan='1' style='text-align: center; background-color: yellow; width: 20%'>College Name</th>";
              table += "<th style='text-align: center; background-color: yellow; width: 4%' rowspan='1'>College Code</th>";
              table += "<th style='text-align: center; background-color: yellow; width: 50%' rowspan='1'>Specializations</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Labs (shortage)</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Computers (shortage)</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Principal</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Faculty (shortage)</th>";
                  //table += "<th style='text-align: center; background-color: yellow; width: 13%' colspan='2'>Ratified</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 5%'>Number</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 5%'>%</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Library (shortage)</th>";                
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>NBA</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>NAAC</th>";
                  table += "<th style='text-align: center; background-color: yellow; width: 10%' rowspan='1'>Deficiencies / Remarks (*)</th>";
                
              table += "</tr></thead>";

              //table += "<tr>"; 
              //    table += "<th style='text-align: center; background-color: yellow; width: 50%'>Number</th>";
              //    table += "<th style='text-align: center; background-color: yellow; width: 50%'>%</th>";
              //table += "</tr>";

              table += "<tr><tbody>";//tableRow

              table += "<td style='text-align: center; vertical-align: top'>1</td>";

              table += "<td style='text-align: left; vertical-align: top'>";
              table += "<table style='border-collapse: collapse; width: 550px'><tr><td style='border: none; text-align: left; vertical-align: top'><label>Azad College of Pharmacy.Replace('’', ''') </label></td></tr><tr><td style='border: none; text-align: left; vertical-align: top'><label>MOINABAD, MOINABAD, RANGA REDDY, 501504</label></td></tr><tr><td style='border: none; text-align: left; vertical-align: top'><label></label></td></tr></table>";
              table += "</td>";

              table += "<td style='text-align: center; vertical-align: top'>";
              table += "<table style='border-collapse: collapse; width: 550px'><tr><td style='border: none; text-align: center; vertical-align: top'><label style='text-transform: uppercase'>T6</label></td></tr><tr><td style='border: none; text-align: center; vertical-align: top'><label>2005</label></td></tr></table>";
              table += "</td>";

              table += "<td style=''>";
              table += "<table style='border-collapse: collapse; width: 100%'><tr><td style='border: none; text-align: left; vertical-align: top'><label>B.Pharmacy</label></td><td style='border: none; text-align: right; vertical-align: top'><label>120</label></td></tr></table>";
              table += "</td>";

              table += "<td style='text-align: center; text-align: center'>";
              table += "<table style='border-collapse: collapse; width: 550px'><tr><td style='border: none; text-align: center; vertical-align: top'><label>NIL</label></td></tr></table>";
              table += "</td>";

              table += "<td style='text-align: center;'>";
              table += "<table style='border-collapse: collapse; text-align: center'><tr><td style='border: none; text-align: center; vertical-align: top'>B.Pharmacy-1:14(40)</td></tr></table>";
              table += "</td>";

              table += "<td style='text-align: center; text-align: center'>";
              table += "<table style='border-collapse: collapse; width: 550px'><tr><td style='border: none; text-align: center; vertical-align: top'><label>B</label></td></tr></table>";
              table += "</td>";

              table += "<td style='text-align: center;'>";
              table += "<table style='border-collapse: collapse; text-align: center'><tr><td style='border: none; text-align: center; vertical-align: top'>B.Pharmacy-1:30(14)</td></tr></table>";
              table += "</td>";

              table += "<td style='width: 200px; text-align: center; vertical-align: top'><label>5/20.</label></td>";

              table += "<td style='width: 200px; text-align: center; vertical-align: top'><label>25</label></td>";


              table += "<td style='width: 200px; text-align: center; vertical-align: top'><label>NIL</label></td>";

              table += "<td style='text-align: center; width: 60px; border: solid 1px #ccc; vertical-align: top'><label>Not Yet Applied</label</td>";

              table += "<td style='text-align: center; width: 60px; border: solid 1px #ccc; vertical-align: top'><label>Not Yet Applied</label></td>";

              table += "<td style='width: 200px; text-align: left; vertical-align: top'>";
              table += "<table style='border-collapse: collapse; width: 550px'><tr><td style='border: none; text-align: left; vertical-align: top'><label>1. Shortage of computers (59)</label></td></tr></table>";
              table += "</td>";

              table += "</tr></tbody></table>";

              //End

              contents = contents.Replace("##DEFICIENCIES##", table);*/









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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SendProformaNotSubmittedLetters()
        {
            List<int?> colleges = db.college_circulars.Where(c => c.circularName == "ProformaNotSubmittedLetter").Select(c => c.collegeId).ToList();

            int count = 0;

            if (colleges != null)
            {
                foreach (int cid in colleges)
                {
                    int collegeid = db.jntuh_college.Where(c => c.id == cid).Select(c => c.id).FirstOrDefault();

                    string path = SaveProformaNotSubmittedLetters(cid);

                    string strTo = db.jntuh_college_principal_director.Where(a => a.collegeId == collegeid && a.type == "PRINCIPAL").Select(a => a.email).FirstOrDefault();
                    string strCc = ConfigurationManager.AppSettings["EmailCC"].ToString();
                    string strBcc = ConfigurationManager.AppSettings["EmailBCC"].ToString();

                    //string strTo = "ramesh.bandi@csstechnergy.com";
                    //string strCc = string.Empty;
                    //string strBcc = string.Empty;

                    if (!string.IsNullOrEmpty(strTo))
                    {
                        List<System.Net.Mail.Attachment> attachments = new List<System.Net.Mail.Attachment>();
                        attachments.Add(new System.Net.Mail.Attachment(Server.MapPath(path)));
                        attachments.Add(new System.Net.Mail.Attachment(Server.MapPath("~/Content/PDFReports/ProformaNotSubmittedLetters/PROFORMA-FOR-COLLEGES.doc")));

                        //send emails to committee members
                        IUserMailer mailer = new UserMailer();
                        mailer.SendProformaNotSubmittedLetter(strTo, strCc, strBcc, "JNTUH: Proforma Not Submitted Letter", attachments).SendAsync();

                        string pMobile = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye == "PRINCIPAL").Select(a => a.mobile).FirstOrDefault();
                        //string pMobile = string.Empty;
                        //pMobile = "7675042301";

                        //send sms to committee members
                        string message = "JNTUH: Proforma Not Submitted Letter has been sent to your email.";
                        UAAAS.Models.Utilities.SendSms(pMobile, message);

                        //college_undertaking def = db.college_undertaking.Where(u => u.collegeId == collegeid).FirstOrDefault();

                        //def.EmailSent = true;

                        //db.Entry(def).State = EntityState.Modified;
                        //db.SaveChanges();
                        count++;
                    }
                    else
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
            }

            ViewBag.SentCount = count;

            return View();
        }

        private string SaveProformaNotSubmittedLetters(int collegeid)
        {
            string collegeCode = db.jntuh_college.Where(c => c.id == collegeid).Select(c => c.collegeCode).FirstOrDefault();

            string fullPath = string.Empty;

            //Set page size as A4
            var pdfDoc = new Document(PageSize.A4, 60, 60, 60, 60);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            int letterNumber = 1;

            fullPath = "~/Content/PDFReports/ProformaNotSubmittedLetters/" + collegeCode + "_IP" + InspectionPhaseId + "_LN" + letterNumber + ".pdf";

            if (System.IO.File.Exists(Server.MapPath(fullPath)))
            {
                System.IO.File.Delete(Server.MapPath(fullPath));
            }

            PdfWriter.GetInstance(pdfDoc, new FileStream(Server.MapPath(fullPath), FileMode.Create));

            //Open PDF Document to write data
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;

            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/ProformaNotSubmittedLetter.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();
            contents = contents.Replace("##COLLEGE_CODE##", collegeCode);


            jntuh_address address = db.jntuh_address.Where(a => a.collegeId == collegeid).Select(a => a).FirstOrDefault();
            string district = db.jntuh_district.Find(address.districtId).districtName;
            string scheduleCollegeAddress = string.Empty;

            string collegeName = db.jntuh_college.Where(c => c.collegeCode == collegeCode).Select(c => c.collegeName).FirstOrDefault();
            scheduleCollegeAddress = collegeName + ",<br />" + address.address;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.townOrCity.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.townOrCity;

            if (!scheduleCollegeAddress.ToUpper().Contains(address.mandal.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ", " + address.mandal;

            if (!scheduleCollegeAddress.ToUpper().Contains(district.ToUpper()))
                scheduleCollegeAddress = scheduleCollegeAddress + ",<br />" + district;

            scheduleCollegeAddress = scheduleCollegeAddress + " - " + address.pincode;
            scheduleCollegeAddress = scheduleCollegeAddress.ToUpper().Replace(",", ", ").Replace("(", " (").Replace(")", ") ");

            contents = contents.Replace("##COLLEGE_ADDRESS##", scheduleCollegeAddress);
            //contents = contents.Replace("##LETTER_DATE##", DateTime.Now.ToString("dd-MM-yyy"));

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

            string returnPath = string.Empty;
            returnPath = fullPath;
            return returnPath;
        }

        //ProposedIntake&Approved for AY-2014-15
        private string collegeIntakeProposed201415(int collegeId, string contents)
        {
            string intakeProposed = string.Empty;
            int count = 1;
            int totalIntakeProposed = 0;
            int totalIntakeExist = 0;
            int academicYear = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.isPresentAcademicYear == true).Select(l => l.actualYear).FirstOrDefault();
            int academicYearId = db.jntuh_academic_year.Where(l => l.isActive == true &&
                                                                   l.actualYear == academicYear + 1).Select(l => l.id).FirstOrDefault();
            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId && p.academicYearId == academicYearId).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.id = item.id;
                newProposed.academicYearId = item.academicYearId;
                newProposed.AcademicYear = db.jntuh_academic_year.Where(a => a.id == item.academicYearId).Select(a => a.academicYear).FirstOrDefault();
                newProposed.collegeId = item.collegeId;
                newProposed.courseAffiliationStatusCodeId = item.courseAffiliationStatusCodeId;
                newProposed.CourseAffiliationStatusCode = db.jntuh_course_affiliation_status.Where(a => a.id == newProposed.courseAffiliationStatusCodeId).Select(a => a.courseAffiliationStatusCode).FirstOrDefault();
                newProposed.proposedIntake = item.proposedIntake;
                newProposed.specializationId = item.specializationId;
                newProposed.Specialization = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.specializationName).FirstOrDefault();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                newProposed.degreeDisplayOrder = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degreeDisplayOrder).FirstOrDefault();
                newProposed.Shift = db.jntuh_shift.Where(s => s.id == item.shiftId).Select(s => s.shiftName).FirstOrDefault();
                newProposed.shiftId = item.shiftId;
                collegeIntakeProposedList.Add(newProposed);
            }

            collegeIntakeProposedList = collegeIntakeProposedList.OrderBy(ei => ei.degreeDisplayOrder).ThenBy(ei => ei.Department).ThenBy(ei => ei.Specialization).ThenBy(ei => ei.shiftId).ToList();

            if (collegeIntakeProposedList.Count > 15)
            {
                foreach (var item in collegeIntakeProposedList)
                {
                    string rowBoldStart = string.Empty;
                    string rowBoldEnd = string.Empty;

                    string intakeBoldStart = string.Empty;
                    string intakeBoldEnd = string.Empty;

                    //if (item.ExistingIntake == 0)
                    //{
                    //    rowBoldStart = "<b>";
                    //    rowBoldEnd = "</b>";
                    //}

                    //if (item.ExistingIntake > item.proposedIntake || item.ExistingIntake < item.proposedIntake || item.proposedIntake == 0)
                    //{
                    //    intakeBoldStart = "<b>";
                    //    intakeBoldEnd = "</b>";
                    //}

                    intakeProposed += "<td colspan='2' width='45'><p align='center'>" + rowBoldStart + count + rowBoldEnd + "</p></td>";
                    intakeProposed += "<td colspan='3' style='font-size: 9px;' width='96'>" + rowBoldStart + item.Degree + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='4' style='font-size: 9px;' width='74'>" + rowBoldStart + item.Department + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='7' style='font-size: 9px;' width='196'>" + rowBoldStart + item.Specialization + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='2' width='41' align='center'>" + rowBoldStart + item.Shift + rowBoldEnd + "</td>";
                    intakeProposed += "<td colspan='3' width='78' align='center'></td>";
                    intakeProposed += "<td colspan='3' width='90' align='center'></td>";
                    totalIntakeProposed += item.proposedIntake;

                    totalIntakeExist += 0;

                    count++;
                }
            }
            else
            {
                for (int i = 1; i <= 15; i++)
                {
                    intakeProposed += "<td colspan='2' width='45'><p align='center'>" + i + "</p></td>";
                    intakeProposed += "<td colspan='3' style='font-size: 9px;' width='96'>##COLLEGEPROPOSEDINTAKEDEGREE" + i + "##</td>";
                    intakeProposed += "<td colspan='4' style='font-size: 9px;' width='74'>##COLLEGEPROPOSEDINTAKEDEPARTMENT" + i + "##</td>";
                    intakeProposed += "<td colspan='7' style='font-size: 9px;' width='196'>##COLLEGEPROPOSEDINTAKESPECIALIZATION" + i + "##</td>";
                    intakeProposed += "<td colspan='2' width='41' align='center'>##COLLEGEPROPOSEDINTAKESHIFT" + i + "##</td>";
                    intakeProposed += "<td colspan='3' width='90' align='center'>##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + i + "##</td>";
                    intakeProposed += "<td colspan='3' width='78' align='center'>##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + i + "##</td>";
                }

                foreach (var item in collegeIntakeProposedList)
                {

                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEGREE" + count + "##", item.Degree);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEPARTMENT" + count + "##", item.Department);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESPECIALIZATION" + count + "##", item.Specialization);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESHIFT" + count + "##", item.Shift);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + count + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + count + "##", string.Empty);
                    totalIntakeProposed += item.proposedIntake;

                    totalIntakeExist += 0;

                    count++;

                }

                for (int i = 1; i <= 15; i++)
                {
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEGREE" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEDEPARTMENT" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESPECIALIZATION" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKESHIFT" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKPROPOSEDINTAKE" + i + "##", string.Empty);
                    intakeProposed = intakeProposed.Replace("##COLLEGEPROPOSEDINTAKEEXISTINGINTAKE" + i + "##", string.Empty);
                }

            }
            contents = contents.Replace("##COLLEGEINTAKEPROPOSEDINFORMATION##", intakeProposed);
            contents = contents.Replace("##COLLEGETOTALINTAKEEXISTING##", string.Empty.ToString());
            contents = contents.Replace("##COLLEGETOTALINTAKEPROPOSED##", string.Empty.ToString());
            return contents;
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public int cmd { get; set; }
        public int date { get; set; }

    }
}