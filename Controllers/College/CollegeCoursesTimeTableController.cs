using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.College
{
    public class CollegeCoursesTimeTableController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegeCoursesTimeTable/
        [Authorize(Roles = "College,Admin")]
        public ActionResult Index(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (collegeId == 0)
            {
                if (Roles.IsUserInRole("Admin"))
                {
                    if (id != null)
                    {
                        collegeId = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            if (collegeId != 0)
            {
                GetDegree(collegeId);
            }
            //GetDegree();
            //GetDepartment();
            //GetSpecilization();
            //var days = new List<Days>
            //{
            //    new Days {Id = 1, DayName = "Monday"},
            //    new Days {Id = 2, DayName = "Tuesday"},
            //    new Days {Id = 3, DayName = "Wednesday"},
            //    new Days {Id = 4, DayName = "Thursday"},
            //    new Days {Id = 5, DayName = "Friday"},
            //    new Days {Id = 6, DayName = "Saturday"}
            //};
            //ViewBag.Days = days;
            //var s = new List<CollegeCourseTimeTable> { new CollegeCourseTimeTable() };
            //var collegeCourseTimeTable = s.FirstOrDefault();
            //if (collegeCourseTimeTable != null)
            //{

            //    collegeCourseTimeTable.AllDays = days;
            //}

            return View();
        }

        [Authorize(Roles = "College,Admin")]
        [HttpPost]
        public ActionResult Index(CollegeCourseTimeTable details)
        {

            if (ModelState.IsValid)
            {

            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "College,Admin")]
        public void GetDegree(int collegeId)
        {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.NViewBag.Degreeame).ProviderUserKey);
            var cSpcIds =
              db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                //.GroupBy(r => new { r.specializationId })
                  .Select(s => s.specializationId)
                  .ToList();

            var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();
            var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

            //var specializationIds =
            //    db.jntuh_college_intake_existing.Where(e => e.collegeId == collegeId && e.academicYearId == 8 && e.courseStatus != "closure").Select(e => e.specializationId).ToArray();

            ViewBag.Specilization = db.jntuh_specialization.Where(e => cSpcIds.Contains(e.id)).Select(e => new { Sid = e.id, Sname = e.specializationName }).ToList();

            //var DepartmentIds =
            //    db.jntuh_specialization.Where(e => specializationIds.Contains(e.id))
            //        .Select(e => e.departmentId)
            //        .ToArray();

            ViewBag.Dept = db.jntuh_department.Where(e => DepartmentsData.Contains(e.id)).Select(e => new { Did = e.id, Dname = e.departmentName }).ToList();

            //var DegreeIds =
            //    db.jntuh_department.Where(e => DepartmentIds.Contains(e.id)).Select(e => e.degreeId).ToArray();

            var values = db.jntuh_degree.Where(e => DegreeIds.Contains(e.id)).Select(e => new { DegreeId = e.id, Degree = e.degreeDescription }).ToList();

            // var depts = new SelectList(values, "Value", "Text");
            ViewBag.Degree = values;
        }

        //public void GetDepartment()
        //{
        //   ViewBag.Dept = db.jntuh_department.Select(e => new {Did = e.id,Dname = e.departmentName}).ToList();
        //}

        //public void GetSpecilization()
        //{
        //   ViewBag.Specilization = db.jntuh_specialization.Select(e => new {Sid = e.id,Sname = e.specializationName}).ToList();
        //}

        public JsonResult Getfaculty(string Prefix)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            if (collegeId != 0)
            {
                var regno = db.jntuh_college_faculty_registered.Where(e => e.collegeId == collegeId)
                    .Select(e => e.RegistrationNumber)
                    .ToArray();
                var collegefaculty =
                    db.jntuh_registered_faculty.Where(e => regno.Contains(e.RegistrationNumber.Trim()))
                        .Select(e => e.FirstName + " " + e.LastName + "-" + e.RegistrationNumber)
                        .ToList();

                var details = collegefaculty.Where(e => e.ToLower().Contains(Prefix.ToLower())).ToList();


                return Json(details, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return null;
            }
        }

        [Authorize(Roles = "College,Admin")]
        [HttpGet]
        public ActionResult AcadamicSchedule(int? eventid)
        {
            DateTime todayDate = DateTime.Now.Date;
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //int collegeId = 375;
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            AcadamicimeTable acadamictab = new AcadamicimeTable();
            if (collegeId != 0)
            {
                int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
                if (status == 0 && Roles.IsUserInRole("College"))
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
                }
                else
                {
                    ViewBag.IsEditable = true;
                    bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AC") && a.CollegeId == collegeId).Select(a => a.IsEditable).FirstOrDefault();
                    if (isPageEditable)
                    {
                        ViewBag.IsEditable = true;
                    }
                    else
                    {
                        ViewBag.IsEditable = false;
                        return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
                    }
                }
                //int[] selectedAffiliationId =
                //    db.jntuh_affiliation_type.Where(s => s.isActive == true)
                //        .OrderBy(s => s.displayOrder)
                //        .Select(s => s.id)
                //        .ToArray();
                var jntuh_college = db.jntuh_college.Find(collegeId);

                string statusType =
                    db.jntuh_college_affiliation.Where(
                        a => a.collegeId == jntuh_college.id && a.affiliationTypeId == 7) // whether autonomous college or not
                        .Select(a => a.affiliationStatus).FirstOrDefault();
                if (statusType == "Yes")
                {
                    ViewBag.Autonomusaffiliation = true;
                }
                else
                {
                    ViewBag.Autonomusaffiliation = false;
                }

                var cSpcIds =
               db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && (i.academicYearId == ay0 - 1 || i.academicYearId == ay0 - 2 || i.academicYearId == ay0 - 3) && (i.approvedIntake != 0 && i.approvedIntake != null))
                    //.GroupBy(r => new { r.specializationId })
                   .Select(s => s.specializationId)
                   .ToList();

                var DepartmentsData = db.jntuh_specialization.Where(e => e.isActive == true && cSpcIds.Contains(e.id)).Select(e => e.departmentId).Distinct().ToArray();

                var DegreeIds = db.jntuh_department.Where(e => e.isActive == true && DepartmentsData.Contains(e.id)).Select(e => e.degreeId).Distinct().ToList();

                //var specializationIds =
                //    db.jntuh_college_intake_existing.Where(
                //        e => e.collegeId == collegeId && e.academicYearId == ay0 && e.courseStatus != "closure")
                //        .Select(e => e.specializationId)
                //        .ToArray();

                ViewBag.Specilization =
                    db.jntuh_specialization.Where(e => cSpcIds.Contains(e.id))
                        .Select(e => new { Sid = e.id, Sname = e.specializationName })
                        .ToList();
                //var DepartmentIds =
                //    db.jntuh_specialization.Where(e => specializationIds.Contains(e.id))
                //        .Select(e => e.departmentId)
                //        .ToArray();

                ViewBag.Dept =
                    db.jntuh_department.Where(e => DepartmentsData.Contains(e.id))
                        .Select(e => new { Did = e.id, Dname = e.departmentName })
                        .ToList();
                //var DegreeIds =
                //    db.jntuh_department.Where(e => DepartmentIds.Contains(e.id)).Select(e => e.degreeId).ToArray();
                var values =
                    db.jntuh_degree.Where(e => DegreeIds.Contains(e.id))
                        .Select(e => new CollegeDegrees { degreeId = e.id, degree = e.degreeDescription })
                        .ToList();
                ViewBag.Degree = values;

                var collegeacdata =
                    db.jntuh_college_academic_data.Where(i => i.isactive == true && i.collegeid == collegeId && i.accademicyaerid == ay0).ToList();
                var academicCalenderlist = new List<jntuh_college_academic_calenderclass>();
                var academicCalenderlistDto = new List<AcadamicimeTableDto>();
                AcadamicimeTableDto acadamictabdto = new AcadamicimeTableDto();
                if (eventid == null && collegeacdata.Count > 0)
                {
                    AcadamicimeTable acadamictabs = new AcadamicimeTable();
                    //acadamictabs.academic_calender = new List<jntuh_college_academic_calenderclass>();
                    List<jntuh_academic_calendermaster> evnetlist =
                        db.jntuh_academic_calendermaster.Where(e => e.isactive == true).Select(e => e).ToList();
                    ;
                    foreach (var item in evnetlist)
                    {
                        jntuh_college_academic_calenderclass calenderclass = new jntuh_college_academic_calenderclass();
                        calenderclass.evntid = item.id;
                        calenderclass.eventname = item.@event;
                        academicCalenderlist.Add(calenderclass);
                    }
                    acadamictabs.isaction = 0;
                    acadamictabs.academic_calender = academicCalenderlist;
                    foreach (var collegeacademicdata in collegeacdata)
                    {
                        var listdate =
                            db.jntuh_college_academic_calender.Where(e => e.acadenicdataid == collegeacademicdata.Id)
                                .Select(e => e)
                                .ToList();
                        var data =
                            db.jntuh_college_academic_data.Where(e => e.Id == collegeacademicdata.Id).Select(e => e).FirstOrDefault();
                        var calendermaster = db.jntuh_academic_calendermaster.ToList();
                        foreach (var item in listdate)
                        {
                            //var objlist = new jntuh_college_academic_calenderclass
                            //{
                            //    id = item.id,
                            //    academicdataid = item.acadenicdataid,
                            //    eventname =
                            //        calendermaster.Where(x => x.id == item.eventid).Select(x => x.@event).FirstOrDefault(),
                            //    evntid = item.eventid
                            //};
                            //if (item.fromdate != null)
                            //{
                            //    objlist.fromdate = "" + item.fromdate.ToString("dd/MM/yyyy");
                            //}
                            //if (item.todate != null)
                            //{
                            //    objlist.todate = "" + item.todate.Value.ToString("dd/MM/yyyy");
                            //}
                            //objlist.Createdon = "" + item.createdon;
                            //academicCalenderlist.Add(objlist);

                            if (data != null)
                            {
                                acadamictabdto = new AcadamicimeTableDto();
                                acadamictabdto.academicdataid = item.acadenicdataid;
                                acadamictabdto.eventname = calendermaster.Where(x => x.id == item.eventid).Select(x => x.@event).FirstOrDefault();
                                acadamictabdto.evntid = item.eventid;

                                if (item.fromdate != null)
                                {
                                    acadamictabdto.fromdate = "" + item.fromdate.ToString("dd/MM/yyyy");
                                }
                                if (item.todate != null)
                                {
                                    acadamictabdto.todate = "" + item.todate.Value.ToString("dd/MM/yyyy");
                                }
                                acadamictabdto.id = data.Id;
                                acadamictabdto.accadamicyearid = data.accademicyaerid;
                                acadamictabdto.Degreeid = data.degreeid;
                                acadamictabdto.Degree = db.jntuh_degree.Where(x => x.id == data.degreeid).Select(x => x.degree).FirstOrDefault();
                                acadamictabdto.Departmentid = data.departmentid;
                                // acadamictab.Department = db.jntuh_department.Where(x => x.id == data.departmentid).Select(x => x.departmentName).FirstOrDefault();
                                acadamictabdto.specializationid = data.specealizationId;
                                //  acadamictab.Specialization = db.jntuh_specialization.Where(x => x.id == data.specealizationId).Select(x => x.specializationName).FirstOrDefault();
                                acadamictabdto.year = "" + data.degreeyear;
                                acadamictabdto.Semester = "" + data.semister;
                                acadamictabdto.SupportingDocname = data.supportingdocument;
                                acadamictabdto.Createdon = data.createdon;
                                acadamictabdto.isaction = 1;
                                academicCalenderlistDto.Add(acadamictabdto);
                            }
                        }
                    }
                    ViewBag.academicCalenderlistDto = academicCalenderlistDto;
                    //acadamictab.academic_calender = academicCalenderlist;
                    return View("AcadamicSchedule", acadamictabs);
                }
                if (eventid == null)
                {
                    List<jntuh_academic_calendermaster> evnetlist =
                        db.jntuh_academic_calendermaster.Where(e => e.isactive == true).Select(e => e).ToList();
                    ;
                    foreach (var item in evnetlist)
                    {
                        jntuh_college_academic_calenderclass calenderclass = new jntuh_college_academic_calenderclass();
                        calenderclass.evntid = item.id;
                        calenderclass.eventname = item.@event;
                        academicCalenderlist.Add(calenderclass);
                    }
                    acadamictab.isaction = 0;
                    acadamictab.academic_calender = academicCalenderlist;
                }
                else
                {
                    List<jntuh_college_academic_calender> listdate =
                        db.jntuh_college_academic_calender.Where(e => e.acadenicdataid == eventid)
                            .Select(e => e)
                            .ToList();
                    jntuh_college_academic_data data =
                        db.jntuh_college_academic_data.Where(e => e.Id == eventid).Select(e => e).FirstOrDefault();
                    List<jntuh_academic_calendermaster> calendermaster = db.jntuh_academic_calendermaster.ToList();
                    foreach (var item in listdate)
                    {

                        jntuh_college_academic_calenderclass objlist = new jntuh_college_academic_calenderclass();
                        objlist.id = item.id;

                        objlist.academicdataid = item.acadenicdataid;
                        objlist.eventname =
                            calendermaster.Where(x => x.id == item.eventid).Select(x => x.@event).FirstOrDefault();
                        objlist.evntid = item.eventid;
                        if (item.fromdate != null)
                        {
                            objlist.fromdate = "" + item.fromdate.ToString("dd/MM/yyyy");
                        }
                        if (item.todate != null)
                        {
                            objlist.todate = "" + item.todate.Value.ToString("dd/MM/yyyy");
                        }

                        objlist.Createdon = "" + item.createdon;
                        academicCalenderlist.Add(objlist);
                    }
                    acadamictab.isaction = 1;
                    acadamictab.id = data.Id;
                    acadamictab.accadamicyearid = data.accademicyaerid;
                    acadamictab.Degreeid = data.degreeid;
                    // acadamictab.Degree = db.jntuh_degree.Where(x => x.id == data.degreeid).Select(x => x.degree).FirstOrDefault();
                    acadamictab.Departmentid = data.departmentid;
                    // acadamictab.Department = db.jntuh_department.Where(x => x.id == data.departmentid).Select(x => x.departmentName).FirstOrDefault();
                    acadamictab.specializationid = data.specealizationId;
                    //  acadamictab.Specialization = db.jntuh_specialization.Where(x => x.id == data.specealizationId).Select(x => x.specializationName).FirstOrDefault();
                    acadamictab.year = "" + data.degreeyear;
                    acadamictab.Semester = "" + data.semister;
                    acadamictab.SupportingDocname = data.supportingdocument;
                    acadamictab.Createdon = data.createdon;
                    acadamictab.academic_calender = academicCalenderlist;
                }

                return View(acadamictab);
            }
            return View(acadamictab);
        }

        [Authorize(Roles = "College,Admin")]
        [HttpPost]
        public ActionResult AcadamicSchedule(AcadamicimeTable objAcadamicimeTable, string Command)
        {
            DateTime todayDate = DateTime.Now.Date;
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //  
            //int collegeId = 375;
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId && editStatus.academicyearId == ay0 &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
            }
            else
            {
                ViewBag.IsEditable = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AC") && a.CollegeId == collegeId).Select(a => a.IsEditable).FirstOrDefault();
                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
                }
            }
            jntuh_college_academic_data academic_data = new jntuh_college_academic_data();
            if (Command == "Save")
            {
                string Collegecode = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.jntuh_college.collegeCode).FirstOrDefault();
                string AcadamicSchedulePath = "~/Content/Upload/AcadamicSchedule/TimeTable";


                if (!Directory.Exists(Server.MapPath(AcadamicSchedulePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(AcadamicSchedulePath));
                }

                var ext = Path.GetExtension(objAcadamicimeTable.SupportingDoc.FileName);
                if (ext.ToUpper().Equals(".PDF"))
                {
                    string filename = Collegecode + "-" + objAcadamicimeTable.Degree + "-" + "AcadamicimeTimeTable" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                    objAcadamicimeTable.SupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(AcadamicSchedulePath), filename, ext));
                    objAcadamicimeTable.SupportingDocname = string.Format("{0}{1}", filename, ext);
                }
                academic_data.accademicyaerid = ay0;
                academic_data.collegeid = collegeId;
                academic_data.degreeid = Convert.ToInt32(objAcadamicimeTable.Degreeid);
                academic_data.departmentid = Convert.ToInt32(objAcadamicimeTable.Departmentid);
                academic_data.specealizationId = Convert.ToInt32(objAcadamicimeTable.specializationid);
                academic_data.supportingdocument = objAcadamicimeTable.SupportingDocname;
                academic_data.degreeyear = Convert.ToInt32(objAcadamicimeTable.year);
                academic_data.semister = Convert.ToInt32(objAcadamicimeTable.Semester);
                academic_data.isactive = true;
                academic_data.createdon = DateTime.Now;

                db.jntuh_college_academic_data.Add(academic_data);
                db.SaveChanges();

                foreach (var item in objAcadamicimeTable.academic_calender)
                {
                    jntuh_college_academic_calender academic_calenderobj = new jntuh_college_academic_calender();
                    academic_calenderobj.acadenicdataid = academic_data.Id;
                    academic_calenderobj.eventid = item.evntid;
                    if (!string.IsNullOrEmpty(item.fromdate))
                    {
                        academic_calenderobj.fromdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.fromdate);

                    }

                    if (!string.IsNullOrEmpty(item.todate))
                    {
                        academic_calenderobj.todate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.todate);
                    }

                    academic_data.createdon = DateTime.Now;
                    db.jntuh_college_academic_calender.Add(academic_calenderobj);
                    db.SaveChanges();
                }

                TempData["SUCCESS"] = "Data Saved Successfully";
            }
            else
            {
                string Collegecode = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.jntuh_college.collegeCode).FirstOrDefault();
                string AcadamicSchedulePath = "~/Content/Upload/AcadamicSchedule/TimeTable";


                if (!Directory.Exists(Server.MapPath(AcadamicSchedulePath)))
                {
                    Directory.CreateDirectory(Server.MapPath(AcadamicSchedulePath));
                }

                var ext = Path.GetExtension(objAcadamicimeTable.SupportingDoc.FileName);
                if (ext.ToUpper().Equals(".PDF"))
                {
                    string filename = Collegecode + "-" + objAcadamicimeTable.Degree + "-" + "AcadamicimeTimeTable" + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                    objAcadamicimeTable.SupportingDoc.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(AcadamicSchedulePath), filename, ext));
                    objAcadamicimeTable.SupportingDocname = string.Format("{0}{1}", filename, ext);
                }
                // jntuh_college_academic_data academic_data = new jntuh_college_academic_data();
                academic_data.Id = objAcadamicimeTable.id;
                academic_data.accademicyaerid = ay0;
                academic_data.collegeid = collegeId;
                academic_data.degreeid = Convert.ToInt32(objAcadamicimeTable.Degreeid);
                academic_data.departmentid = Convert.ToInt32(objAcadamicimeTable.Departmentid);
                academic_data.specealizationId = Convert.ToInt32(objAcadamicimeTable.specializationid);
                academic_data.supportingdocument = objAcadamicimeTable.SupportingDocname;
                academic_data.degreeyear = Convert.ToInt32(objAcadamicimeTable.year);
                academic_data.semister = Convert.ToInt32(objAcadamicimeTable.Semester);
                academic_data.isactive = true;
                academic_data.updatedon = DateTime.Now;

                //db.jntuh_college_academic_data.Add(academic_data);
                db.Entry(academic_data).State = EntityState.Modified;
                db.SaveChanges();

                foreach (var item in objAcadamicimeTable.academic_calender)
                {
                    jntuh_college_academic_calender academic_calenderobj = new jntuh_college_academic_calender();
                    academic_calenderobj.id = item.id;
                    academic_calenderobj.acadenicdataid = academic_data.Id;
                    academic_calenderobj.eventid = item.evntid;
                    if (!string.IsNullOrEmpty(item.fromdate))
                    {
                        academic_calenderobj.fromdate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.fromdate);

                    }

                    if (!string.IsNullOrEmpty(item.todate))
                    {
                        academic_calenderobj.todate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(item.todate);
                    }

                    academic_data.updatedon = DateTime.Now;
                    db.Entry(academic_calenderobj).State = EntityState.Modified;
                    // db.jntuh_college_academic_calender.Add(academic_calenderobj);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Data Updated Successfully";
                }
            }

            return RedirectToAction("AcadamicSchedule");
            //return RedirectToAction("AcadamicScheduleview", new { eventid = academic_data.Id });
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult AcadamicScheduleview(int? eventid)
        {
            DateTime todayDate = DateTime.Now.Date;
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            //int collegeId = 375;
            if (collegeId == 375)
            {
                collegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            //int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == collegeId && editStatus.academicyearId == ay0 &&
            //                                                                         editStatus.IsCollegeEditable == true &&
            //                                                                         editStatus.editFromDate <= todayDate &&
            //                                                                         editStatus.editToDate >= todayDate)
            //                                                    .Select(editStatus => editStatus.id)
            //                                                    .FirstOrDefault();
            //if (status == 0 && Roles.IsUserInRole("College"))
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //    bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("AC") && a.CollegeId == collegeId).Select(a => a.IsEditable).FirstOrDefault();
            //    if (isPageEditable)
            //    {
            //        ViewBag.IsEditable = true;
            //    }
            //    else
            //    {
            //        ViewBag.IsEditable = false;
            //        return RedirectToAction("AcadamicScheduleview", "CollegeCoursesTimeTable");
            //    }
            //}
            //AcadamicimeTable accadamicdata = new AcadamicimeTable();
            int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).Select(s => s.id).ToArray();
            int affiliationCount = 1;
            var jntuh_college = db.jntuh_college.Find(collegeId);
            foreach (var item in selectedAffiliationId)
            {
                if (affiliationCount == 1)
                {
                    string statusType =
                        db.jntuh_college_affiliation.Where(
                            a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                            .Select(a => a.affiliationStatus).FirstOrDefault();
                    if (statusType == "Yes")
                    {
                        ViewBag.Autonomusaffiliation = true;
                    }
                    else
                    {
                        ViewBag.Autonomusaffiliation = false;
                    }
                }
            }
            AcadamicimeTable acadamictab = new AcadamicimeTable();
            List<jntuh_college_academic_calenderclass> academic_calenderlist =
                    new List<jntuh_college_academic_calenderclass>();
            var academicCalenderlist = new List<jntuh_college_academic_calenderclass>();
            var collegeacdata =
                    db.jntuh_college_academic_data.Where(i => i.isactive == true && i.collegeid == collegeId && i.accademicyaerid == ay0).ToList();
            var academicCalenderlistDto = new List<AcadamicimeTableDto>();
            AcadamicimeTableDto acadamictabdto = new AcadamicimeTableDto();
            if (eventid == null && collegeacdata.Count > 0)
            {
                AcadamicimeTable acadamictabs = new AcadamicimeTable();
                //acadamictabs.academic_calender = new List<jntuh_college_academic_calenderclass>();
                List<jntuh_academic_calendermaster> evnetlist =
                    db.jntuh_academic_calendermaster.Where(e => e.isactive == true).Select(e => e).ToList();
                ;
                foreach (var item in evnetlist)
                {
                    jntuh_college_academic_calenderclass calenderclass = new jntuh_college_academic_calenderclass();
                    calenderclass.evntid = item.id;
                    calenderclass.eventname = item.@event;
                    academicCalenderlist.Add(calenderclass);
                }
                acadamictabs.isaction = 0;
                acadamictabs.academic_calender = academicCalenderlist;
                foreach (var collegeacademicdata in collegeacdata)
                {
                    var listdate =
                        db.jntuh_college_academic_calender.Where(e => e.acadenicdataid == collegeacademicdata.Id)
                            .Select(e => e)
                            .ToList();
                    var data =
                        db.jntuh_college_academic_data.Where(e => e.Id == collegeacademicdata.Id).Select(e => e).FirstOrDefault();
                    var calendermaster = db.jntuh_academic_calendermaster.ToList();
                    foreach (var item in listdate)
                    {
                        if (data != null)
                        {
                            acadamictabdto = new AcadamicimeTableDto();
                            acadamictabdto.academicdataid = item.acadenicdataid;
                            acadamictabdto.eventname = calendermaster.Where(x => x.id == item.eventid).Select(x => x.@event).FirstOrDefault();
                            acadamictabdto.evntid = item.eventid;

                            if (item.fromdate != null)
                            {
                                acadamictabdto.fromdate = "" + item.fromdate.ToString("dd/MM/yyyy");
                            }
                            if (item.todate != null)
                            {
                                acadamictabdto.todate = "" + item.todate.Value.ToString("dd/MM/yyyy");
                            }
                            acadamictabdto.id = data.Id;
                            acadamictabdto.accadamicyearid = data.accademicyaerid;
                            acadamictabdto.Degreeid = data.degreeid;
                            acadamictabdto.Degree = db.jntuh_degree.Where(x => x.id == data.degreeid).Select(x => x.degree).FirstOrDefault();
                            acadamictabdto.Departmentid = data.departmentid;
                            // acadamictab.Department = db.jntuh_department.Where(x => x.id == data.departmentid).Select(x => x.departmentName).FirstOrDefault();
                            acadamictabdto.specializationid = data.specealizationId;
                            //  acadamictab.Specialization = db.jntuh_specialization.Where(x => x.id == data.specealizationId).Select(x => x.specializationName).FirstOrDefault();
                            acadamictabdto.year = "" + data.degreeyear;
                            acadamictabdto.Semester = "" + data.semister;
                            acadamictabdto.SupportingDocname = data.supportingdocument;
                            acadamictabdto.Createdon = data.createdon;
                            acadamictabdto.isaction = 1;
                            academicCalenderlistDto.Add(acadamictabdto);
                        }
                    }
                }
                ViewBag.academicCalenderlistDto = academicCalenderlistDto;
                //acadamictab.academic_calender = academicCalenderlist;
                return View(acadamictabs);
            }
            else
            {
                List<jntuh_college_academic_calender> listdate =
                    db.jntuh_college_academic_calender.Where(e => e.acadenicdataid == eventid)
                        .Select(e => e)
                        .ToList();
                jntuh_college_academic_data data =
                    db.jntuh_college_academic_data.Where(e => e.Id == eventid).Select(e => e).FirstOrDefault();
                List<jntuh_academic_calendermaster> calendermaster = db.jntuh_academic_calendermaster.ToList();
                foreach (var item in listdate)
                {

                    jntuh_college_academic_calenderclass objlist = new jntuh_college_academic_calenderclass();
                    objlist.id = item.id;

                    objlist.academicdataid = item.acadenicdataid;
                    objlist.eventname =
                        calendermaster.Where(x => x.id == item.eventid).Select(x => x.@event).FirstOrDefault();
                    objlist.evntid = item.eventid;
                    if (item.fromdate != null)
                    {
                        objlist.fromdate = "" + item.fromdate.ToString("dd/MM/yyyy");
                    }
                    if (item.todate != null)
                    {
                        objlist.todate = "" + item.todate.Value.ToString("dd/MM/yyyy");
                    }

                    objlist.Createdon = "" + item.createdon;
                    academicCalenderlist.Add(objlist);
                }
                acadamictab.isaction = 1;
                if (data != null)
                {
                    acadamictab.id = data.Id;
                    acadamictab.accadamicyearid = data.accademicyaerid;
                    acadamictab.Degreeid = data.degreeid;
                    acadamictab.Degree = db.jntuh_degree.Where(x => x.id == data.degreeid).Select(x => x.degree).FirstOrDefault();
                    acadamictab.Departmentid = data.departmentid;
                    // acadamictab.Department = db.jntuh_department.Where(x => x.id == data.departmentid).Select(x => x.departmentName).FirstOrDefault();
                    acadamictab.specializationid = data.specealizationId;
                    //  acadamictab.Specialization = db.jntuh_specialization.Where(x => x.id == data.specealizationId).Select(x => x.specializationName).FirstOrDefault();
                    acadamictab.year = "" + data.degreeyear;
                    acadamictab.Semester = "" + data.semister;
                    acadamictab.SupportingDocname = data.supportingdocument;
                    acadamictab.Createdon = data.createdon;
                }
                acadamictab.academic_calender = academicCalenderlist;
            }
            return View(acadamictab);
        }
    }


    public class Days
    {
        public int Id { get; set; }
        public string DayName { get; set; }
    }

    public class CollegeCourseTimeTable
    {
        public string Department { get; set; }
        public string Degree { get; set; }
        public string Specilization { get; set; }
        public List<Days> AllDays { get; set; }
        //  public List<CollegeCourseTimeTableProparties> CollegeCourseTimeTableProparties { get; set; }

        #region monday Slot
        //slot1 Monday
        public string Slot1MFacultyname { get; set; }
        public string Slot1MSubject { get; set; }
        public string Slot1MFromTime { get; set; }
        public string Slot1MToTime { get; set; }

        //slot2 Monday
        public string Slot2MFacultyname { get; set; }
        public string Slot2MSubject { get; set; }
        public string Slot2MFromTime { get; set; }
        public string Slot2MToTime { get; set; }

        //slot3 Monday
        public string Slot3MFacultyname { get; set; }
        public string Slot3MSubject { get; set; }
        public string Slot3MFromTime { get; set; }
        public string Slot3MToTime { get; set; }


        //slot4 Monday
        public string Slot4MFacultyname { get; set; }
        public string Slot4MSubject { get; set; }
        public string Slot4MFromTime { get; set; }
        public string Slot4MToTime { get; set; }

        //slot5 Monday
        public string Slot5MFacultyname { get; set; }
        public string Slot5MSubject { get; set; }
        public string Slot5MFromTime { get; set; }
        public string Slot5MToTime { get; set; }

        //slot6 Monday
        public string Slot6MFacultyname { get; set; }
        public string Slot6MSubject { get; set; }
        public string Slot6MFromTime { get; set; }
        public string Slot6MToTime { get; set; }

        //slot7 Monday
        public string Slot7MFacultyname { get; set; }
        public string Slot7MSubject { get; set; }
        public string Slot7MFromTime { get; set; }
        public string Slot7MToTime { get; set; }

        //slot8 Monday
        public string Slot8MFacultyname { get; set; }
        public string Slot8MSubject { get; set; }
        public string Slot8MFromTime { get; set; }
        public string Slot8MToTime { get; set; }

        #endregion

        #region Tuesday Slot
        //slot1 Monday
        public string Slot1TFacultyname { get; set; }
        public string Slot1TSubject { get; set; }
        public string Slot1TFromTime { get; set; }
        public string Slot1TToTime { get; set; }

        //slot2 Monday
        public string Slot2TFacultyname { get; set; }
        public string Slot2TSubject { get; set; }
        public string Slot2TFromTime { get; set; }
        public string Slot2TToTime { get; set; }

        //slot3 Monday
        public string Slot3TFacultyname { get; set; }
        public string Slot3TSubject { get; set; }
        public string Slot3TFromTime { get; set; }
        public string Slot3TToTime { get; set; }


        //slot4 Monday
        public string Slot4TFacultyname { get; set; }
        public string Slot4TSubject { get; set; }
        public string Slot4TFromTime { get; set; }
        public string Slot4TToTime { get; set; }

        //slot5 Monday
        public string Slot5TFacultyname { get; set; }
        public string Slot5TSubject { get; set; }
        public string Slot5TFromTime { get; set; }
        public string Slot5TToTime { get; set; }

        //slot6 Monday
        public string Slot6TFacultyname { get; set; }
        public string Slot6TSubject { get; set; }
        public string Slot6TFromTime { get; set; }
        public string Slot6TToTime { get; set; }

        //slot7 Monday
        public string Slot7TFacultyname { get; set; }
        public string Slot7TSubject { get; set; }
        public string Slot7TFromTime { get; set; }
        public string Slot7TToTime { get; set; }

        //slot8 Monday
        public string Slot8TFacultyname { get; set; }
        public string Slot8TSubject { get; set; }
        public string Slot8TFromTime { get; set; }
        public string Slot8TToTime { get; set; }

        #endregion

        #region WEDNESDAY Slot
        //slot1 Monday
        public string Slot1WFacultyname { get; set; }
        public string Slot1WSubject { get; set; }
        public string Slot1WFromTime { get; set; }
        public string Slot1WToTime { get; set; }

        //slot2 Monday
        public string Slot2WFacultyname { get; set; }
        public string Slot2WSubject { get; set; }
        public string Slot2WFromTime { get; set; }
        public string Slot2WToTime { get; set; }

        //slot3 Monday
        public string Slot3WFacultyname { get; set; }
        public string Slot3WSubject { get; set; }
        public string Slot3WFromTime { get; set; }
        public string Slot3WToTime { get; set; }


        //slot4 Monday
        public string Slot4WFacultyname { get; set; }
        public string Slot4WSubject { get; set; }
        public string Slot4WFromTime { get; set; }
        public string Slot4WToTime { get; set; }

        //slot5 Monday
        public string Slot5WFacultyname { get; set; }
        public string Slot5WSubject { get; set; }
        public string Slot5WFromTime { get; set; }
        public string Slot5WToTime { get; set; }

        //slot6 Monday
        public string Slot6WFacultyname { get; set; }
        public string Slot6WSubject { get; set; }
        public string Slot6WFromTime { get; set; }
        public string Slot6WToTime { get; set; }

        //slot7 Monday
        public string Slot7WFacultyname { get; set; }
        public string Slot7WSubject { get; set; }
        public string Slot7WFromTime { get; set; }
        public string Slot7WToTime { get; set; }

        //slot8 Monday
        public string Slot8WFacultyname { get; set; }
        public string Slot8WSubject { get; set; }
        public string Slot8WFromTime { get; set; }
        public string Slot8WToTime { get; set; }

        #endregion

        #region THURSDAY Slot
        //slot1 Monday
        public string Slot1ThFacultyname { get; set; }
        public string Slot1ThSubject { get; set; }
        public string Slot1ThFromTime { get; set; }
        public string Slot1ThToTime { get; set; }

        //slot2 Monday
        public string Slot2ThFacultyname { get; set; }
        public string Slot2ThSubject { get; set; }
        public string Slot2ThFromTime { get; set; }
        public string Slot2ThToTime { get; set; }

        //slot3 Monday
        public string Slot3ThFacultyname { get; set; }
        public string Slot3ThSubject { get; set; }
        public string Slot3ThFromTime { get; set; }
        public string Slot3ThToTime { get; set; }


        //slot4 Monday
        public string Slot4ThFacultyname { get; set; }
        public string Slot4ThSubject { get; set; }
        public string Slot4ThFromTime { get; set; }
        public string Slot4ThToTime { get; set; }

        //slot5 Monday
        public string Slot5ThFacultyname { get; set; }
        public string Slot5ThSubject { get; set; }
        public string Slot5ThFromTime { get; set; }
        public string Slot5ThToTime { get; set; }

        //slot6 Monday
        public string Slot6ThFacultyname { get; set; }
        public string Slot6ThSubject { get; set; }
        public string Slot6ThFromTime { get; set; }
        public string Slot6ThToTime { get; set; }

        //slot7 Monday
        public string Slot7ThFacultyname { get; set; }
        public string Slot7ThSubject { get; set; }
        public string Slot7ThFromTime { get; set; }
        public string Slot7ThToTime { get; set; }

        //slot8 Monday
        public string Slot8ThFacultyname { get; set; }
        public string Slot8ThSubject { get; set; }
        public string Slot8ThFromTime { get; set; }
        public string Slot8ThToTime { get; set; }

        #endregion

        #region FRIDAY Slot
        //slot1 Monday
        public string Slot1FFacultyname { get; set; }
        public string Slot1FSubject { get; set; }
        public string Slot1FFromTime { get; set; }
        public string Slot1FToTime { get; set; }

        //slot2 Monday
        public string Slot2FFacultyname { get; set; }
        public string Slot2FSubject { get; set; }
        public string Slot2FFromTime { get; set; }
        public string Slot2FToTime { get; set; }

        //slot3 Monday
        public string Slot3FFacultyname { get; set; }
        public string Slot3FSubject { get; set; }
        public string Slot3FFromTime { get; set; }
        public string Slot3FToTime { get; set; }


        //slot4 Monday
        public string Slot4FFacultyname { get; set; }
        public string Slot4FSubject { get; set; }
        public string Slot4FFromTime { get; set; }
        public string Slot4FToTime { get; set; }

        //slot5 Monday
        public string Slot5FFacultyname { get; set; }
        public string Slot5FSubject { get; set; }
        public string Slot5FFromTime { get; set; }
        public string Slot5FToTime { get; set; }

        //slot6 Monday
        public string Slot6FFacultyname { get; set; }
        public string Slot6FSubject { get; set; }
        public string Slot6FFromTime { get; set; }
        public string Slot6FToTime { get; set; }

        //slot7 Monday
        public string Slot7FFacultyname { get; set; }
        public string Slot7FSubject { get; set; }
        public string Slot7FFromTime { get; set; }
        public string Slot7FToTime { get; set; }

        //slot8 Monday
        public string Slot8FFacultyname { get; set; }
        public string Slot8FSubject { get; set; }
        public string Slot8FFromTime { get; set; }
        public string Slot8FToTime { get; set; }

        #endregion

        #region SATURDAY Slot
        //slot1 Monday
        public string Slot1SFacultyname { get; set; }
        public string Slot1SSubject { get; set; }
        public string Slot1SFromTime { get; set; }
        public string Slot1SToTime { get; set; }

        //slot2 Monday
        public string Slot2SFacultyname { get; set; }
        public string Slot2SSubject { get; set; }
        public string Slot2SFromTime { get; set; }
        public string Slot2SToTime { get; set; }


        public string Slot3SFacultyname { get; set; }
        public string Slot3SSubject { get; set; }
        public string Slot3SFromTime { get; set; }
        public string Slot3SToTime { get; set; }



        public string Slot4SFacultyname { get; set; }
        public string Slot4SSubject { get; set; }
        public string Slot4SFromTime { get; set; }
        public string Slot4SToTime { get; set; }


        public string Slot5SFacultyname { get; set; }
        public string Slot5SSubject { get; set; }
        public string Slot5SFromTime { get; set; }
        public string Slot5SToTime { get; set; }


        public string Slot6SFacultyname { get; set; }
        public string Slot6SSubject { get; set; }
        public string Slot6SFromTime { get; set; }
        public string Slot6SToTime { get; set; }


        public string Slot7SFacultyname { get; set; }
        public string Slot7SSubject { get; set; }
        public string Slot7SFromTime { get; set; }
        public string Slot7SToTime { get; set; }


        public string Slot8SFacultyname { get; set; }
        public string Slot8SSubject { get; set; }
        public string Slot8SFromTime { get; set; }
        public string Slot8SToTime { get; set; }

        #endregion
    }

    public class CollegeCourseTimeTableProparties
    {
        public string Facultyname { get; set; }
        public string Subject { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
    }

    public class AcadamicimeTable
    {
        public int id { get; set; }
        public int accadamicyearid { get; set; }
        public string Degree { get; set; }
        public int Degreeid { get; set; }
        public string Department { get; set; }
        public int Departmentid { get; set; }
        public string Specialization { get; set; }
        public int specializationid { get; set; }

        [Display(Name = "Year")]
        public string year { get; set; }
        public string Semester { get; set; }
        public HttpPostedFileBase SupportingDoc { get; set; }
        public string SupportingDocname { get; set; }
        public DateTime Createdon { get; set; }

        public int isaction { get; set; }

        public List<jntuh_college_academic_calenderclass> academic_calender { get; set; }
    }


    public class AcadamicimeTableDto
    {
        public int id { get; set; }
        public int accadamicyearid { get; set; }
        public string Degree { get; set; }
        public int Degreeid { get; set; }
        public string Department { get; set; }
        public int Departmentid { get; set; }
        public string Specialization { get; set; }
        public int specializationid { get; set; }
        public string year { get; set; }
        public string Semester { get; set; }
        public HttpPostedFileBase SupportingDoc { get; set; }
        public string SupportingDocname { get; set; }
        public DateTime Createdon { get; set; }
        public int isaction { get; set; }
        public int academicdataid { get; set; }
        public int evntid { get; set; }
        public string eventname { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
    }

    public class jntuh_college_academic_calenderclass
    {
        public int id { get; set; }
        public int academicdataid { get; set; }
        public int evntid { get; set; }
        public string eventname { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
        public string Createdon { get; set; }
    }

    public class CollegeDegrees
    {
        public int degreeId { get; set; }
        public string degree { get; set; }
    }
}
