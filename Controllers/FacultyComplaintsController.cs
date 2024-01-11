using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class FacultyComplaintsController : BaseController
    {
        //
        // GET: /FacultyComplaints/

        #region This is for Faculty so we getting based on Faculty Role Id - 7
        private uaaasDBContext db = new uaaasDBContext();

        List<SelectListItem> Yearofstudy = new List<SelectListItem>()
        {
            new SelectListItem(){Value = "1",Text ="First Year"},
            new SelectListItem(){Value = "2",Text ="Second Year"},
            new SelectListItem(){Value = "3",Text ="Third Year"},
            new SelectListItem(){Value = "4",Text ="Fourth Year"}
        };
        List<SelectListItem> CoursesDropdown = new List<SelectListItem>();
        //List<SelectListItem> CoursesDropdown = new List<SelectListItem>()
        //{
        //    new SelectListItem(){Value = "0",Text ="---Select---"} 
        //};

        List<SelectListItem> DepartmentsDropdown = new List<SelectListItem>();
        //List<SelectListItem> DepartmentsDropdown = new List<SelectListItem>()
        //{
        //    new SelectListItem(){Value = "0",Text ="---Select---"} 
        //};

        public ActionResult Index()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<jntuh_college_complaints> facultycomplaints =
                db.jntuh_college_complaints.Where(r => r.roleId == 7).Select(s => s).ToList();
            int[] facultyids = facultycomplaints.Select(s => s.college_faculty_Id).ToArray();
            List<jntuh_registered_faculty> registredfaculty =
                db.jntuh_registered_faculty.Where(r => facultyids.Contains(r.id)).Select(s => s).ToList();
            List<FacultyComplaints> facultyComplaintslist = new List<FacultyComplaints>();
            foreach (var item in facultycomplaints)
            {
                FacultyComplaints newcomplaint = new FacultyComplaints();
                var faculty =
                    registredfaculty.Where(r => r.id == item.college_faculty_Id).Select(s => s).FirstOrDefault();
                newcomplaint.RegistrationNumber = faculty.RegistrationNumber.Trim();
                newcomplaint.Facultyname = faculty.FirstName + " " + faculty.MiddleName + " " + faculty.LastName;
                newcomplaint.complaintid = (int)item.complaintId;
                newcomplaint.complaintname = db.jntuh_complaints.Where(c => c.id == newcomplaint.complaintid).Select(s => s.complaintType).FirstOrDefault();
                newcomplaint.Facultycomplaintdate = item.complaintDate.ToString().Split(' ')[0];
                newcomplaint.remarks = item.remarks;
                facultyComplaintslist.Add(newcomplaint);
            }

            return View(facultyComplaintslist);
        }

        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult FacultyComplaint()
        {
            //This is for Faculty so we getting based on Faculty Role Id - 7
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            FacultyComplaints facultycomplaints = new FacultyComplaints();
            List<jntuh_complaints> jntucomplaints =
            db.jntuh_complaints.Where(r => r.isActive == true && r.roleId == 7).Select(s => s).ToList();
            List<jntuh_complaints_givenby> jntuh_complaintsgivenby =
                    db.jntuh_complaints_givenby.Where(r => r.isActive == true && r.roleId == 7).Select(s => s).ToList();

            ViewBag.Complaints = jntucomplaints;
            ViewBag.Complaintsgivenby = jntuh_complaintsgivenby;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public ActionResult FacultyComplaint(FacultyComplaints Complaintsnew)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (Complaintsnew != null)
            {
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                string complaintFilepath = "~/Content/Upload/FacultyComplaints/ComplaintFile";
                //string replyFilepath = "~/Content/Upload/FacultyComplaints/ReplyFile";
                jntuh_registered_faculty registeredFaculty =
                    db.jntuh_registered_faculty.Where(
                        r => r.RegistrationNumber == Complaintsnew.RegistrationNumber.Trim())
                        .Select(s => s)
                        .FirstOrDefault();

                if (Complaintsnew.complaintid != 0 && registeredFaculty != null)
                {
                    int checkfacultycomplaint =
                        db.jntuh_college_complaints.Where(
                            c =>
                                c.college_faculty_Id == registeredFaculty.id && c.roleId == 7 &&
                                c.complaintId == Complaintsnew.complaintid).Select(s => s.id).FirstOrDefault();
                    if (checkfacultycomplaint != 0)
                    {
                        TempData["ERROR"] = Complaintsnew.RegistrationNumber.Trim() + " alredy added Complaint.";
                        return RedirectToAction("Index");
                    }
                    List<jntuh_complaints> jntuhComplaints =
                        db.jntuh_complaints.Where(c => c.isActive == true && c.roleId == 7).Select(s => s).ToList();
                    jntuh_college_complaints collegeComplaints = new jntuh_college_complaints();
                    collegeComplaints.college_faculty_Id = registeredFaculty.id;
                    collegeComplaints.roleId = 7;
                    collegeComplaints.academicyearId = ay0;
                    collegeComplaints.complaintId = Complaintsnew.complaintid;
                    Complaintsnew.complaintname =
                        jntuhComplaints.Where(c => c.id == Complaintsnew.complaintid)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    if (Complaintsnew.complaintname == "Others")
                    {
                        collegeComplaints.otherComplaint = Complaintsnew.otherscomplaint;
                    }
                    collegeComplaints.subcomplaintId = 0;
                    //collegeComplaints.replayStatus = Complaintsnew.replaystatus;
                    if (Complaintsnew.Facultycomplaintdate != null)
                        collegeComplaints.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.Facultycomplaintdate);
                    //if (Complaintsnew.replaystatusdate != null)
                    //    collegeComplaints.replayDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(Complaintsnew.replaystatusdate);

                    if (Complaintsnew.givenById != 0)
                    {
                        Complaintsnew.givenBy = db.jntuh_complaints_givenby.Where(g => g.id == Complaintsnew.givenById).Select(s => s.givenbyName).FirstOrDefault();
                        if (Complaintsnew.OthergivenBy != null)
                        {
                            collegeComplaints.givenBy = Complaintsnew.givenBy + "-" + Complaintsnew.OthergivenBy;
                        }
                        else
                        {
                            collegeComplaints.givenBy = Complaintsnew.givenBy;
                        }
                    }

                    //Complaint File Save
                    if (Complaintsnew.FacultycomplaintFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                        }

                        var ext = Path.GetExtension(Complaintsnew.FacultycomplaintFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = Complaintsnew.RegistrationNumber + "-" + Complaintsnew.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                            Complaintsnew.FacultycomplaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                            Complaintsnew.FacultycomplaintFileview = string.Format("{0}{1}", filename, ext);
                        }
                    }
                    collegeComplaints.complaintFile = Complaintsnew.FacultycomplaintFileview;
                    collegeComplaints.replayFile = null;
                    collegeComplaints.email = Complaintsnew.Email;
                    collegeComplaints.mobile = Complaintsnew.Moblie;
                    collegeComplaints.nooftimes = 1;
                    collegeComplaints.remarks = Complaintsnew.remarks;
                    collegeComplaints.isActive = true;
                    collegeComplaints.createdBy = userID;
                    collegeComplaints.createdOn = DateTime.Now;
                    db.jntuh_college_complaints.Add(collegeComplaints);
                    db.SaveChanges();
                    TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.RegistrationNumber.Trim();
                    //TempData["SUCCESS"] = "Complaint Added to " + Complaintsnew.collegeName;
                }
                else
                {
                    TempData["ERROR"] = Complaintsnew.RegistrationNumber.Trim() + " No Data found.";
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Complaints")]
        public JsonResult GetFacultyName(string regno)
        {
            string Details = "";
            jntuh_registered_faculty faculty =
                db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regno.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
            if (faculty != null)
            {
                Details = faculty.FirstName + " " + faculty.MiddleName + " " + faculty.LastName;
            }
            else
            {
                Details = "Invalid Registration Number.";
            }
            return Json(new { Details }, "application/json", JsonRequestBehavior.AllowGet);
        }

        //Excel Download
        public ActionResult GenuinenessFaculty()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_complaints> facultycomplaints =
                db.jntuh_college_complaints.Where(c => c.roleId == 7 && c.complaintId == 16).Select(s => s).ToList();
            int[] facultyids = facultycomplaints.Select(s => s.college_faculty_Id).ToArray();
            List<jntuh_registered_faculty> registeredFaculty =
                db.jntuh_registered_faculty.Where(r => facultyids.Contains(r.id)).Select(s => s).ToList();
            List<FacultyComplaints> FacultyComplaintslist = new List<FacultyComplaints>();
            foreach (var item in facultycomplaints)
            {
                FacultyComplaints complaints = new FacultyComplaints();
                var facultydetails =
                    registeredFaculty.Where(r => r.id == item.college_faculty_Id).Select(s => s).FirstOrDefault();
                complaints.Facultyid = item.college_faculty_Id;
                if (facultydetails != null)
                {
                    complaints.RegistrationNumber = facultydetails.RegistrationNumber.Trim();
                    complaints.Facultyname = facultydetails.FirstName + " " + facultydetails.MiddleName + " " + facultydetails.LastName;
                    complaints.Email = facultydetails.Email;
                    complaints.Moblie = facultydetails.Mobile;
                    complaints.remarks = item.remarks;
                    complaints.Profiledepartmentid = facultydetails.DepartmentId == null ? 0 : Convert.ToInt32(facultydetails.DepartmentId);
                    complaints.Profiledepartment =
                        db.jntuh_department.Where(r => r.id == complaints.Profiledepartmentid)
                            .Select(s => s.departmentName)
                            .FirstOrDefault();
                    var facultyphddata = db.jntuh_registered_faculty_education.Where(
                            e => e.facultyId == complaints.Facultyid && e.educationId == 6)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (facultyphddata != null)
                    {
                        complaints.PhdUniversity = facultyphddata.boardOrUniversity;
                        complaints.Phdpassedyear = facultyphddata.passedYear;
                    }
                    complaints.Facultycomplaintdate = item.complaintDate.ToString().Split(' ')[0];
                    complaints.givenBy = item.givenBy.Trim();
                    var collegefaculty =
                        db.jntuh_college_faculty_registered.Where(
                            c => c.RegistrationNumber == complaints.RegistrationNumber.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    //complaints.complaintname = "Genuineness Certificate";
                    if (collegefaculty != null)
                    {
                        var college =
                            db.jntuh_college.Where(c => c.id == collegefaculty.collegeId)
                                .Select(s => s)
                                .FirstOrDefault();
                        if (college != null)
                        {
                            complaints.Collegecode = college.collegeCode;
                            complaints.Collegename = college.collegeName;
                        }
                    }
                }
                FacultyComplaintslist.Add(complaints);
            }

            string ReportHeader = "Genuineness Certificate.xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/FacultyComplaints/GenuinenessFaculty.cshtml", FacultyComplaintslist);
        }

        public ActionResult FakephdFaculty()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            List<jntuh_college_complaints> facultycomplaints =
                db.jntuh_college_complaints.Where(c => c.roleId == 7 && c.complaintId == 17).Select(s => s).ToList();
            int[] facultyids = facultycomplaints.Select(s => s.college_faculty_Id).ToArray();
            List<jntuh_registered_faculty> registeredFaculty =
                db.jntuh_registered_faculty.Where(r => facultyids.Contains(r.id)).Select(s => s).ToList();
            List<FacultyComplaints> FacultyComplaintslist = new List<FacultyComplaints>();
            foreach (var item in facultycomplaints)
            {
                FacultyComplaints complaints = new FacultyComplaints();
                var facultydetails =
                    registeredFaculty.Where(r => r.id == item.college_faculty_Id).Select(s => s).FirstOrDefault();
                complaints.Facultyid = item.college_faculty_Id;
                if (facultydetails != null)
                {
                    complaints.RegistrationNumber = facultydetails.RegistrationNumber.Trim();
                    complaints.Facultyname = facultydetails.FirstName + " " + facultydetails.MiddleName + " " + facultydetails.LastName;
                    complaints.Email = facultydetails.Email;
                    complaints.Moblie = facultydetails.Mobile;
                    complaints.remarks = item.remarks;
                    complaints.Profiledepartmentid = facultydetails.DepartmentId == null ? 0 : Convert.ToInt32(facultydetails.DepartmentId);
                    complaints.Profiledepartment =
                        db.jntuh_department.Where(r => r.id == complaints.Profiledepartmentid)
                            .Select(s => s.departmentName)
                            .FirstOrDefault();
                    var facultyphddata = db.jntuh_registered_faculty_education.Where(
                           e => e.facultyId == complaints.Facultyid && e.educationId == 6)
                           .Select(s => s)
                           .FirstOrDefault();
                    if (facultyphddata != null)
                    {
                        complaints.PhdUniversity = facultyphddata.boardOrUniversity;
                        complaints.Phdpassedyear = facultyphddata.passedYear;
                    }
                    complaints.Facultycomplaintdate = item.complaintDate.ToString().Split(' ')[0];
                    var collegefaculty =
                        db.jntuh_college_faculty_registered.Where(
                            c => c.RegistrationNumber == complaints.RegistrationNumber.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    //complaints.complaintname = "Genuineness Certificate";
                    if (collegefaculty != null)
                    {
                        var college =
                            db.jntuh_college.Where(c => c.id == collegefaculty.collegeId)
                                .Select(s => s)
                                .FirstOrDefault();
                        if (college != null)
                        {
                            complaints.Collegecode = college.collegeCode;
                            complaints.Collegename = college.collegeName;
                        }
                    }
                }
                FacultyComplaintslist.Add(complaints);
            }

            string ReportHeader = "Fake PHD Certificate.xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/FacultyComplaints/GenuinenessFaculty.cshtml", FacultyComplaintslist);
        }

        //Given by getting college name Auto complete
        public JsonResult Getcollegenames(string prefix)
        {
            var colleges =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(s => new { id = s.id, Collegename = s.collegeName.ToUpper() + " (" + s.collegeCode + ")" })
                    .ToList();
            var populatecolleges = (from c in colleges
                                    where c.Collegename.StartsWith(prefix.ToUpper())
                                    select new { c.Collegename });
            return Json(populatecolleges, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region This is for Faculty giving complaints based on Faculty Role Id - 7

        [HttpGet]
        public ActionResult ComplaintFaculty()
        {
            var collegelist =
                db.jntuh_college.Where(c => c.isActive == true && c.id != 375)
                    .Select(s => new { collegeId = s.id, collegeNmae = s.collegeCode + "-" + s.collegeName })
                    .ToList();
            collegelist.Add(new { collegeId = 0, collegeNmae = "Others" });
            FacultyComplaints complaints = new FacultyComplaints();
            ViewBag.Colleges = collegelist;
            List<jntuh_complaints> jntucomplaints =
            db.jntuh_complaints.Where(r => r.isActive == true && r.roleId == 7).Select(s => s).ToList();
            ViewBag.Complaints = jntucomplaints;
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                if (userID != 0)
                {
                    var facultydetails = db.jntuh_registered_faculty.Where(u => u.UserId == userID).Select(s => s).FirstOrDefault();
                    if (facultydetails != null)
                    {
                        complaints.Facultyid = facultydetails.id;
                        complaints.RegistrationNumber = facultydetails.RegistrationNumber.Trim();
                    }
                }
            }
            else
            {

            }
            return View(complaints);
        }

        [HttpPost]
        public ActionResult ComplaintFaculty(FacultyComplaints facultycomplaints)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            if (facultycomplaints.Facultyid != 0)
            {
                jntuh_college_complaints savecomplaint = new jntuh_college_complaints();
                //Save Complaint File
                string complaintFilepath = "~/Content/Upload/FacultyComplaints/ComplaintFilefromFaculty";
                if (facultycomplaints.FacultycomplaintFile != null)
                {
                    if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                    }
                    var ext = Path.GetExtension(facultycomplaints.FacultycomplaintFile.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string filename = facultycomplaints.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                        facultycomplaints.FacultycomplaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                        facultycomplaints.FacultycomplaintFileview = string.Format("{0}{1}", filename, ext);
                    }
                }
                //Ticket Id Generation
                var ticketid = "JF" + DateTime.Now.ToString("yyMMdd") + "-" + "CR" + DateTime.Now.ToString("HHmmss");

                savecomplaint.roleId = 7;
                savecomplaint.academicyearId = ay0;
                savecomplaint.TicketId = ticketid;
                savecomplaint.complaintDate = DateTime.Now.Date;
                savecomplaint.college_faculty_Id = facultycomplaints.Facultyid;
                savecomplaint.complaintFile = facultycomplaints.FacultycomplaintFileview;
                savecomplaint.complaintId = facultycomplaints.complaintid;
                facultycomplaints.complaintname =
                    db.jntuh_complaints.Where(s => s.id == savecomplaint.complaintId)
                        .Select(s => s.complaintType)
                        .FirstOrDefault();
                if (facultycomplaints.complaintname == "Others")
                {
                    facultycomplaints.otherscomplaint = facultycomplaints.otherscomplaint;
                }
                savecomplaint.givenBy = facultycomplaints.RegistrationNumber.Trim();
                savecomplaint.email = facultycomplaints.Email.Trim();
                savecomplaint.mobile = facultycomplaints.Moblie.Trim();
                savecomplaint.createdOn = DateTime.Now;
                savecomplaint.createdBy = userID;
                savecomplaint.updatedOn = null;
                savecomplaint.updatedBy = null;
            }
            return View(facultycomplaints);
        }

        public string TicketId()
        {
            var ticketid = "JF" + DateTime.Now.ToString("yyMMdd") + "-" + "CR" + DateTime.Now.ToString("HHmmss");
            return ticketid;
        }
        #endregion

        #region Complaints Given by registrated Faculty

        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyComplaintScreen(string fid)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int fID = 0;

            if (!string.IsNullOrEmpty(fid))
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));
            else if (userId != null)
                fID = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.id).FirstOrDefault();
            else
                return RedirectToAction("Index", "NewOnlineRegistration");

            FacultyComplaintsClass complaint = new FacultyComplaintsClass();
            var jntucollege = db.jntuh_college.Where(r => r.isActive == true && r.id != 375).Select(s => new { collegeId = s.id, collegeName = s.collegeCode + "-" + s.collegeName }).ToList();
            var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.districtName, disctrictName = s.districtName }).ToList();
            List<jntuh_complaints> jntucomplaints = db.jntuh_complaints.Where(r => r.isActive == true && r.roleId == 7 && r.typeId == 1).Select(s => s).ToList();
            List<jntuh_complaints_givenby> jntuh_complaintsgivenby = db.jntuh_complaints_givenby.Where(r => r.isActive == true && r.roleId == 7).Select(s => s).ToList();
            ViewBag.Complaints = jntucomplaints;
            ViewBag.Complaintsgivenby = jntuh_complaintsgivenby;
            ViewBag.Colleges = jntucollege;
            ViewBag.Disctricts = jntudistricte;
            if (fID != 0)
            {
                var RegFaculty = db.jntuh_registered_faculty.Find(fID);

                if (RegFaculty != null)
                {
                    complaint.Facultyid = RegFaculty.id;
                    complaint.fsid = fid;
                    complaint.RegistrationNumber = RegFaculty.RegistrationNumber;
                    complaint.Facultyname = RegFaculty.FirstName + " " + RegFaculty.MiddleName + " " + RegFaculty.LastName;
                    complaint.Moblie = RegFaculty.Mobile;
                    complaint.Email = RegFaculty.Email;
                    complaint.Facultycomplaintdate = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                }
                return View(complaint);
            }
            else
            {
                return View(complaint);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyComplaintScreen(FacultyComplaintsClass ComplaintObj, string ajaxcall)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (ComplaintObj != null && ModelState.IsValid == true)
            {
                string GenerateTicketId = string.Empty;
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                string complaintFilepath = "~/Content/Upload/ComplaintsGivenbyFaculty/ComplaintFile";
                jntuh_registered_faculty registeredFaculty =
                    db.jntuh_registered_faculty.Where(
                        r => r.id == ComplaintObj.Facultyid)
                        .Select(s => s)
                        .FirstOrDefault();

                if (ComplaintObj.complaintid != 0 && registeredFaculty != null)
                {
                    int checkfacultycomplaint =
                        db.jntuh_college_complaints.Where(c => c.college_faculty_Id == registeredFaculty.id && c.roleId == 7 && c.complaintStatus != 5).
                        Select(s => s.id).FirstOrDefault(); //&& c.complaintId == ComplaintObj.complaintid
                    if (checkfacultycomplaint != 0)
                    {
                        TempData["ERROR"] = ComplaintObj.RegistrationNumber.Trim() + " already added Grievance.";
                        return RedirectToAction("FacultyComplaintScreen", "FacultyComplaints", new { fid = UAAAS.Models.Utilities.EncryptString(ComplaintObj.Facultyid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]) });
                    }
                    List<jntuh_complaints> jntuhComplaints =
                        db.jntuh_complaints.Where(c => c.isActive == true && c.roleId == 7 && c.typeId == 1).Select(s => s).ToList();
                    jntuh_college_complaints collegeComplaints = new jntuh_college_complaints();
                    collegeComplaints.academicyearId = ay0;
                    collegeComplaints.collegeId = ComplaintObj.collegeId;
                    var clg = db.jntuh_college.Where(e => e.id == collegeComplaints.collegeId).Select(r => r.collegeCode).FirstOrDefault();
                    GenerateTicketId = clg + "-" + "FCR-" + registeredFaculty.PANNumber.Substring(5, 4).ToString() + DateTime.Now.ToString("yyyyMMdd-HHmmss").Substring(2);
                    collegeComplaints.TicketId = GenerateTicketId;
                    collegeComplaints.roleId = 7;
                    collegeComplaints.college_faculty_Id = registeredFaculty.id;
                    collegeComplaints.collegeId = ComplaintObj.collegeId;
                    collegeComplaints.complaintId = ComplaintObj.complaintid;
                    ComplaintObj.complaintname =
                        jntuhComplaints.Where(c => c.id == ComplaintObj.complaintid)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    if (ComplaintObj.complaintname == "Others")
                    {
                        collegeComplaints.otherComplaint = ComplaintObj.otherscomplaint;
                    }
                    collegeComplaints.subcomplaintId = 0;

                    if (ComplaintObj.Facultycomplaintdate != null)
                        collegeComplaints.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(ComplaintObj.Facultycomplaintdate);

                    collegeComplaints.complaintDescription = ComplaintObj.ComplaintDesc.Replace("\n", " ");
                    collegeComplaints.complaintDescription = collegeComplaints.complaintDescription.Replace("\r\n", " ");
                    collegeComplaints.complaintStatus = 1;
                    collegeComplaints.facultyAddress = ComplaintObj.FacultyAddress.Replace("\n", " ");
                    collegeComplaints.facultyAddress = collegeComplaints.facultyAddress.Replace("\r\n", " ") + " ," + ComplaintObj.District;

                    if (ComplaintObj.givenById != 0)
                    {
                        ComplaintObj.givenBy = db.jntuh_complaints_givenby.Where(g => g.id == ComplaintObj.givenById).Select(s => s.givenbyName).FirstOrDefault();
                        if (ComplaintObj.OthergivenBy != null)
                        {
                            collegeComplaints.givenBy = ComplaintObj.givenBy + "-" + ComplaintObj.OthergivenBy;
                        }
                        else
                        {
                            collegeComplaints.givenBy = ComplaintObj.givenBy;
                        }
                    }

                    //Complaint File Save
                    if (ComplaintObj.FacultycomplaintFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                        }

                        var ext = Path.GetExtension(ComplaintObj.FacultycomplaintFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = ComplaintObj.RegistrationNumber + "-" + ComplaintObj.complaintid + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                            ComplaintObj.FacultycomplaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                            ComplaintObj.FacultycomplaintFileview = string.Format("{0}{1}", filename, ext);
                        }
                    }
                    collegeComplaints.complaintFile = ComplaintObj.FacultycomplaintFileview;
                    collegeComplaints.replayFile = null;
                    collegeComplaints.email = ComplaintObj.Email;
                    collegeComplaints.mobile = ComplaintObj.Moblie;
                    collegeComplaints.nooftimes = 1;
                    //collegeComplaints.remarks = ComplaintObj.remarks;
                    collegeComplaints.isActive = true;
                    collegeComplaints.createdBy = userID;
                    collegeComplaints.createdOn = DateTime.Now;
                    db.jntuh_college_complaints.Add(collegeComplaints);
                    db.SaveChanges();
                    var successmsg = "Your Complaint is taken by University. and your ticketid is " + collegeComplaints.TicketId;
                    TempData["SUCCESS"] = successmsg;
                    //TempData["SUCCESS"] = "Your Complaint is taken by University.";

                    //send email
                    //IUserMailer mailer = new UserMailer();
                    //mailer.FacultyComplaintRequestMail(registeredFaculty.Email, "FacultyComplaintRequestMail",
                    //    "JNTUH Faculty Complaint Details.", collegeComplaints.TicketId, ComplaintObj.Facultyname,
                    //    registeredFaculty.RegistrationNumber, ComplaintObj.Facultycomplaintdate)
                    //    .SendAsync();

                    string MailBody = string.Empty;
                    MailBody += "<p>Dear " + ComplaintObj.Facultyname + "</p>";
                    MailBody += "<p style='font-weight: bold'>Your Registration Number :" + registeredFaculty.RegistrationNumber + "</p>";
                    MailBody += "<p>With reference to your Grievance, you are hereby informed to furnish Grievance id :" + collegeComplaints.TicketId + " for further correspondence and can track the status of your Grievance at the same portal.</p>";
                    MailBody += "<table style='text-align:left;border:1px solid; width: 100%;'>";
                    MailBody += "<tr><td colspan=2 style='font-weight: bold; text-align: center; background: burlywood;'>GRIEVANCE DETAILS</td></tr>";
                    MailBody += "<tr><td colspan=2 >Grievance Registered Through DIRECTORATE OF AFFILIATIONS & ACADEMIC AUDIT  Portal</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Type :</td><td>" + ComplaintObj.complaintname + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Date :</td><td>" + ComplaintObj.Facultycomplaintdate + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Id :</td><td>" + collegeComplaints.TicketId + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Details :</td><td>" + collegeComplaints.complaintDescription + "</td></tr>";
                    MailBody += "</table>";
                    MailBody += "<table>";
                    MailBody += "<tr><td>Thanks & Regards</td></tr>";
                    MailBody += "<tr><td>Director, UAAC,</td></tr>";
                    MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                    MailBody += "</table>";

                    MailMessage message = new MailMessage();
                    message.To.Add(registeredFaculty.Email);
                    message.Subject = "JNTUH-UAAC-Your Grievance ID:" + collegeComplaints.TicketId;
                    message.Body = MailBody;
                    message.IsBodyHtml = true;
                    var smtp = new SmtpClient();
                    smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(message);

                    //send sms
                    if (!string.IsNullOrEmpty(registeredFaculty.Mobile))
                    {
                        //bool pStatus = UAAAS.Models.Utilities.SendSMS(registeredFaculty.Mobile, "JNTUH: Your Complaint is taken by University. Your reference number is :TExt123");
                    }

                    if (ComplaintObj.text == "AjaxCall")
                        return Json(new { result = successmsg }, "application/json", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ERROR"] = ComplaintObj.RegistrationNumber.Trim() + " No Data found.";
                }
            }
            else
            {
                if (ajaxcall == "AjaxCall")
                    return Json(new { error = "All fields are mandatory" }, "application/json", JsonRequestBehavior.AllowGet);
                else
                    TempData["ERROR"] = "All star fields are mandatory";
            }
            return RedirectToAction("FacultyComplaintScreen");
        }

        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyTrackComplaint(string fid)
        {
            int fID = 0;
            if (string.IsNullOrEmpty(fid))
            {
                int? userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var id = db.jntuh_registered_faculty.Where(e => e.UserId == userId).Select(e => e.id).FirstOrDefault();
                fid = UAAAS.Models.Utilities.EncryptString(id.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
            }

            if (!string.IsNullOrEmpty(fid))
                fID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(fid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]));

            var complaintdatalist = db.jntuh_college_complaints.Where(r => r.college_faculty_Id == fID && r.roleId == 7).Select(t => t).ToList();
            List<FacultyComplaintsClass> FacultyComplaintsClasslis = new List<FacultyComplaintsClass>();
            foreach (var complaintdata in complaintdatalist)
            {
                FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var regfaculty = db.jntuh_registered_faculty.Where(r => r.id == complaintdata.college_faculty_Id).Select(r => r).FirstOrDefault();
                complaint.TicketId = complaintdata.TicketId;
                complaint.id = complaintdata.id;
                complaint.collegeId = Convert.ToInt32(complaintdata.collegeId);
                complaint.Facultyid = complaintdata.college_faculty_Id;
                complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                complaint.complaintid = Convert.ToInt32(complaintdata.complaintId);
                complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 7 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                complaint.complaintdate = Convert.ToDateTime(complaintdata.complaintDate).ToString("dd-MM-yyyy");
                complaint.otherscomplaint = complaintdata.otherComplaint;
                complaint.ComplaintDesc = complaintdata.complaintDescription;
                complaint.complaintFileview = complaintdata.complaintFile;
                complaint.Facultyname = regfaculty.FirstName + " " + regfaculty.MiddleName + " " + regfaculty.LastName;
                complaint.gender = regfaculty.GenderId == 1 ? "Male" : "Female";
                complaint.Email = complaintdata.email;
                complaint.Moblie = complaintdata.mobile;
                complaint.RegistrationNumber = regfaculty.RegistrationNumber;
                complaint.FacultyAddress = complaintdata.facultyAddress;
                complaint.complaintStatusName = (from obj in db.jntuh_complaints_status where obj.id == complaintdata.complaintStatus select obj.name).SingleOrDefault();
                FacultyComplaintsClasslis.Add(complaint);
            }
            return View(FacultyComplaintsClasslis);

        }

        [Authorize(Roles = "Admin,Faculty")]
        public ActionResult FacultyTrackComplaintpoup(int id)
        {

            var complaintdata = db.jntuh_college_complaints.Where(r => r.id == id).Select(t => t).FirstOrDefault();
            FacultyComplaintsClass complaint = new FacultyComplaintsClass();
            if (complaintdata != null)
            {
                var regfaculty = db.jntuh_registered_faculty.Where(r => r.id == complaintdata.college_faculty_Id).Select(r => r).FirstOrDefault();
                complaint.TicketId = complaintdata.TicketId;
                complaint.collegeId = Convert.ToInt32(complaintdata.collegeId);
                complaint.Facultyid = complaintdata.college_faculty_Id;
                complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                complaint.complaintid = Convert.ToInt32(complaintdata.complaintId);
                complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 7 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                complaint.complaintdate = Convert.ToDateTime(complaintdata.complaintDate).ToString("dd-MM-yyyy");
                complaint.otherscomplaint = complaintdata.otherComplaint;
                complaint.ComplaintDesc = complaintdata.complaintDescription;
                complaint.complaintFileview = complaintdata.complaintFile;
                complaint.Facultyname = regfaculty.FirstName + " " + regfaculty.MiddleName + " " + regfaculty.LastName;
                complaint.gender = regfaculty.GenderId == 1 ? "Male" : "Female";
                complaint.Email = complaintdata.email;
                complaint.Moblie = complaintdata.mobile;
                complaint.RegistrationNumber = regfaculty.RegistrationNumber;
                complaint.FacultyAddress = complaintdata.facultyAddress;
                complaint.complaintStatus = Convert.ToInt32(complaintdata.complaintStatus);
            }
            return PartialView("_FacultyTrackComplaintpoup", complaint);

        }

        [Authorize(Roles = "Admin")]
        public ActionResult FacultyComplaintsList()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<StudentsComplaints> complaintsList = new List<StudentsComplaints>();
            var Collegecomplaints = db.jntuh_college_complaints.Where(c => c.roleId == 7 && c.createdBy != 124636 && (c.createdOn.Value.Year == 2022 || c.createdOn.Value.Year == 2023)).ToList();
            var Complaints = db.jntuh_complaints.ToList();
            int[] facultyids = Collegecomplaints.Select(c => c.college_faculty_Id).Distinct().ToArray();
            var Colleges = db.jntuh_college.Where(c => c.isActive == true).ToList();
            var Facultynames = db.jntuh_registered_faculty.Where(rf => facultyids.Contains(rf.id)).Select(e => new { e.id, e.RegistrationNumber, e.FirstName, e.MiddleName, e.LastName }).ToList();
            var complaintsStatus = db.jntuh_complaints_status.ToList();
            List<FacultyComplaintsClass> FacultyComplaints = new List<FacultyComplaintsClass>();
            foreach (var item in Collegecomplaints)
            {
                FacultyComplaintsClass Complaint = new FacultyComplaintsClass();
                Complaint.id = item.id;
                Complaint.TicketId = item.TicketId;
                Complaint.RegistrationNumber = Facultynames.Where(f => f.id == item.college_faculty_Id).Select(f => f.RegistrationNumber).FirstOrDefault();
                Complaint.complaintname = Complaints.Where(cp => cp.id == item.complaintId).Select(cp => cp.complaintType).FirstOrDefault();
                Complaint.complaintid = Complaints.Where(cp => cp.id == item.complaintId).Select(cp => cp.id).FirstOrDefault();
                Complaint.complaintdate = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => String.Format("{0:d/M/yyyy}", cp.complaintDate)).FirstOrDefault().ToString();
                int? complaint_status = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => cp.complaintStatus).FirstOrDefault();
                Complaint.complaintStatus = (int)complaint_status;
                Complaint.complaintStatusName = complaintsStatus.Where(i => i.id == (int)item.complaintStatus).FirstOrDefault().name;
                Complaint.complaintFileview = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => cp.complaintFile).FirstOrDefault();
                Complaint.collegeId = Colleges.Where(c => c.id == item.collegeId).Select(c => c.id).FirstOrDefault();
                Complaint.Collegename = Colleges.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault();
                var fullname = Facultynames.Where(f => f.id == item.college_faculty_Id).Select(f => new { f.FirstName, f.MiddleName, f.LastName }).FirstOrDefault();
                Complaint.Facultyname = fullname.FirstName + " " + fullname.MiddleName + " " + fullname.LastName;
                Complaint.FacultyAddress = item.facultyAddress;

                //If College Given for Replay of Complaint 
                var collegegivenreplay =
                    db.jntuh_college_grievancesassigned.Where(r => r.College_facultyid == Complaint.collegeId && r.Ticketid == Complaint.TicketId.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (collegegivenreplay != null)
                {
                    Complaint.Replaycomplaintsfile = collegegivenreplay.Collegesupportingdocument;
                    Complaint.Replayremarks = collegegivenreplay.CollegeRemarks;
                }

                FacultyComplaints.Add(Complaint);
            }
            return View(FacultyComplaints);
            //return null;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult FacultyComplaintsUpdate(string id)
        {
            try
            {
                var cmpltsStats = new List<ComplaintStats>();
                var cmplantId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
                var complaintsStatus = db.jntuh_complaints_status.ToList();
                if (cmplantId != null && cmplantId != 0)
                {
                    var complaintdata = db.jntuh_college_complaints.Find(cmplantId);
                    if (complaintdata != null)
                    {
                        var Facultynames = db.jntuh_registered_faculty.Where(rf => rf.id == complaintdata.college_faculty_Id).Select(e => new { e.id, e.RegistrationNumber, e.FirstName, e.MiddleName, e.LastName }).FirstOrDefault();

                        FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                        complaint.id = complaintdata.id;
                        complaint.TicketId = complaintdata.TicketId;
                        complaint.collegeId = Convert.ToInt32(complaintdata.collegeId);
                        complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                        complaint.complaintid = Convert.ToInt32(complaintdata.complaintId);
                        complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 7 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                        complaint.complaintdate = complaintdata.complaintDate.ToString();
                        complaint.complaintdate = complaint.complaintdate.Split(' ')[0];
                        complaint.otherscomplaint = complaintdata.otherComplaint;
                        complaint.ComplaintDesc = complaintdata.complaintDescription;
                        complaint.complaintFileview = complaintdata.complaintFile;
                        complaint.Facultyname = Facultynames.FirstName + " " + Facultynames.MiddleName + " " + Facultynames.LastName;
                        complaint.RegistrationNumber = Facultynames.RegistrationNumber;
                        complaint.complaintStatus = Convert.ToInt32(complaintdata.complaintStatus);
                        complaint.FacultyAddress = complaintdata.facultyAddress;
                        complaint.FacultycomplaintFileview = complaintdata.complaintFile;
                        complaint.complaintStatusName = complaintsStatus.Where(i => i.id == (int)complaintdata.complaintStatus).FirstOrDefault().name;
                        foreach (var item in complaintsStatus)
                        {
                            if (item.id != complaintdata.complaintStatus && item.id != 1)
                            {
                                cmpltsStats.Add(new ComplaintStats() { cmpltId = item.id, cmpltName = item.name });
                            }
                        }
                        ViewBag.cmpltStats = cmpltsStats;
                        var collegegivenreplay =
                        db.jntuh_college_grievancesassigned.Where(r => r.College_facultyid == complaint.collegeId && r.Ticketid == complaint.TicketId.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                        if (collegegivenreplay != null)
                        {
                            complaint.Replaycomplaintsfile = collegegivenreplay.Collegesupportingdocument;
                            complaint.Replayremarks = collegegivenreplay.CollegeRemarks;
                        }
                        //return PartialView("~/Views/FacultyComplaints/_facultycomplaintview.cshtml", complaint);
                        return View(complaint);
                    }
                    else
                    {
                        TempData["ERROR"] = "Data is Not Found";
                        return RedirectToAction("FacultyComplaintsList");
                    }
                }
                else
                {
                    TempData["ERROR"] = "Parameter Value is Missing,Please Try Agian.";
                    return RedirectToAction("FacultyComplaintsList");
                }
            }
            catch (Exception)
            {
                TempData["ERROR"] = "Parameter Value is Missing,Please Try Agian.";
                return RedirectToAction("FacultyComplaintsList");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FacultyComplaintsUpdate(FacultyComplaintsClass objFacultyStat)
        {
            try
            {
                if (objFacultyStat.complaintStatusId == 2)
                {
                    Grievancecanotbeaddressed(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 3)
                {
                    GreivanceNotUnderstood(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 4)
                {
                    GrievanceProcedtocollege(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 5)
                {
                    GreivanceClosed(objFacultyStat.id);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("FacultyComplaintsList");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GivanceNotunderstood(int id, string ticketid)
        {
            GreivanceNotUnderstood(id);
            return RedirectToAction("FacultyComplaintsList");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Grievancecanotbeaddressed(int id, string ticketid)
        {
            Grievancecanotbeaddressed(id);
            return RedirectToAction("FacultyComplaintsList");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GrievanceProcedtocollege(int id, string ticketid)
        {
            GrievanceProcedtocollege(id);
            return RedirectToAction("FacultyComplaintsList");
        }

        private void GreivanceClosed(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_complaints.Where(c => c.id == id && c.roleId == 7).FirstOrDefault();
                var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id }).FirstOrDefault();
                complaint.complaintStatus = 5;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + faculty.FirstName + " " + faculty.LastName + "</p>";
                MailBody += "<p>Your grievance submitted at the UAAC portal bearing the Grievance id :" + complaint.TicketId + " , is closed.</p>";
                MailBody += "<p style='background: yellow; width: 235px; font-weight: bold;'>Status: Closed</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        private void GreivanceNotUnderstood(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_complaints.Where(c => c.id == id && c.roleId == 7).FirstOrDefault();
                var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id }).FirstOrDefault();
                complaint.complaintStatus = 3;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + faculty.FirstName + " " + faculty.LastName + "</p>";
                MailBody += "<p>Your grievance submitted at the UAAC portal bearing the Grievance id :" + complaint.TicketId + " , cannot be addressed to until you submit your grievance with clarity and by attaching relevant documents.</p>";
                MailBody += "<p style='background: yellow; width: 235px; font-weight: bold;'>Status: Send Relevant Documents</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        private void Grievancecanotbeaddressed(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_complaints.Where(c => c.id == id && c.roleId == 7).FirstOrDefault();
                var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id }).FirstOrDefault();
                complaint.complaintStatus = 2;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + faculty.FirstName + " " + faculty.LastName + "</p>";
                MailBody += "<p>Your Grievance id :" + complaint.TicketId + " , cannot be processed as it doesn’t came under grievance category. Hence, you need to submit your request to the University separately.</p>";
                MailBody += "<p style='background: yellow; width: 180px; font-weight: bold;'>Status: Not in Grievance</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        private void GrievanceProcedtocollege(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_complaints.Where(c => c.id == id && c.roleId == 7 && c.TicketId != null).FirstOrDefault();
                var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id, rf.Email }).FirstOrDefault();
                var collegeemail = db.jntuh_address.Where(a => a.collegeId == complaint.collegeId).Select(a => new { a.email, a.address, a.addressTye }).ToList();
                var collegename = db.jntuh_college.Where(c => c.id == complaint.collegeId).Select(c => new { c.id, c.collegeCode, c.collegeName }).FirstOrDefault();
                string PrincipalRegNum = db.jntuh_college_principal_registered.Where(pr => pr.collegeId == complaint.collegeId).Select(pr => pr.RegistrationNumber).FirstOrDefault().ToString();
                var Principaldata = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == PrincipalRegNum).Select(rf => new { rf.RegistrationNumber, rf.Email, rf.FirstName, rf.LastName }).FirstOrDefault();
                string complainttype = db.jntuh_complaints.Where(cp => cp.id == complaint.complaintId).Select(cp => cp.complaintType).FirstOrDefault().ToString();
                complaint.complaintStatus = 4;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string grievanceletter = Pdfreportgenerate(complaint.id);
                jntuh_college_grievancesassigned collegecomplaint = new jntuh_college_grievancesassigned();
                collegecomplaint.College_facultyid = (int)complaint.collegeId;
                collegecomplaint.grievanceassiegnedto = 4;
                collegecomplaint.Grievancetype = 7;
                collegecomplaint.Ticketid = complaint.TicketId;
                collegecomplaint.Grievanceid = complaint.id;
                collegecomplaint.Ticketstatus = 4;
                collegecomplaint.Collegeletter = grievanceletter;
                collegecomplaint.admincreatedby = userID;
                collegecomplaint.isactive = true;
                collegecomplaint.admincreatedon = DateTime.Now;
                db.jntuh_college_grievancesassigned.Add(collegecomplaint);
                db.SaveChanges();

                var adminproceddeddate = DateTime.Now.AddDays(7).ToShortDateString();

                string colemail = collegeemail.Where(c => c.addressTye == "COLLEGE").Select(c => c.address).FirstOrDefault();
                string collegeMailBody = string.Empty;
                collegeMailBody += "<div style='border:1px solid red;padding:10px;background:beige'><table><tr><td>To</td></tr>";
                collegeMailBody += "<tr><td>The Principal,</td></tr>";
                collegeMailBody += "<tr><td>" + collegename.collegeName + "</td></tr>";
                collegeMailBody += "<tr><td>" + colemail + "</td></tr>";
                collegeMailBody += "</table>";
                //collegeMailBody += "<p>To</p>";
                //collegeMailBody += "<p>The Principal,</p>";
                //collegeMailBody += "<p>" + collegename .collegeName+ "</p>";
                //collegeMailBody += "<p>" + colemail + "</p>";
                collegeMailBody += "<p>It is to bring to your kind notice that the faculty member bearing Registration Number " + faculty.RegistrationNumber + " (" + faculty.FirstName + " " + faculty.LastName + ") has submitted a Grievance against your College. You are hereby informed to go through the complaint and supporting document (if any) in your College portal and submit your clarification on or before <b>" + DateTime.Now.AddDays(7).ToString("dd/MM/yyyy").Split(' ')[0] + "</b> 5:00 P.M.</p>";
                collegeMailBody += "<table><tr><td>Thanks & Regards</td></tr>";
                collegeMailBody += "<tr><td>Director, UAAC,</td></tr>";
                collegeMailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                collegeMailBody += "</table></div>";

                MailMessage Collegemessage = new MailMessage();
                Collegemessage.To.Add(complaint.email);
                //Collegemessage.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                //Collegemessage.To.Add("srinu.thummalapalli09@gmail.com,meda.nagarakesh@gmail.com,duaac@jntuh.ac.in");
                Collegemessage.Subject = "JNTUH- UAAC-" + faculty.FirstName + " " + faculty.LastName + " " + "-Grievance raised- Call for clarification-Reg.";
                Collegemessage.Body = collegeMailBody;
                Collegemessage.IsBodyHtml = true;
                var collegesmtp = new SmtpClient();
                collegesmtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                collegesmtp.Host = "smtp.gmail.com";
                collegesmtp.Port = 587;
                collegesmtp.EnableSsl = true;
                collegesmtp.Send(Collegemessage);


                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + faculty.FirstName + " " + faculty.LastName + "</p>";
                MailBody += "<p>Your Grievance id :" + complaint.TicketId + " ,has been forwarded to the College and called for clarification.</p>";
                MailBody += "<p style='background: yellow; width: 130px; font-weight: bold;'>Status: In Process</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        public string Pdfreportgenerate(int Grievanceid)
        {
            var complaint = db.jntuh_college_complaints.Where(c => c.id == Grievanceid && c.roleId == 7).FirstOrDefault();
            var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id, rf.Email }).FirstOrDefault();
            var collegedata = db.jntuh_address.Where(a => a.collegeId == complaint.collegeId).Select(a => a).ToList();
            var collegename = db.jntuh_college.Where(c => c.id == complaint.collegeId).Select(c => new { c.id, c.collegeCode, c.collegeName }).FirstOrDefault();
            string PrincipalRegNum = db.jntuh_college_principal_registered.Where(pr => pr.collegeId == complaint.collegeId).Select(pr => pr.RegistrationNumber).FirstOrDefault().ToString();
            var Principaldata = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == PrincipalRegNum).Select(rf => new { rf.RegistrationNumber, rf.Email, rf.FirstName, rf.LastName }).FirstOrDefault();
            string complainttype = db.jntuh_complaints.Where(cp => cp.id == complaint.complaintId).Select(cp => cp.complaintType).FirstOrDefault().ToString();
            string fullPath = string.Empty;

            Document pdfDoc = new Document(PageSize.A4, 40f, 50f, 60f, 30f);

            // Document pdfDoc = new Document(PageSize.A3, 0, 0, 0, 0);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/Upload/Collegegrievance/faculty/");
            string returnpath = string.Empty;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegename.collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            returnpath = collegename.collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;


            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/collegegrievance.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            string Subject = string.Empty;
            string Description = string.Empty;
            string clgename = string.Empty;
            string collegeaddress = string.Empty;
            var college_address = collegedata.Where(c => c.addressTye == "COLLEGE").FirstOrDefault();
            collegeaddress = "<p>" + college_address.address + " , " + college_address.townOrCity + " , " + college_address.mandal + "</p>";
            collegeaddress += "<p>" + db.jntuh_district.Where(d => d.id == college_address.districtId).Select(d => d.districtName).FirstOrDefault() + " , " + college_address.pincode + "</p>";

            Subject = "<p>     Sub-JNTUH- UAAC-" + faculty.FirstName + "  " + faculty.LastName + " -Grievance raised-Call for clarification-Reg.</p>";
            Description = "<p style='padding-left:20px;'> It is to bring to your kind notice that the faculty member bearing Registration Number" + faculty.RegistrationNumber + "(" + faculty.FirstName + " " + faculty.LastName + ") " + "has submitted a Grievance against your College. You are hereby informed to go through the complaint and supporting document (if any) in your College portal and submit your clarification on or before <date – system date + 7 days > (5:00 P.M) </p>";
            contents = contents.Replace("##CollegeName##", collegename.collegeName + "(" + collegename.collegeCode + "),");
            contents = contents.Replace("##COLLEGE_ADDRESS##", collegeaddress);
            contents = contents.Replace("##Subject##", Subject);
            contents = contents.Replace("##Description##", Description);



            contents = contents.Replace("##ToDate##", DateTime.Now.ToShortDateString());


            // contents = contents.Replace("##url##", Get_BarCode(1));

            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToShortDateString());



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

                            pdfDoc.SetMargins(60, 60, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(60, 60, 60, 60);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }

            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            //string returnpath = string.Empty;
            //returnpath = fullPath;
            TempData["Employee"] = fullPath;
            return returnpath;
        }
        #endregion

        #region Complaints Given by College Students.

        public ActionResult StudentIndex()
        {
            List<studentnews> Circulars = new List<studentnews>() 
            { 
                new studentnews() { Title = "Collecting additional fees under different heads –reg." , filename = "Additional Fee Collection.pdf" ,NewsOrder = 1},
                new studentnews() { Title = "GT-DTE-  Student Cancelling their Admission- Return of original Certificates / Refund of fee- Certain Instructions –Reg." , filename = "Not Collecting Fee- TSCHE-GO.pdf" ,NewsOrder = 2},
                new studentnews() { Title = "AICTE Regulations -Redressal of Grievance of Students-2019-Reg." , filename = "Regulations_Students 2019.pdf" ,NewsOrder = 3},
                // new studentnews() { Title = "Circular-Details of University OMBUDSPERSON-Reg." , filename = "OmbudsmanDetails.pdf" ,NewsOrder = 4},
            };
            return View(Circulars);
        }

        public ActionResult StudentComplaintScreen()
        {
            StudentsComplaints complaint = new StudentsComplaints();
            var jntucollege = db.jntuh_college.Where(r => r.isActive == true && r.id != 375).Select(s => new { collegeId = s.id, collegeName = s.collegeCode + "-" + s.collegeName }).ToList();
            var jntudistricte = db.jntuh_district.Where(r => r.isActive == true).Select(s => new { disctrictID = s.districtName, disctrictName = s.districtName }).ToList();
            List<jntuh_complaints> jntucomplaints = db.jntuh_complaints.Where(r => r.isActive == true && r.roleId == 5).Select(s => s).ToList();
            List<jntuh_complaints_givenby> jntuh_complaintsgivenby = db.jntuh_complaints_givenby.Where(r => r.isActive == true && r.roleId == 7).Select(s => s).ToList();
            var jntuh_degree_type = db.jntuh_degree_type.Where(t => t.isActive == true).Select(r => new { degreeTypeId = r.id, degreeType = r.degreeType }).ToList();
            List<int> humantiesdepts = new List<int>() { 29, 30, 31, 32, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78 };

            List<Departments> Courses = new List<Departments>();
            Courses = (from deg in db.jntuh_degree.ToList()
                       join dd in db.jntuh_department.ToList() on deg.id equals dd.degreeId
                       join sp in db.jntuh_specialization.ToList() on dd.id equals sp.departmentId
                       where !humantiesdepts.Contains(dd.id) && deg.isActive == true && sp.isActive == true
                       select new Departments()
                       {
                           //DegreeId = deg.id + "-" + sp.id,
                           DegreeId = sp.id,
                           DegreeName = deg.degree + "-" + sp.specializationName,
                       }).ToList();

            var jntuh_degree = db.jntuh_degree.Where(t => t.isActive == true).Select(r => new { degreeId = r.id, degreeType = r.degree }).ToList();
            var jntuh_department = db.jntuh_department.Where(t => t.isActive == true).Select(r => new { Id = r.id, Type = r.departmentName }).ToList();

            ViewBag.Complaints = jntucomplaints;
            ViewBag.Complaintsgivenby = jntuh_complaintsgivenby;
            ViewBag.Colleges = jntucollege;
            ViewBag.Disctricts = jntudistricte;
            ViewBag.jntuh_degree_type = jntuh_degree_type;
            ViewBag.Courses = Courses;
            //ViewBag.CoursesDropdown = jntuh_degree;
            //ViewBag.DepartmentsDropdown = jntuh_department.Distinct();
            ViewBag.YearofStudy = Yearofstudy;
            complaint.complaintdate = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
            return View(complaint);
        }

        [HttpPost]
        public ActionResult StudentComplaintScreen(StudentsComplaints ComplaintObj, string ajaxcall)
        {
            int checkstudentcomplaint =
                        db.jntuh_college_students_complaints.Where(
                            c =>
                                c.HallticketNo == ComplaintObj.HallticketNo &&
                                c.complaintStatus != 5).Select(s => s.id).FirstOrDefault(); //c.complaintId == ComplaintObj.complaintid && 
            if (checkstudentcomplaint != 0)
            {
                TempData["ERROR"] = "Hall Ticket Number " + ComplaintObj.HallticketNo.Trim() + " already added Grievance.";
                return RedirectToAction("StudentComplaintScreen");
            }
            if (ComplaintObj != null && ModelState.IsValid == true && ComplaintObj.courseId != 0)
            {
                string GenerateTicketId = string.Empty;
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                string complaintFilepath = "~/Content/Upload/ComplaintsGivenbyStudents/ComplaintFile";
                string IdProofFilepath = "~/Content/Upload/ComplaintsGivenbyStudents/IDProof";
                if (ComplaintObj.complaintid != 0)
                {
                    List<jntuh_complaints> jntuhComplaints =
                        db.jntuh_complaints.Where(c => c.isActive == true && c.roleId == 5 && c.typeId == 0).Select(s => s).ToList();
                    jntuh_college_students_complaints collegeComplaints = new jntuh_college_students_complaints();
                    collegeComplaints.academicyearId = ay0;
                    collegeComplaints.collegeId = ComplaintObj.collegeId;
                    var clg = db.jntuh_college.Where(e => e.id == collegeComplaints.collegeId).Select(r => r.collegeCode).FirstOrDefault();
                    GenerateTicketId = clg + "-" + "SCR-" + DateTime.Now.ToString("yyyyMMdd").Substring(2) + "-" + ComplaintObj.complaintid + "-" + DateTime.Now.ToString("HHmmss");
                    collegeComplaints.TicketId = GenerateTicketId;

                    collegeComplaints.complaintId = ComplaintObj.complaintid;
                    ComplaintObj.complaintname =
                        jntuhComplaints.Where(c => c.id == ComplaintObj.complaintid && c.roleId == 5)
                            .Select(s => s.complaintType)
                            .FirstOrDefault();
                    if (ComplaintObj.complaintname == "Others")
                    {
                        collegeComplaints.otherComplaint = ComplaintObj.otherscomplaint;
                    }

                    if (ComplaintObj.complaintdate != null)
                        collegeComplaints.complaintDate = UAAAS.Models.Utilities.DDMMYY2MMDDYY(ComplaintObj.complaintdate);

                    string DateofBirth = string.Empty;
                    if (ComplaintObj.studentDOB != null)
                        DateofBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(ComplaintObj.studentDOB);

                    collegeComplaints.complaintDescription = ComplaintObj.ComplaintDesc;
                    collegeComplaints.complaintStatus = 1;

                    //Complaint File Save
                    if (ComplaintObj.complaintFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                        }

                        var ext = Path.GetExtension(ComplaintObj.complaintFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = ComplaintObj.HallticketNo + "-" + ComplaintObj.complaintid + "-" + DateTime.Now.ToString("yyMMdd-HHmmss") + clg;
                            ComplaintObj.complaintFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                            ComplaintObj.complaintFileview = string.Format("{0}{1}", filename, ext);
                        }
                    }

                    //Student ID Proof Save
                    if (ComplaintObj.IDProofFile != null)
                    {
                        if (!Directory.Exists(Server.MapPath(IdProofFilepath)))
                        {
                            Directory.CreateDirectory(Server.MapPath(IdProofFilepath));
                        }

                        var ext = Path.GetExtension(ComplaintObj.IDProofFile.FileName);
                        if (ext.ToUpper().Equals(".PDF"))
                        {
                            string filename = ComplaintObj.HallticketNo + "-ID" + "-" + DateTime.Now.ToString("yyMMdd-HHmmss") + clg;
                            ComplaintObj.IDProofFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(IdProofFilepath), filename, ext));
                            ComplaintObj.IDProof = string.Format("{0}{1}", filename, ext);
                        }
                    }

                    collegeComplaints.complaintFile = ComplaintObj.complaintFileview;
                    collegeComplaints.IdProof = ComplaintObj.IDProof;
                    collegeComplaints.Name = ComplaintObj.studentfirstname + " " + ComplaintObj.studentlastname;
                    collegeComplaints.HallticketNo = ComplaintObj.HallticketNo;
                    collegeComplaints.Gender = ComplaintObj.studentgender == "Male" ? 1 : 2;
                    collegeComplaints.Dateofbirth = Convert.ToDateTime(DateofBirth);
                    collegeComplaints.Email = ComplaintObj.Email;
                    collegeComplaints.Mobile = ComplaintObj.Moblie;
                    collegeComplaints.Address = ComplaintObj.Address.Replace("\r\n", " ");
                    collegeComplaints.Address = collegeComplaints.Address.Replace("\n", " ") + " ," + ComplaintObj.District.Trim();

                    collegeComplaints.City = ComplaintObj.city;
                    collegeComplaints.ParentOrGuardiansname = ComplaintObj.Guardianname;
                    collegeComplaints.ParentOrGuardiansmobile = ComplaintObj.Guardianmobile;
                    // collegeComplaints.courseStudiedId = ComplaintObj.coursestudiedId;

                    var finddegreeID = (from deg in db.jntuh_degree.ToList()
                                        join dd in db.jntuh_department.ToList() on deg.id equals dd.degreeId
                                        join sp in db.jntuh_specialization.ToList() on dd.id equals sp.departmentId
                                        where sp.isActive == true && sp.id == ComplaintObj.courseId
                                        select deg.id).FirstOrDefault();

                    var finddegreetypeID = (from deg in db.jntuh_degree.ToList()
                                            join dd in db.jntuh_department.ToList() on deg.id equals dd.degreeId
                                            join sp in db.jntuh_specialization.ToList() on dd.id equals sp.departmentId
                                            where sp.isActive == true && sp.id == ComplaintObj.courseId
                                            select deg.degreeTypeId).FirstOrDefault();

                    collegeComplaints.courseStudiedId = finddegreetypeID;
                    collegeComplaints.CourseId = finddegreeID;
                    collegeComplaints.departmentId = ComplaintObj.courseId;
                    collegeComplaints.yearofStudy = ComplaintObj.yearstudy;
                    collegeComplaints.isActive = true;
                    //collegeComplaints.createdBy = Convert.ToInt32(collegeComplaints.HallticketNo);
                    //collegeComplaints.createdBy = usedID;
                    collegeComplaints.createdBy = 1;
                    collegeComplaints.createdOn = DateTime.Now;
                    db.jntuh_college_students_complaints.Add(collegeComplaints);
                    db.SaveChanges();
                    var successmsg = "Your Complaint is taken by University. and your ticketid is " + collegeComplaints.TicketId;
                    TempData["SUCCESS"] = successmsg;

                    //send email
                    //IUserMailer mailer = new UserMailer();
                    //mailer.FacultyComplaintRequestMail(collegeComplaints.Email, "FacultyComplaintRequestMail",
                    //    "JNTUH Student Complaint Details.", collegeComplaints.TicketId, collegeComplaints.Name,
                    //    ComplaintObj.HallticketNo, ComplaintObj.complaintdate)
                    //    .SendAsync();

                    string MailBody = string.Empty;
                    MailBody += "<p>Dear " + collegeComplaints.Name + "</p>";
                    MailBody += "<p style='font-weight: bold'>Your Hall Ticket Number :" + collegeComplaints.HallticketNo + "</p>";
                    MailBody += "<p>With reference to your Grievance, you are hereby informed to furnish Grievance id :" + collegeComplaints.TicketId + " for further correspondence and can track the status of your Grievance at the same portal.</p>";
                    MailBody += "<table style='text-align:left;border:1px solid; width: 100%;'>";
                    MailBody += "<tr><td colspan=2 style='font-weight: bold; text-align: center; background: burlywood;'>GRIEVANCE DETAILS</td></tr>";
                    MailBody += "<tr><td colspan=2 >Grievance Registered Through DIRECTORATE OF AFFILIATIONS & ACADEMIC AUDIT  Portal</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Type :</td><td>" + ComplaintObj.complaintname + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Date :</td><td>" + ComplaintObj.complaintdate + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Id :</td><td>" + collegeComplaints.TicketId + "</td></tr>";
                    MailBody += "<tr><td style='width: 13%; font-weight: bold;'>Grievance Details :</td><td>" + collegeComplaints.complaintDescription + "</td></tr>";
                    MailBody += "</table>";
                    MailBody += "<table>";
                    MailBody += "<tr><td>Thanks & Regards</td></tr>";
                    MailBody += "<tr><td>Director, UAAC,</td></tr>";
                    MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                    MailBody += "</table>";

                    MailMessage message = new MailMessage();
                    message.To.Add(collegeComplaints.Email);
                    //message.To.Add("srinu.thummalapalli09@gmail.com");
                    message.Subject = "JNTUH-UAAC-Your Grievance ID:" + collegeComplaints.TicketId;
                    message.Body = MailBody;
                    message.IsBodyHtml = true;
                    var smtp = new SmtpClient();
                    smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(message);

                    //send sms
                    if (!string.IsNullOrEmpty(collegeComplaints.Email))
                    {
                        // bool pStatus = UAAAS.Models.Utilities.SendSMS(ComplaintObj.Moblie, "JNTUH: Your Complaint is taken by University. Your reference number is :TExt123");
                    }

                    if (ComplaintObj.text == "AjaxCall")
                        return Json(new { result = successmsg }, "application/json", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["ERROR"] = "Some thing Went Wrong.please select the complaint type";
                }
            }
            else
            {
                var modelErrors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        modelErrors.Add(modelError.ErrorMessage);
                    }
                }

                if (ajaxcall == "AjaxCall")
                    return Json(new { error = "All fields are mandatory" }, "application/json", JsonRequestBehavior.AllowGet);
                else
                    TempData["ERROR"] = "All star fields are mandatory";
            }
            return RedirectToAction("StudentComplaintScreen");
        }

        public ActionResult TrackComplaint(string ticketid)
        {
            if (!string.IsNullOrEmpty(ticketid))
            {
                var complaintdata = db.jntuh_college_students_complaints.Where(r => r.TicketId == ticketid.Trim()).Select(t => t).FirstOrDefault();
                StudentsComplaints complaint = new StudentsComplaints();
                if (complaintdata != null)
                {
                    complaint.TicketId = complaintdata.TicketId;
                    complaint.collegeId = complaintdata.collegeId;
                    complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                    complaint.complaintid = complaintdata.complaintId;
                    complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 5 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                    complaint.complaintdate = complaintdata.complaintDate.ToString();
                    complaint.otherscomplaint = complaintdata.otherComplaint;
                    complaint.ComplaintDesc = complaintdata.complaintDescription;
                    complaint.complaintFileview = complaintdata.complaintFile;
                    complaint.studentfullname = complaintdata.Name;
                    complaint.studentgender = complaintdata.Gender == 1 ? "Male" : "Female";
                    complaint.Email = complaintdata.Email;
                    complaint.Moblie = complaintdata.Mobile;
                    complaint.studentDOB = complaintdata.Dateofbirth.ToString();
                    complaint.HallticketNo = complaintdata.HallticketNo;
                    complaint.Address = complaintdata.Address;
                    complaint.city = complaintdata.City;
                    complaint.Guardianname = complaintdata.ParentOrGuardiansname;
                    complaint.Guardianmobile = complaintdata.ParentOrGuardiansmobile;
                    complaint.coursestudiedId = Convert.ToInt32(complaintdata.courseStudiedId);
                    complaint.coursestudied = db.jntuh_degree_type.Find(complaint.coursestudiedId).degreeType;
                    complaint.courseId = Convert.ToInt32(complaintdata.CourseId);
                    complaint.course = db.jntuh_degree.Find(complaint.courseId).degree;
                    complaint.departmentId = Convert.ToInt32(complaintdata.departmentId);
                    complaint.department = db.jntuh_specialization.Find(complaint.departmentId).specializationName;

                    complaint.complaintStatusName = (from obj in db.jntuh_complaints_status where obj.id == complaintdata.complaintStatus select obj.name).SingleOrDefault();

                }
                return View(complaint);
            }
            else
            {
                //TempData["ERROR"] = "Input Information is missed.Try Again";
                return View();
            }
        }

        [HttpGet]
        public ActionResult GetDepartments(int collegeid)
        {
            if (collegeid != null)
            {
                var jntuh_academivYear = db.jntuh_academic_year.ToList();
                int actualYear = jntuh_academivYear.Where(s => s.isActive == true && s.isPresentAcademicYear == true).Select(a => a.id).FirstOrDefault();
                int PresentYear = jntuh_academivYear.Where(s => s.id == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                int ay1 = jntuh_academivYear.Where(s => s.id == (actualYear)).Select(s => s.id).FirstOrDefault();
                int ay2 = jntuh_academivYear.Where(s => s.id == (actualYear - 1)).Select(s => s.id).FirstOrDefault();
                int ay3 = jntuh_academivYear.Where(s => s.id == (actualYear - 2)).Select(s => s.id).FirstOrDefault();
                int ay4 = jntuh_academivYear.Where(s => s.id == (actualYear - 3)).Select(s => s.id).FirstOrDefault();

                var academicyears = new int[] { PresentYear, ay1, ay2, ay3 };

                var jntuh_college_intake_existing = db.jntuh_college_intake_existing.Where(a => a.collegeId == collegeid && academicyears.Contains(a.academicYearId)).Select(w => w).ToList();
                var ExistingIntake = (from e in jntuh_college_intake_existing
                                      join s in db.jntuh_specialization on e.specializationId equals s.id
                                      join d in db.jntuh_department on s.departmentId equals d.id
                                      join de in db.jntuh_degree on d.degreeId equals de.id

                                      select new
                                      {
                                          DegreeId = s.id,
                                          DegreeName = de.degree + "-" + s.specializationName,
                                          SpecializationId = s.id,
                                          shiftId = e.shiftId
                                      }).ToList();

                ExistingIntake = ExistingIntake.AsEnumerable().GroupBy(s => new { s.SpecializationId, s.shiftId }).Select(q => q.First()).ToList();
                var jntuh_dept = ExistingIntake.Select(r => new { DegreeId = r.DegreeId, DegreeName = r.DegreeName }).Distinct().ToList();
                return Json(new { result = jntuh_dept }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { error = "Data is Not Bind to Drop Down." }, "application/json", JsonRequestBehavior.AllowGet);
            }

            //if (!string.IsNullOrEmpty(degree))
            //{
            //    var DegreeId = db.jntuh_degree.Where(t => t.degree == degree.Trim()).Select(e => e.id).FirstOrDefault();
            //    var jntuh_dept = db.jntuh_department.Where(t => t.isActive == true && t.degreeId == DegreeId).Select(r => new { deptId = r.id, dept = r.departmentName }).ToList();
            //    return Json(new { result = jntuh_dept }, "application/json", JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    return Json(new { error = "Data is Not Bind to Drop Down." }, "application/json", JsonRequestBehavior.AllowGet);
            //}
        }

        // [Authorize(Roles = "Admin")]
        public ActionResult Complaints()
        {
            //  int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var complaintsStatus = db.jntuh_complaints_status.ToList();
            List<StudentsComplaints> complaintsList = new List<StudentsComplaints>();
            var ComplaintsData = (from c in db.jntuh_college_students_complaints.ToList()
                                  join co in db.jntuh_complaints.ToList() on c.complaintId equals co.id
                                  join col in db.jntuh_college.ToList() on c.collegeId equals col.id
                                  where co.roleId == 5
                                  select new StudentsComplaints
                                  {
                                      id = c.id,
                                      TicketId = c.TicketId,
                                      HallticketNo = c.HallticketNo,
                                      studentfullname = c.Name,
                                      complaintid = c.complaintId,
                                      complaintname = co.complaintType,
                                      collegeId = col.id,
                                      Collegename = col.collegeName + "(" + col.collegeCode + ")",
                                      complaintdate = c.complaintDate.ToString(),
                                      complaintStatus = Convert.ToInt32(c.complaintStatus),
                                      complaintStatusName = complaintsStatus.Where(i => i.id == (int)c.complaintStatus).FirstOrDefault().name,
                                      complaintFileview = c.complaintFile
                                  }).ToList();

            foreach (var item in ComplaintsData)
            {
                //If College Given for Replay of Complaint 
                var collegegivenreplay =
               db.jntuh_college_grievancesassigned.Where(r => r.College_facultyid == item.collegeId && r.Ticketid == item.TicketId.Trim())
                   .Select(s => s)
                   .FirstOrDefault();
                if (collegegivenreplay != null)
                {
                    item.Replaycomplaintsfile = collegegivenreplay.Collegesupportingdocument;
                    item.Replayremarks = collegegivenreplay.CollegeRemarks;
                }
            }
            return View(ComplaintsData);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ComplaintsUpdate(int id)
        {
            if (id != null && id != 0)
            {
                var complaintdata = db.jntuh_college_students_complaints.Find(id);
                if (complaintdata != null)
                {
                    StudentsComplaints complaint = new StudentsComplaints();
                    complaint.id = complaintdata.id;
                    complaint.TicketId = complaintdata.TicketId;
                    complaint.collegeId = complaintdata.collegeId;
                    complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                    complaint.complaintid = complaintdata.complaintId;
                    complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 5 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                    complaint.complaintdate = complaintdata.complaintDate.ToString();
                    complaint.complaintdate = complaint.complaintdate.Split(' ')[0];
                    complaint.otherscomplaint = complaintdata.otherComplaint;
                    complaint.ComplaintDesc = complaintdata.complaintDescription;
                    complaint.complaintFileview = complaintdata.complaintFile;
                    complaint.studentfullname = complaintdata.Name;
                    complaint.studentgender = complaintdata.Gender == 1 ? "Male" : "Female";
                    complaint.Email = complaintdata.Email;
                    complaint.Moblie = complaintdata.Mobile;
                    complaint.studentDOB = complaintdata.Dateofbirth.ToString();
                    complaint.HallticketNo = complaintdata.HallticketNo;
                    complaint.Address = complaintdata.Address;
                    complaint.city = complaintdata.City;
                    complaint.Guardianname = complaintdata.ParentOrGuardiansname;
                    complaint.Guardianmobile = complaintdata.ParentOrGuardiansmobile;
                    complaint.coursestudiedId = Convert.ToInt32(complaintdata.courseStudiedId);
                    complaint.coursestudied = db.jntuh_degree_type.Find(complaint.coursestudiedId).degreeType;
                    complaint.courseId = Convert.ToInt32(complaintdata.CourseId);
                    complaint.course = db.jntuh_degree.Find(complaint.courseId).degree;
                    complaint.departmentId = Convert.ToInt32(complaintdata.departmentId);
                    complaint.department = db.jntuh_specialization.Find(complaint.departmentId).specializationName;
                    complaint.complaintStatus = Convert.ToInt32(complaintdata.complaintStatus);
                    //complaint.complaintFileview =complaintdata.complaintFile;
                    return PartialView("~/Views/FacultyComplaints/_complaintview.cshtml", complaint);
                }
                else
                {
                    TempData["ERROR"] = "Data is Not Found";
                    return RedirectToAction("Complaints");
                }
            }
            else
            {
                TempData["ERROR"] = "Parameter Value is Missing,Please Try Agian.";
                return RedirectToAction("Complaints");
            }
        }

        public ActionResult StudentComplaintsUpdate(string id)
        {
            var cmplantId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(id,
                        WebConfigurationManager.AppSettings["CryptoKey"]));
            if (cmplantId != null && cmplantId != 0)
            {
                var cmpltsStats = new List<ComplaintStats>();
                var complaintsStatus = db.jntuh_complaints_status.ToList();
                var complaintdata = db.jntuh_college_students_complaints.Find(cmplantId);
                if (complaintdata != null)
                {
                    FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                    complaint.id = complaintdata.id;
                    complaint.TicketId = complaintdata.TicketId;
                    complaint.collegeId = Convert.ToInt32(complaintdata.collegeId);
                    complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                    complaint.complaintid = Convert.ToInt32(complaintdata.complaintId);
                    complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 5 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                    complaint.complaintdate = complaintdata.complaintDate.ToString();
                    complaint.complaintdate = complaint.complaintdate.Split(' ')[0];
                    complaint.otherscomplaint = complaintdata.otherComplaint;
                    complaint.ComplaintDesc = complaintdata.complaintDescription;
                    complaint.complaintFileview = complaintdata.complaintFile;
                    complaint.Facultyname = complaintdata.Name;
                    complaint.RegistrationNumber = complaintdata.HallticketNo;
                    complaint.complaintStatus = Convert.ToInt32(complaintdata.complaintStatus);
                    complaint.FacultyAddress = complaintdata.Address;
                    complaint.FacultycomplaintFileview = complaintdata.complaintFile;
                    complaint.complaintStatusName = complaintsStatus.Where(i => i.id == (int)complaintdata.complaintStatus).FirstOrDefault().name;
                    foreach (var item in complaintsStatus)
                    {
                        if (item.id != complaintdata.complaintStatus && item.id != 1)
                        {
                            cmpltsStats.Add(new ComplaintStats() { cmpltId = item.id, cmpltName = item.name });
                        }
                    }
                    ViewBag.cmpltStats = cmpltsStats;
                    var collegegivenreplay =
                    db.jntuh_college_grievancesassigned.Where(r => r.College_facultyid == complaint.collegeId && r.Ticketid == complaint.TicketId.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                    if (collegegivenreplay != null)
                    {
                        complaint.Replaycomplaintsfile = collegegivenreplay.Collegesupportingdocument;
                        complaint.Replayremarks = collegegivenreplay.CollegeRemarks;
                    }
                    //return PartialView("~/Views/FacultyComplaints/_facultycomplaintview.cshtml", complaint);
                    return View(complaint);
                }
                else
                {
                    TempData["ERROR"] = "Data is Not Found";
                    return RedirectToAction("Complaints");
                }
            }
            else
            {
                TempData["ERROR"] = "Parameter Value is Missing,Please Try Agian.";
                return RedirectToAction("Complaints");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult StudentComplaintsUpdate(FacultyComplaintsClass objFacultyStat)
        {
            try
            {
                if (objFacultyStat.complaintStatusId == 2)
                {
                    stuGrievancecanotbeaddressed(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 3)
                {
                    stuGivanceNotunderstood(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 4)
                {
                    stuGrievanceProcedtocollege(objFacultyStat.id);
                }
                else if (objFacultyStat.complaintStatusId == 5)
                {
                    stuGreivanceClosed(objFacultyStat.id);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return RedirectToAction("Complaints");
        }

        // [Authorize(Roles = "Admin")]
        public ActionResult stuGivanceNotunderstood(int id, string ticketid)
        {
            stuGivanceNotunderstood(id);
            return RedirectToAction("Complaints");
        }

        private void stuGivanceNotunderstood(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_students_complaints.Where(c => c.id == id).FirstOrDefault();
                complaint.complaintStatus = 3;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + complaint.Name + "</p>";
                MailBody += "<p>Your grievance submitted at the UAAC portal bearing the Grievance id :" + complaint.TicketId + " , cannot be addressed to until you submit your grievance with clarity and by attaching relevant documents.</p>";
                MailBody += "<p style='background: yellow; width: 235px; font-weight: bold;'>Status: Send Relevant Documents</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.To.Add(complaint.Email);
                //message.To.Add("meda.rakesh@syntizen.com");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }
        // [Authorize(Roles = "Admin")]
        public ActionResult stuGrievancecanotbeaddressed(int id, string ticketid)
        {
            stuGrievancecanotbeaddressed(id);
            return RedirectToAction("Complaints");
        }

        private void stuGrievancecanotbeaddressed(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_students_complaints.Where(c => c.id == id).FirstOrDefault();
                // var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id }).FirstOrDefault();
                complaint.complaintStatus = 2;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + complaint.Name + "</p>";
                MailBody += "<p>Your Grievance id :" + complaint.TicketId + " , cannot be processed as it doesn’t came under grievance category. Hence, you need to submit your request to the University separately.</p>";
                MailBody += "<p style='background: yellow; width: 180px; font-weight: bold;'>Status: Not in Grievance</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.To.Add(complaint.Email);
                // message.To.Add("meda.rakesh@syntizen.com");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        //  [Authorize(Roles = "Admin")]
        public ActionResult stuGrievanceProcedtocollege(int id, string ticketid)
        {
            stuGrievanceProcedtocollege(id);
            return RedirectToAction("Complaints");
        }

        private void stuGrievanceProcedtocollege(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_students_complaints.Where(c => c.id == id && c.TicketId != null).FirstOrDefault();
                //  var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id, rf.Email }).FirstOrDefault();
                var collegeemail = db.jntuh_address.Where(a => a.collegeId == complaint.collegeId).Select(a => new { a.email, a.address, a.addressTye }).ToList();
                var collegename = db.jntuh_college.Where(c => c.id == complaint.collegeId).Select(c => new { c.id, c.collegeCode, c.collegeName }).FirstOrDefault();
                string PrincipalRegNum = db.jntuh_college_principal_registered.Where(pr => pr.collegeId == complaint.collegeId).Select(pr => pr.RegistrationNumber).FirstOrDefault().ToString();
                var Principaldata = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == PrincipalRegNum).Select(rf => new { rf.RegistrationNumber, rf.Email, rf.FirstName, rf.LastName }).FirstOrDefault();
                string complainttype = db.jntuh_complaints.Where(cp => cp.id == complaint.complaintId).Select(cp => cp.complaintType).FirstOrDefault().ToString();
                complaint.complaintStatus = 4;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string grievanceletter = stuPdfreportgenerate(complaint.id);
                jntuh_college_grievancesassigned collegecomplaint = new jntuh_college_grievancesassigned();
                collegecomplaint.College_facultyid = (int)complaint.collegeId;
                collegecomplaint.grievanceassiegnedto = 4;
                collegecomplaint.Grievancetype = 5;
                collegecomplaint.Ticketid = complaint.TicketId;
                collegecomplaint.Grievanceid = complaint.id;
                collegecomplaint.Ticketstatus = 4;
                collegecomplaint.Collegeletter = grievanceletter;
                collegecomplaint.admincreatedby = userID;
                collegecomplaint.isactive = true;
                collegecomplaint.admincreatedon = DateTime.Now;
                db.jntuh_college_grievancesassigned.Add(collegecomplaint);
                db.SaveChanges();

                string colemail = collegeemail.Where(c => c.addressTye == "COLLEGE").Select(c => c.address).FirstOrDefault();
                string collegeMailBody = string.Empty;
                collegeMailBody += "<div style='border:1px solid red;padding:10px;background:beige'><table><tr><td>To</td></tr>";
                collegeMailBody += "<tr><td>The Principal,</td></tr>";
                collegeMailBody += "<tr><td>" + collegename.collegeName + "," + "</td></tr>";
                collegeMailBody += "<tr><td>" + colemail + "." + "</td></tr>";
                collegeMailBody += "</table>";
                //collegeMailBody += "<p>To</p>";
                //collegeMailBody += "<p>The Principal,</p>";
                //collegeMailBody += "<p>" + collegename .collegeName+ "</p>";
                //collegeMailBody += "<p>" + colemail + "</p>";
                collegeMailBody += "<p>It is to bring to your kind notice that the student bearing  Hall ticket Number :" + "  " + complaint.HallticketNo + " Name :" + complaint.Name + " has submitted a Grievance against your College. You are hereby informed to go through the complaint and supporting document (if any) in your College portal and submit your clarification on or before " + DateTime.Now.AddDays(7).ToShortDateString() + " (5:00 P.M).</p>";
                collegeMailBody += "<table><tr><td>Thanks & Regards</td></tr>";
                collegeMailBody += "<tr><td>Director, UAAC,</td></tr>";
                collegeMailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                collegeMailBody += "</table></div>";

                MailMessage Collegemessage = new MailMessage();
                Collegemessage.To.Add(complaint.Email);
                //Collegemessage.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                //Collegemessage.To.Add("srinu.thummalapalli09@gmail.com,meda.nagarakesh@gmail.com,duaac@jntuh.ac.in");
                Collegemessage.Subject = "JNTUH- UAAC-" + complaint.Name + " " + "-Grievance raised-Call for clarification-Reg.";
                Collegemessage.Body = collegeMailBody;
                Collegemessage.IsBodyHtml = true;
                var collegesmtp = new SmtpClient();
                collegesmtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                collegesmtp.Host = "smtp.gmail.com";
                collegesmtp.Port = 587;
                collegesmtp.EnableSsl = true;
                collegesmtp.Send(Collegemessage);


                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + complaint.Name + "</p>";
                MailBody += "<p>Your Grievance id :" + complaint.TicketId + " ,has been forwarded to the College and called for clarification.</p>";
                MailBody += "<p style='background: yellow; width: 130px; font-weight: bold;'>Status: In Process</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.Email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        private void stuGreivanceClosed(int id)
        {
            if (id != null && id != 0)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                // FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                var complaint = db.jntuh_college_students_complaints.Where(c => c.id == id && c.TicketId != null).FirstOrDefault();
                //var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.c).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id }).FirstOrDefault();
                complaint.complaintStatus = 5;
                complaint.updatedOn = DateTime.Now;
                complaint.updatedBy = userID;
                db.Entry(complaint).State = System.Data.EntityState.Modified;
                db.SaveChanges();
                string MailBody = string.Empty;
                MailBody += "<div style='border:1px solid red;padding:10px;background:beige'><p>Dear " + complaint.Name + "</p>";
                MailBody += "<p>Your grievance submitted at the UAAC portal bearing the Grievance id :" + complaint.TicketId + " , is closed.</p>";
                MailBody += "<p style='background: yellow; width: 235px; font-weight: bold;'>Status: Closed</p>";
                MailBody += "<tr><td>Thanks & Regards</td></tr>";
                MailBody += "<tr><td>Director, UAAC,</td></tr>";
                MailBody += "<tr><td>JNTUH, Hyderabad</td></tr>";
                MailBody += "</table></div>";

                MailMessage message = new MailMessage();
                message.To.Add(complaint.Email);
                //message.To.Add("nanidd@gmail.com,duaac@jntuh.ac.in,supportaac@jntuh.ac.in");
                message.Subject = "JNTUH-UAAC-Your Grievance ID:" + complaint.TicketId;
                message.Body = MailBody;
                message.IsBodyHtml = true;
                var smtp = new SmtpClient();
                smtp.Credentials = new NetworkCredential("grmaac@jntuh.ac.in", "uaaac@123");
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
            }
            TempData["Success"] = "grievance email is successfully Sent";
        }

        public string stuPdfreportgenerate(int Grievanceid)
        {

            var complaint = db.jntuh_college_students_complaints.Where(c => c.id == Grievanceid).FirstOrDefault();
            //var faculty = db.jntuh_registered_faculty.Where(rf => rf.id == complaint.college_faculty_Id).Select(rf => new { rf.RegistrationNumber, rf.FirstName, rf.LastName, rf.id, rf.Email }).FirstOrDefault();
            var collegedata = db.jntuh_address.Where(a => a.collegeId == complaint.collegeId).Select(a => a).ToList();
            var collegename = db.jntuh_college.Where(c => c.id == complaint.collegeId).Select(c => new { c.id, c.collegeCode, c.collegeName }).FirstOrDefault();
            string PrincipalRegNum = db.jntuh_college_principal_registered.Where(pr => pr.collegeId == complaint.collegeId).Select(pr => pr.RegistrationNumber).FirstOrDefault().ToString();
            var Principaldata = db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == PrincipalRegNum).Select(rf => new { rf.RegistrationNumber, rf.Email, rf.FirstName, rf.LastName }).FirstOrDefault();
            string complainttype = db.jntuh_complaints.Where(cp => cp.id == complaint.complaintId).Select(cp => cp.complaintType).FirstOrDefault().ToString();
            string fullPath = string.Empty;

            Document pdfDoc = new Document(PageSize.A4, 40f, 50f, 60f, 30f);

            // Document pdfDoc = new Document(PageSize.A3, 0, 0, 0, 0);

            // Create a new PdfWrite object, writing the output to a MemoryStream
            //open pdf
            var output = new MemoryStream();
            var writer = PdfWriter.GetInstance(pdfDoc, output);

            string path = Server.MapPath("~/Content/Upload/Collegegrievance/Students/");
            string returnpath = string.Empty;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            fullPath = path + collegename.collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            returnpath = collegename.collegeCode + "_" + DateTime.Now.ToString("yyyMMddHHmmss") + ".pdf";
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, new FileStream(fullPath, FileMode.Create));
            pdfDoc.Open();

            //Assign Html content in a string to write in PDF
            string contents = string.Empty;

            StreamReader sr;


            //Read file from server path
            sr = System.IO.File.OpenText(Server.MapPath("~/Content/studentcollegegrievance.html"));

            //store content in the variable
            contents = sr.ReadToEnd();
            sr.Close();

            string Subject = string.Empty;
            string Description = string.Empty;
            string clgename = string.Empty;
            string collegeaddress = string.Empty;
            var college_address = collegedata.Where(c => c.addressTye == "COLLEGE").FirstOrDefault();
            collegeaddress = "<p>" + college_address.address + " , " + college_address.townOrCity + " , " + college_address.mandal + "</p>";
            collegeaddress += "<p>" + db.jntuh_district.Where(d => d.id == college_address.districtId).Select(d => d.districtName).FirstOrDefault() + " , " + college_address.pincode + "</p>";

            Subject = "<p>     Sub-JNTUH- UAAC-" + complaint.Name + " -Grievance raised-Call for clarification-Reg.</p>";

            //Description = "<p style='padding-left:20px;'> It is to bring to your kind notice that the faculty member bearing Registration Number" + complaint.HallticketNo + "(" + complaint.Name + ") " + "has submitted a Grievance against your College. You are hereby informed to go through the complaint and supporting document (if any) in your College portal and submit your clarification on or before <date – system date + 7 days > (5:00 P.M) </p>";
            contents = contents.Replace("##stuenthallticketnumber##", complaint.HallticketNo);
            contents = contents.Replace("##studentname##", complaint.Name);
            contents = contents.Replace("##CollegeName##", collegename.collegeName + "(" + collegename.collegeCode + "),");
            contents = contents.Replace("##COLLEGE_ADDRESS##", collegeaddress);

            contents = contents.Replace("##Subject##", Subject);



            contents = contents.Replace("##ToDate##", DateTime.Now.ToShortDateString());

            contents = contents.Replace("##submitDate##", DateTime.Now.AddDays(7).ToShortDateString());
            // contents = contents.Replace("##url##", Get_BarCode(1));

            contents = contents.Replace("##CurrentDate##", DateTime.Now.ToShortDateString());



            var parsedHtmlElements = HTMLWorker.ParseToList(new StringReader(contents), null);

            bool pageRotated = false;

            //Get each array values from parsed elements and add to the PDF document
            foreach (var htmlElement in parsedHtmlElements)
            {
                try
                {
                    if (htmlElement.Equals("<textarea>"))
                    {
                        pdfDoc.NewPage();
                    }

                    if (htmlElement.Chunks.Count >= 3)
                    {
                        if (htmlElement.Chunks.Count == 4)
                        {
                            pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);

                            pdfDoc.SetMargins(60, 60, 60, 60);
                            pageRotated = true;
                        }
                        else
                        {
                            if (pageRotated == true)
                            {
                                pdfDoc.SetPageSize(iTextSharp.text.PageSize.A4);
                                pdfDoc.SetMargins(60, 60, 60, 60);
                                pageRotated = false;
                            }
                        }
                        pdfDoc.NewPage();
                    }
                    else
                    {
                        pdfDoc.Add((IElement)htmlElement);
                    }
                }
                catch (Exception ex)
                {
                    continue;

                }

            }

            //Close your PDF
            pdfDoc.Close();
            if (pdfDoc.IsOpen())
            {
                pdfDoc.Close();
            }
            //string returnpath = string.Empty;
            //returnpath = fullPath;
            TempData["Employee"] = fullPath;
            return returnpath;
        }

        #endregion

        #region College Grievance View
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeGrievance()
        {
            //int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userId).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();

            int userCollegeID = 375;
            //if (userCollegeID == 0)
            //{
            //    userCollegeID = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            //}
            var collegegrivance = db.jntuh_college_grievancesassigned.Where(g => g.College_facultyid == userCollegeID && g.Grievancetype == 7 && g.grievanceassiegnedto == 4 && g.Subgrievanceid == null).Select(g => g).ToList();
            var Grievance_Details = db.jntuh_college_complaints.Where(cp => cp.TicketId != null && cp.roleId == 7 && cp.collegeId == userCollegeID).Select(c => c).ToList();
            int[] Facultids = Grievance_Details.Select(g => g.college_faculty_Id).ToArray();
            int?[] Complainttypes = Grievance_Details.Select(g => g.complaintId).ToArray();
            var facultyDetails = db.jntuh_registered_faculty.Where(rf => Facultids.Contains(rf.id)).Select(rf => new { rf.id, rf.RegistrationNumber, rf.FirstName, rf.LastName }).ToList();
            var Complaint_details = db.jntuh_complaints.Where(c => Complainttypes.Contains(c.id)).Select(c => new { c.id, c.complaintType }).ToList();
            List<Collegecomplaints> Collegecomplaintslist = new List<Collegecomplaints>();
            foreach (var item in collegegrivance)
            {
                Collegecomplaints College_complaints = new Collegecomplaints();

                var grievancedetails = Grievance_Details.Where(g => g.id == item.Grievanceid && g.TicketId == item.Ticketid).Select(g => g).FirstOrDefault();
                if (grievancedetails != null)
                {
                    var facultydetails = facultyDetails.Where(rf => rf.id == grievancedetails.college_faculty_Id).Select(rf => rf).FirstOrDefault();
                    var Complaint_types = Complaint_details.Where(c => c.id == grievancedetails.complaintId).Select(c => c).FirstOrDefault();
                    //string facultyName=facultydetails
                    College_complaints.Grievanceid = item.Grievanceid;
                    College_complaints.Ticketid = grievancedetails.TicketId;
                    College_complaints.RegistrationNumber = facultydetails.RegistrationNumber;
                    College_complaints.Name = facultydetails.FirstName + " " + facultydetails.LastName;
                    College_complaints.Grievancename = Complaint_types.complaintType;
                    College_complaints.Collegeletter = item.Collegeletter;
                    DateTime grivancedate = (DateTime)grievancedetails.complaintDate;
                    College_complaints.GrievanceDate = grivancedate.ToString("dd/MM/yyyy");
                    Collegecomplaintslist.Add(College_complaints);
                }

            }

            return View(Collegecomplaintslist);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeGrievanceupdate(int id)
        {
            if (id != null && id != 0)
            {
                var complaintdata = db.jntuh_college_complaints.Find(id);
                if (complaintdata != null)
                {
                    var Facultynames = db.jntuh_registered_faculty.Where(rf => rf.id == complaintdata.college_faculty_Id).Select(e => new { e.id, e.RegistrationNumber, e.FirstName, e.MiddleName, e.LastName }).FirstOrDefault();
                    FacultyComplaintsClass complaint = new FacultyComplaintsClass();
                    complaint.id = complaintdata.id;
                    complaint.TicketId = complaintdata.TicketId;
                    complaint.collegeId = Convert.ToInt32(complaintdata.collegeId);
                    complaint.Collegename = db.jntuh_college.Where(r => r.id == complaintdata.collegeId).Select(r => r.collegeName + "(" + r.collegeCode + ")").FirstOrDefault();
                    complaint.complaintid = Convert.ToInt32(complaintdata.complaintId);
                    complaint.complaintname = db.jntuh_complaints.Where(r => r.roleId == 5 && r.id == complaintdata.complaintId).Select(r => r.complaintType).FirstOrDefault();
                    complaint.complaintdate = complaintdata.complaintDate.ToString();
                    complaint.complaintdate = complaint.complaintdate.Split(' ')[0];
                    complaint.otherscomplaint = complaintdata.otherComplaint;
                    complaint.ComplaintDesc = complaintdata.complaintDescription;
                    complaint.complaintFileview = complaintdata.complaintFile;
                    complaint.Facultyname = Facultynames.FirstName + " " + Facultynames.MiddleName + " " + Facultynames.LastName;
                    complaint.RegistrationNumber = Facultynames.RegistrationNumber;
                    complaint.complaintStatus = Convert.ToInt32(complaintdata.complaintStatus);
                    complaint.FacultyAddress = complaintdata.facultyAddress;
                    complaint.FacultycomplaintFileview = complaintdata.complaintFile;



                    jntuh_college_grievancesassigned grievancesassigned = new jntuh_college_grievancesassigned();

                    grievancesassigned = db.jntuh_college_grievancesassigned.Where(e => e.Grievanceid == complaintdata.id && e.Subgrievanceid == null).Select(e => e).FirstOrDefault();

                    if (!string.IsNullOrEmpty(grievancesassigned.Collegesupportingdocument))
                    {
                        complaint.Collegesupportingdocumentname = grievancesassigned.Collegesupportingdocument;
                    }

                    complaint.CollegeRemarks = grievancesassigned.CollegeRemarks;
                    complaint.createddate = grievancesassigned.createdon.ToString();

                    //return PartialView("~/Views/FacultyComplaints/_facultycomplaintview.cshtml", complaint);
                    return View(complaint);
                }
                else
                {
                    TempData["ERROR"] = "Data is Not Found";
                    return RedirectToAction("FacultyComplaintsList");
                }
            }
            else
            {
                TempData["ERROR"] = "Parameter Value is Missing,Please Try Agian.";
                return RedirectToAction("FacultyComplaintsList");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeGrievanceupdate(FacultyComplaintsClass complaint, string Command)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_college_grievancesassigned grievancesassigneddata = new jntuh_college_grievancesassigned();
            if (complaint.id != null)
            {
                grievancesassigneddata = db.jntuh_college_grievancesassigned.Where(e => e.Grievanceid == complaint.id && e.Subgrievanceid == null).Select(e => e).FirstOrDefault();

                string complaintFilepath = "~/Content/Upload/FacultyComplaints/CollegeGrievancesupportdoc";
                if (complaint.Collegesupportingdocument != null)
                {
                    if (!Directory.Exists(Server.MapPath(complaintFilepath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(complaintFilepath));
                    }
                    var ext = Path.GetExtension(complaint.Collegesupportingdocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        string filename = complaint.id + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "_CF";
                        complaint.Collegesupportingdocument.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(complaintFilepath), filename, ext));
                        complaint.Collegesupportingdocumentname = string.Format("{0}{1}", filename, ext);

                    }
                }
                else
                {
                    complaint.Collegesupportingdocumentname = complaint.Collegesupportingdocumentname;
                }
                if (Command == "Save")
                {
                    grievancesassigneddata.Collegesupportingdocument = complaint.Collegesupportingdocumentname;
                    grievancesassigneddata.CollegeRemarks = complaint.CollegeRemarks;
                    grievancesassigneddata.createdon = DateTime.Now;
                    grievancesassigneddata.createdby = userId;

                    db.Entry(grievancesassigneddata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Save successfully";
                }
                else
                {
                    grievancesassigneddata.Collegesupportingdocument = complaint.Collegesupportingdocumentname;
                    grievancesassigneddata.CollegeRemarks = complaint.CollegeRemarks;
                    grievancesassigneddata.updatedon = DateTime.Now;
                    grievancesassigneddata.updatedby = userId;

                    db.Entry(grievancesassigneddata).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Save successfully";
                }
                return RedirectToAction("CollegeGrievanceupdate", new { id = complaint.id });
            }
            return RedirectToAction("CollegeGrievance");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeGrievancesupportdoc(int id)
        {

            return PartialView("_CollegeGrievancesupportdoc");
        }
        [HttpPost]
        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeGrievancesupportdoc(FacultyComplaintsClass complaint)
        {
            return RedirectToAction("CollegeGrievance");
        }

        #endregion

        #region Grievance Excel Report download
        [Authorize(Roles = "Admin")]
        public ActionResult FacultyGrievancedownload()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            List<StudentsComplaints> complaintsList = new List<StudentsComplaints>();
            var Collegecomplaints = db.jntuh_college_complaints.Where(c => c.roleId == 7 && c.createdBy != 124636 && (c.createdOn.Value.Year == 2022 || c.createdOn.Value.Year == 2023)).ToList();
            var Complaints = db.jntuh_complaints.ToList();
            int[] facultyids = Collegecomplaints.Select(c => c.college_faculty_Id).Distinct().ToArray();
            var Colleges = db.jntuh_college.Where(c => c.isActive == true).ToList();
            var Facultynames = db.jntuh_registered_faculty.Where(rf => facultyids.Contains(rf.id)).Select(e => new { e.id, e.RegistrationNumber, e.FirstName, e.MiddleName, e.LastName }).ToList();
            List<FacultyComplaintsClass> FacultyComplaints = new List<FacultyComplaintsClass>();

            var jntuhcollegegrievancesassigned = db.jntuh_college_grievancesassigned.Where(r => r.Ticketid != null).Select(s => s).ToList();
            foreach (var item in Collegecomplaints)
            {
                FacultyComplaintsClass Complaint = new FacultyComplaintsClass();
                Complaint.id = item.id;
                Complaint.TicketId = item.TicketId;
                Complaint.RegistrationNumber = Facultynames.Where(f => f.id == item.college_faculty_Id).Select(f => f.RegistrationNumber).FirstOrDefault();
                Complaint.complaintname = Complaints.Where(cp => cp.id == item.complaintId).Select(cp => cp.complaintType).FirstOrDefault();
                Complaint.complaintid = Complaints.Where(cp => cp.id == item.complaintId).Select(cp => cp.id).FirstOrDefault();
                Complaint.complaintdate = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => String.Format("{0:d/M/yyyy}", cp.complaintDate)).FirstOrDefault().ToString();
                int? complaint_status = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => cp.complaintStatus).FirstOrDefault();
                Complaint.complaintStatus = (int)complaint_status;
                Complaint.complaintFileview = Collegecomplaints.Where(cp => cp.id == item.id).Select(cp => cp.complaintFile).FirstOrDefault();
                Complaint.collegeId = Colleges.Where(c => c.id == item.collegeId).Select(c => c.id).FirstOrDefault();
                Complaint.Collegename = Colleges.Where(c => c.id == item.collegeId).Select(c => c.collegeName).FirstOrDefault() + "(" + Colleges.Where(c => c.id == item.collegeId).Select(c => c.collegeCode).FirstOrDefault() + ")";
                var fullname = Facultynames.Where(f => f.id == item.college_faculty_Id).Select(f => new { f.FirstName, f.MiddleName, f.LastName }).FirstOrDefault();
                Complaint.Facultyname = fullname.FirstName + " " + fullname.MiddleName + " " + fullname.LastName;
                Complaint.FacultyAddress = item.facultyAddress;
                Complaint.ComplaintDesc = item.complaintDescription;

                //If College Given for Replay of Complaint 
                var collegegivenreplay =
                    jntuhcollegegrievancesassigned.Where(r => r.College_facultyid == Complaint.collegeId && r.Ticketid == Complaint.TicketId.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (collegegivenreplay != null)
                {
                    Complaint.Replaycomplaintsfile = collegegivenreplay.Collegesupportingdocument;
                    Complaint.Replayremarks = collegegivenreplay.CollegeRemarks;
                }

                FacultyComplaints.Add(Complaint);
            }
            int year = DateTime.Today.Year; int month = DateTime.Today.Month; int date = DateTime.Today.Day;
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=FacultyGrievance.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            //return View(FacultyComplaints);
            return PartialView("~/Views/FacultyComplaints/_FacultyGrievancedownload.cshtml", FacultyComplaints);
            //return null;
        }
        #endregion
    }

    public class Departments
    {
        public int DegreeId { get; set; }
        public string DegreeName { get; set; }
    }

    public class studentnews
    {
        public string Title { get; set; }
        public string filename { get; set; }
        public int NewsOrder { get; set; }
    }
}
