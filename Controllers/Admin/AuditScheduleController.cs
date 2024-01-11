using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AuditScheduleController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddEditSchedule(string aid)
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int[] CollegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false).Select(e => e.collegeId).ToArray();
            var Colleges = db.jntuh_college.Where(c => c.isActive == true && CollegeIds.Contains(c.id)).Select(c => new { collegeId = c.id ,collegeName = c.collegeCode +"-"+ c.collegeName}).ToList();
            ViewBag.Colleges = Colleges;

            AuditSchedule schedule = new AuditSchedule();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName).ToList();

            List<SelectListItem> seq = new List<SelectListItem>();

            for (int i = 1; i <= 10; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = i.ToString();
                item.Value = i.ToString();
                seq.Add(item);
            }

            ViewBag.Sequence = seq;

            var jntuh_ffc_schedule = db.jntuh_ffc_schedule.Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_designation = db.jntuh_designation.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(e => e).ToList();
            var jntuh_ffc_committee = db.jntuh_ffc_committee.Select(e => e).ToList();

            if (aid != null)
            {
                int scheduleId = Convert.ToInt32(aid);
                schedule.collegeId = (int)jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.collegeID).FirstOrDefault();
                schedule.districtid = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.districtId).FirstOrDefault();
                schedule.pincode = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.pincode).FirstOrDefault();
                schedule.collegeCode = db.jntuh_college.Where(c => c.id == schedule.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                schedule.auditDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.inspectionDate).FirstOrDefault().ToString());
                schedule.alternateAuditDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.alternateInspectionDate).FirstOrDefault().ToString());
                schedule.orderDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.orderDate).FirstOrDefault().ToString());
                schedule.isRevised = jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.isRevisedOrder).FirstOrDefault() == 1 ? true : false;
                schedule.InspectionPhaseId = (int)jntuh_ffc_schedule.Where(s => s.id == scheduleId).Select(s => s.InspectionPhaseId).FirstOrDefault();

                int alreadyScheduled = 0;
                int value = jntuh_ffc_schedule.Where(s => s.collegeID == schedule.collegeId && s.InspectionPhaseId == schedule.InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();
                if (value > 0)
                    alreadyScheduled = value;

                if (alreadyScheduled > 0 && aid == null)
                {
                    TempData["Error"] = "College already scheduled. Please edit the existing schedule.";
                    return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
                }

                if (schedule.assignedAuditors != null)
                {
                    //make auditorSelected = false when it is deleted from assigned auditors
                    List<int> deleted = schedule.assignedAuditors.Where(a => a.isDeleted == true).Select(a => a.auditorId).ToList();

                    foreach (var item in schedule.availableAuditors)
                    {
                        if (deleted.Contains(item.auditorId))
                        {
                            item.auditorSelected = false;
                        }
                    }

                    //make selected = false when it is deleted from assigned auditors
                    List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();

                    if (convenor.Count() > 1)
                    {
                        ViewBag.MoreConvenors = true;
                        foreach (var item in schedule.assignedAuditors)
                        {
                            if (convenor.Contains(item.auditorId))
                            {
                                item.isConvenor = false;
                            }
                        }
                    }
                    else
                    { ViewBag.MoreConvenors = false; }

                }
                else
                {

                }

                //get college departments
                IQueryable<jntuh_college_intake_existing> existing = db.jntuh_college_intake_existing.Where(p => p.collegeId == schedule.collegeId).Select(e => e);
                var existing_new = existing.AsEnumerable().GroupBy(e => new { e.specializationId }).Select(e => e.First()).ToList();
               
                List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
                List<string> listDepartments = new List<string>();
                foreach (var item in existing_new)
                {
                    CollegeDepartments dept = new CollegeDepartments();
                    dept.deptartmentId = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    dept.deptartmentName = jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
                    dept.degreeId = jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
                    dept.degreeName = jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
                    collegeDepartments.Add(dept);
                    //if (!listDepartments.Contains(dept.deptartmentName))
                    //{
                    //    collegeDepartments.Add(dept);
                    //    listDepartments.Add(dept.deptartmentName);
                    //}
                }
                schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

                List<jntuh_ffc_committee> existingCommittee = jntuh_ffc_committee.OrderBy(c => c.memberOrder).Where(c => c.scheduleID == scheduleId).Select(c => c).ToList();

                if (schedule.availableAuditors == null)
                {
                    //get all auditors
                    List<jntuh_ffc_auditor> auditors = jntuh_ffc_auditor.Where(a => a.isActive == true).OrderBy(a => new { a.auditorDepartmentID, a.auditorDesignationID, a.auditorName }).ToList();
                    List<AvailableAuditors> available = new List<AvailableAuditors>();

                    foreach (var item in auditors)
                    {
                        AvailableAuditors auditor = new AvailableAuditors();
                        auditor.departmentId = (int)item.auditorDepartmentID;
                        auditor.deptartmentName = jntuh_department.Where(d => d.id == auditor.departmentId).Select(d => d.departmentName).FirstOrDefault();
                        auditor.auditorId = item.id;
                        auditor.auditorName = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorName).FirstOrDefault().Replace(".", ". ");
                        auditor.auditorDesignation = jntuh_designation.Where(d => d.id == item.auditorDesignationID).Select(d => d.designation).FirstOrDefault();
                        auditor.preferredDesignation = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorPreferredDesignation).FirstOrDefault();
                        DateTime aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                        int load = (from s in jntuh_ffc_schedule
                                    join c in jntuh_ffc_committee on s.id equals c.scheduleID
                                    where s.inspectionDate == aDate && c.auditorID == auditor.auditorId
                                    select c.id).Count();

                        auditor.auditorLoad = load.ToString();
                        auditor.auditorSelected = false;

                        jntuh_ffc_committee existingCommitteeMember = jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.id).Select(c => c).FirstOrDefault();
                        if (existingCommitteeMember != null)
                        { auditor.auditorSelected = true; }

                        available.Add(auditor);
                    }
                    schedule.availableAuditors = available.OrderBy(a => a.deptartmentName).ThenByDescending(a => a.auditorDesignation).ThenBy(a => a.auditorName).Select(a => a).ToList();
                }

                if (schedule.availableAuditors != null)
                {
                    List<AvailableAuditors> available = schedule.availableAuditors.Where(a => a.auditorSelected == true).ToList();
                    List<AssignedAuditors> newAssigned = new List<AssignedAuditors>();
                    var assigned = schedule.assignedAuditors;
                    foreach (var item in available)
                    {
                        AssignedAuditors auditor = new AssignedAuditors();
                        auditor.departmentId = item.departmentId;
                        auditor.deptartmentName = item.deptartmentName;
                        auditor.auditorId = item.auditorId;
                        auditor.auditorName = item.auditorName;
                        auditor.auditorDesignation = item.auditorDesignation;
                        auditor.preferredDesignation = item.preferredDesignation;
                        auditor.auditorLoad = item.auditorLoad;
                        auditor.isConvenor = false;

                        jntuh_ffc_committee isCommitteeMemberConvenor = jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.auditorId && c.isConvenor == 1).Select(c => c).FirstOrDefault();
                        if (isCommitteeMemberConvenor != null)
                        { auditor.isConvenor = true; }

                        auditor.isDeleted = false;

                        jntuh_ffc_committee committeeMember = jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.auditorId).Select(c => c).FirstOrDefault();

                        if (committeeMember != null)
                        {
                            if (committeeMember.memberOrder != null)
                            {
                                auditor.memberOrder = (int)committeeMember.memberOrder;
                            }
                        }
                        newAssigned.Add(auditor);
                    }
                    schedule.assignedAuditors = newAssigned.ToList();
                }
            }
            return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddEditSchedule(AuditSchedule schedule, string aid, string cmd)
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();

            int[] CollegeIds = db.jntuh_college_edit_status.Where(s => s.IsCollegeEditable == false).Select(e => e.collegeId).ToArray();
            var Colleges = db.jntuh_college.Where(c => c.isActive == true && CollegeIds.Contains(c.id)).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).ToList();
            ViewBag.Colleges = Colleges;

            var jntuh_ffc_schedule = db.jntuh_ffc_schedule.Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_designation = db.jntuh_designation.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(e => e).ToList();
            var jntuh_ffc_committee = db.jntuh_ffc_committee.Select(e => e).ToList();

          


            List<SelectListItem> seq = new List<SelectListItem>();

            for (int i = 1; i <= 10; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = i.ToString();
                item.Value = i.ToString();
                seq.Add(item);
            }

            ViewBag.Sequence = seq;

            int InspectionPhaseId = 0;

            if (aid == null)
            {
                InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
            }
            else
            {
                InspectionPhaseId = (int)schedule.InspectionPhaseId;
            }

            int alreadyScheduled = 0;
            int value = jntuh_ffc_schedule.Where(s => s.collegeID == schedule.collegeId && s.InspectionPhaseId == InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();
            if (value > 0)
                alreadyScheduled = value;

            if (alreadyScheduled > 0 && aid == null)
            {
                TempData["Error"] = "College already scheduled. Please edit the existing schedule.";
                return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
            }

            if (schedule.assignedAuditors != null)
            {
                //make auditorSelected = false when it is deleted from assigned auditors
                List<int> deleted = schedule.assignedAuditors.Where(a => a.isDeleted == true).Select(a => a.auditorId).ToList();

                foreach (var item in schedule.availableAuditors)
                {
                    if (deleted.Contains(item.auditorId))
                    {
                        item.auditorSelected = false;
                    }
                }

                //make selected = false when it is deleted from assigned auditors
                List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();

                if (convenor.Count() > 1)
                {
                    ViewBag.MoreConvenors = true;
                    foreach (var item in schedule.assignedAuditors)
                    {
                        if (convenor.Contains(item.auditorId))
                        {
                            item.isConvenor = false;
                        }
                    }
                }
                else
                { ViewBag.MoreConvenors = false; }

            }

            //get college departments
            IQueryable<jntuh_college_intake_existing> existing = db.jntuh_college_intake_existing.Where(p => p.collegeId == schedule.collegeId).Select(e=>e);
            var existing_new = existing.AsEnumerable().GroupBy(e => new { e.specializationId }).Select(e => e.First()).ToList();
            List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
            List<string> listDepartments = new List<string>();
            foreach (var item in existing_new)
            {
                CollegeDepartments dept = new CollegeDepartments();
                dept.deptartmentId = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                dept.deptartmentName = jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
                dept.degreeId = jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
                dept.degreeName = jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
                collegeDepartments.Add(dept);
                //if (!listDepartments.Contains(dept.deptartmentName))
                //{
                //    collegeDepartments.Add(dept);
                //    listDepartments.Add(dept.deptartmentName);
                //}
            }
            schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

            if (schedule.availableAuditors == null)
            {
                //get all auditors
                List<jntuh_ffc_auditor> auditors = db.jntuh_ffc_auditor.Where(a => a.isActive == true).OrderBy(a => new { a.auditorDepartmentID, a.auditorDesignationID, a.auditorName }).ToList();
                List<AvailableAuditors> available = new List<AvailableAuditors>();

                foreach (var item in auditors)
                {
                    AvailableAuditors auditor = new AvailableAuditors();
                    auditor.departmentId = (int)item.auditorDepartmentID;
                    auditor.deptartmentName = jntuh_department.Where(d => d.id == auditor.departmentId).Select(d => d.departmentName).FirstOrDefault();
                    auditor.auditorId = item.id;
                    auditor.auditorName = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorName).FirstOrDefault().Replace(".", ". ");
                    auditor.auditorDesignation = jntuh_designation.Where(d => d.id == item.auditorDesignationID).Select(d => d.designation).FirstOrDefault();
                    auditor.preferredDesignation = jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorPreferredDesignation).FirstOrDefault();
                    DateTime aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    int load = (from s in jntuh_ffc_schedule
                                join c in jntuh_ffc_committee on s.id equals c.scheduleID
                                where s.inspectionDate == aDate && c.auditorID == auditor.auditorId
                                select c.id).Count();

                    auditor.auditorLoad = load.ToString();
                    auditor.auditorSelected = false;
                    available.Add(auditor);
                }
                schedule.availableAuditors = available.OrderBy(a => a.deptartmentName).ThenByDescending(a => a.auditorDesignation).ThenBy(a => a.auditorName).Select(a => a).ToList();
            }
            else
            {
                if ((cmd != null && cmd.ToUpper() != "SAVE") || cmd == null)
                {
                    List<AvailableAuditors> available = schedule.availableAuditors.Where(a => a.auditorSelected == true).ToList();
                    List<AssignedAuditors> newAssigned = new List<AssignedAuditors>();
                    var assigned = schedule.assignedAuditors;
                    foreach (var item in available)
                    {
                        AssignedAuditors auditor = new AssignedAuditors();
                        auditor.departmentId = item.departmentId;
                        auditor.deptartmentName = item.deptartmentName;
                        auditor.auditorId = item.auditorId;
                        auditor.auditorName = item.auditorName;
                        auditor.auditorDesignation = item.auditorDesignation;
                        auditor.preferredDesignation = item.preferredDesignation;
                        auditor.auditorLoad = item.auditorLoad;
                        if (schedule.assignedAuditors != null && assigned.Where(a => a.auditorId == auditor.auditorId).Select(a => a.isConvenor).FirstOrDefault() == true)
                            auditor.isConvenor = true;
                        else
                            auditor.isConvenor = false;

                        auditor.isDeleted = false;
                        newAssigned.Add(auditor);
                    }
                    schedule.assignedAuditors = newAssigned.ToList();
                }
            }

            if (cmd != null && cmd.ToUpper() == "SAVE")
            {
                //must have atleast one assigned auditor & must have a convenor
                TempData["auditorsCount"] = null;

                //if(schedule.assignedAuditors == null)
                //{
                //    TempData["auditorsCount"] = "Please Select auditors";
                //    return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
                //}


               // List<int> auditors = schedule.assignedAuditors.Select(a => a.auditorId).ToList();
              //  List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();




              //  if (auditors.Count() == 0 || convenor.Count() == 0)
                //if (auditors.Count() == 0)
                //{
                //    TempData["auditorsCount"] = "Please Select auditors";
                //    return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
                //}
                if (schedule.assignedAuditors == null)
                {
                    ViewBag.NoConvenor = false;

                    //insert the schedule
                    jntuh_ffc_schedule newSchedule = new jntuh_ffc_schedule();
                    newSchedule.collegeID = schedule.collegeId;
                    //newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.orderDate);
                    newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    newSchedule.inspectionDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
                    newSchedule.InspectionPhaseId = InspectionPhaseId;

                    if (schedule.alternateAuditDate != null)
                        newSchedule.alternateInspectionDate = Utilities.DDMMYY2MMDDYY(schedule.alternateAuditDate);

                    int existingId = jntuh_ffc_schedule.Where(f => f.collegeID == schedule.collegeId && f.InspectionPhaseId == InspectionPhaseId).Select(f => f.id).FirstOrDefault();

                    if (existingId == 0)
                    {
                        //newSchedule.isRevisedOrder = 0;
                        newSchedule.createdBy = userID;
                        newSchedule.createdOn = DateTime.Now;
                        db.jntuh_ffc_schedule.Add(newSchedule);
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule added successfully";
                    }
                    else
                    {
                        newSchedule.id = existingId;
                        //newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
                        newSchedule.createdBy = jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdBy).FirstOrDefault();
                        newSchedule.createdOn = jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdOn).FirstOrDefault();
                        newSchedule.updatedBy = userID;
                        newSchedule.updatedOn = DateTime.Now;
                        db.Entry(newSchedule).State = EntityState.Modified;
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule updated successfully";
                    }

                    int scheduleId = newSchedule.id;

                    //insert committee members
                    //if (scheduleId > 0)
                    //{
                    //    var existingAuditors = jntuh_ffc_committee.Where(f => f.scheduleID == scheduleId).Select(f => f).ToList();

                    //    foreach (var item in existingAuditors)
                    //    {
                    //        jntuh_ffc_committee jntuh_ffc_committee_new = jntuh_ffc_committee.Where(f => f.id == item.id).Select(e => e).FirstOrDefault();
                    //        db.jntuh_ffc_committee.Remove(jntuh_ffc_committee_new);
                    //        db.SaveChanges();
                    //    }
                    //    if (schedule.assignedAuditors == null)
                    //    {
                    //        jntuh_ffc_committee committee = new jntuh_ffc_committee();
                    //        committee.scheduleID = scheduleId;
                    //        committee.auditorID = null;
                    //        committee.isConvenor = 0;
                    //        committee.memberOrder = 0;
                    //        db.jntuh_ffc_committee.Add(committee);
                    //        db.SaveChanges();
                    //    }
                       

                    //}

                    TempData["Success"] = "Schedule saved successfully";
                }
                else if (schedule.assignedAuditors.Count() == 1)
                {
                    TempData["auditorsCount"] = "Please Select 2 auditors";
                    return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
                }
                else
                {
                    ViewBag.NoConvenor = false;

                    //insert the schedule
                    jntuh_ffc_schedule newSchedule = new jntuh_ffc_schedule();
                    newSchedule.collegeID = schedule.collegeId;
                    //newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.orderDate);
                    newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    newSchedule.inspectionDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
                    newSchedule.InspectionPhaseId = InspectionPhaseId;

                    if (schedule.alternateAuditDate != null)
                        newSchedule.alternateInspectionDate = Utilities.DDMMYY2MMDDYY(schedule.alternateAuditDate);

                    int existingId = jntuh_ffc_schedule.Where(f => f.collegeID == schedule.collegeId && f.InspectionPhaseId == InspectionPhaseId).Select(f => f.id).FirstOrDefault();

                    if (existingId == 0)
                    {
                        //newSchedule.isRevisedOrder = 0;
                        newSchedule.createdBy = userID;
                        newSchedule.createdOn = DateTime.Now;
                        db.jntuh_ffc_schedule.Add(newSchedule);
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule added successfully";
                    }
                    else
                    {
                        newSchedule.id = existingId;
                        //newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
                        newSchedule.createdBy = jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdBy).FirstOrDefault();
                        newSchedule.createdOn = jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdOn).FirstOrDefault();
                        newSchedule.updatedBy = userID;
                        newSchedule.updatedOn = DateTime.Now;
                        db.Entry(newSchedule).State = EntityState.Modified;
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule updated successfully";
                    }

                    int scheduleId = newSchedule.id;

                    //insert committee members
                    if (scheduleId > 0)
                    {
                        var existingAuditors = jntuh_ffc_committee.Where(f => f.scheduleID == scheduleId).Select(f => f).ToList();

                        foreach (var item in existingAuditors)
                        {
                            jntuh_ffc_committee jntuh_ffc_committee_new = jntuh_ffc_committee.Where(f => f.id == item.id).Select(e => e).FirstOrDefault();
                            db.jntuh_ffc_committee.Remove(jntuh_ffc_committee_new);
                            db.SaveChanges();
                        }
                       
                            foreach (var item in schedule.assignedAuditors)
                            {
                                jntuh_ffc_committee committee = new jntuh_ffc_committee();
                                committee.scheduleID = scheduleId;
                                committee.auditorID = item.auditorId;
                                committee.isConvenor = item.isConvenor == true ? 1 : 0;
                                committee.memberOrder = item.memberOrder;

                                int existingAuditor = jntuh_ffc_committee.Where(f => f.auditorID == item.auditorId && f.scheduleID == scheduleId).Select(f => f.id).FirstOrDefault();

                                if (existingAuditor == 0)
                                {
                                    db.jntuh_ffc_committee.Add(committee);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    committee.id = existingAuditor;
                                    db.Entry(committee).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }
                        

                    }

                    TempData["Success"] = "Schedule saved successfully";
                }
            }

            return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetPincodes(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }

            int did = Convert.ToInt32(id);
            var data = db.jntuh_address.Where(a => a.districtId == did && a.addressTye == "COLLEGE").Select(a => a.pincode).Distinct().OrderBy(a => a).ToList()
                         .Select(a => new SelectListItem()
                          {
                              Text = a.ToString(),
                              Value = a.ToString(),
                          });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetPincodeColleges(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }

            int pin = Convert.ToInt32(id);
            var data = db.jntuh_address.Join(db.jntuh_college, address => address.collegeId, college => college.id, (address, college) => new { college.id, college.collegeName, college.collegeCode, address.pincode, address.addressTye, college.isActive })
                                       .Where(address => address.pincode == pin && address.addressTye == "COLLEGE" && address.isActive == true)
                                       .Select(college => new
                                       {
                                           Value = college.id,
                                           Text = college.collegeName
                                       }).OrderBy(a => a.Text);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetCollegeDepartments(string id)
        {
            if (id == string.Empty)
            {
                id = "0";
            }

            int collegeId = db.jntuh_college.Where(c => c.collegeCode == id).Select(c => c.id).FirstOrDefault();

            var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();

            List<jntuh_college_intake_existing> existing = db.jntuh_college_intake_existing.Where(p => p.collegeId == collegeId).ToList();

            List<CollegeIntakeExisting> collegeIntakeExistingList = new List<CollegeIntakeExisting>();

            foreach (var item in existing)
            {
                CollegeIntakeExisting newProposed = new CollegeIntakeExisting();
               // CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.DepartmentID = jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                collegeIntakeExistingList.Add(newProposed);
            }

            var data = collegeIntakeExistingList.OrderBy(a => new { a.Degree, a.Department });
            ViewBag.Departments = collegeIntakeExistingList.OrderBy(a => new { a.Degree, a.Department });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
