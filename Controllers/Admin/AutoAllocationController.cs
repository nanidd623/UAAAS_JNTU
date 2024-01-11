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
    public class AutoAllocationController : BaseController
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
            //ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName).ToList();
            ViewBag.District = db.college_clusters.Select(c => c.clusterName).Distinct()
                                .Select(c => new
                                       {
                                           Value = c,
                                           Text = c
                                       }).OrderBy(c => c).ToList();

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
                schedule.collegeId = (int)db.jntuh_ffc_schedule.Find(scheduleId).collegeID;
                schedule.districtid = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.districtId).FirstOrDefault();
                schedule.pincode = db.jntuh_address.Where(a => a.addressTye == "COLLEGE" && a.collegeId == schedule.collegeId).Select(a => a.pincode).FirstOrDefault();
                schedule.collegeCode = db.jntuh_college.Where(c => c.id == schedule.collegeId).Select(c => c.collegeCode).FirstOrDefault();
                schedule.auditDate = Utilities.MMDDYY2DDMMYY(db.jntuh_ffc_schedule.Find(scheduleId).inspectionDate.ToString());
                schedule.alternateAuditDate = Utilities.MMDDYY2DDMMYY(db.jntuh_ffc_schedule.Find(scheduleId).alternateInspectionDate.ToString());
                schedule.orderDate = Utilities.MMDDYY2DDMMYY(db.jntuh_ffc_schedule.Find(scheduleId).orderDate.ToString());
                schedule.isRevised = db.jntuh_ffc_schedule.Find(scheduleId).isRevisedOrder == 1 ? true : false;
                schedule.InspectionPhaseId = (int)db.jntuh_ffc_schedule.Find(scheduleId).InspectionPhaseId;

                int alreadyScheduled = 0;
                int value = db.jntuh_ffc_schedule.Where(s => s.collegeID == schedule.collegeId && s.InspectionPhaseId == schedule.InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();
                if (value > 0)
                    alreadyScheduled = value;

                if (alreadyScheduled > 0 && aid == null)
                {
                    TempData["Error"] = "College already scheduled. Please edit the existing schedule.";
                    return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
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
                            string ePlace = db.jntuh_ffc_auditor.Find(member.auditorId).auditorPlace;
                            string Group = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == ePlace).Select(g => g.Group).FirstOrDefault();

                            //get places of the group
                            List<string> places = db.jntuh_ffc_external_auditor_groups.Where(a => a.Group == Group).Select(a => a.University).ToList();

                            //get auditor ids of the group
                            List<int> lstAuditors = db.jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.id != member.auditorId && a.isActive == true).Select(a => a.id).ToList();

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
                                auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
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
                    List<int?> existingAuditors = db.jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId).OrderBy(c => c.id).Select(c => c.auditorID).ToList();

                    List<AssignedAuditors> autoAssigned = new List<AssignedAuditors>();

                    foreach (var auditorId in existingAuditors)
                    {
                        if (auditorId > 0)
                        {
                            var item = db.jntuh_ffc_auditor.Find(auditorId);
                            AssignedAuditors auditor = new AssignedAuditors();
                            auditor.departmentId = 0; //item.auditorDepartmentID;
                            auditor.deptartmentName = item.auditorPlace;
                            auditor.auditorId = item.id;
                            auditor.auditorName = item.auditorName;
                            auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                            auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
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
                List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == schedule.collegeId).ToList();
                List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
                List<string> listDepartments = new List<string>();
                foreach (var item in proposed)
                {
                    CollegeDepartments dept = new CollegeDepartments();
                    dept.deptartmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                    dept.deptartmentName = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
                    dept.degreeId = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
                    dept.degreeName = db.jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
                    if (!listDepartments.Contains(dept.deptartmentName))
                    {
                        collegeDepartments.Add(dept);
                        listDepartments.Add(dept.deptartmentName);
                    }
                }
                schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

                List<jntuh_ffc_committee> existingCommittee = db.jntuh_ffc_committee.OrderBy(c => c.memberOrder).Where(c => c.scheduleID == scheduleId).Select(c => c).ToList();

                #region AvailableAuditors

                //if (schedule.availableAuditors == null)
                //{
                //    //get all auditors
                //    List<jntuh_ffc_auditor> auditors = db.jntuh_ffc_auditor.Where(a => a.isActive == true).OrderBy(a => new { a.auditorDepartmentID, a.auditorDesignationID, a.auditorName }).ToList();
                //    List<AvailableAuditors> available = new List<AvailableAuditors>();

                //    foreach (var item in auditors)
                //    {
                //        AvailableAuditors auditor = new AvailableAuditors();
                //        auditor.departmentId = (int)item.auditorDepartmentID;
                //        auditor.deptartmentName = db.jntuh_department.Where(d => d.id == auditor.departmentId).Select(d => d.departmentName).FirstOrDefault();
                //        auditor.auditorId = item.id;
                //        auditor.auditorName = db.jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorName).FirstOrDefault().Replace(".", ". ");
                //        auditor.auditorDesignation = db.jntuh_designation.Where(d => d.id == item.auditorDesignationID).Select(d => d.designation).FirstOrDefault();
                //        auditor.preferredDesignation = db.jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorPreferredDesignation).FirstOrDefault();
                //        DateTime aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                //        int load = (from s in db.jntuh_ffc_schedule
                //                    join c in db.jntuh_ffc_committee on s.id equals c.scheduleID
                //                    where s.inspectionDate == aDate && c.auditorID == auditor.auditorId
                //                    select c.id).Count();

                //        auditor.auditorLoad = load.ToString();
                //        auditor.auditorSelected = false;

                //        jntuh_ffc_committee existingCommitteeMember = db.jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.id).Select(c => c).FirstOrDefault();
                //        if (existingCommitteeMember != null)
                //        { auditor.auditorSelected = true; }

                //        available.Add(auditor);
                //    }
                //    schedule.availableAuditors = available.OrderBy(a => a.deptartmentName).ThenByDescending(a => a.auditorDesignation).ThenBy(a => a.auditorName).Select(a => a).ToList();
                //}

                //if (schedule.availableAuditors != null)
                //{
                //    List<AvailableAuditors> available = schedule.availableAuditors.Where(a => a.auditorSelected == true).ToList();
                //    List<AssignedAuditors> newAssigned = new List<AssignedAuditors>();
                //    var assigned = schedule.assignedAuditors;
                //    foreach (var item in available)
                //    {
                //        AssignedAuditors auditor = new AssignedAuditors();
                //        auditor.departmentId = item.departmentId;
                //        auditor.deptartmentName = item.deptartmentName;
                //        auditor.auditorId = item.auditorId;
                //        auditor.auditorName = item.auditorName;
                //        auditor.auditorDesignation = item.auditorDesignation;
                //        auditor.preferredDesignation = item.preferredDesignation;
                //        auditor.auditorLoad = item.auditorLoad;
                //        auditor.isConvenor = false;

                //        jntuh_ffc_committee isCommitteeMemberConvenor = db.jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.auditorId && c.isConvenor == 1).Select(c => c).FirstOrDefault();
                //        if (isCommitteeMemberConvenor != null)
                //        { auditor.isConvenor = true; }

                //        auditor.isDeleted = false;

                //        jntuh_ffc_committee committeeMember = db.jntuh_ffc_committee.Where(c => c.scheduleID == scheduleId && c.auditorID == item.auditorId).Select(c => c).FirstOrDefault();

                //        if (committeeMember != null)
                //        {
                //            if (committeeMember.memberOrder != null)
                //            {
                //                auditor.memberOrder = (int)committeeMember.memberOrder;
                //            }
                //        }
                //        newAssigned.Add(auditor);
                //    }
                //    schedule.assignedAuditors = newAssigned.ToList();
                //}

                #endregion
            }

            return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
        }

        //[HttpPost]
        //[Authorize(Roles = "Admin")]
        //public ActionResult AddEditSchedule(AuditSchedule schedule, string aid, string cmd)
        //{
        //    TempData["Error"] = null;
        //    TempData["Success"] = null;

        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    //ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName).ToList();
        //    ViewBag.District = db.college_clusters.Select(c => c.clusterName).Distinct()
        //                        .Select(c => new
        //                        {
        //                            Value = c,
        //                            Text = c
        //                        }).OrderBy(c => c).ToList();

        //    List<SelectListItem> seq = new List<SelectListItem>();

        //    for (int i = 1; i <= 10; i++)
        //    {
        //        SelectListItem item = new SelectListItem();
        //        item.Text = i.ToString();
        //        item.Value = i.ToString();
        //        seq.Add(item);
        //    }

        //    ViewBag.Sequence = seq;
        //    ViewBag.DisableSave = false;

        //    #region CollegeDepartments

        //    //get college departments
        //    List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == schedule.collegeId).ToList();
        //    List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
        //    List<string> listDepartments = new List<string>();
        //    foreach (var item in proposed)
        //    {
        //        CollegeDepartments dept = new CollegeDepartments();
        //        dept.deptartmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
        //        dept.deptartmentName = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
        //        dept.degreeId = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
        //        dept.degreeName = db.jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
        //        if (!listDepartments.Contains(dept.deptartmentName))
        //        {
        //            collegeDepartments.Add(dept);
        //            listDepartments.Add(dept.deptartmentName);
        //        }
        //    }
        //    schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

        //    #endregion

        //    if (aid != null)
        //    {
        //        Save(schedule, aid, cmd);
        //    }
        //    else
        //    {
        //        List<colleges_groups> lstColleges = db.colleges_groups.Where(c => c.clusterName == schedule.collegeCode).OrderBy(c => c.collegeGroup).Select(c => c).ToList();

        //        foreach (var college in lstColleges)
        //        {
        //            int collegeId = db.jntuh_college.Where(c => c.collegeCode == college.collegeCode).Select(c => c.id).FirstOrDefault();

        //            int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();
        //            int alreadyScheduled = db.jntuh_ffc_schedule.Where(s => s.collegeID == collegeId && s.InspectionPhaseId == InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();

        //            if (alreadyScheduled == 0)
        //            {
        //                List<string> groups = new List<string>();
        //                groups.Add(college.firstMemberGroup);
        //                groups.Add(college.SecondMemberGroup);

        //                List<AssignedAuditors> autoAssigned = new List<AssignedAuditors>();

        //                foreach (string group in groups)
        //                {
        //                    if (autoAssigned.Count() < Convert.ToInt32(WebConfigurationManager.AppSettings["CommitteeCount"].ToString()))
        //                    {
        //                        //get places of the group
        //                        List<string> places = db.jntuh_ffc_external_auditor_groups.Where(a => a.Group == group).Select(a => a.University).ToList();

        //                        //get auditor ids of the group
        //                        List<int> lstAuditors = db.jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.isActive == true).Select(a => a.id).ToList();


        //                        DateTime? aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
        //                        List<int?> sameDayAuditors = db.jntuh_ffc_committee
        //                                                      .Join(db.jntuh_ffc_schedule, c => c.scheduleID, s => s.id, (c, s) => new { c, s })
        //                                                      .Join(db.jntuh_ffc_auditor, top1 => top1.c.auditorID, a => a.id, (top1, a) => new { top1, a })
        //                                                      .Join(db.jntuh_ffc_external_auditor_groups, top2 => top2.a.auditorPlace, g => g.University, (top2, g) => new { top2, g })
        //                                                      .Where(a => a.top2.top1.s.inspectionDate == aDate && a.g.Group == group).Select(a => a.top2.top1.c.auditorID).ToList();

        //                        List<int> remaining = new List<int>();

        //                        foreach (int mAudi in lstAuditors)
        //                        {
        //                            if (!sameDayAuditors.Contains(mAudi))
        //                            {
        //                                remaining.Add(mAudi);
        //                            }
        //                        }

        //                        if (remaining.Count() > 0)
        //                        {
        //                            int auditorId = GetRandomAuditor(remaining, schedule.auditDate);

        //                            if (auditorId > 0)
        //                            {
        //                                var item = db.jntuh_ffc_auditor.Find(auditorId);
        //                                AssignedAuditors auditor = new AssignedAuditors();
        //                                auditor.departmentId = 0; //item.auditorDepartmentID;
        //                                auditor.deptartmentName = item.auditorPlace;
        //                                auditor.auditorId = item.id;
        //                                auditor.auditorName = item.auditorName;
        //                                auditor.auditorDesignation = "";// item.jntuh_designation.designation;
        //                                auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
        //                                auditor.auditorLoad = "0";

        //                                if (autoAssigned.Count() == 0)
        //                                {
        //                                    auditor.isConvenor = true;
        //                                }

        //                                auditor.isDeleted = false;
        //                                auditor.memberOrder = autoAssigned.Count() + 1;
        //                                autoAssigned.Add(auditor);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            TempData["Error"] = "Few colleges not scheduled due to in-sufficient members in " + group + ".";
        //                            return View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
        //                        }
        //                    }
        //                }

        //                schedule.assignedAuditors = autoAssigned.ToList();


        //                #region SAVE

        //                if (cmd != null && cmd.ToUpper() == "SAVE" && schedule.assignedAuditors != null)
        //                {
        //                    //must have atleast one assigned auditor & must have a convenor

        //                    List<int> auditors = schedule.assignedAuditors.Select(a => a.auditorId).ToList();
        //                    List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();

        //                    if (auditors.Count() == 0 || convenor.Count() == 0)
        //                    {
        //                        ViewBag.NoConvenor = true;
        //                        return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
        //                    }
        //                    else
        //                    {
        //                        ViewBag.NoConvenor = false;

        //                        List<int> groupCollegeIDs = new List<int>();

        //                        if (!string.IsNullOrEmpty(college.collegeGroup))
        //                        {
        //                            groupCollegeIDs = db.colleges_groups
        //                                                .Join(db.jntuh_college, g => g.collegeCode, c => c.collegeCode, (g, c) => new { g, c })
        //                                                .Where(a => a.g.collegeGroup == college.collegeGroup && a.g.collegeGroup != null)
        //                                                .Select(a => a.c.id).ToList();
        //                        }
        //                        else
        //                        {
        //                            groupCollegeIDs.Add(collegeId);
        //                        }

        //                        foreach (var cid in groupCollegeIDs)
        //                        {

        //                            //insert the schedule
        //                            jntuh_ffc_schedule newSchedule = new jntuh_ffc_schedule();
        //                            newSchedule.collegeID = cid;
        //                            newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.orderDate);
        //                            newSchedule.inspectionDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
        //                            newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
        //                            newSchedule.InspectionPhaseId = InspectionPhaseId;

        //                            if (schedule.alternateAuditDate != null)
        //                                newSchedule.alternateInspectionDate = Utilities.DDMMYY2MMDDYY(schedule.alternateAuditDate);

        //                            int existingId = db.jntuh_ffc_schedule.Where(f => f.collegeID == schedule.collegeId && f.InspectionPhaseId == InspectionPhaseId).Select(f => f.id).FirstOrDefault();

        //                            if (existingId == 0)
        //                            {
        //                                //newSchedule.isRevisedOrder = 0;
        //                                newSchedule.createdBy = userID;
        //                                newSchedule.createdOn = DateTime.Now;
        //                                db.jntuh_ffc_schedule.Add(newSchedule);
        //                                db.SaveChanges();
        //                                //TempData["Success"] = "Schedule added successfully";
        //                            }
        //                            else
        //                            {
        //                                newSchedule.id = existingId;
        //                                //newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
        //                                newSchedule.createdBy = db.jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdBy).FirstOrDefault();
        //                                newSchedule.createdOn = db.jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdOn).FirstOrDefault();
        //                                newSchedule.updatedBy = userID;
        //                                newSchedule.updatedOn = DateTime.Now;
        //                                jntuh_ffc_schedule sch = db.jntuh_ffc_schedule.Find(newSchedule.id);
        //                                ((IObjectContextAdapter)db).ObjectContext.Detach(sch);

        //                                db.Entry(newSchedule).State = EntityState.Modified;
        //                                db.SaveChanges();
        //                                //TempData["Success"] = "Schedule updated successfully";
        //                            }

        //                            int scheduleId = newSchedule.id;

        //                            //insert committee members
        //                            if (scheduleId > 0)
        //                            {
        //                                var existingAuditors = db.jntuh_ffc_committee.AsNoTracking().Where(f => f.scheduleID == scheduleId).Select(f => f).ToList();

        //                                foreach (var item in existingAuditors)
        //                                {
        //                                    jntuh_ffc_committee jntuh_ffc_committee = db.jntuh_ffc_committee.Find(item.id);
        //                                    db.jntuh_ffc_committee.Remove(jntuh_ffc_committee);
        //                                    db.SaveChanges();
        //                                }

        //                                foreach (var item in schedule.assignedAuditors)
        //                                {
        //                                    jntuh_ffc_committee committee = new jntuh_ffc_committee();
        //                                    committee.scheduleID = scheduleId;
        //                                    committee.auditorID = item.auditorId;
        //                                    committee.isConvenor = item.isConvenor == true ? 1 : 0;
        //                                    committee.memberOrder = item.memberOrder;

        //                                    int existingAuditor = db.jntuh_ffc_committee.AsNoTracking().Where(f => f.auditorID == item.auditorId && f.scheduleID == scheduleId).Select(f => f.id).FirstOrDefault();

        //                                    if (existingAuditor == 0)
        //                                    {
        //                                        db.jntuh_ffc_committee.Add(committee);
        //                                        db.SaveChanges();
        //                                    }
        //                                    else
        //                                    {
        //                                        committee.id = existingAuditor;
        //                                        jntuh_ffc_committee comm = db.jntuh_ffc_committee.Find(committee.id);
        //                                        ((IObjectContextAdapter)db).ObjectContext.Detach(comm);

        //                                        db.Entry(committee).State = EntityState.Modified;
        //                                        db.SaveChanges();
        //                                    }
        //                                }
        //                            }

        //                        }

        //                        TempData["Success"] = "Schedule saved successfully";
        //                    }
        //                }
        //                else if (cmd != null && cmd.ToUpper() == "SAVE" && schedule.assignedAuditors != null)
        //                {
        //                    TempData["Error"] = "Please click on Generate button to auto select committee members.";
        //                    return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
        //                }

        //                #endregion
        //            }

        //            //Save(schedule, aid, cmd);
        //        }

        //    }

        //    TempData["Success"] = "Schedule saved successfully";

        //    return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
        //}

        private AuditSchedule Save(AuditSchedule schedule, string aid, string cmd)
        {


            return schedule;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddEditSchedule_old(AuditSchedule schedule, string aid, string cmd)
        {
            if (aid != null)
            {
                Save(schedule, aid, cmd);
            }
            else
            {
                List<int?> lstColleges = db.college_clusters.Where(c => c.clusterName == schedule.collegeCode).Select(c => c.collegeId).OrderBy(c => c).ToList();

                foreach (int cId in lstColleges)
                {
                    schedule.collegeId = cId;
                    Save(schedule, aid, cmd);
                }

            }

            return View("~/Views/Admin/AutoAllocation.cshtml", schedule);
        }

        private AuditSchedule Save_old(AuditSchedule schedule, string aid, string cmd)
        {
            TempData["Error"] = null;
            TempData["Success"] = null;

            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //ViewBag.District = db.jntuh_district.Where(d => d.isActive == true).OrderBy(d => d.districtName).ToList();
            ViewBag.District = db.college_clusters.Select(c => c.clusterName).Distinct()
                                .Select(c => new
                                {
                                    Value = c,
                                    Text = c
                                }).OrderBy(c => c).ToList();

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
            int value = db.jntuh_ffc_schedule.Where(s => s.collegeID == schedule.collegeId && s.InspectionPhaseId == InspectionPhaseId).Select(s => (int)s.collegeID).FirstOrDefault();
            if (value > 0)
                alreadyScheduled = value;

            if (alreadyScheduled > 0 && aid == null)
            {
                TempData["Error"] = "College already scheduled. Please edit the existing schedule.";
                return schedule; //View("~/Views/Admin/AutoAllocation.cshtml", schedule);
            }

            #endregion

            #region AssignedAuditors

            ViewBag.DisableSave = false;
            bool sameInspectionDate = false;
            DateTime? newDate = new DateTime();
            if (aid != null)
            {
                int sId = Convert.ToInt32(aid);
                DateTime? existingDate = db.jntuh_ffc_schedule.Find(sId).inspectionDate;
                newDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);

                if (newDate == existingDate)
                {
                    sameInspectionDate = true;
                }
            }

            if (schedule.assignedAuditors != null && aid != null)
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
                            var eRow = db.jntuh_ffc_committee.Join(db.jntuh_ffc_schedule, comm => comm.scheduleID, sche => sche.id, (comm, sche) => new { comm, sche })
                                         .Join(db.jntuh_ffc_auditor, all => all.comm.auditorID, aud => aud.id, (all, aud) => new { all, aud })
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
                        string ePlace = db.jntuh_ffc_auditor.Find(member.auditorId).auditorPlace;
                        string Group = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == ePlace).Select(g => g.Group).FirstOrDefault();

                        //get places of the group
                        List<string> places = db.jntuh_ffc_external_auditor_groups.Where(a => a.Group == Group).Select(a => a.University).ToList();

                        //get auditor ids of the group
                        List<int> lstAuditors = db.jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.id != member.auditorId && a.isActive == true).Select(a => a.id).ToList();

                        int auditorId = GetRandomAuditor(lstAuditors, schedule.auditDate);

                        if (auditorId > 0)
                        {
                            var item = db.jntuh_ffc_auditor.Find(auditorId);
                            AssignedAuditors auditor = new AssignedAuditors();
                            auditor.departmentId = 0; //item.auditorDepartmentID;
                            auditor.deptartmentName = item.auditorPlace;
                            auditor.auditorId = item.id;
                            auditor.auditorName = item.auditorName;
                            auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                            auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
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
                //get group wise count and get least assigned groups
                var groups = db.jntuh_ffc_committee.Join(db.jntuh_ffc_schedule, comm => comm.scheduleID, sche => sche.id, (comm, sche) => new { comm, sche })
                    .Join(db.jntuh_ffc_auditor, all => all.comm.auditorID, aud => aud.id, (all, aud) => new { all, aud })
                    .Join(db.jntuh_ffc_external_auditor_groups, all => all.aud.auditorPlace, group => group.University, (all, group) => new { all, group })
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

                var lstGroups = db.jntuh_ffc_external_auditor_groups.Select(g => g.Group).Distinct().OrderBy(g => g).Select(g => new { Place = g, Count = 0 }).ToList();

                foreach (var item in lstGroups)
                {
                    if (!groups.Contains(item))
                    {
                        groups.Add(item);
                    }
                }

                groups = groups.OrderBy(a => a.Count).ThenBy(a => a.Place).ToList();

                List<AssignedAuditors> autoAssigned = new List<AssignedAuditors>();

                //get pharmacy status
                var pStatus = db.jntuh_college
                                .Join(db.jntuh_college_intake_proposed, college => college.id, prop => prop.collegeId, (college, prop) => new { college, prop })
                                .Join(db.jntuh_specialization, top1 => top1.prop.specializationId, spec => spec.id, (top1, spec) => new { top1, spec })
                                .Join(db.jntuh_department, top2 => top2.spec.departmentId, dep => dep.id, (top2, dep) => new { top2, dep })
                                .Join(db.jntuh_degree, top3 => top3.dep.degreeId, deg => deg.id, (top3, deg) => new { top3, deg })
                                .Where(d => (d.deg.id == 2 || d.deg.id == 5) && d.top3.top2.top1.college.id == schedule.collegeId).ToList().Count();

                foreach (var group in groups)
                {
                    if (autoAssigned.Count() < Convert.ToInt32(WebConfigurationManager.AppSettings["CommitteeCount"].ToString()) && group.Place != "Group 4")
                    {
                        //get places of the group
                        List<string> places = db.jntuh_ffc_external_auditor_groups.Where(a => a.Group == group.Place).Select(a => a.University).ToList();

                        //get auditor ids of the group
                        List<int> lstAuditors = db.jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.isActive == true).Select(a => a.id).ToList();


                        DateTime? aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                        List<int?> sameDayAuditors = db.jntuh_ffc_committee
                                                      .Join(db.jntuh_ffc_schedule, c => c.scheduleID, s => s.id, (c, s) => new { c, s })
                                                      .Join(db.jntuh_ffc_auditor, top1 => top1.c.auditorID, a => a.id, (top1, a) => new { top1, a })
                                                      .Join(db.jntuh_ffc_external_auditor_groups, top2 => top2.a.auditorPlace, g => g.University, (top2, g) => new { top2, g })
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
                                auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
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
                            return schedule; // View("~/Views/Admin/AutoAllocationEdit.cshtml", schedule);
                        }
                    }

                    if (group.Place == "Group 4" && pStatus > 0)
                    {
                        //get places of the group
                        List<string> places = db.jntuh_ffc_external_auditor_groups.Where(a => a.Group == group.Place).Select(a => a.University).ToList();

                        //get auditor ids of the group
                        List<int> lstAuditors = db.jntuh_ffc_auditor.Where(a => places.Contains(a.auditorPlace) && a.isActive == true).Select(a => a.id).ToList();

                        int auditorId = GetRandomAuditor(lstAuditors, schedule.auditDate);

                        if (auditorId > 0)
                        {
                            var item = db.jntuh_ffc_auditor.Find(auditorId);
                            AssignedAuditors auditor = new AssignedAuditors();
                            auditor.departmentId = 0; //item.auditorDepartmentID;
                            auditor.deptartmentName = item.auditorPlace;
                            auditor.auditorId = item.id;
                            auditor.auditorName = item.auditorName;
                            auditor.auditorDesignation = "";// item.jntuh_designation.designation;
                            auditor.preferredDesignation = db.jntuh_ffc_external_auditor_groups.Where(g => g.University == item.auditorPlace).Select(g => g.Group).FirstOrDefault();
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
                }

                schedule.assignedAuditors = autoAssigned.ToList();
            }


            #endregion

            #region CollegeDepartments

            //get college departments
            List<jntuh_college_intake_proposed> proposed = db.jntuh_college_intake_proposed.Where(p => p.collegeId == schedule.collegeId).ToList();
            List<CollegeDepartments> collegeDepartments = new List<CollegeDepartments>();
            List<string> listDepartments = new List<string>();
            foreach (var item in proposed)
            {
                CollegeDepartments dept = new CollegeDepartments();
                dept.deptartmentId = db.jntuh_specialization.Where(s => s.id == item.specializationId).Select(s => s.departmentId).FirstOrDefault();
                dept.deptartmentName = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.departmentName).FirstOrDefault();
                dept.degreeId = db.jntuh_department.Where(d => d.id == dept.deptartmentId).Select(d => d.degreeId).FirstOrDefault();
                dept.degreeName = db.jntuh_degree.Where(d => d.id == dept.degreeId).Select(d => d.degree).FirstOrDefault();
                if (!listDepartments.Contains(dept.deptartmentName))
                {
                    collegeDepartments.Add(dept);
                    listDepartments.Add(dept.deptartmentName);
                }
            }
            schedule.departments = collegeDepartments.OrderBy(a => a.degreeName).ThenBy(a => a.deptartmentName).Select(a => a).ToList();

            #endregion

            #region AvailableAuditors

            //if (schedule.availableAuditors == null)
            //{
            //    //get all auditors
            //    List<jntuh_ffc_auditor> auditors = db.jntuh_ffc_auditor.Where(a => a.isActive == true).OrderBy(a => new { a.auditorDepartmentID, a.auditorDesignationID, a.auditorName }).ToList();
            //    List<AvailableAuditors> available = new List<AvailableAuditors>();

            //    foreach (var item in auditors)
            //    {
            //        AvailableAuditors auditor = new AvailableAuditors();
            //        auditor.departmentId = (int)item.auditorDepartmentID;
            //        auditor.deptartmentName = db.jntuh_department.Where(d => d.id == auditor.departmentId).Select(d => d.departmentName).FirstOrDefault();
            //        auditor.auditorId = item.id;
            //        auditor.auditorName = db.jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorName).FirstOrDefault().Replace(".", ". ");
            //        auditor.auditorDesignation = db.jntuh_designation.Where(d => d.id == item.auditorDesignationID).Select(d => d.designation).FirstOrDefault();
            //        auditor.preferredDesignation = db.jntuh_ffc_auditor.Where(a => a.id == auditor.auditorId).Select(a => a.auditorPreferredDesignation).FirstOrDefault();
            //        DateTime aDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
            //        int load = (from s in db.jntuh_ffc_schedule
            //                    join c in db.jntuh_ffc_committee on s.id equals c.scheduleID
            //                    where s.inspectionDate == aDate && c.auditorID == auditor.auditorId
            //                    select c.id).Count();

            //        auditor.auditorLoad = load.ToString();
            //        auditor.auditorSelected = false;
            //        available.Add(auditor);
            //    }
            //    schedule.availableAuditors = available.OrderBy(a => a.deptartmentName).ThenByDescending(a => a.auditorDesignation).ThenBy(a => a.auditorName).Select(a => a).ToList();
            //}
            //else
            //{
            //    if ((cmd != null && cmd.ToUpper() != "SAVE") || cmd == null)
            //    {
            //        List<AvailableAuditors> available = schedule.availableAuditors.Where(a => a.auditorSelected == true).ToList();
            //        List<AssignedAuditors> newAssigned = new List<AssignedAuditors>();
            //        var assigned = schedule.assignedAuditors;
            //        foreach (var item in available)
            //        {
            //            AssignedAuditors auditor = new AssignedAuditors();
            //            auditor.departmentId = item.departmentId;
            //            auditor.deptartmentName = item.deptartmentName;
            //            auditor.auditorId = item.auditorId;
            //            auditor.auditorName = item.auditorName;
            //            auditor.auditorDesignation = item.auditorDesignation;
            //            auditor.preferredDesignation = item.preferredDesignation;
            //            auditor.auditorLoad = item.auditorLoad;
            //            if (schedule.assignedAuditors != null && assigned.Where(a => a.auditorId == auditor.auditorId).Select(a => a.isConvenor).FirstOrDefault() == true)
            //                auditor.isConvenor = true;
            //            else
            //                auditor.isConvenor = false;

            //            auditor.isDeleted = false;
            //            newAssigned.Add(auditor);
            //        }
            //        schedule.assignedAuditors = newAssigned.ToList();
            //    }
            //}

            #endregion

            #region SAVE

            if (cmd != null && cmd.ToUpper() == "SAVE" && schedule.assignedAuditors != null)
            {
                //must have atleast one assigned auditor & must have a convenor

                List<int> auditors = schedule.assignedAuditors.Select(a => a.auditorId).ToList();
                List<int> convenor = schedule.assignedAuditors.Where(a => a.isConvenor == true).Select(a => a.auditorId).ToList();

                if (auditors.Count() == 0 || convenor.Count() == 0)
                {
                    ViewBag.NoConvenor = true;
                    return schedule; //View("~/Views/Admin/AutoAllocation.cshtml", schedule);
                }
                else
                {
                    ViewBag.NoConvenor = false;

                    //insert the schedule
                    jntuh_ffc_schedule newSchedule = new jntuh_ffc_schedule();
                    newSchedule.collegeID = schedule.collegeId;
                    newSchedule.orderDate = Utilities.DDMMYY2MMDDYY(schedule.orderDate);
                    newSchedule.inspectionDate = Utilities.DDMMYY2MMDDYY(schedule.auditDate);
                    newSchedule.isRevisedOrder = schedule.isRevised == true ? 1 : 0;
                    newSchedule.InspectionPhaseId = InspectionPhaseId;

                    if (schedule.alternateAuditDate != null)
                        newSchedule.alternateInspectionDate = Utilities.DDMMYY2MMDDYY(schedule.alternateAuditDate);

                    int existingId = db.jntuh_ffc_schedule.Where(f => f.collegeID == schedule.collegeId && f.InspectionPhaseId == InspectionPhaseId).Select(f => f.id).FirstOrDefault();

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
                        newSchedule.createdBy = db.jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdBy).FirstOrDefault();
                        newSchedule.createdOn = db.jntuh_ffc_schedule.Where(f => f.id == existingId).Select(f => f.createdOn).FirstOrDefault();
                        newSchedule.updatedBy = userID;
                        newSchedule.updatedOn = DateTime.Now;
                        jntuh_ffc_schedule sch = db.jntuh_ffc_schedule.Find(newSchedule.id);
                        ((IObjectContextAdapter)db).ObjectContext.Detach(sch);

                        db.Entry(newSchedule).State = EntityState.Modified;
                        db.SaveChanges();
                        //TempData["Success"] = "Schedule updated successfully";
                    }

                    int scheduleId = newSchedule.id;

                    //insert committee members
                    if (scheduleId > 0)
                    {
                        var existingAuditors = db.jntuh_ffc_committee.AsNoTracking().Where(f => f.scheduleID == scheduleId).Select(f => f).ToList();

                        foreach (var item in existingAuditors)
                        {
                            jntuh_ffc_committee jntuh_ffc_committee = db.jntuh_ffc_committee.Find(item.id);
                            db.jntuh_ffc_committee.Remove(jntuh_ffc_committee);
                            db.SaveChanges();
                        }

                        foreach (var item in schedule.assignedAuditors)
                        {
                            jntuh_ffc_committee committee = new jntuh_ffc_committee();
                            committee.scheduleID = scheduleId;
                            committee.auditorID = item.auditorId;
                            committee.isConvenor = item.isConvenor == true ? 1 : 0;
                            committee.memberOrder = item.memberOrder;

                            int existingAuditor = db.jntuh_ffc_committee.AsNoTracking().Where(f => f.auditorID == item.auditorId && f.scheduleID == scheduleId).Select(f => f.id).FirstOrDefault();

                            if (existingAuditor == 0)
                            {
                                db.jntuh_ffc_committee.Add(committee);
                                db.SaveChanges();
                            }
                            else
                            {
                                committee.id = existingAuditor;
                                jntuh_ffc_committee comm = db.jntuh_ffc_committee.Find(committee.id);
                                ((IObjectContextAdapter)db).ObjectContext.Detach(comm);

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
                return schedule;// View("~/Views/Admin/AutoAllocation.cshtml", schedule);
            }

            #endregion
            return schedule;
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

            //var data = db.college_clusters.Where(a => a.clusterName == id && a.isEditable == true).Select(a => a.clusterName).Distinct().OrderBy(a => a).FirstOrDefault();
            var data = db.college_clusters.Where(a => a.clusterName == id && a.isEditable == true).Select(a => a.clusterName).Distinct().OrderBy(a => a).FirstOrDefault();
            string msg = string.Empty;

            if (data != null)
            {
                msg = "0 colleges";
            }
            else
            {
                msg = db.college_clusters.Where(a => a.clusterName == id).Select(a => a.clusterName).ToList().Count().ToString() + " colleges";
            }

            return Json(msg, JsonRequestBehavior.AllowGet);
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
