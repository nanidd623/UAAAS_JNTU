using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class AdminDashbordController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
        //[HttpGet]
        //public ActionResult AdminDashbordColleges(string strtype, string cmd)
        //{
        //    string ReportHeader = string.Empty;
        //    List<AdminDashbord> collegesList = new List<AdminDashbord>();
        //    collegesList = (from c in db.jntuh_college
        //                    //join a in db.jntuh_address on c.id equals a.collegeId into t
        //                    //from rt in t.Where(ad => ad.addressTye == "College").DefaultIfEmpty()
        //                    select new AdminDashbord
        //                    {
        //                        collegeid = c.id,
        //                        collegeCode = c.collegeCode,
        //                        collegeName = c.collegeName,
        //                        eamcetcode = c.eamcetCode,
        //                        icetcode=c.icetCode,
        //                        isActive = c.isActive,
        //                        isPermanent = c.isPermant,
        //                        isNew = c.isNew,
        //                        isClosed = c.isClosed,
        //                       // address = rt.address,
        //                       // townorCity = rt.townOrCity,
        //                       // pincode = rt.pincode,
        //                       // landline = rt.landline,
        //                       // mobile = rt.mobile,
        //                       // email = rt.email,
        //                        //addressType = rt.addressTye
        //                    }).ToList();
        //    if (strtype == "All Colleges")
        //    {
        //        ReportHeader = "Total Colleges";
        //        collegesList = collegesList.ToList();
        //    }
        //    else if (strtype == "Active Colleges")
        //    {
        //        ReportHeader = "Active Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true).ToList();
        //    }
        //    else if (strtype == "Deleted Colleges")
        //    {
        //        ReportHeader = "Deleted Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == false && c.isClosed==false).ToList();
        //    }
        //    else if (strtype == "Permanent Colleges")
        //    {
        //        ReportHeader = "Permanent Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && c.isPermanent == true).ToList();
        //    }
        //    else if (strtype == "New Colleges")
        //    {
        //        ReportHeader = "New Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && c.isNew == true).ToList();
        //    }
        //    else if (strtype == "Closed Colleges")
        //    {
        //        ReportHeader = "Closed Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isClosed == true).ToList();
        //    }
        //    else if (strtype == "Committee Submitted Colleges")
        //    {
        //        int[] committeeSubmittedCollegesIds = db.jntuh_committee_submission.Where(d => d.isActive == true).Select(d => d.collegeID).ToArray();
        //        ReportHeader = "Committee Submitted Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && committeeSubmittedCollegesIds.Contains(c.collegeid)).ToList();
        //    }
        //    else if (strtype == "Committee Pending Submitted Colleges")
        //    {
        //        int[] committeeSubmittedCollegesIds = db.jntuh_committee_submission.Where(d => d.isActive == true).Select(d => d.collegeID).ToArray();
        //        ReportHeader = "Committee Pending Submitted Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && !committeeSubmittedCollegesIds.Contains(c.collegeid)).ToList();
        //    }
        //    else if (strtype == "DataEntry Asigned Colleges")
        //    {
        //        int[] dataentryCompletedCollegesIds = db.jntuh_dataentry_allotment.Where(d => d.isActive == true).Select(d => d.collegeID).ToArray();
        //        ReportHeader = "DataEntry Asigned Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && dataentryCompletedCollegesIds.Contains(c.collegeid)).ToList();
        //    }
        //    else if (strtype == "DataEntry Completed Colleges")
        //    {
        //        int[] dataentryCompletedCollegesIds = db.jntuh_dataentry_allotment.Where(d => d.isActive == true && d.isCompleted == true).Select(d => d.collegeID).ToArray();
        //        ReportHeader = "DataEntry Completed Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && dataentryCompletedCollegesIds.Contains(c.collegeid)).ToList();
        //    }
        //    else if (strtype == "DataEntry Verified Colleges")
        //    {
        //        int[] dataentryCompletedCollegesIds = db.jntuh_dataentry_allotment.Where(d => d.isActive == true && d.isCompleted == true && d.isVerified == true).Select(d => d.collegeID).ToArray();
        //        ReportHeader = "DataEntry Verified Colleges.xls";
        //        collegesList = collegesList.Where(c => c.isActive == true && dataentryCompletedCollegesIds.Contains(c.collegeid)).ToList();
        //    }
        //    else 
        //    {
        //        ReportHeader = "Total Colleges.xls";
        //        collegesList = collegesList.ToList();
        //    }
        //    collegesList = collegesList.OrderBy(c => c.collegeName.ToUpper().Trim()).ToList();
        //    ViewBag.collegesList = collegesList;
        //    ViewBag.Count = collegesList.Count;
        //    if (cmd == "Export" && collegesList.Count > 0)
        //    {
        //        Response.ClearContent();
        //        Response.Buffer = true;
        //        Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
        //        Response.ContentType = "application/vnd.ms-excel";
        //        return PartialView("~/Views/Reports/_AdminDashbordColleges.cshtml");
        //    }
        //    return View("~/Views/Reports/AdminDashbordColleges.cshtml", collegesList);
        //}
        [HttpPost]
        public ActionResult AdminDashbordColleges(IList<AdminDashbord> adminDashbord, string cmd, string Header)
        {
            Header = Regex.Replace(Header, @"\s+", string.Empty);
            Header = Header + ".xls";
            List<AdminDashbord> collegesList = new List<AdminDashbord>();
            collegesList = adminDashbord.ToList();
            ViewBag.collegesList = collegesList;
            if (cmd == "Export" && collegesList.Count > 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + Header);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_AdminDashbordColleges.cshtml");
            }
            return View("~/Views/Reports/AdminDashbordColleges.cshtml", collegesList);
        }

        [HttpGet]
        public ActionResult SchedulingReport(string cmd)
        {
            List<AdminDashbord> SchedulingList = new List<AdminDashbord>();
            SchedulingList = (from a in db.jntuh_ffc_schedule
                              join c in db.jntuh_college on a.collegeID equals c.id
                              where (c.isActive == true)
                              select new AdminDashbord
                              {
                                  collegeid = c.id,
                                  collegeCode = c.collegeCode,
                                  collegeName = c.collegeName,
                                  orderDate = a.orderDate,
                                  inspectiondate = a.inspectionDate,
                                  alternateInspectionDate = a.alternateInspectionDate
                              }).ToList();
            ViewBag.SchedulingList = SchedulingList;

            if (cmd == "Export")
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=SchedulingReport.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_SchedulingReport.cshtml");
            }
            return View("~/Views/Reports/SchedulingReport.cshtml");
        }

        #region PrinicipalSCMExcel Download

        [Authorize(Roles = "Admin")]
        public ActionResult DownloadPrinipalExcel(string cmd)
        {
            var gv = new GridView();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            gv.DataSource = Getscmprinicpaldetails(prAy);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "SCMPrinicipal" + DateTime.Now.ToShortDateString() + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            var objStringWriter = new StringWriter();
            var objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("Admin", "Dashboard");
        }

        private List<ScmPrinicipal> Getscmprinicpaldetails(int prAy)
        {
            var lstprinipals = new List<ScmPrinicipal>();
            try
            {
                var plinkdetails =
                    db.jntuh_college_links_assigned.Where(
                        a => a.academicyearId == prAy && a.isActive == true && a.linkId == 2)
                        .Select(s => s)
                        .FirstOrDefault();
                if (plinkdetails != null)
                {
                    var fromdate = Convert.ToDateTime(plinkdetails.fromdate);
                    var scmproceedingsrequests =
                        db.jntuh_scmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId == 0 && s.SpecializationId == 0 && s.ISActive == true)
                            .Select(s => s)
                            .ToList();
                    var scmproceedingsfaculty =
                        db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.CreatedOn >= fromdate)
                            .Select(s => s)
                            .ToList();
                    var scmids = scmproceedingsrequests.Where(s => s.CollegeId != 375 && s.RequestSubmittedDate != null).OrderByDescending(i => i.RequestSubmittedDate).Select(s => s.ID).ToArray();
                    var scmprinicipalregnos = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).Select(i => i.RegistrationNumber.Trim()).ToArray();
                    var scmprinicipals = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).ToList();
                    var colleges = db.jntuh_college.Where(i => i.isActive).ToList();
                    var deparments = db.jntuh_department.ToList();
                    var regfaculty = db.jntuh_registered_faculty.Where(i => scmprinicipalregnos.Contains(i.RegistrationNumber.Trim())).ToList();
                    var totalphdsfaculty = db.jntuh_faculty_phddetails.AsNoTracking().ToList();
                    foreach (var prinip in scmprinicipals)
                    {
                        var clg = colleges.FirstOrDefault(i => i.id == prinip.CollegeId);
                        var faculty = regfaculty.FirstOrDefault(i => i.RegistrationNumber.Trim() == prinip.RegistrationNumber.Trim());
                        var jntuhDepartment = deparments.FirstOrDefault(i => faculty != null && i.id == faculty.DepartmentId);
                        var facultyphd = totalphdsfaculty.Where(i => i.Facultyid == faculty.id).FirstOrDefault();
                        var scmrequest = scmproceedingsrequests.Where(i => i.ID == prinip.ScmProceedingId).FirstOrDefault();
                        lstprinipals.Add(new ScmPrinicipal
                        {
                            RegistrationNo = prinip.RegistrationNumber,
                            CollegeCode = clg != null ? clg.collegeCode : "",
                            CollegeName = clg != null ? clg.collegeName : "",
                            FacultyName = faculty != null ? (faculty.FirstName.ToUpper() + " " + (faculty.MiddleName != null ? faculty.MiddleName.ToUpper() : "") + " " + faculty.LastName.ToUpper()) : "",
                            FacultyDateofBirth = UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString()),
                            FacultyMobile = faculty.Mobile,
                            PhDDepartment = facultyphd != null ? facultyphd.Department.ToUpper() : "",
                            PhDUniverstity = facultyphd != null ? facultyphd.University.ToUpper() : "",
                            Department = jntuhDepartment != null ? jntuhDepartment.departmentName.ToUpper() : "",
                            //SubmittedDate = Convert.ToDateTime(scmrequest.RequestSubmittedDate).ToString("dd/MM/yyyy hh:mm tt")
                            SubmittedDate = scmrequest.RequestSubmittedDate.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            lstprinipals = lstprinipals.OrderByDescending(i => i.SubmittedDate).ToList();
            return lstprinipals;
        }

        public class ScmPrinicipal
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string FacultyName { get; set; }
            public string RegistrationNo { get; set; }
            public string Department { get; set; }
            public string FacultyDateofBirth { get; set; }
            public string FacultyMobile { get; set; }
            public string SubmittedDate { get; set; }
            public string PhDDepartment { get; set; }
            public string PhDUniverstity { get; set; }
        }

        #endregion

        #region FacultySCMExcel Download

        [Authorize(Roles = "Admin")]
        public ActionResult DownloadFacultyExcel(string cmd)
        {
            var gv = new GridView();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            int prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            gv.DataSource = Getscmfacultydetails(prAy);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "SCMFaculty" + DateTime.Now.ToShortDateString() + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("Admin", "Dashboard");
        }

        private List<ScmFaculty> Getscmfacultydetails(int prAy)
        {
            var lstprinipals = new List<ScmFaculty>();
            var plinkdetails =
                db.jntuh_college_links_assigned.Where(
                    a => a.academicyearId == prAy && a.isActive && a.linkId == 1)
                    .Select(s => s)
                    .FirstOrDefault();
            if (plinkdetails != null)
            {
                var fromdate = Convert.ToDateTime(plinkdetails.fromdate);
                var scmproceedingsrequests =
                     db.jntuh_scmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId != 0 && s.SpecializationId != 0)
                        .Select(s => s)
                        .ToList();
                var scmproceedingsfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.CreatedOn >= fromdate)
                        .Select(s => s)
                        .ToList();
                var clgdistrict = string.Empty;
                var scmids = scmproceedingsrequests.Where(s => s.CollegeId != 375 && s.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                var scmprinicipalregnos = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).Select(i => i.RegistrationNumber).ToArray();
                var scmprinicipals = scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).ToList();
                var colleges = db.jntuh_college.AsNoTracking().Where(i => i.isActive).ToList();
                var collegeAddresses = db.jntuh_address.AsNoTracking().Where(i => i.addressTye == "COLLEGE").ToList();
                var districts = db.jntuh_district.AsNoTracking().ToList();
                var deparments = db.jntuh_department.AsNoTracking().ToList();
                var regfaculty = db.jntuh_registered_faculty.AsNoTracking().Where(i => scmprinicipalregnos.Contains(i.RegistrationNumber)).ToList();
                foreach (var fac in scmprinicipals)
                {
                    var clg = colleges.FirstOrDefault(i => i.id == fac.CollegeId);
                    var clgAddress = collegeAddresses.FirstOrDefault(i => i.collegeId == fac.CollegeId);
                    if (clgAddress != null)
                    {
                        clgdistrict = districts.Where(i => i.id == clgAddress.districtId).FirstOrDefault().districtName;
                    }
                    var faculty = regfaculty.FirstOrDefault(i => i.RegistrationNumber == fac.RegistrationNumber);
                    var fac1 = fac;
                    var req = scmproceedingsrequests.FirstOrDefault(i => i.ID == fac1.ScmProceedingId);
                    var jntuhDepartment = deparments.FirstOrDefault(i => req != null && i.id == req.DEpartmentId);

                    lstprinipals.Add(new ScmFaculty
                    {
                        RegistrationNo = fac.RegistrationNumber,
                        CollegeCode = clg != null ? "" + clg.collegeCode : "",
                        CollegeName = clg != null ? clg.collegeName : "",
                        District = clgdistrict != null ? clgdistrict : "",
                        FacultyName = faculty != null ? (faculty.FirstName.ToUpper() + " " + (faculty.MiddleName != null ? faculty.MiddleName.ToUpper() : "") + " " + faculty.LastName.ToUpper()) : "",
                        Department = jntuhDepartment != null ? jntuhDepartment.departmentName : ""
                    });
                }
            }
            return lstprinipals;
        }

        public class ScmFaculty
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string District { get; set; }
            public string Department { get; set; }
            public string RegistrationNo { get; set; }
            public string FacultyName { get; set; }
        }

        #endregion

        #region CovidExcel Download

        public List<Customdates> GetAllDatesAndInitializeTickets(DateTime startingDate, DateTime endingDate)
        {
            var allDates = new List<Customdates>();

            for (var date = startingDate; date <= endingDate; date = date.AddDays(1))
                allDates.Add(new Customdates { Id = date.Date, Date = date.Date.ToString("dd-MM-yyyy") });
            allDates.Add(new Customdates { Id = new DateTime(2020, 03, 16), Date = "ALL" });
            return allDates;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Download_ExcelCovid()
        {
            var fromdate = new DateTime(2020, 03, 17, 00, 00, 00);
            var todate = fromdate.AddDays(14);
            var dates = GetAllDatesAndInitializeTickets(fromdate, todate);
            ViewBag.dates = dates;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Download_ExcelCovid(Customdates obj)
        {
            var gv = new GridView();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            gv.DataSource = GetCovid19Compliancedetails(prAy, obj.Id);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "Covid19_ComplianceReport_(" + obj.Id.ToString("dd-MM-yyyy") + ")" + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            var objStringWriter = new StringWriter();
            var objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("Download_ExcelCovid");
        }

        private List<Covid19Compliance> GetCovid19Compliancedetails(int prAy, DateTime date)
        {
            var lstcovids = new List<Covid19Compliance>();
            var lstcomplianceDetails =
                db.jntuh_college_covidactivities.Where(
                    i =>
                        i.academicyear == prAy && i.collegeid != 375).ToList();
            var exactdate = date.Date.ToString("dd-MM-yyyy");
            if (exactdate == "16-03-2020")
            {
                var fromdate = new DateTime(2020, 03, 17, 00, 00, 00);
                var todate = fromdate.AddDays(14);
                var dates = GetAllDatesAndInitializeTickets(fromdate, todate);
                foreach (var dt in dates)
                {
                    var excolleges = lstcomplianceDetails.Where(i => i.submitteddate.Date == dt.Id.Date && i.collegeid != 375).GroupBy(i => new { collegeId = i.collegeid, date = i.submitteddate }).ToList();
                    if (excolleges.Count > 0)
                    {
                        var exobjcovid = new Covid19Compliance
                        {
                            Date = dt.Date,
                            NameoftheUniversity = "JNTUH",
                            NoofUniversityConstituentGovernmentDegreeColleges = "07",
                            NoofAffiliatedPrivateAffiliatedDegreeColleges = excolleges.Count.ToString(),
                            NoofUniversityConstituentGovernmentDegreeCollegesfunctioning = string.Empty,
                            NoofAffiliatedPrivateAffiliatedDegreeCollegesfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == dt.Id.Date && i.activityid == 3 && i.activitystatus),
                            NoofAffiliatedPrivateAffiliatedDegreeCollegesNotfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == dt.Id.Date && i.activityid == 3 && i.activitystatus == false),
                            NoofHostelsfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == dt.Id.Date && i.activityid == 7 && i.activitystatus),
                            IfanyCollegeundercolumnisfunctioningindeviationofGovtordersactiontakenagainstthem = string.Empty
                        };
                        lstcovids.Add(exobjcovid);
                    }
                }
                return lstcovids;
            }
            else
            {
                var colleges = lstcomplianceDetails.Where(i => i.submitteddate.Date == date.Date && i.collegeid != 375).GroupBy(i => new { collegeId = i.collegeid, date = i.submitteddate }).ToList();
                //var subcolleges = lstcomplianceDetails.Where(i => i.submitteddate.Date == date.Date && i.collegeid != 375).GroupBy(i => new { date = i.submitteddate }).ToList();
                var objcovid = new Covid19Compliance
                {
                    Date = date.Date.ToString("dd-MM-yyyy"),
                    NameoftheUniversity = "JNTUH",
                    NoofUniversityConstituentGovernmentDegreeColleges = "07",
                    NoofAffiliatedPrivateAffiliatedDegreeColleges = colleges.Count.ToString(),
                    NoofUniversityConstituentGovernmentDegreeCollegesfunctioning = string.Empty,
                    NoofAffiliatedPrivateAffiliatedDegreeCollegesfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == date.Date && i.activityid == 3 && i.activitystatus),
                    NoofAffiliatedPrivateAffiliatedDegreeCollegesNotfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == date.Date && i.activityid == 3 && i.activitystatus == false),
                    NoofHostelsfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == date.Date && i.activityid == 7 && i.activitystatus),
                    //NoofHostelsNotfunctioning = lstcomplianceDetails.Count(i => i.submitteddate.Date == date.Date && i.activityid == 7 && i.activitystatus == false),
                    IfanyCollegeundercolumnisfunctioningindeviationofGovtordersactiontakenagainstthem = string.Empty
                };
                lstcovids.Add(objcovid);
                return lstcovids;
            }
        }

        public class Covid19Compliance
        {
            public string Date { get; set; }
            public string NameoftheUniversity { get; set; }
            public string NoofUniversityConstituentGovernmentDegreeColleges { get; set; }
            public string NoofAffiliatedPrivateAffiliatedDegreeColleges { get; set; }
            public string NoofUniversityConstituentGovernmentDegreeCollegesfunctioning { get; set; }
            public int NoofAffiliatedPrivateAffiliatedDegreeCollegesfunctioning { get; set; }
            public int NoofAffiliatedPrivateAffiliatedDegreeCollegesNotfunctioning { get; set; }
            public int NoofHostelsfunctioning { get; set; }
            //public int NoofHostelsNotfunctioning { get; set; }
            public string IfanyCollegeundercolumnisfunctioningindeviationofGovtordersactiontakenagainstthem { get; set; }
        }

        public class Customdates
        {
            public DateTime Id { get; set; }
            public string Date { get; set; }
        }

        #endregion

        #region ExportFunctioningColleges

        public ActionResult ExportFunctioningColleges(DateTime date)
        {
            var gv = new GridView();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            gv.DataSource = GetCovid19ExportFunctioningColleges(prAy, date);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "Covid19_ComplianceReport" + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            var objStringWriter = new StringWriter();
            var objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("Download_ExcelCovid");
        }

        private List<ExportColleges> GetCovid19ExportFunctioningColleges(int prAy, DateTime date)
        {
            var lstcovids = new List<ExportColleges>();
            var lstcomplianceDetails =
                db.jntuh_college_covidactivities.Where(
                    i =>
                        i.academicyear == prAy && i.collegeid != 375).ToList();
            var excolleges = lstcomplianceDetails.Where(i => i.submitteddate.Date == date.Date && i.activityid == 3 && i.activitystatus).ToList();
            var colleges = db.jntuh_college.Where(i => i.isActive).ToList();
            foreach (var clg in excolleges)
            {
                var clgdetails = colleges.FirstOrDefault(i => i.id == clg.collegeid);
                if (clgdetails == null) continue;
                var objcovid = new ExportColleges
                {
                    CollegeCode = clgdetails.collegeCode,
                    CollegeName = clgdetails.collegeName,
                    Link = string.Concat(string.Empty, "http://jntuhaac.in/Content/Upload/College/CovidActivities/", clg.supportingdocuments)
                };
                lstcovids.Add(objcovid);
            }

            return lstcovids;
        }

        public class ExportColleges
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string Link { get; set; }
        }

        #endregion

        [Authorize(Roles = "Admin")]
        public ActionResult DownloadAICTEPCIExcel(string cmd)
        {
            var gv = new GridView();
            var actualYear = db.jntuh_academic_year.Where(a => a.isActive && a.isPresentAcademicYear).Select(q => q.actualYear).FirstOrDefault();
            var prAy = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();

            gv.DataSource = GetAICTEPCIdetails(prAy);
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + "AICTEPCIEOADETAILS_" + DateTime.Now.ToShortDateString() + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            var objStringWriter = new StringWriter();
            var objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return RedirectToAction("Admin", "Dashboard");
        }

        private List<AICTEPCIEoa> GetAICTEPCIdetails(int prAy)
        {
            var lstEoa = new List<AICTEPCIEoa>();
            var eoadate = new DateTime(2023, 6, 5);
            var clgEnclsdetails =
                db.jntuh_college_enclosures.Where(
                    a => a.academicyearId == prAy && a.isActive == true && a.enclosureId == 17 && a.collegeID != 375 && a.path != null && (a.updatedOn >= eoadate || a.createdOn >= eoadate))
                    .Select(s => s)
                    .ToList();
            var colleges = db.jntuh_college.Where(i => i.isActive).ToList();
            foreach (var item in clgEnclsdetails)
            {
                var clg = colleges.FirstOrDefault(i => i.id == item.collegeID);
                lstEoa.Add(new AICTEPCIEoa
                {
                    CollegeCode = clg != null ? "" + clg.collegeCode : "",
                    CollegeName = clg != null ? clg.collegeName : "",
                    EnclosurePath = String.Format("{0}{1}", "http://www.jntuhaac.in/Content/Upload/CollegeEnclosures/", item.path)
                });
            }
            return lstEoa;
        }

        public class AICTEPCIEoa
        {
            public string CollegeCode { get; set; }
            public string CollegeName { get; set; }
            public string EnclosurePath { get; set; }
        }
    }
}
