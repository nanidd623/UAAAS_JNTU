using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
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
    public class AutoAllocationEditController : BaseController
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

            AuditSchedule schedule = new AuditSchedule();
            ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName).ToList();

            int[] CollegeIds = db.jntuh_college_edit_status.Where(s=>s.IsCollegeEditable ==false).Select(e=>e.collegeId).ToArray();
            IQueryable<jntuh_college> CollegeList = db.jntuh_college.Where(e => e.isActive == true && CollegeIds.Contains(e.id)).Select(c => c);
            var Colleges = CollegeList.Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).ToList();
            ViewBag.Colleges = Colleges;


            var jntuh_ffc_schedule = db.jntuh_ffc_schedule.Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_specialization = db.jntuh_specialization.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_designation = db.jntuh_designation.Where(d => d.isActive == true).Select(e => e).ToList();
            var jntuh_ffc_auditor = db.jntuh_ffc_auditor.Select(e => e).ToList();
            var jntuh_ffc_committee = db.jntuh_ffc_committee.Select(e => e).ToList();
            var jntuh_address = db.jntuh_address.Select(e => e).ToList();
            var jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Select(e => e).ToList();

            List<SelectListItem> seq = new List<SelectListItem>();

            for (int i = 1; i <= 10; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = i.ToString();
                item.Value = i.ToString();
                seq.Add(item);
            }

            ViewBag.Sequence = seq;

            if (aid != null)
            {
                int scheduleId = Convert.ToInt32(aid);
                schedule.collegeId = (int)jntuh_ffc_schedule.Where(e=>e.id == scheduleId).Select(e=>e.collegeID).FirstOrDefault();
                schedule.districtid = jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.districtId).FirstOrDefault();
                schedule.pincode = jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.pincode).FirstOrDefault();
                schedule.collegeCode = CollegeList.Where(c => c.id == schedule.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                schedule.auditDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(e => e.id == scheduleId).Select(e => e.inspectionDate).FirstOrDefault().ToString());
                schedule.alternateAuditDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(e => e.id == scheduleId).Select(e => e.alternateInspectionDate).FirstOrDefault().ToString());
                schedule.orderDate = Utilities.MMDDYY2DDMMYY(jntuh_ffc_schedule.Where(e => e.id == scheduleId).Select(e => e.orderDate).FirstOrDefault().ToString());
                schedule.isRevised = jntuh_ffc_schedule.Where(e => e.id == scheduleId).Select(e => e.isRevisedOrder).FirstOrDefault() == 1 ? true : false;
                schedule.InspectionPhaseId = (int)jntuh_ffc_schedule.Where(e => e.id == scheduleId).Select(e => e.InspectionPhaseId).FirstOrDefault();

                int alreadyScheduled = 0;
                int value = jntuh_ffc_schedule.Where(s => s.collegeID == schedule.collegeId && s.InspectionPhaseId == schedule.InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();
                if (value > 0)
                    alreadyScheduled = value;

                if (alreadyScheduled > 0 && aid == null)
                {
                    TempData["Error"] = "College already scheduled. Please edit the existing schedule.";
                    return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
                }

                #region AssignedAuditors

                if (schedule.assignedAuditors != null)
                {
                    //make auditorSelected = false when it is deleted from assigned auditors
                    List<int> deleted = schedule.assignedAuditors.Where(a => a.isDeleted == true).Select(a => a.auditorId).ToList();

                    List<AssignedAuditors> alreadyAssigned = new List<AssignedAuditors>();

                    foreach (var member in schedule.assignedAuditors)
                    {
                        if (!deleted.Contains(member.auditorId))
                        {
                            alreadyAssigned.Add(member);
                        }
                        else
                        {
                            string ePlace = jntuh_ffc_auditor.Where(s => s.id == member.auditorId).Select(e=>e.auditorPlace).FirstOrDefault();
                            string Group = jntuh_ffc_external_auditor_groups.Where(g => g.University == ePlace).Select(g => g.Group).FirstOrDefault();

                            //get places of the group
                            List<string> places = jntuh_ffc_external_auditor_groups.Where(a => a.Group == Group).Select(a => a.University).ToList();

                            //get auditor ids of the group
                            List<int> lstAuditors = jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.id != member.auditorId && a.isActive == true).Select(a => a.id).ToList();

                            int auditorId = GetRandomAuditor(lstAuditors, schedule.auditDate);

                            if (auditorId > 0)
                            {
                                var item = db.jntuh_ffc_auditor.Find(auditorId);
                                AssignedAuditors auditor = new AssignedAuditors();
                                auditor.departmentId = 0; //item.auditorDepartmentID;
                                auditor.deptartmentName = item.auditorPlace;
                                auditor.auditorId = item.id;
                                auditor.auditorName = item.auditorName;
                                auditor.auditorDesignation = item.jntuh_designation.designation;
                                auditor.preferredDesignation = jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
                                auditor.auditorLoad = "0";

                                if (alreadyAssigned.Count() == 0)
                                {
                                    auditor.isConvenor = true;
                                }

                                auditor.isDeleted = false;
                                auditor.memberOrder = alreadyAssigned.Count() + 1;
                                alreadyAssigned.Add(auditor);
                            }
                        }
                    }

                    schedule.assignedAuditors.Clear();
                    schedule.assignedAuditors = alreadyAssigned.ToList();

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
                    List<int?> existingAuditors = jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId).OrderBy(c => c.id).Select(c => c.auditorID).ToList();

                    List<AssignedAuditors> autoAssigned = new List<AssignedAuditors>();

                    foreach (var auditorId in existingAuditors)
                    {
                        if (auditorId > 0)
                        {
                            var item = jntuh_ffc_auditor.Where(a => a.id == auditorId).Select(e => e).FirstOrDefault();
                            AssignedAuditors auditor = new AssignedAuditors();
                            auditor.departmentId = 0; //item.auditorDepartmentID;
                            auditor.deptartmentName = item.auditorPlace;
                            auditor.auditorId = item.id;
                            auditor.auditorName = item.auditorName;
                            auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                            auditor.preferredDesignation = jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
                            auditor.auditorLoad = "0";

                            if (autoAssigned.Count() == 0)
                            {
                                auditor.isConvenor = true;
                            }

                            auditor.isDeleted = false;
                            auditor.memberOrder = autoAssigned.Count() + 1;
                            autoAssigned.Add(auditor);
                        }
                    }

                    schedule.assignedAuditors = autoAssigned.ToList();
                }

                #endregion

                //get college departments
                List<jntuh_college_intake_existing> existing = db.jntuh_college_intake_existing.Where(p => p.collegeId == schedule.collegeId).ToList();
                var existing_new = existing.AsEnumerable().GroupBy(e => new { e.specializationId }).Select(e=>e.First()).ToList();
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

                #region AvailableAuditors

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

                #endregion
            }

            return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddEditSchedule(AuditSchedule schedule, string aid, string cmd)
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
           // ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).ToList();

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
            var jntuh_address = db.jntuh_address.Select(e => e).ToList();
            var jntuh_ffc_external_auditor_groups = db.jntuh_ffc_external_auditor_groups.Select(e => e).ToList();

            List<SelectListItem> seq = new List<SelectListItem>();

            for (int i = 1; i <= 10; i++)
            {
                SelectListItem item = new SelectListItem();
                item.Text = i.ToString();
                item.Value = i.ToString();
                seq.Add(item);
            }

            ViewBag.Sequence = seq;

            #region CollegeAlreadyScheduled

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
                return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
            }

            #endregion

            #region AssignedAuditors

            ViewBag.DisableSave = false;
            bool sameInspectionDate = false;
            DateTime? newDate = new DateTime();
            if (aid != null)
            {
                int sId = Convert.ToInt32(aid);
                DateTime? existingDate = jntuh_ffc_schedule.Where(s => s.id == sId).Select(s => s.inspectionDate).FirstOrDefault() ;
                newDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);

                if (newDate == existingDate)
                {
                    sameInspectionDate = true;
                }
            }

            if (schedule.assignedAuditors != null)
            {
                //make auditorSelected = false when it is deleted from assigned auditors
                List<int> deleted = schedule.assignedAuditors.Where(a => a.isDeleted == true).Select(a => a.auditorId).ToList();

                List<AssignedAuditors> alreadyAssigned = new List<AssignedAuditors>();

                foreach (var member in schedule.assignedAuditors)
                {
                    if (!deleted.Contains(member.auditorId))
                    {
                        if (sameInspectionDate == false && aid != null)
                        {
                            //verify whether the aid is allotted on the given date
                            var eRow = jntuh_ffc_committee.Join(db.jntuh_ffc_schedule, comm => comm.scheduleID, sche => sche.id, (comm, sche) => new { comm, sche })
                                         .Join(jntuh_ffc_auditor, all => all.comm.auditorID, aud => aud.id, (all, aud) => new { all, aud })
                                         .Where(a => a.all.sche.InspectionPhaseId == 3 && a.aud.id == member.auditorId && a.all.sche.inspectionDate == newDate)
                                         .Select(c => c).FirstOrDefault();

                            if (eRow != null)
                            {
                                member.auditorLoad = "Already Assigned";
                                ViewBag.DisableSave = true;
                            }

                        }

                        alreadyAssigned.Add(member);
                    }
                    else
                    {
                        string ePlace = jntuh_ffc_auditor.Where(s => s.id==member.auditorId).Select(e=>e.auditorPlace).FirstOrDefault();
                        string Group = jntuh_ffc_external_auditor_groups.Where(g => g.University == ePlace).Select(g => g.Group).FirstOrDefault();

                        //get places of the group
                        List<string> places = jntuh_ffc_external_auditor_groups.Where(a => a.Group == Group).Select(a => a.University).ToList();

                        //get auditor ids of the group
                        List<int> lstAuditors = jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.id != member.auditorId && a.isActive == true).Select(a => a.id).ToList();
                        DateTime? aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                        List<int?> sameDayAuditors = jntuh_ffc_committee
                                                      .Join(jntuh_ffc_schedule, c => c.scheduleID, s => s.id, (c, s) => new { c, s })
                                                      .Join(jntuh_ffc_auditor, top1 => top1.c.auditorID, a => a.id, (top1, a) => new { top1, a })
                                                      .Join(jntuh_ffc_external_auditor_groups, top2 => top2.a.auditorPlace, g => g.University, (top2, g) => new { top2, g })
                                                      .Where(a => a.top2.top1.s.inspectionDate == aDate && a.g.Group == Group).Select(a => a.top2.top1.c.auditorID).ToList();

                        List<int> remaining = new List<int>();

                        foreach (int mAudi in lstAuditors)
                        {
                            if (!sameDayAuditors.Contains(mAudi))
                            {
                                remaining.Add(mAudi);
                            }
                        }

                        if (remaining.Count() > 0)
                        {
                            int auditorId = GetRandomAuditor(remaining, schedule.auditDate);

                            if (auditorId > 0)
                            {
                                var item = db.jntuh_ffc_auditor.Find(auditorId);
                                AssignedAuditors auditor = new AssignedAuditors();
                                auditor.departmentId = 0; //item.auditorDepartmentID;
                                auditor.deptartmentName = item.auditorPlace;
                                auditor.auditorId = item.id;
                                auditor.auditorName = item.auditorName;
                                auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                                auditor.preferredDesignation = jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
                                auditor.auditorLoad = "0";

                                if (alreadyAssigned.Count() == 0)
                                {
                                    auditor.isConvenor = true;
                                }

                                auditor.isDeleted = false;
                                auditor.memberOrder = alreadyAssigned.Count() + 1;
                                alreadyAssigned.Add(auditor);
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Do not have sufficient members to replace.";
                            return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
                        }
                    }
                }

                schedule.assignedAuditors.Clear();
                schedule.assignedAuditors = alreadyAssigned.ToList();

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
            else if(schedule.assignedAuditors == null)
            {

            }
            else
            {
                //get group wise count and get least assigned groups
                var groups = jntuh_ffc_committee.Join(db.jntuh_ffc_schedule, comm => comm.scheduleID, sche => sche.id, (comm, sche) => new { comm, sche })
                    .Join(jntuh_ffc_auditor, all => all.comm.auditorID, aud => aud.id, (all, aud) => new { all, aud })
                    .Join(jntuh_ffc_external_auditor_groups, all => all.aud.auditorPlace, group => group.University, (all, group) => new { all, group })
                             .Where(a => a.all.all.sche.InspectionPhaseId == 3)
                             .Select(a => new
                             {
                                 Place = a.group.Group
                             }).ToList()
                             .GroupBy(a => a.Place)
                             .Select(a => new
                             {
                                 Place = a.Key,
                                 Count = a.Count()
                             }).OrderBy(a => a.Count).ToList();

                var lstGroups = jntuh_ffc_external_auditor_groups.Select(g => new { Place = g.Group, Count = 0 }).ToList();

                foreach (var item in lstGroups)
                {
                    if (!groups.Contains(item))
                    {
                        groups.Add(item);
                    }
                }

                groups = groups.OrderBy(a => a.Count).ThenBy(a => a.Place).ToList();

                List<AssignedAuditors> autoAssigned = new List<AssignedAuditors>();

                foreach (var group in groups)
                {
                    if (autoAssigned.Count() < 3)
                    {
                        //get places of the group
                        List<string> places = jntuh_ffc_external_auditor_groups.Where(a => a.Group == group.Place).Select(a => a.University).ToList();

                        //get auditor ids of the group
                        List<int> lstAuditors = jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.isActive == true).Select(a => a.id).ToList();

                        DateTime? aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                        List<int?> sameDayAuditors = jntuh_ffc_committee
                                                      .Join(jntuh_ffc_schedule, c => c.scheduleID, s => s.id, (c, s) => new { c, s })
                                                      .Join(jntuh_ffc_auditor, top1 => top1.c.auditorID, a => a.id, (top1, a) => new { top1, a })
                                                      .Join(jntuh_ffc_external_auditor_groups, top2 => top2.a.auditorPlace, g => g.University, (top2, g) => new { top2, g })
                                                      .Where(a => a.top2.top1.s.inspectionDate == aDate && a.g.Group == group.Place).Select(a => a.top2.top1.c.auditorID).ToList();

                        List<int> remaining = new List<int>();

                        foreach (int mAudi in lstAuditors)
                        {
                            if (!sameDayAuditors.Contains(mAudi))
                            {
                                remaining.Add(mAudi);
                            }
                        }

                        if (remaining.Count() > 0)
                        {

                            int auditorId = GetRandomAuditor(remaining, schedule.auditDate);

                            if (auditorId > 0)
                            {
                                var item = db.jntuh_ffc_auditor.Find(auditorId);
                                AssignedAuditors auditor = new AssignedAuditors();
                                auditor.departmentId = 0; //item.auditorDepartmentID;
                                auditor.deptartmentName = item.auditorPlace;
                                auditor.auditorId = item.id;
                                auditor.auditorName = item.auditorName;
                                auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                                auditor.preferredDesignation = jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
                                auditor.auditorLoad = "0";

                                if (autoAssigned.Count() == 0)
                                {
                                    auditor.isConvenor = true;
                                }

                                auditor.isDeleted = false;
                                auditor.memberOrder = autoAssigned.Count() + 1;
                                autoAssigned.Add(auditor);
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Few colleges not scheduled due to in-sufficient members in " + group.Place + ".";
                            return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
                        }
                    }
                }

                schedule.assignedAuditors = autoAssigned.ToList();
            }


            #endregion

            #region CollegeDepartments

            //get college departments
            List<jntuh_college_intake_existing> existing = db.jntuh_college_intake_existing.Where(p => p.collegeId == schedule.collegeId).ToList();
            var existing_new = existing.AsEnumerable().GroupBy(e => new { e.specializationId }).Select(e => e.First()).ToList();
            List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
            List<string> listDepartments = new List<string>();
            foreach (var item in existing_new)
            {
                CollegeDepartments dept = new CollegeDepartments();
                dept.deptartmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                dept.deptartmentName = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
                dept.degreeId = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
                dept.degreeName = db.jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
                collegeDepartments.Add(dept);
                //if (!listDepartments.Contains(dept.deptartmentName))
                //{
                //    collegeDepartments.Add(dept);
                //    listDepartments.Add(dept.deptartmentName);
                //}
            }
            schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

            #endregion

            #region AvailableAuditors

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

            #endregion

            #region SAVE

            if (cmd != null && cmd.ToUpper() == "SAVE" && schedule.assignedAuditors != null)
            {
                //must have atleast one assigned auditor & must have a convenor

                List<int> auditors = schedule.assignedAuditors.Select(a => a.auditorId).ToList();
                List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();
               
                //if (auditors.Count() == 0 || convenor.Count() == 0)
                if (auditors.Count() == 0)
                {
                    TempData["auditorsCount"] = "Please Select auditors";
                    return View("~/Views/Admin/AuditSchedule.cshtml", schedule);
                }
                else if (auditors.Count() == 1)
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
                        jntuh_ffc_schedule sch = jntuh_ffc_schedule.Where(s => s.id == newSchedule.id).Select(e=>e).FirstOrDefault();
                        ((IObjectContextAdapter)db).ObjectContext.Detach(sch);

                        db.Entry(newSchedule).State = EntityState.Modified;
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule updated successfully";
                    }

                    int scheduleId = newSchedule.id;

                    //insert committee members
                    if (scheduleId > 0)
                    {
                        var existingAuditors =jntuh_ffc_committee.Where(f => f.scheduleID == scheduleId).Select(f => f).ToList();

                        foreach (var item in existingAuditors)
                        {
                            jntuh_ffc_committee jntuh_ffc_committee_new = jntuh_ffc_committee.Where(f=>f.id ==item.id).Select(e=>e).FirstOrDefault();
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
            else if (cmd != null && cmd.ToUpper() == "SAVE" && schedule.assignedAuditors != null)
            {
                TempData["Error"] = "Please click on Generate button to auto select committee members.";
                return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
            }

            #endregion

            return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
        }

        private int GetRandomAuditor(List<int> lstAuditors, string auditDate)
        {
            int aid = 0;
            DateTime aDate = Utilities.DDMMYY2MMDDYY(auditDate);
            Random random = new Random();
            var randomNumber = random.Next(0, lstAuditors.Count() - 1);
            aid = lstAuditors[randomNumber];

            //verify whether the aid is allotted on the given date
            var eRow = db.jntuh_ffc_committee.Join(db.jntuh_ffc_schedule, comm => comm.scheduleID, sche => sche.id, (comm, sche) => new { comm, sche })
                         .Join(db.jntuh_ffc_auditor, all => all.comm.auditorID, aud => aud.id, (all, aud) => new { all, aud })
                         .Where(a => a.all.sche.InspectionPhaseId == 3 && a.aud.id == aid && a.all.sche.inspectionDate == aDate)
                         .Select(c => c).FirstOrDefault();

            if (eRow != null)
            {
                lstAuditors.Remove(aid);

                aid = GetRandomAuditor(lstAuditors, auditDate);
            }

            return aid;
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

            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == collegeId).ToList();

            List<CollegeIntakeProposed> collegeIntakeProposedList = new List<CollegeIntakeProposed>();

            foreach (var item in proposed)
            {
                CollegeIntakeProposed newProposed = new CollegeIntakeProposed();
                newProposed.DepartmentID = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                newProposed.Department = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.departmentName).FirstOrDefault();
                newProposed.degreeID = db.jntuh_department.Where(d => d.id == newProposed.DepartmentID).Select(d => d.degreeId).FirstOrDefault();
                newProposed.Degree = db.jntuh_degree.Where(d => d.id == newProposed.degreeID).Select(d => d.degree).FirstOrDefault();
                collegeIntakeProposedList.Add(newProposed);
            }

            var data = collegeIntakeProposedList.OrderBy(a => new { a.Degree, a.Department });
            ViewBag.Departments = collegeIntakeProposedList.OrderBy(a => new { a.Degree, a.Department });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
