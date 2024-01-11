using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using UAAAS.Models;
using System.Configuration;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DashboardController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        int adminCollegeId = 0;
        private string serverURL;

        List<SelectListItem> Orderlist = new List<SelectListItem>()
        {
            new SelectListItem(){Value = "01",Text = "B.Tech"},
            new SelectListItem(){Value = "02",Text = "B.Pharmacy"},
            new SelectListItem(){Value = "03",Text = "M.Tech"},
            new SelectListItem(){Value = "04",Text = "M.Pharmacy"},
            new SelectListItem(){Value = "05",Text = "MCA"},
            new SelectListItem(){Value = "06",Text = "MBA"},
            new SelectListItem(){Value = "07",Text = "MAM"},
            new SelectListItem(){Value = "08",Text = "MTM"},
            new SelectListItem(){Value = "09",Text = "Pharm.D"},
            new SelectListItem(){Value = "10",Text = "Pharm.D PB"}
        };
        [Authorize(Roles = "College,Admin")]
        public ActionResult College(string collegecode)
        {
            //get current user Id
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //get current user CollegeId
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(collegecode))
            {
                adminCollegeId = db.jntuh_college.Where(c => c.collegeCode == collegecode).Select(c => c.id).FirstOrDefault();
                userCollegeID = adminCollegeId;
            }

            #region CollegeNews

            //Get current Date and time
            DateTime dateTime = DateTime.Now.Date;

            //Get bulletin board events based on start date and end date
            List<CollegeDashboardNews> news = db.jntuh_newsevents.Where(n => n.isActive == true &&
                                                            n.isNews == false &&
                                                            (n.startDate == null || n.startDate <= dateTime) &&
                                                            (n.endDate == null || n.endDate >= dateTime))
                                                            .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(n => n.createdDate).ToList();

            //Get College News and events based on start date and end date
            List<CollegeDashboardNews> cNews = db.jntuh_college_news.Where(collegeNews => collegeNews.isActive == true &&
                                                              collegeNews.collegeId == userCollegeID &&
                                                              (collegeNews.startDate == null ||
                                                              collegeNews.startDate <= dateTime) &&
                                                              (collegeNews.endDate == null ||
                                                              collegeNews.endDate >= dateTime)
                                                              && collegeNews.title != "DEFICIENCY REPORT AS PER FORM 415"
                                                              )
                                                              .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(collegeNews => collegeNews.createdDate).ToList();

            //List<CollegeDashboardNews> cNews1 = new List<CollegeDashboardNews>();
            CollegeDashboardNews cn = new CollegeDashboardNews();
            //Commented on 18-06-2018 by Narayana Reddy
            //var submittedIds = db.college_circulars.Where(cs => cs.isActive == true).Select(cs => cs.collegeId).ToList();

            //if (submittedIds.Contains(userCollegeID))
            //{
            //    cn.url = "/Content/Upload/News/AcknowledgementforBoth.pdf";
            //    cn.newstitle = "Acknowledgement for PG Courses submitted - Both Soft and Hard Copies";
            //    cn.createdDate = Convert.ToDateTime("09/04/2014");

            //    cNews.Add(cn);
            //}
            //else
            //{
            var notSubmittedIds = db.jntuh_college_pgcourses.Where(pg => pg.isActive == true).Select(pg => pg.collegeId).ToList();

            if (notSubmittedIds.Contains(userCollegeID))
            {
                cn.url = "/Content/Upload/News/OnlySoftCopySubmitted.pdf";
                cn.newstitle = "Acknowledgement for PG Courses submitted - Soft Copy only";
                cn.createdDate = Convert.ToDateTime("09/04/2014");

                cNews.Add(cn);
            }
            //}

            ViewBag.Events = news.Union(cNews).OrderByDescending(collegeNews => collegeNews.createdDate).Take(5);
            ViewBag.Affiliationfree =
                db.jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.afrcFee).FirstOrDefault();
            #endregion

            //SCM Faculty & Principal Closing Condition
            ViewBag.scmeditable = true;
            var currentDate = DateTime.Now;
            if (currentDate > new DateTime(2018, 02, 08, 23, 59, 59))
            {
                ViewBag.scmeditable = false;
            }

            UGWithDeficiency UGWithDeficiency = new UGWithDeficiency();
            UGWithDeficiency.CollegeId = userCollegeID;

            return View("~/Views/Reports/CollegeDashboard.cshtml", UGWithDeficiency);
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult Top100News()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //get current user CollegeId
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //Get current Date and time
            DateTime dateTime = DateTime.Now.Date;
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            List<CollegeDashboardNews> cNews = db.jntuh_college_news.Where(collegeNews => collegeNews.isActive == true &&
                                                              collegeNews.collegeId == userCollegeID &&
                                                              (collegeNews.startDate == null ||
                                                              collegeNews.startDate <= dateTime) &&
                                                              (collegeNews.endDate == null ||
                                                              collegeNews.endDate >= dateTime)
                                                              && collegeNews.title != "DEFICIENCY REPORT AS PER FORM 415"
                                                              )
                                                              .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(collegeNews => collegeNews.createdDate).ToList();
            List<CollegeDashboardNews> news = db.jntuh_newsevents.Where(n => n.isActive == true &&
                                                            n.isNews == false &&
                                                            (n.startDate == null || n.startDate <= dateTime) &&
                                                            (n.endDate == null || n.endDate >= dateTime))
                                                            .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(n => n.createdDate).ToList();
            //db.jntuh_newsevents.Select(c => new CollegeDashboardNews { url = c.navigateURL, newstitle = c.title, createdDate = c.createdOn }).OrderByDescending(c => c.createdDate).Take(100).ToList();
            ViewBag.NewEvents = news.Union(cNews).OrderByDescending(collegeNews => collegeNews.createdDate);
            return View();
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult CollegeDashboard(string collegecode)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //get current user CollegeId
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(collegecode))
            {
                adminCollegeId = db.jntuh_college.Where(c => c.collegeCode == collegecode).Select(c => c.id).FirstOrDefault();
                userCollegeID = adminCollegeId;
            }

            serverURL = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);

            var college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(e => e).FirstOrDefault();

            //int userCollegeID = college.id;
            var college_Director = db.jntuh_college_principal_director.Where(w => w.collegeId == college.id && w.type == "DIRECTOR").Select(e => e).FirstOrDefault();
            var college_chairman = db.jntuh_college_chairperson.Where(w => w.collegeId == college.id).Select(e => e).FirstOrDefault();

            var college_Principal = db.jntuh_college_principal_registered.Where(w => w.collegeId == college.id).Select(e => e.RegistrationNumber).FirstOrDefault();

            var Principal = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == college_Principal).Select(e => e).FirstOrDefault();

            var college_Faculty = db.jntuh_college_faculty_registered.Where(w => w.collegeId == college.id).Select(e => e).ToList();

            var DeptIDs = college_Faculty.Select(e => e.DepartmentId).Distinct().ToList();

            var jntuh_college_address = db.jntuh_address.Where(e => e.collegeId == college.id).Select(e => e).ToList();

            var jntuh_college_establishment = db.jntuh_college_establishment.Where(e => e.collegeId == college.id).Select(e => e).FirstOrDefault();







            var address = (from a in jntuh_college_address
                           join d in db.jntuh_district.ToList() on a.districtId equals d.id
                           where a.addressTye == "COLLEGE"
                           select new
                           {
                               Collegeaddress = a.address + "," + a.mandal + "," + d.districtName
                           }).ToList();

            ViewBag.UpdateStatus = false;
            DateTime TodayDateNew = DateTime.Now;
            DateTime CloseingDate = new DateTime(2019, 12, 16, 17, 00, 00);
            if (TodayDateNew > CloseingDate)
            {
                ViewBag.UpdateStatus = true;
                TempData["UpdateStatus"] = "Updated";
            }

            College_Dachboard Dashboard = new College_Dachboard();
            Dashboard.collegeId = college.id;
            Dashboard.CollegeCode = college.collegeCode;
            Dashboard.CollegeName = college.collegeName;
            Dashboard.College_eamsetCode = college.eamcetCode;
            Dashboard.SocietyName = jntuh_college_establishment.societyName;
            Dashboard.Address = address.Select(e => e.Collegeaddress).FirstOrDefault();

            // Dashboard.ChairmanPhoto = ;
            //Dashboard.Principal = Principal == null ? null : Principal.FirstName + " " + Principal.MiddleName + " " + Principal.LastName;
            //Dashboard.PrincipalRegNo = Principal == null ? null : Principal.RegistrationNumber;
            //Dashboard.PrincipalPhoto = Principal == null ? null : Principal.Photo;
            //Dashboard.PrincipalMobile = Principal == null ? null : Principal.Mobile;
            //Dashboard.PrincipalEmail = Principal == null ? null : Principal.Email;

            Dashboard.Director = college_Director == null ? null : college_Director.firstName + " " + college_Director.lastName + " " + college_Director.surname;
            Dashboard.DirectorEmail = college_Director == null ? null : college_Director.email;
            Dashboard.DirectorMobile = college_Director == null ? null : college_Director.mobile;
            Dashboard.DirectorPhoneNo = college_Director == null ? null : college_Director.landline;

            Dashboard.CollegeEmail = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.email).FirstOrDefault();
            Dashboard.CollegePhoneNo = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.landline).FirstOrDefault();
            Dashboard.CollegeMobile = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.mobile).FirstOrDefault();
            Dashboard.College_Website = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.website).FirstOrDefault();

            Dashboard.SocietyEmail = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.email).FirstOrDefault();
            Dashboard.SocietyPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SocietyMobile = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.mobile).FirstOrDefault();

            Dashboard.Chairman = college_chairman == null ? null : college_chairman.firstName + " " + college_chairman.lastName + " " + college_chairman.surname;
            Dashboard.SecretaryEmail = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.email).FirstOrDefault();
            Dashboard.SecretaryPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SecretaryMobile = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.mobile).FirstOrDefault();


            var jntuh_degree_Department = (from de in db.jntuh_degree
                                           join d in db.jntuh_department on de.id equals d.degreeId
                                           where de.isActive == true && d.isActive == true
                                           select new
                                           {
                                               degeeid = de.id,
                                               degee = de.degree,
                                               did = d.id,
                                               dept = d.departmentName
                                           }).ToList();

            List<DeptWise_Faculty> count_faculty = new List<DeptWise_Faculty>();

            foreach (var item in DeptIDs)
            {
                DeptWise_Faculty dept = new DeptWise_Faculty();
                dept.Deptid = item;
                dept.Deptname = UAAAS.Models.Utilities.EncryptString(dept.Deptid.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]);
                dept.Department = jntuh_degree_Department.Where(a => a.did == item).Select(e => e.degee + "-" + e.dept).FirstOrDefault();
                dept.FacultyCount = college_Faculty.Where(e => e.DepartmentId == item).Select(e => e.RegistrationNumber).Count();
                count_faculty.Add(dept);
            }

            Dashboard.DeptWise_FacultyCount = count_faculty;

            #region CollegeNews

            //Get current Date and time
            DateTime dateTime = DateTime.Now.Date;

            //Get bulletin board events based on start date and end date
            List<CollegeDashboardNews> news = db.jntuh_newsevents.Where(n => n.isActive == true &&
                                                            n.isNews == false &&
                                                            (n.startDate == null || n.startDate <= dateTime) &&
                                                            (n.endDate == null || n.endDate >= dateTime))
                                                            .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(n => n.createdDate).ToList();

            //Get College News and events based on start date and end date
            List<CollegeDashboardNews> cNews = db.jntuh_college_news.Where(collegeNews => collegeNews.isActive == true &&
                                                              collegeNews.collegeId == userCollegeID &&
                                                              (collegeNews.startDate == null ||
                                                              collegeNews.startDate <= dateTime) &&
                                                              (collegeNews.endDate == null ||
                                                              collegeNews.endDate >= dateTime)
                                                              && collegeNews.title != "DEFICIENCY REPORT AS PER FORM 415"
                                                              )
                                                              .Select(n => new CollegeDashboardNews { url = n.navigateURL, newstitle = n.title, createdDate = n.createdOn }).OrderByDescending(collegeNews => collegeNews.createdDate).ToList();


            CollegeDashboardNews cn = new CollegeDashboardNews();

            //var submittedIds = db.college_circulars.Where(cs => cs.isActive == true).Select(cs => cs.collegeId).ToList();

            //if (submittedIds.Contains(userCollegeID))
            //{
            //    cn.url = "/Content/Upload/News/AcknowledgementforBoth.pdf";
            //    cn.newstitle = "Acknowledgement for PG Courses submitted - Both Soft and Hard Copies";
            //    cn.createdDate = Convert.ToDateTime("09/04/2014");

            //    cNews.Add(cn);
            //}
            //else
            //{
            var notSubmittedIds = db.jntuh_college_pgcourses.Where(pg => pg.isActive == true).Select(pg => pg.collegeId).ToList();

            if (notSubmittedIds.Contains(userCollegeID))
            {
                cn.url = "/Content/Upload/News/OnlySoftCopySubmitted.pdf";
                cn.newstitle = "Acknowledgement for PG Courses submitted - Soft Copy only";
                cn.createdDate = Convert.ToDateTime("09/04/2014");

                cNews.Add(cn);
            }
            // }

            ViewBag.Events = news.Union(cNews).OrderByDescending(collegeNews => collegeNews.createdDate).Take(10);
            ViewBag.Affiliationfree = db.jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.afrcFee).FirstOrDefault();
            #endregion

            #region College Photos
            var college_photos = db.jntuh_college_document.Where(e => e.collegeId == userCollegeID).Select(e => e.scannedCopy).ToList();
            Dashboard.CollegePhotos = college_photos;

            #endregion

            #region College BAS Reports Student and Faculty

            List<BASMonthlyReports> monthlybasreports =
                db.jntuh_college_monthlybasreports.Where(m => m.collegeid == userCollegeID && m.isactive == true)
                .Select(a => new BASMonthlyReports
                {
                    title = a.title,
                    path = a.path,
                    createdDate = a.createdon
                }).OrderByDescending(s => s.createdDate).ToList();
            ViewBag.collegebasreports = monthlybasreports;

            #endregion

            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            int actualYear = jntuh_academic_year.Where(e => e.isActive = true && e.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();

            int PresentYear = jntuh_academic_year.Where(q => q.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(x => x.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(e => e.actualYear == (actualYear)).Select(x => x.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(e => e.actualYear == (actualYear - 1)).Select(x => x.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(e => e.actualYear == (actualYear - 2)).Select(x => x.id).FirstOrDefault();

            var jntuh__college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == userCollegeID).Select(e => e).ToList();

            var LastThreeData = jntuh__college_intake_existing.Where(e => e.academicYearId == AY2 || e.academicYearId == AY3 || e.academicYearId == AY4).Select(e => e).ToList();

            int[] YearIds = new int[] { AY2, AY3, AY4 };

            List<Academic_Year_Student_Count> Student_Count = new List<Academic_Year_Student_Count>();

            //Last 3 Years Intake Data....
            //foreach (var item in YearIds)
            //{
            //    Academic_Year_Student_Count student = new Academic_Year_Student_Count();
            //    student.Year = jntuh_academic_year.Where(e => e.id == item).Select(e => e.academicYear).FirstOrDefault();
            //    student.Approved_Count = jntuh__college_intake_existing.Where(s => s.academicYearId == item).Select(e => e.approvedIntake).Sum();
            //    student.Admitted_Count = jntuh__college_intake_existing.Where(s => s.academicYearId == item).Select(e => e.admittedIntake).Sum();
            //    Student_Count.Add(student);
            //}

            var Present_Year_Data = jntuh__college_intake_existing.Where(e => e.academicYearId == AY1).Select(e => e).ToList();

            var Departments = db.jntuh_department.ToList();
            var Specializations = db.jntuh_specialization.ToList();

            foreach (var item in Present_Year_Data)
            {
                Academic_Year_Student_Count Intake = new Academic_Year_Student_Count();
                Intake.CourseName = Specializations.Where(e => e.id == item.specializationId).Select(e => e.jntuh_department.jntuh_degree.degree + "-" + e.specializationName).FirstOrDefault();
                Intake.ApprovedIntake = item.approvedIntake;
                Student_Count.Add(Intake);
            }

            Dashboard.YearWise_Student_Count = Student_Count;
            //Links Opening Part string.Format("/{0}/{1}", menu.menuControllerName, menu.menuActionName)
            DateTime todayDate = DateTime.Now;
            List<actionlinks> actionlinks = (from li in db.jntuh_link_screens
                                             join ls in db.jntuh_college_links_assigned on li.id equals ls.linkId
                                             join lm in db.jntuh_menu on li.linkName equals lm.menuName
                                             where ls.isActive == true && lm.isActive == true && ls.fromdate <= todayDate && ls.todate >= todayDate
                                             select new actionlinks
                                             {
                                                 Id = lm.id,
                                                 ParentId = (int)lm.menuParentID,
                                                 Text = lm.menuName,
                                                 controller = lm.menuControllerName,
                                                 action = lm.menuActionName
                                             }).OrderByDescending(i => i.Id).ToList();

            ViewBag.Openliks = actionlinks;

            var AffiliationFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 5 && e.academicyearId == (PresentYear - 2)).Select(e => e.duesAmount).FirstOrDefault();
            var CommanserviceFee1920 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 3 && e.academicyearId == (PresentYear - 1)).Select(e => e.duesAmount).FirstOrDefault();

            var AffiliationFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 5 && e.academicyearId == (PresentYear)).Select(e => e.duesAmount).FirstOrDefault();
            var CommanserviceFee = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 3 && e.academicyearId == (PresentYear)).Select(e => e.duesAmount).FirstOrDefault();
            var AffiliationFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 5 && e.academicyearId == (PresentYear - 2)).Select(e => e.duesAmount).FirstOrDefault();
            ViewBag.AffiliationFeeDues2022_23 = AffiliationFeeDues2022_23;

            var CmnSerFeeDues2022_23 = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 3 && e.academicyearId == (PresentYear - 2)).Select(e => e.duesAmount).FirstOrDefault();
            ViewBag.CSCFeeDues2022_23 = CmnSerFeeDues2022_23;
            //var gstnotpaidclgs = new int[] { 4, 9, 26, 40, 68, 106, 141, 148, 156, 166, 183, 188, 228, 300, 334, 373, 380, 423 };
            //if (!string.IsNullOrEmpty(AffiliationFeeDues2022_23))
            //{
            //    var coversionGST = Convert.ToDecimal(AffiliationFeeDues2022_23);
            //    ViewBag.GSTAmount = coversionGST > 0 ? Math.Round((coversionGST * 18) / 100) : 0;
            //}
            //if (gstnotpaidclgs.Contains(userCollegeID))
            //{
            //    ViewBag.GSTAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 5 && e.academicyearId == (PresentYear - 1)).Select(e => e.gstamount).FirstOrDefault();
            //}
            ViewBag.GSTAmount = db.jntuh_college_paymentoffee.Where(e => e.collegeId == userCollegeID && e.FeeTypeID == 5 && e.academicyearId == (PresentYear - 2)).Select(e => e.gstamount).FirstOrDefault();
            if (AffiliationFee != null || AffiliationFee1920 != null)
            {
                decimal Aff1920amount = !string.IsNullOrEmpty(AffiliationFee1920) ? Convert.ToDecimal(AffiliationFee1920) : 0;
                decimal amount = !string.IsNullOrEmpty(AffiliationFee) ? Convert.ToDecimal(AffiliationFee) : 0;
                var totalamount = Aff1920amount + amount;
                if (totalamount >= 0)
                    ViewBag.AffiliationFee = totalamount;
                else
                    ViewBag.AffiliationFee = 0;
            }
            else
            {
                ViewBag.AffiliationFee = 0;
            }
            if (CommanserviceFee != null || CommanserviceFee1920 != null)
            {
                decimal Cmn1920amount = !string.IsNullOrEmpty(CommanserviceFee1920) ? Convert.ToDecimal(CommanserviceFee1920) : 0;
                decimal amount = !string.IsNullOrEmpty(CommanserviceFee) ? Convert.ToDecimal(CommanserviceFee) : 0;
                var totalamount = Cmn1920amount + amount;
                if (totalamount >= 0)
                    ViewBag.CommanserviceFee = totalamount;
                else
                    ViewBag.CommanserviceFee = 0;
            }
            else
            {
                ViewBag.CommanserviceFee = 0;
            }
            //ViewBag.CommanserviceFee = CommanserviceFee;
            TempData["ServerUrl"] = serverURL;

            return View(Dashboard);
        }

        [HttpGet]
        [Authorize(Roles = "College,Admin")]
        public ActionResult UpdateCollegeDiary(string command)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (User.IsInRole("Admin"))
            {
                adminCollegeId = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.id).FirstOrDefault();
                userCollegeID = adminCollegeId;
            }

            DateTime TodayDateNew = DateTime.Now;
            DateTime CloseingDate = new DateTime(2019, 12, 16, 17, 00, 00);
            if (TodayDateNew > CloseingDate)
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            var college = db.jntuh_college.Where(c => c.id == userCollegeID).Select(e => e).FirstOrDefault();
            var jntuh_college_address = db.jntuh_address.Where(e => e.collegeId == college.id).Select(e => e).ToList();

            if (command == "NoChange")
            {
                var addres = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e).FirstOrDefault();
                addres.isUpdated = true;
                addres.updatedOn = DateTime.Now;
                addres.updatedBy = userID;
                db.Entry(addres).State = EntityState.Modified;
                db.SaveChanges();


                var Society = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e).FirstOrDefault();
                Society.isUpdated = true;
                Society.updatedOn = DateTime.Now;
                Society.updatedBy = userID;
                db.Entry(Society).State = EntityState.Modified;
                db.SaveChanges();


                var secretaryn = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e).FirstOrDefault();
                secretaryn.isUpdated = true;
                secretaryn.updatedOn = DateTime.Now;
                secretaryn.updatedBy = userID;
                db.Entry(secretaryn).State = EntityState.Modified;
                db.SaveChanges();

                TempData["UpdateStatus"] = "Updated";

                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            ViewBag.Districts = db.jntuh_district.Where(e => e.isActive == true).Select(w => new { Did = w.id, district = w.districtName }).ToList();

            var college_chairman = db.jntuh_college_chairperson.Where(w => w.collegeId == college.id).Select(e => e).FirstOrDefault();

            var college_Director = db.jntuh_college_principal_director.Where(w => w.collegeId == college.id && w.type == "DIRECTOR").Select(e => e).FirstOrDefault();

            var college_Principal = db.jntuh_college_principal_registered.Where(w => w.collegeId == college.id).Select(e => e.RegistrationNumber).FirstOrDefault();

            //var Principal = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == college_Principal).Select(e => e).FirstOrDefault();
            var Principal = db.jntuh_college_principal_director.Where(w => w.collegeId == college.id && w.type == "PRINCIPAL").Select(e => e).FirstOrDefault();


            //var UpdatedDate = jntuh_college_address.OrderByDescending(w => w.updatedOn).Select(r=>r.updatedOn).FirstOrDefault();           
            //DateTime todayDate = new DateTime(2019,12,03,17,00,00);
            //if(UpdatedDate > todayDate)
            //{
            //    return RedirectToAction("CollegeDashboard","Dashboard");
            //}

            var jntuh_college_establishment = db.jntuh_college_establishment.Where(e => e.collegeId == college.id).Select(e => e).FirstOrDefault();

            var address = (from a in jntuh_college_address
                           join d in db.jntuh_district.ToList() on a.districtId equals d.id
                           where a.addressTye == "COLLEGE"
                           select new
                           {
                               Collegeaddress = a.address + "," + a.mandal + "," + d.districtName
                           }).ToList();

            DiaryDetails Dashboard = new DiaryDetails();
            Dashboard.collegeId = college.id;
            Dashboard.CollegeCode = college.collegeCode;
            Dashboard.CollegeName = college.collegeName;
            Dashboard.CollegeAddress = address.Select(a => a.Collegeaddress).FirstOrDefault();
            Dashboard.College_eamsetCode = college.eamcetCode;
            Dashboard.SocietyName = jntuh_college_establishment.societyName;

            Dashboard.Director = college_Director == null ? null : college_Director.firstName + " " + college_Director.lastName + " " + college_Director.surname;
            Dashboard.DirectorEmail = college_Director == null ? null : college_Director.email;
            Dashboard.DirectorMobile = college_Director == null ? null : college_Director.mobile;
            Dashboard.DirectorPhoneNo = college_Director == null ? null : college_Director.landline;
            Dashboard.DirectorFax = college_Director == null ? null : college_Director.fax;

            Dashboard.Principal = Principal == null ? null : Principal.firstName + " " + Principal.lastName + " " + Principal.surname;
            Dashboard.PrincipalFirstName = Principal == null ? null : Principal.firstName;
            Dashboard.PrincipalMiddleName = Principal == null ? null : Principal.lastName;
            Dashboard.PrincipalLastName = Principal == null ? null : Principal.surname;
            Dashboard.PrincipalRegNo = Principal == null ? null : Principal.photo;
            Dashboard.PrincipalEmail = Principal == null ? null : Principal.email;
            Dashboard.PrincipalMobile = Principal == null ? null : Principal.mobile;

            Dashboard.CollegeEmail = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.email).FirstOrDefault();
            Dashboard.CollegePhoneNo = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.landline).FirstOrDefault();
            Dashboard.CollegeMobile = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.mobile).FirstOrDefault();
            Dashboard.CollegetownOrCity = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.townOrCity).FirstOrDefault();
            Dashboard.CollegeMandal = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.mandal).FirstOrDefault();
            Dashboard.CollegeDistrictID = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.districtId).FirstOrDefault();

            Dashboard.SocietyEmail = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.email).FirstOrDefault();
            Dashboard.SocietyPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SocietyMobile = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.mobile).FirstOrDefault();
            Dashboard.SocietytownOrCity = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.townOrCity).FirstOrDefault();
            Dashboard.SocietyMandal = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.mandal).FirstOrDefault();
            Dashboard.SocietyDistrictID = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.districtId).FirstOrDefault();

            Dashboard.Chairman = college_chairman == null ? null : college_chairman.firstName + " " + college_chairman.lastName + " " + college_chairman.surname;
            Dashboard.SecretaryEmail = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.email).FirstOrDefault();
            Dashboard.SecretaryPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SecretaryMobile = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.mobile).FirstOrDefault();
            Dashboard.SecretarytownOrCity = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.townOrCity).FirstOrDefault();
            Dashboard.SecretaryMandal = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.mandal).FirstOrDefault();
            Dashboard.SecretaryDistrictID = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.districtId).FirstOrDefault();

            return View(Dashboard);
        }

        [HttpPost]
        [Authorize(Roles = "College,Admin")]
        public ActionResult UpdateCollegeDiary(DiaryDetails Diary)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();

            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            if (User.IsInRole("Admin"))
            {
                adminCollegeId = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.id).FirstOrDefault();
                userCollegeID = adminCollegeId;
            }

            DateTime TodayDateNew = DateTime.Now;
            DateTime CloseingDate = new DateTime(2019, 12, 16, 17, 00, 00);
            if (TodayDateNew > CloseingDate)
            {
                return RedirectToAction("CollegeDashboard", "Dashboard");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var CollegeData = db.jntuh_college.Where(q => q.id == userCollegeID).Select(t => t).FirstOrDefault();
                    CollegeData.eamcetCode = Diary.College_eamsetCode;
                    db.Entry(CollegeData).State = EntityState.Modified;
                    db.SaveChanges();

                    var jntuh_college_address = db.jntuh_address.Where(e => e.collegeId == userCollegeID).Select(e => e).ToList();

                    var CollegeAddress = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e).FirstOrDefault();
                    CollegeAddress.email = Diary.CollegeEmail;
                    CollegeAddress.landline = Diary.CollegePhoneNo;
                    CollegeAddress.mobile = Diary.CollegeMobile;
                    CollegeAddress.mandal = Diary.CollegeMandal;
                    CollegeAddress.townOrCity = Diary.CollegetownOrCity;
                    CollegeAddress.districtId = Diary.CollegeDistrictID;
                    CollegeAddress.isUpdated = false;
                    CollegeAddress.updatedOn = DateTime.Now;
                    CollegeAddress.updatedBy = userID;
                    db.Entry(CollegeAddress).State = EntityState.Modified;
                    db.SaveChanges();

                    var SOCIETYAddress = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e).FirstOrDefault();
                    SOCIETYAddress.email = Diary.SocietyEmail;
                    SOCIETYAddress.landline = Diary.SocietyPhoneNo;
                    SOCIETYAddress.mobile = Diary.SocietyMobile;
                    SOCIETYAddress.townOrCity = Diary.SocietytownOrCity;
                    SOCIETYAddress.mandal = Diary.SocietyMandal;
                    SOCIETYAddress.districtId = Diary.SocietyDistrictID;
                    SOCIETYAddress.isUpdated = false;
                    SOCIETYAddress.updatedOn = DateTime.Now;
                    SOCIETYAddress.updatedBy = userID;
                    db.Entry(SOCIETYAddress).State = EntityState.Modified;
                    db.SaveChanges();

                    var SECRETARYAddress = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e).FirstOrDefault();
                    SECRETARYAddress.email = Diary.SecretaryEmail;
                    SECRETARYAddress.landline = Diary.SecretaryPhoneNo;
                    SECRETARYAddress.mobile = Diary.SecretaryMobile;
                    SECRETARYAddress.townOrCity = Diary.SocietytownOrCity;
                    SECRETARYAddress.mandal = Diary.SecretaryMandal;
                    SECRETARYAddress.districtId = Diary.SecretaryDistrictID;
                    SECRETARYAddress.isUpdated = false;
                    SECRETARYAddress.updatedOn = DateTime.Now;
                    SECRETARYAddress.updatedBy = userID;
                    db.Entry(SECRETARYAddress).State = EntityState.Modified;
                    db.SaveChanges();

                    if (Diary.PrincipalRegNo != null)
                    {
                        var PrinicipalCheck = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type == "PRINCIPAL").Select(r => r).FirstOrDefault();
                        if (PrinicipalCheck == null)
                        {
                            DateTime todayDateNew = new DateTime(2019, 12, 12, 20, 00, 00);
                            jntuh_college_principal_director Prin = new jntuh_college_principal_director();
                            Prin.collegeId = userCollegeID;
                            Prin.type = "PRINCIPAL";
                            Prin.photo = Diary.PrincipalRegNo.Trim();
                            var RegFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == Prin.photo).Select(r => r).FirstOrDefault();
                            Prin.firstName = RegFaculty.FirstName;
                            Prin.lastName = string.IsNullOrEmpty(RegFaculty.MiddleName) ? null : RegFaculty.MiddleName;
                            Prin.surname = RegFaculty.LastName;
                            Prin.email = RegFaculty.Email;
                            Prin.mobile = RegFaculty.Mobile;
                            Prin.qualificationId = 1;
                            Prin.dateOfAppointment = Convert.ToDateTime(todayDateNew.ToString());
                            Prin.dateOfBirth = Convert.ToDateTime(todayDateNew.ToString());
                            Prin.fax = "000000000";
                            Prin.landline = "000000000";
                            Prin.isRatified = true;
                            Prin.createdBy = userID;
                            Prin.createdOn = DateTime.Now;
                            db.jntuh_college_principal_director.Add(Prin);
                            db.SaveChanges();
                        }
                        else if (PrinicipalCheck.id != 0 && PrinicipalCheck.photo.Trim() == Diary.PrincipalRegNo.Trim())
                        {
                            PrinicipalCheck.updatedBy = userID;
                            PrinicipalCheck.updatedOn = DateTime.Now;
                            db.Entry(PrinicipalCheck).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else if (PrinicipalCheck.id != 0 && PrinicipalCheck.photo.Trim() != Diary.PrincipalRegNo.Trim())
                        {
                            var PrinicipalCheckNew = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type == "PRINCIPAL").Select(r => r).FirstOrDefault();
                            db.jntuh_college_principal_director.Remove(PrinicipalCheckNew);
                            db.SaveChanges();

                            DateTime todayDateNew = new DateTime(2019, 12, 12, 20, 00, 00);
                            jntuh_college_principal_director Prin = new jntuh_college_principal_director();
                            Prin.collegeId = userCollegeID;
                            Prin.type = "PRINCIPAL";
                            Prin.photo = Diary.PrincipalRegNo.Trim();
                            var RegFaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == Prin.photo).Select(r => r).FirstOrDefault();
                            Prin.firstName = RegFaculty.FirstName;
                            Prin.lastName = string.IsNullOrEmpty(RegFaculty.MiddleName) ? "No Middle Name" : RegFaculty.MiddleName;
                            Prin.surname = RegFaculty.LastName;
                            Prin.email = RegFaculty.Email;
                            Prin.mobile = RegFaculty.Mobile;
                            Prin.qualificationId = 1;
                            Prin.dateOfAppointment = Convert.ToDateTime(todayDateNew.ToString());
                            Prin.dateOfBirth = Convert.ToDateTime(todayDateNew.ToString());
                            Prin.fax = "000000000";
                            Prin.landline = "000000000";
                            Prin.isRatified = true;
                            Prin.createdBy = userID;
                            Prin.createdOn = DateTime.Now;
                            db.jntuh_college_principal_director.Add(Prin);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var PrinicipalCheckNew = db.jntuh_college_principal_director.Where(e => e.collegeId == userCollegeID && e.type == "PRINCIPAL").Select(r => r).FirstOrDefault();
                        if (PrinicipalCheckNew == null)
                        {

                        }
                        else
                        {
                            db.jntuh_college_principal_director.Remove(PrinicipalCheckNew);
                            db.SaveChanges();
                        }

                    }

                    if (Diary.Director != null)
                    {
                        var college_Director = db.jntuh_college_principal_director.Where(w => w.collegeId == userCollegeID && w.type == "DIRECTOR").Select(e => e).FirstOrDefault();
                        college_Director.email = Diary.DirectorEmail;
                        college_Director.mobile = Diary.DirectorMobile;
                        college_Director.landline = Diary.DirectorPhoneNo;
                        college_Director.updatedOn = DateTime.Now;
                        college_Director.updatedBy = userID;
                        db.Entry(college_Director).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    TempData["SUCCESS"] = "Diary Information is Updated.";
                    return RedirectToAction("UpdateCollegeDiary", "Dashboard");
                }
                catch (Exception ex)
                {
                    TempData["ERROR"] = "Something went wrong, Try Again.." + ex.Message;
                    return RedirectToAction("UpdateCollegeDiary", "Dashboard");
                }
            }
            else
            {
                var errors = ModelState.Where(v => v.Value.Errors.Any());
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ERROR"] = "Required Fields are Mandatory..";
                return RedirectToAction("UpdateCollegeDiary", "Dashboard");
            }
        }

        [HttpPost]
        public JsonResult GetRegData(string PrincipalRegNo)
        {
            var CheckingRegistrationNumber = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == PrincipalRegNo.Trim()).Select(e => e).FirstOrDefault();
            if (CheckingRegistrationNumber == null)
            {
                return Json("Registration Number is not Exists.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var PrinData = new
                {
                    FirstName = CheckingRegistrationNumber.FirstName,
                    MiddleName = CheckingRegistrationNumber.MiddleName,
                    LastName = CheckingRegistrationNumber.LastName,
                    email = CheckingRegistrationNumber.Email,
                    mobile = CheckingRegistrationNumber.Mobile
                };
                return Json(new { Data = PrinData }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult eachCollegediarydata(int Cid)
        {
            var college = db.jntuh_college.Where(c => c.id == Cid).Select(e => e).FirstOrDefault();

            var college_Director = db.jntuh_college_principal_director.Where(w => w.collegeId == college.id && w.type == "DIRECTOR").Select(e => e).FirstOrDefault();

            var college_Principal = db.jntuh_college_principal_registered.Where(w => w.collegeId == college.id).Select(e => e.RegistrationNumber).FirstOrDefault();

            var Principal = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber == college_Principal).Select(e => e).FirstOrDefault();

            var jntuh_college_address = db.jntuh_address.Where(e => e.collegeId == college.id).Select(e => e).ToList();

            var jntuh_college_establishment = db.jntuh_college_establishment.Where(e => e.collegeId == college.id).Select(e => e).FirstOrDefault();
            var address = (from a in jntuh_college_address
                           join d in db.jntuh_district.ToList() on a.districtId equals d.id
                           where a.addressTye == "COLLEGE"
                           select new
                           {
                               Collegeaddress = a.address + "," + a.mandal + "," + d.districtName
                           }).ToList();

            DiaryDetails Dashboard = new DiaryDetails();
            Dashboard.collegeId = college.id;
            Dashboard.CollegeCode = college.collegeCode;
            Dashboard.CollegeName = college.collegeName;
            Dashboard.CollegeAddress = address.Select(e => e.Collegeaddress).FirstOrDefault();

            Dashboard.Director = college_Director == null ? null : college_Director.firstName + " " + college_Director.lastName + " " + college_Director.surname;
            Dashboard.DirectorEmail = college_Director == null ? null : college_Director.email;
            Dashboard.DirectorMobile = college_Director == null ? null : college_Director.mobile;
            Dashboard.DirectorPhoneNo = college_Director == null ? null : college_Director.landline;

            Dashboard.Principal = Principal == null ? null : Principal.FirstName + " " + Principal.MiddleName + " " + Principal.LastName;
            Dashboard.PrincipalRegNo = Principal == null ? null : Principal.RegistrationNumber;
            Dashboard.PrincipalEmail = Principal == null ? null : Principal.Email;
            Dashboard.PrincipalMobile = Principal == null ? null : Principal.Mobile;

            Dashboard.CollegeEmail = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.email).FirstOrDefault();
            Dashboard.CollegePhoneNo = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.landline).FirstOrDefault();
            Dashboard.CollegeMobile = jntuh_college_address.Where(e => e.addressTye == "COLLEGE").Select(e => e.mobile).FirstOrDefault();
            // Dashboard.CollegeDistrictID = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.districtId).FirstOrDefault();
            // Dashboard.CollegeDistrict = db.jntuh_district.Where(e => e.isActive == true && e.id == Dashboard.CollegeDistrictID).Select(w =>w.districtName).FirstOrDefault();

            Dashboard.SocietyName = jntuh_college_establishment == null ? null : jntuh_college_establishment.societyName;
            Dashboard.SocietyEmail = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.email).FirstOrDefault();
            Dashboard.SocietyPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SocietyMobile = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.mobile).FirstOrDefault();
            // Dashboard.SocietyDistrictID = jntuh_college_address.Where(e => e.addressTye == "SOCIETY").Select(e => e.districtId).FirstOrDefault();
            // Dashboard.SocietyDistrict = db.jntuh_district.Where(e => e.isActive == true && e.id == Dashboard.CollegeDistrictID).Select(w => w.districtName).FirstOrDefault();

            var Chairman = db.jntuh_college_chairperson.Where(w => w.collegeId == college.id).Select(w => w).FirstOrDefault();
            Dashboard.Chairman = Chairman == null ? null : Chairman.firstName + " " + Chairman.lastName + " " + Chairman.surname;
            Dashboard.SecretaryEmail = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.email).FirstOrDefault();
            Dashboard.SecretaryPhoneNo = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.landline).FirstOrDefault();
            Dashboard.SecretaryMobile = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.mobile).FirstOrDefault();
            // Dashboard.SecretaryDistrictID = jntuh_college_address.Where(e => e.addressTye == "SECRETARY").Select(e => e.districtId).FirstOrDefault();
            // Dashboard.SecretaryDistrict = db.jntuh_district.Where(e => e.isActive == true && e.id == Dashboard.CollegeDistrictID).Select(w => w.districtName).FirstOrDefault();
            return PartialView("~/Views/Dashboard/eachCollegediarydata.cshtml", Dashboard);
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult GetDepartmentFaculty(string DeptNAME)
        {
            int userCollegeID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int CollegeId = db.jntuh_college_users.Where(e => e.userID == userCollegeID).Select(e => e.collegeID).FirstOrDefault();

            List<CollegeFacultyNew> FacultyList = new List<CollegeFacultyNew>();

            // var CollegeId = db.jntuh_college.Where(e=>e.collegeCode == collegecode).Select(e=>e.id).FirstOrDefault();
            int DeptID = 0;
            if (!string.IsNullOrEmpty(DeptNAME))
            {
                DeptID = Convert.ToInt32((UAAAS.Models.Utilities.DecryptString(DeptNAME,
                               WebConfigurationManager.AppSettings["CryptoKey"])));
            }
            if (CollegeId == 375)
            {
                CollegeId = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }


            if (CollegeId != null && DeptID != 0)
            {
                var college_Faculty = db.jntuh_college_faculty_registered.Where(w => w.collegeId == CollegeId && w.DepartmentId == DeptID).Select(e => e).ToList();

                var RegNos = college_Faculty.Select(e => e.RegistrationNumber).ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(s => RegNos.Contains(s.RegistrationNumber)).Select(e => e).ToList();

                var Departments = db.jntuh_department.ToList();
                var designation = db.jntuh_designation.ToList();
                var Specializations = db.jntuh_specialization.ToList();



                foreach (var item in college_Faculty)
                {
                    CollegeFacultyNew Faculty = new CollegeFacultyNew();
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.Name = registeredFaculty.Where(e => e.RegistrationNumber == item.RegistrationNumber).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault();
                    Faculty.DesignationId = registeredFaculty.Where(e => e.RegistrationNumber == item.RegistrationNumber).Select(e => e.DesignationId).FirstOrDefault();
                    if (Faculty.DesignationId != null)
                    {
                        Faculty.Designation = designation.Where(e => e.id == Faculty.DesignationId).Select(e => e.designation).FirstOrDefault();
                    }
                    else
                    {
                        Faculty.Designation = "";
                    }
                    Faculty.Department = Departments.Where(e => e.id == item.DepartmentId).Select(w => w.jntuh_degree.degree + "-" + w.departmentName).FirstOrDefault();
                    Faculty.Specialization = Specializations.Where(a => a.id == item.SpecializationId).Select(s => s.specializationName).FirstOrDefault();
                    FacultyList.Add(Faculty);
                }

            }
            else if (CollegeId != null && DeptID == 0)
            {
                var college_Faculty = db.jntuh_college_faculty_registered.Where(w => w.collegeId == CollegeId && w.DepartmentId == null).Select(e => e).ToList();

                var RegNos = college_Faculty.Select(e => e.RegistrationNumber).ToList();

                var registeredFaculty = db.jntuh_registered_faculty.Where(s => RegNos.Contains(s.RegistrationNumber)).Select(e => e).ToList();

                //List<CollegeFacultyNew> FacultyList = new List<CollegeFacultyNew>();

                foreach (var item in college_Faculty)
                {
                    CollegeFacultyNew Faculty = new CollegeFacultyNew();
                    Faculty.RegistrationNumber = item.RegistrationNumber;
                    Faculty.Name = registeredFaculty.Where(e => e.RegistrationNumber == item.RegistrationNumber).Select(e => e.FirstName + " " + e.LastName).FirstOrDefault();
                    Faculty.Department = null;
                    Faculty.Specialization = null;
                    FacultyList.Add(Faculty);
                }

            }

            return View(FacultyList);
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult Get_Intake_Details()
        {
            int userCollegeID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            int CollegeId = db.jntuh_college_users.Where(e => e.userID == userCollegeID).Select(e => e.collegeID).FirstOrDefault();

            //var CollegeId = db.jntuh_college.Where(e => e.collegeCode == collegecode).Select(e => e.id).FirstOrDefault();

            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            int actualYear = jntuh_academic_year.Where(e => e.isActive = true && e.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();

            int PresentYear = jntuh_academic_year.Where(q => q.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();

            int AY1 = jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(x => x.id).FirstOrDefault();
            int AY2 = jntuh_academic_year.Where(e => e.actualYear == (actualYear)).Select(x => x.id).FirstOrDefault();
            int AY3 = jntuh_academic_year.Where(e => e.actualYear == (actualYear - 1)).Select(x => x.id).FirstOrDefault();
            int AY4 = jntuh_academic_year.Where(e => e.actualYear == (actualYear - 2)).Select(x => x.id).FirstOrDefault();

            var jntuh__college_intake_existing = db.jntuh_college_intake_existing.Where(e => e.collegeId == CollegeId).Select(e => e).ToList();

            var LastThreeData = jntuh__college_intake_existing.Where(e => e.academicYearId == AY2 || e.academicYearId == AY3 || e.academicYearId == AY4).Select(e => e).ToList();

            int[] YearIds = new int[] { AY2, AY3, AY4 };

            List<Academic_Year_Student_Count> Student_Count = new List<Academic_Year_Student_Count>();


            var Present_Year_Data = jntuh__college_intake_existing.Where(e => e.academicYearId == AY1).Select(e => e).ToList();

            var Departments = db.jntuh_department.ToList();
            var Specializations = db.jntuh_specialization.ToList();

            foreach (var item in Present_Year_Data)
            {
                Academic_Year_Student_Count Intake = new Academic_Year_Student_Count();
                Intake.CourseName = Specializations.Where(e => e.id == item.specializationId).Select(e => e.jntuh_department.jntuh_degree.degree + "-" + e.specializationName).FirstOrDefault();
                Intake.ApprovedIntake = item.approvedIntake;
                Student_Count.Add(Intake);
            }

            //Dashboard.YearWise_Student_Count = Student_Count;

            return View(Student_Count);
        }

        //Get total students passed and placed based on academic Year Id
        private decimal? GetStudents(int collegeId, int academicYearId, int specializationId, int flag)
        {
            decimal? student = 0;
            if (flag == 1)
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPassed == null ? 0 : i.totalStudentsPassed.Value).FirstOrDefault();
            else
                student = db.jntuh_college_placement.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId).Select(i => i.totalStudentsPlaced == null ? 0 : i.totalStudentsPlaced.Value).FirstOrDefault();
            return student == null ? (int?)null : Convert.ToInt32(student);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Admin(string exportType, string YearId)
        {
            //int InspectionPhaseId = db.jntuh_inspection_phase.Where(p => p.isActive == true).Select(p => p.id).SingleOrDefault();

            AdminDashbord adminDashbord = new AdminDashbord();
            //Colleges/SentMails
            adminDashbord.totalColleges = db.jntuh_college.Select(c => c.id).ToList().Count();
            adminDashbord.activeColleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToList().Count();
            adminDashbord.inActiveColleges = db.jntuh_college.Where(c => c.isActive == false && c.isClosed == false).Select(c => c.id).ToList().Count();
            adminDashbord.permanentColleges = db.jntuh_college.Where(c => c.isActive == true && c.isPermant == true).Select(c => c.id).ToList().Count();
            adminDashbord.newColleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c.id).ToList().Count();
            adminDashbord.closerColleges = db.jntuh_college.Where(c => c.isActive == false && c.isClosed == true).Select(c => c.id).ToList().Count();
            adminDashbord.AutonomousColleges = db.jntuh_college_affiliation.Where(s => s.affiliationTypeId == 7 && s.affiliationStatus == "Yes").Select(e => e).Count();

            //Degree wise intake Present AY
            var jntuh_academic_year = db.jntuh_academic_year.ToList();

            int ActualYearAcademicYearID = jntuh_academic_year.Where(ay => ay.isActive == true && ay.isPresentAcademicYear == true).Select(ay => ay.id).FirstOrDefault();
            int PresentAcademicYearId = 0;
            if (YearId != null)
            {
                PresentAcademicYearId = jntuh_academic_year.Where(ay => ay.academicYear == YearId).Select(ay => ay.id).FirstOrDefault();
            }
            else
            {
                PresentAcademicYearId = jntuh_academic_year.Where(ay => ay.id == (ActualYearAcademicYearID + 1)).Select(ay => ay.id).FirstOrDefault();
            }


            ViewBag.AcademicYear1 = jntuh_academic_year.Where(ay => ay.id == PresentAcademicYearId).Select(ay => ay.academicYear).FirstOrDefault();
            ViewBag.AcademicYear2 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 1)).Select(ay => ay.academicYear).FirstOrDefault();
            ViewBag.AcademicYear3 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 2)).Select(ay => ay.academicYear).FirstOrDefault();
            ViewBag.AcademicYear4 = jntuh_academic_year.Where(ay => ay.id == (PresentAcademicYearId - 3)).Select(ay => ay.academicYear).FirstOrDefault();

            var subColleges = db.jntuh_college_edit_status.Where(i => i.IsCollegeEditable == false && i.academicyearId == PresentAcademicYearId).Select(e => e.collegeId).ToList();
            //subColleges.Add(100);
            //subColleges.Add(297);
            //subColleges.Add(313);
            //jntuh_college_intake_existing table
            adminDashbord.degreewiseTotalIntake1 = (from i in db.jntuh_college_intake_existing
                                                    join s in db.jntuh_specialization on i.specializationId equals s.id
                                                    join de in db.jntuh_department on s.departmentId equals de.id
                                                    join d in db.jntuh_degree on de.degreeId equals d.id
                                                    //join c in db.jntuh_college on i.collegeId equals c.id
                                                    where (subColleges.Contains(i.collegeId) && i.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.academicYearId == PresentAcademicYearId && i.proposedIntake != 0)
                                                    orderby d.degreeDisplayOrder
                                                    group i by new
                                                    {
                                                        Id = d.id,
                                                        degree = d.degree,
                                                        degreeDisplayOrder = d.degreeDisplayOrder
                                                    } into g
                                                    select new DegreewiseTotalIntake
                                                    {
                                                        degreeId = g.Key.Id,
                                                        degree = g.Key.degree,
                                                        proposedIntake = g.Sum(a => a.proposedIntake),
                                                        admittedIntake = g.Sum(a => a.admittedIntake),
                                                        degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                                    }).OrderBy(a => a.degreeDisplayOrder).ToList();

            var SpecealizationsCount = (from i in db.jntuh_college_intake_existing
                                        join s in db.jntuh_specialization on i.specializationId equals s.id
                                        join de in db.jntuh_department on s.departmentId equals de.id
                                        join d in db.jntuh_degree on de.degreeId equals d.id
                                        join c in db.jntuh_college on i.collegeId equals c.id
                                        where (c.id != 375 && c.isActive == true && i.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.academicYearId == PresentAcademicYearId && i.proposedIntake != 0)
                                        orderby d.degreeDisplayOrder
                                        select new DegreewiseTotalIntake
                                                  {
                                                      degreeId = d.id,
                                                      departmentId = de.id,
                                                      SpecealizationId = s.id
                                                  });

            foreach (var item in adminDashbord.degreewiseTotalIntake1)
            {

                item.ProposedCourses = SpecealizationsCount.Count(s => s.degreeId == item.degreeId);

            }

            adminDashbord.Praposedintakedownload = (from i in db.jntuh_college_intake_existing
                                                    join es in db.jntuh_college_edit_status on i.collegeId equals es.collegeId
                                                    join s in db.jntuh_specialization on i.specializationId equals s.id
                                                    join de in db.jntuh_department on s.departmentId equals de.id
                                                    join d in db.jntuh_degree on de.degreeId equals d.id
                                                    join c in db.jntuh_college on i.collegeId equals c.id
                                                    join ad in db.jntuh_address on c.id equals ad.collegeId
                                                    join dd in db.jntuh_district on ad.districtId equals dd.id
                                                    where (subColleges.Contains(c.id) && c.id != 375 && c.isActive == true && i.isActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.academicYearId == PresentAcademicYearId && es.IsCollegeEditable == false && es.academicyearId == PresentAcademicYearId && i.proposedIntake != 0 && ad.addressTye == "COLLEGE")
                                                    orderby d.degreeDisplayOrder
                                                    select new DegreewiseTotalIntake
                                                    {
                                                        collegeCode = c.collegeCode,
                                                        collegeName = c.collegeName,
                                                        SpecealizationId = s.id,
                                                        degree = d.degree + "-" + s.specializationName,
                                                        proposedIntake = i.proposedIntake,
                                                        CourseStatus = i.courseStatus,
                                                        address = ad.address + " " + ad.mandal,
                                                        district = dd.districtName,
                                                        degreeDisplayOrder = (int)d.degreeDisplayOrder
                                                    }).Distinct().OrderBy(a => a.collegeCode).ToList();

            //jntuh_approvedadmitted_intake table
            var approvedadmitted_intake = (from i in db.jntuh_approvedadmitted_intake
                                           join s in db.jntuh_specialization on i.SpecializationId equals s.id
                                           join de in db.jntuh_department on s.departmentId equals de.id
                                           join d in db.jntuh_degree on de.degreeId equals d.id
                                           join c in db.jntuh_college on i.collegeId equals c.id
                                           where (c.id != 375 && c.isActive == true && i.IsActive == true && s.isActive == true && de.isActive == true && d.isActive == true && i.AcademicYearId == PresentAcademicYearId)
                                           orderby d.degreeDisplayOrder
                                           group i by new
                                           {
                                               degree = d.degree,
                                               degreeDisplayOrder = d.degreeDisplayOrder
                                           } into g
                                           select new DegreewiseTotalIntake
                                           {
                                               degree = g.Key.degree,
                                               approvedIntake = g.Sum(a => a.ApprovedIntake),
                                               admittedIntake = g.Sum(a => a.AdmittedIntake),
                                               degreeDisplayOrder = (int)g.Key.degreeDisplayOrder
                                           }).OrderBy(a => a.degreeDisplayOrder).ToList();

            foreach (var item in adminDashbord.degreewiseTotalIntake1)
            {
                item.approvedIntake = approvedadmitted_intake.Where(s => s.degree == item.degree).Select(s => s.approvedIntake).FirstOrDefault();
                item.admittedIntake = approvedadmitted_intake.Where(s => s.degree == item.degree).Select(s => s.admittedIntake).FirstOrDefault();
            }

            ViewBag.degreewiseTotalIntake = adminDashbord.degreewiseTotalIntake1;
            ViewBag.degreewisePraposedIntake = adminDashbord.Praposedintakedownload;

            if (exportType == "Export")
            {
                string fileName = "Degree Wise Intake A.Y-" + ViewBag.AcademicYear + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_DegreewiseIntake.cshtml");
            }
            else if (exportType == "ProposedExport")
            {
                string fileName = "Praposed Intake A.Y-" + ViewBag.AcademicYear + ".xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_ProposedintakeDownload.cshtml");
            }

            //Users Count
            var usersList = (from u in db.my_aspnet_users
                             join ur in db.my_aspnet_usersinroles on u.id equals ur.userId
                             join r in db.my_aspnet_roles on ur.roleId equals r.id
                             join m in db.my_aspnet_membership on u.id equals m.userId
                             select new
                             {
                                 r.id,
                                 r.name,
                                 m.IsApproved,
                             }).ToList();

            adminDashbord.adminTotalUsersCount = usersList.Where(r => r.name == "Admin").Count();
            adminDashbord.adminActiveUsersCount = usersList.Where(r => r.name == "Admin" && r.IsApproved == true).Count();
            adminDashbord.adminInActiveUsersCount = usersList.Where(r => r.name == "Admin" && r.IsApproved == false).Count();

            adminDashbord.committeeTotalUsersCount = usersList.Where(r => r.name == "Committee").Count();
            adminDashbord.committeeActiveUsersCount = usersList.Where(r => r.name == "Committee" && r.IsApproved == true).Count();
            adminDashbord.committeeInActiveUsersCount = usersList.Where(r => r.name == "Committee" && r.IsApproved == false).Count();

            adminDashbord.collegeTotalUsersCount = usersList.Where(r => r.name == "College").Count();
            adminDashbord.collegeActiveUsersCount = usersList.Where(r => r.name == "College" && r.IsApproved == true).Count();
            adminDashbord.collegeInActiveUsersCount = usersList.Where(r => r.name == "College" && r.IsApproved == false).Count();

            adminDashbord.dataentryTotalUsersCount = usersList.Where(r => r.name == "DataEntry").Count();
            adminDashbord.dataentryActiveUsersCount = usersList.Where(r => r.name == "DataEntry" && r.IsApproved == true).Count();
            adminDashbord.dataentryInActiveUsersCount = usersList.Where(r => r.name == "DataEntry" && r.IsApproved == false).Count();

            //CollegesSubmiisionCount
            var collegeEditStatus = (from ce in db.jntuh_college_edit_status
                                     join c in db.jntuh_college on ce.collegeId equals c.id
                                     select new
                                     {
                                         ce.id,
                                         ce.IsCollegeEditable,
                                         ce.editFromDate,
                                         ce.editToDate,
                                         c.isActive
                                     }).ToList();
            //adminDashbord.submissionCount = collegeEditStatus.Where(s => s.IsCollegeEditable == false && s.isActive == true).Select(s => s.id).Count();
            //adminDashbord.submissionPendingCount = collegeEditStatus.Where(s => s.IsCollegeEditable == true && s.isActive == true).Select(s => s.id).Count();
            adminDashbord.editstatusCount = collegeEditStatus.Where(s => s.editFromDate < DateTime.Now && s.editToDate > DateTime.Now && s.isActive == true).Select(s => s.id).Count();

            //Remaining sms count
            string strUrl = "http://api.mvaayoo.com/mvaayooapi/APIUtil?user=jm.sarma@csstechnergy.com:c55tech&type=0";
            string sentStatus = "Credit balance is not found";
            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();
                StreamReader readStream = new StreamReader(s);
                string dataString = readStream.ReadToEnd();
                response.Close();
                s.Close();
                readStream.Close();

                sentStatus = dataString;
            }
            catch (WebException e)
            {
                sentStatus = "Credit balance is not found";
            }
            catch (Exception ex)
            {
                sentStatus = "Credit balance is not found";
            }

            ViewBag.SMS = sentStatus.Replace("Status=0,Credit balance is", "");

            List<UAAAS.Controllers.AdminController.OnlineUAAASUsers> loggedInUsers = new List<UAAAS.Controllers.AdminController.OnlineUAAASUsers>();
            MembershipUserCollection allUsers = Membership.GetAllUsers();
            MembershipUserCollection filteredUsers = new MembershipUserCollection();

            bool isOnline = true;
            foreach (MembershipUser user in allUsers)
            {
                // if user is currently online, add to gridview list
                if (user.IsOnline == isOnline)
                {
                    filteredUsers.Add(user);

                    int userID = Convert.ToInt32(user.ProviderUserKey);
                    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                    string code = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault();
                    string name = db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeName).FirstOrDefault();
                    string email = db.my_aspnet_membership.Where(m => m.userId == userID).Select(m => m.Email).FirstOrDefault();
                    loggedInUsers.Add(new UAAAS.Controllers.AdminController.OnlineUAAASUsers { username = user.UserName, datetime = user.LastActivityDate.ToString(), code = code, name = name, email = email });
                }
            }
            ViewBag.OnlineUsers = loggedInUsers.Count();
            DateTime scmuploadstartdate = new DateTime(2018, 02, 09);
            int[] SubmittedColleges = { 2, 4, 5, 6, 7, 8, 9, 11, 12, 17, 20, 22, 23, 24, 26, 27, 29, 30, 32, 34, 35, 38, 39, 40, 41, 42, 43, 44, 46, 47, 48, 50, 52, 54, 55, 56, 58, 59, 60, 65, 67, 68, 69, 70, 72, 74, 75, 77, 78, 79, 80, 81, 84, 85, 86, 87, 88, 90, 91, 95, 97, 100, 103, 104, 105, 106, 107, 108, 109, 110, 111, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 125, 127, 128, 129, 130, 132, 134, 135, 136, 137, 138, 139, 140, 141, 143, 144, 145, 146, 147, 148, 150, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 194, 195, 196, 197, 198, 201, 202, 203, 204, 206, 207, 210, 211, 213, 214, 215, 217, 218, 219, 222, 223, 225, 227, 228, 229, 230, 234, 235, 236, 237, 238, 241, 242, 243, 244, 245, 246, 249, 250, 252, 253, 254, 256, 260, 261, 262, 263, 264, 266, 267, 271, 273, 276, 282, 283, 286, 287, 290, 291, 292, 293, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 306, 307, 308, 309, 310, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 324, 327, 329, 330, 332, 334, 335, 336, 342, 343, 348, 349, 350, 352, 353, 355, 360, 364, 365, 366, 367, 368, 369, 370, 371, 373, 374, 376, 379, 380, 382, 384, 385, 386, 389, 391, 392, 393, 394, 395, 399, 400, 403, 410, 411, 413, 414, 416, 420, 421, 422, 423, 424, 428, 429, 430, 435, 436, 439, 441, 442, 443, 445, 448, 449, 452, 454, 455 };
            int[] totalactivecolleges = { 2, 4, 7, 8, 9, 11, 12, 17, 20, 22, 23, 26, 29, 32, 34, 38, 40, 41, 46, 48, 56, 59, 68, 69, 70, 72, 74, 77, 79, 80, 81, 84, 85, 86, 87, 88, 100, 102, 103, 104, 106, 108, 109, 111, 113, 115, 116, 119, 121, 122, 123, 124, 125, 128, 129, 130, 132, 134, 137, 138, 141, 143, 144, 145, 147, 148, 151, 152, 153, 155, 156, 157, 158, 159, 161, 162, 163, 164, 165, 166, 168, 170, 171, 172, 173, 175, 176, 177, 178, 179, 181, 182, 183, 184, 185, 186, 187, 188, 189, 192, 193, 195, 196, 197, 198, 201, 203, 207, 210, 211, 214, 215, 218, 222, 225, 227, 228, 229, 236, 238, 241, 242, 243, 244, 245, 247, 249, 250, 254, 256, 259, 260, 261, 264, 269, 271, 273, 276, 282, 283, 286, 287, 291, 292, 293, 299, 300, 304, 305, 306, 307, 308, 309, 310, 315, 316, 321, 322, 324, 326, 327, 329, 330, 334, 335, 336, 342, 349, 350, 352, 360, 365, 366, 367, 368, 369, 371, 373, 374, 376, 380, 382, 385, 391, 393, 394, 395, 399, 400, 401, 402, 403, 414, 415, 416, 419, 420, 422, 423, 424, 428, 429, 430, 6, 24, 27, 30, 44, 45, 47, 52, 54, 55, 58, 60, 65, 66, 78, 90, 95, 97, 105, 107, 110, 114, 117, 118, 120, 127, 135, 136, 139, 146, 150, 169, 202, 204, 206, 213, 219, 234, 237, 239, 252, 253, 262, 263, 267, 290, 295, 297, 298, 301, 302, 303, 313, 314, 317, 318, 319, 320, 348, 353, 362, 370, 379, 384, 389, 392, 410, 427, 5, 67, 246, 279, 296, 325, 343, 355, 386, 411, 421, 39, 42, 43, 75, 140, 180, 194, 217, 223, 230, 235, 266, 332, 364, 35, 50, 91, 174, 435, 436, 439, 441, 442, 443, 445, 447, 448, 452, 454, 455, 413, 449 };
            List<jntuh_college_faculty_registered> jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(f => SubmittedColleges.Contains(f.collegeId)).Select(s => s).ToList();
            ViewBag.TotalRegisteredFaculty = db.jntuh_registered_faculty.Count();
            ViewBag.TotalTeachingFaculty = jntuh_college_faculty_registered.Count();
            ViewBag.BlackListFaculty = db.jntuh_registered_faculty.Where(s => s.Blacklistfaculy == true).Count();



            jntuh_college_links_assigned linkdetails =
                db.jntuh_college_links_assigned.Where(
                    a => a.academicyearId == PresentAcademicYearId && a.isActive == true && a.linkId == 1)
                    .Select(s => s)
                    .FirstOrDefault();
            if (linkdetails != null)
            {
                DateTime fromdate = Convert.ToDateTime(linkdetails.fromdate);
                List<jntuh_scmproceedingsrequests> scmproceedingsrequests =
                    db.jntuh_scmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId != 0 && s.SpecializationId != 0)
                        .Select(s => s)
                        .ToList();
                List<jntuh_scmproceedingrequest_addfaculty> scmproceedingsfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.CreatedOn >= fromdate)
                        .Select(s => s)
                        .ToList();
                int[] scmids = scmproceedingsrequests.Where(s => s.CollegeId != 375 && s.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                ViewBag.FSCMnorequestColleges = scmproceedingsrequests.Where(s => s.RequestSubmittedDate == null).GroupBy(s => s.CollegeId).Count();
                ViewBag.FSCMrequestColleges = scmproceedingsrequests.Where(s => s.RequestSubmittedDate != null).GroupBy(s => s.CollegeId).Count();
                ViewBag.FSCMrequestfaculty =
                    scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).Count();
            }

            jntuh_college_links_assigned plinkdetails =
                db.jntuh_college_links_assigned.Where(
                    a => a.academicyearId == PresentAcademicYearId && a.isActive == true && a.linkId == 2)
                    .Select(s => s)
                    .FirstOrDefault();
            if (plinkdetails != null)
            {
                DateTime fromdate = Convert.ToDateTime(plinkdetails.fromdate);
                List<jntuh_scmproceedingsrequests> scmproceedingsrequests =
                    db.jntuh_scmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId == 0 && s.SpecializationId == 0 && s.ISActive == true)
                        .Select(s => s)
                        .ToList();
                List<jntuh_scmproceedingrequest_addfaculty> scmproceedingsfaculty =
                    db.jntuh_scmproceedingrequest_addfaculty.Where(s => s.CreatedOn >= fromdate)
                        .Select(s => s)
                        .ToList();
                int[] scmids = scmproceedingsrequests.Where(s => s.CollegeId != 375 && s.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                ViewBag.PSCMnorequestColleges = scmproceedingsrequests.Where(s => s.RequestSubmittedDate == null).GroupBy(s => s.CollegeId).Count();
                ViewBag.PSCMrequestColleges = scmproceedingsrequests.Where(s => s.RequestSubmittedDate != null).GroupBy(s => s.CollegeId).Count();
                ViewBag.PSCMrequestfaculty =
                    scmproceedingsfaculty.Where(s => scmids.Contains(s.ScmProceedingId)).Count();
            }


            var Facultycomplaints = db.jntuh_college_complaints.Where(c => c.roleId == 7 && c.createdBy != 124636 && c.createdOn.Value.Year == 2023).Count();
            var StudentsComplaints = db.jntuh_college_students_complaints.Count();
            ViewBag.Facultycomplaints = Facultycomplaints;
            ViewBag.StudentsComplaints = StudentsComplaints;

            //ViewBag.TotalTeachingFacultywithaadhaar = jntuh_college_faculty_registered.Where(a=>a.AadhaarDocument!=null).ToList().Count();
            //ViewBag.TotalTeachingFacultywithnoaadhaar = jntuh_college_faculty_registered.Where(a => a.AadhaarDocument == null).ToList().Count();
            //ViewBag.TotalTeachingFacultyscmuploadcount = db.jntuh_scmupload.Where(a => a.CreatedOn >= scmuploadstartdate && totalactivecolleges.Contains(a.CollegeId)).ToList().Count();

            return View("~/Views/Reports/AdminDashboard.cshtml", adminDashbord);
        }

        //SCM New Dashboad added by Narayana Reddy
        //[Authorize(Roles = "Admin")]
        public ActionResult ScmDashboardview()
        {
            ScmDashboard scmDashboard = new ScmDashboard();
            int flid =
                db.jntuh_link_screens.Where(l => l.linkCode == "FSCM" && l.isActive == true)
                    .Select(s => s.id)
                    .FirstOrDefault();
            var jntuhScmproceedingsrequests = db.jntuh_scmproceedingsrequests.AsNoTracking().ToList();
            var jntuhscmproceedingrequestaddfaculty = db.jntuh_scmproceedingrequest_addfaculty.AsNoTracking().ToList();
            jntuh_college_links_assigned facultscmdates = db.jntuh_college_links_assigned.Where(l => l.isAssigned == false).Select(s => s).FirstOrDefault();
            if (facultscmdates != null)
            {
                DateTime fromdate = Convert.ToDateTime(facultscmdates.fromdate);
                scmDashboard.Fscmfromdate = facultscmdates.fromdate.ToString().Split(' ')[0];
                scmDashboard.Fscmtodate = facultscmdates.todate.ToString().Split(' ')[0];
                List<jntuh_scmproceedingsrequests> fscmrequests =
                    jntuhScmproceedingsrequests.Where(s => s.CreatedOn >= fromdate && s.DEpartmentId != 0 && s.SpecializationId != 0).Select(s => s).ToList();
                scmDashboard.Frequestcollegecount = fscmrequests.GroupBy(g => g.CollegeId).Count();
                scmDashboard.Frequestsubmitcollegecount = fscmrequests.Where(r => r.RequestSubmittedDate != null).GroupBy(g => g.CollegeId).Count();
                scmDashboard.Frequestnotsubmitcollegecount = fscmrequests.Where(r => r.RequestSubmittedDate == null).GroupBy(g => g.CollegeId).Count();
                int[] submitids = fscmrequests.Where(r => r.RequestSubmittedDate != null).Select(s => s.ID).ToArray();
                int[] notsubmitids = fscmrequests.Where(r => r.RequestSubmittedDate == null).Select(s => s.ID).ToArray();
                scmDashboard.submitfacultycount =
                    jntuhscmproceedingrequestaddfaculty.Where(s => submitids.Contains(s.ScmProceedingId)).Select(s => s).Count();
                scmDashboard.notsubmitfacultycount =
                    jntuhscmproceedingrequestaddfaculty.Where(s => notsubmitids.Contains(s.ScmProceedingId)).Select(s => s).Count();
            }

            return View(scmDashboard);
        }

        public ActionResult Deficiency12345(string strcollegeId)
        {
            int collegeId = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(strcollegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            string strcc = db.jntuh_college.Find(collegeId).collegeCode;
            string strpath = db.jntuh_college_news.Where(c => c.collegeId == collegeId && c.title == "DEFICIENCY REPORT AS PER FORM 415").Select(c => c.navigateURL).FirstOrDefault();
            return File(strpath, "application/pdf", strcc + "Deficiency" + ".pdf");
        }

        [Authorize(Roles = "College,Admin")]
        public ActionResult Deficiency()
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeId = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            string strcc = db.jntuh_college.Find(collegeId).collegeCode;
            string strpath = "/Content/PdfReports/DeficiencyReports/2015/" + strcc + "-Deficiency.pdf";
            return File(strpath, "application/pdf", strcc + "-Deficiency" + ".pdf");
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification,DataEntry")]
        public ActionResult AllColleges()
        {
            int[] collegeIds = db.jntuh_approvedadmitted_intake.GroupBy(e => e).Select(e => e.Key.collegeId).ToArray();
            List<jntuh_college> colleges = db.jntuh_college.Where(e => collegeIds.Contains(e.id)).Select(e => e).ToList();
            return View(colleges);
        }

        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification,DataEntry")]
        public ActionResult FeeCalculation(string collegeId)
        {
            //get current user Id
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //get current user CollegeId
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("FacultyVerification"))
                {
                    if (collegeId != null)
                    {
                        userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                    }
                }
            }
            var CollegeData = db.jntuh_college.Where(e => e.id == userCollegeID).Select(e => new { Name = e.collegeCode + "-" + e.collegeName }).FirstOrDefault();
            int[] AtanamousColleges = new int[] { 9, 11, 26, 32, 38, 39, 42, 68, 70, 75, 108, 109, 134, 140, 171, 179, 180, 183, 192, 196, 198, 335, 364, 367, 374, 399 };
            if (AtanamousColleges.Contains(userCollegeID))
            {
                string CollegeName = string.Empty;
                if (CollegeData != null)
                {
                    CollegeName = CollegeData.Name;
                }
                ViewBag.DuesFee = db.jntuh_college_paymentoffee.Where(s => s.collegeId == userCollegeID && s.FeeTypeID == 4).Select(s => s.duesAmount).FirstOrDefault();
                var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.afrcFee).FirstOrDefault();
                //First Year Calculation
                int afrcFee = Convert.ToInt32(afrcAmount);


                AutonamusCollegeFee collegeFee = new AutonamusCollegeFee();
                collegeFee.CollegeName = CollegeName;
                collegeFee.Fee = afrcFee;
                //  return RedirectToAction("AutonamusCollegeFee", collegeFee);
                return View("AutonamusCollegeFee", collegeFee);
            }
            else
            {
                //  userCollegeID = 198;
                //  List<jntuh_approvedadmitted_intake> collegeapprovedList =db.jntuh_approvedadmitted_intake.Where(e => e.collegeId == userCollegeID).Select(e => e).ToList();


                string CollegeName = string.Empty;
                if (CollegeData != null)
                {
                    CollegeName = CollegeData.Name;
                }

                List<CollegeApprovedList> collegeApproved = (from app in db.jntuh_approvedadmitted_intake
                                                             join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                             join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                             join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id

                                                             where app.collegeId == userCollegeID
                                                             select new CollegeApprovedList()
                                                             {
                                                                 AcademicYearId = app.AcademicYearId,
                                                                 ShiftId = app.ShiftId,
                                                                 CollegeName = CollegeName,
                                                                 collegeId = app.collegeId,
                                                                 ApprovedIntake = app.ApprovedIntake,
                                                                 AdmittedIntake = app.AdmittedIntake,
                                                                 SpecializationId = app.SpecializationId,
                                                                 IsActive = app.IsActive,
                                                                 degreeId = Deg.id,
                                                                 deptId = Dept.id,
                                                                 Department = Dept.departmentName,
                                                                 Degree = Deg.degree,
                                                                 SpecializationName = Spec.specializationName
                                                             }).ToList();



                //  List<acdemicFees> acdemicFeeses=new List<acdemicFees>();


                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                actualYear = actualYear - 1;
                //Suresh
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));




                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();
                var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.afrcFee).FirstOrDefault();
                //First Year Calculation
                int afrcFee = Convert.ToInt32(afrcAmount);

                ViewBag.DuesFee = db.jntuh_college_paymentoffee.Where(s => s.collegeId == userCollegeID && s.FeeTypeID == 4).Select(s => s.duesAmount).FirstOrDefault();

                foreach (var item in collegeApproved)
                {
                    int FirstYearFee = 0;
                    int UGSecondYearFee = 0;
                    int UGThirdYearFee = 0;
                    int UGFourYearFee = 0;
                    item.ApprovedIntake1 = GetIntake(userCollegeID, AY1, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake2 = GetIntake(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake3 = GetIntake(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake4 = GetIntake(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake1 = GetIntake(userCollegeID, AY1, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake2 = GetIntake(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake3 = GetIntake(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake4 = GetIntake(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 0);
                    item.LeteralentryIntake2 = LeteralGetIntake(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake3 = LeteralGetIntake(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake4 = LeteralGetIntake(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake2 += item.LeteralentryIntake2;
                    item.AdmittedIntake3 += item.LeteralentryIntake3;
                    item.AdmittedIntake4 += item.LeteralentryIntake4;



                    item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
                    item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                    item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                    item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                    item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                    item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                }
                ViewBag.TotalAmount = collegeApproved.Sum(e => e.SpecializationwiseSalary);
                return View(collegeApproved.OrderBy(e => e.DisplayOrder).ToList());
            }
        }

        public int FeeCalculationYearWise(int Year, int DegreeId, int collegeId, int SpecializationId, int flag, int afrcFee, int shift)
        {
            int Fee = 0;


            if ((DegreeId == 4 || DegreeId == 5) && flag == 1)
            {
                List<jntuh_approvedadmitted_intake> collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                                                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                                                           where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                                                           select app).ToList();
                if (collegeapprovedList.Count() != 0)
                {
                    int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                    double percentahevalue = (double)(afrcFee * 0.5) / 100;
                    int FirstFee = (int)Math.Ceiling(percentahevalue);
                    Fee = FirstFee * ApprovedIntake;
                }
            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 2)
            {
                List<jntuh_approvedadmitted_intake> collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                                                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                                                           where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                                                           select app).ToList();

                if (collegeapprovedList.Count() != 0)
                {
                    int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                    int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                    int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                    AdmittedIntake += LateralentryIntake;
                    double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                    int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                    int calculatepercentage = slap(admittedpercentage);
                    int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                    int initialFee = Convert.ToInt32((afrcFee * 0.5) / 100);
                    Fee = (initialFee * ApprovedPercentage);
                }



            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 3)
            {
                List<jntuh_approvedadmitted_intake> collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                                                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                                                           where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                                                           select app).ToList();
                if (collegeapprovedList.Count() != 0)
                {
                    int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                    int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                    int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                    AdmittedIntake += LateralentryIntake;
                    double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                    int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                    int calculatepercentage = slap(admittedpercentage);
                    int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                    const int initialFee = 175; //Convert.ToInt32((afrcFee * 0.5) / 100);
                    Fee = (initialFee * ApprovedPercentage);
                }

            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 4)
            {
                List<jntuh_approvedadmitted_intake> collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                                                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                                                           where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                                                           select app).ToList();
                if (collegeapprovedList.Count() != 0)
                {
                    int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                    int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                    int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                    AdmittedIntake += LateralentryIntake;
                    double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                    int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                    int calculatepercentage = slap(admittedpercentage);
                    int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                    const int initialFee = 175; //Convert.ToInt32((afrcFee * 0.5) / 100);
                    Fee = (initialFee * ApprovedPercentage);
                }

            }


            jntuh_approvedadmitted_intake collegeapprovedList1 = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == Year && e.collegeId == collegeId && e.SpecializationId == SpecializationId && e.ShiftId == shift).Select(e => e).FirstOrDefault();
            if (collegeapprovedList1 != null)
            {
                if ((DegreeId == 1 || DegreeId == 2 || DegreeId == 3 || DegreeId == 6 || DegreeId == 9) && flag == 1)
                {
                    Fee = 30000;
                }
                if ((DegreeId == 7 || DegreeId == 8 || DegreeId == 10) && flag == 1)
                {
                    Fee = 40000;
                }
            }

            // }
            return Fee;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            //approved
            if (flag == 1)
            {
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.ApprovedIntake).FirstOrDefault();
            }
            else //admitted
            {
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.AdmittedIntake).FirstOrDefault();
            }

            return intake;
        }


        private int LeteralGetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            //approved
            if (flag == 1)
            {
                intake = Convert.ToInt32(db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.LateralentryIntake).FirstOrDefault());
            }
            else //admitted
            {
                intake = db.jntuh_approvedadmitted_intake.Where(i => i.collegeId == collegeId && i.AcademicYearId == academicYearId && i.SpecializationId == specializationId && i.ShiftId == shiftId).Select(i => i.AdmittedIntake).FirstOrDefault();
            }

            return intake;
        }

        public int slap(int percentage)
        {
            int totalPercentage = 0;
            if (percentage <= 25)
            {
                totalPercentage = 25;
            }
            else if (percentage <= 50)
            {
                totalPercentage = 50;
            }
            else if (percentage <= 75)
            {
                totalPercentage = 75;
            }
            else if (percentage > 75)
            {
                totalPercentage = 100;
            }
            return totalPercentage;
        }

        [Authorize(Roles = "Admin,DataEntry,FacultyVerification")]
        public ActionResult AffiliationFeeReport(int? CollegeId)
        {
            List<AffiliationFeeCollegeList> CollegesList = new List<AffiliationFeeCollegeList>();

            int[] AtanamousColleges = new int[] { 9, 11, 26, 32, 38, 39, 42, 68, 70, 75, 108, 109, 134, 140, 171, 179, 180, 183, 192, 196, 198, 335, 364, 367, 374, 399 };
            //CollegeId = 364;
            if (CollegeId == null)
            {
                var CollegeIds = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == 9).GroupBy(e => e.collegeId).Select(e => e.Key).ToArray();
                // var CollegeIds =db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false).GroupBy(e => e.collegeId).Select(e => e.Key).ToArray();
                CollegesList = db.jntuh_college.Where(e => CollegeIds.Contains(e.id) && !AtanamousColleges.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
                 {
                     CollegeId = e.id,
                     CollegeCode = e.collegeCode,
                     CollegeName = e.collegeName
                 }).OrderBy(e => e.CollegeCode).ToList();
            }
            else
            {
                CollegesList = db.jntuh_college.Where(e => e.id == CollegeId && !AtanamousColleges.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
                {
                    CollegeId = e.id,
                    CollegeCode = e.collegeCode,
                    CollegeName = e.collegeName
                }).ToList();
            }
            CollegesList = CollegesList.ToList();
            foreach (var item in CollegesList)
            {

                item.CollegeApprovedList = GenerateReportApprovedLists(item.CollegeId);


            }

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=AffiliationFeeReport.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/Reports/_AffiliationFeeReport.cshtml", CollegesList);
        }

        public List<CollegeApprovedList> GenerateReportApprovedLists(int collegeId)
        {
            List<CollegeApprovedList> collegeApproved = new List<CollegeApprovedList>();
            if (collegeId != 0)
            {

                collegeApproved = (from app in db.jntuh_approvedadmitted_intake
                                   join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                   where app.collegeId == collegeId
                                   select new CollegeApprovedList()
                                   {
                                       AcademicYearId = app.AcademicYearId,
                                       ShiftId = app.ShiftId,
                                       collegeId = app.collegeId,
                                       ApprovedIntake = app.ApprovedIntake,
                                       AdmittedIntake = app.AdmittedIntake,
                                       SpecializationId = app.SpecializationId,
                                       IsActive = app.IsActive,
                                       degreeId = Deg.id,
                                       deptId = Dept.id,
                                       Department = Dept.departmentName,
                                       Degree = Deg.degree,
                                       SpecializationName = Spec.specializationName
                                   }).ToList();



                //  List<acdemicFees> acdemicFeeses=new List<acdemicFees>();


                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();

                //Suresh
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));




                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();
                var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == collegeId).Select(e => e.afrcFee).FirstOrDefault();
                //First Year Calculation
                int afrcFee = 0;
                if (afrcAmount != 0)
                {
                    afrcFee = Convert.ToInt32(afrcAmount);
                }



                foreach (var item in collegeApproved)
                {
                    int FirstYearFee = 0;
                    int UGSecondYearFee = 0;
                    int UGThirdYearFee = 0;
                    int UGFourYearFee = 0;
                    item.ApprovedIntake1 = GetIntake(collegeId, AY1, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake2 = GetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake3 = GetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake4 = GetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake1 = GetIntake(collegeId, AY1, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake2 = GetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake3 = GetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake4 = GetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 0);

                    item.LeteralentryIntake2 = LeteralGetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake3 = LeteralGetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake4 = LeteralGetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake2 += item.LeteralentryIntake2;
                    item.AdmittedIntake3 += item.LeteralentryIntake3;
                    item.AdmittedIntake4 += item.LeteralentryIntake4;



                    item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
                    item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                    item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                    item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                    item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                    item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                }
                return collegeApproved.OrderBy(e => e.DisplayOrder).ToList();
            }
            else
            {
                return collegeApproved;
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification,DataEntry")]
        public ActionResult AicteFaculty()
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);


            int UserCollegeId = db.jntuh_college_users.Where(u => u.userID == userid).Select(u => u.collegeID).FirstOrDefault();
            if (UserCollegeId == 0)
            {
                return RedirectToAction("College", "Dashboard");
            }
            //AICTEFacultyClass AICTEFacultyClass = new AICTEFacultyClass();

            // var jntuh_college_aictefaculty= db.jntuh_college_aictefaculty.Where(s => s.FacultyId == FacultyId).Select(e => e).FirstOrDefault();
            // if (jntuh_college_aictefaculty!=null)
            // {
            //     AICTEFacultyClass.FacultyId = jntuh_college_aictefaculty.FacultyId;
            //     AICTEFacultyClass.Programme = jntuh_college_aictefaculty.Programme;
            //     AICTEFacultyClass.Cource = jntuh_college_aictefaculty.Cource;
            //     AICTEFacultyClass.FacultyType = jntuh_college_aictefaculty.FacultyType;
            //     AICTEFacultyClass.JobType = jntuh_college_aictefaculty.JobType;
            //     AICTEFacultyClass.FirstName = jntuh_college_aictefaculty.FirstName;
            //     AICTEFacultyClass.SurName = jntuh_college_aictefaculty.SurName;
            //     AICTEFacultyClass.ExactDesignation = jntuh_college_aictefaculty.ExactDesignation;
            //     AICTEFacultyClass.DateOfJoiningTheInstitute = jntuh_college_aictefaculty.DateOfJoiningTheInstitute.ToString();
            //     AICTEFacultyClass.AppionmentType = jntuh_college_aictefaculty.AppointmentType;
            //     AICTEFacultyClass.Docotorate = jntuh_college_aictefaculty.Docotorate;
            //     AICTEFacultyClass.MastersDegree = jntuh_college_aictefaculty.MastersDegree;
            //     AICTEFacultyClass.BachelorsDegree = jntuh_college_aictefaculty.BachelorsDegree;
            //     AICTEFacultyClass.OtherQualification = jntuh_college_aictefaculty.OtherQualification;
            //     AICTEFacultyClass.RegistrationNumber = jntuh_college_aictefaculty.RegistrationNumber;
            //     AICTEFacultyClass.PanNumber = jntuh_college_aictefaculty.PanNumber;
            //     AICTEFacultyClass.AadhaarNumber = jntuh_college_aictefaculty.AadhaarNumber;
            //     AICTEFacultyClass.CollegeId = jntuh_college_aictefaculty.CollegeId;

            //     return View(AICTEFacultyClass);
            // }
            // else
            // {
            //     return View(AICTEFacultyClass);
            // }
            List<SelectListItem> Facultytype = new List<SelectListItem>();
            Facultytype.Add(new SelectListItem() { Text = "UG", Value = "1" });
            Facultytype.Add(new SelectListItem() { Text = "PG", Value = "2" });
            ViewBag.FacultyType = Facultytype;

            List<SelectListItem> FacultyJobType = new List<SelectListItem>();
            FacultyJobType.Add(new SelectListItem() { Text = "FT", Value = "1" });
            FacultyJobType.Add(new SelectListItem() { Text = "PT", Value = "2" });
            ViewBag.FacultyJobType = FacultyJobType;

            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;
            //List<SelectListItem> Jobtype = new List<SelectListItem>();
            //Jobtype.Add(new SelectListItem() { Text = "FT", Value = "1" });
            //Jobtype.Add(new SelectListItem() { Text = "PT", Value = "2" });
            //ViewBag.Jobtype = Jobtype;

            List<SelectListItem> Doctorate = new List<SelectListItem>();
            Doctorate.Add(new SelectListItem() { Text = "YES", Value = "1" });
            Doctorate.Add(new SelectListItem() { Text = "No", Value = "2" });
            ViewBag.doctorate = Doctorate;

            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();
            //ViewBag.designation = db.jntuh_designation.Where(s => s.isActive == true).Take(4).ToList();

            var Aicte = db.jntuh_college_aictefaculty.Where(s => s.IsActive == true && s.CollegeId == UserCollegeId).Select(e => e).ToList();
            ViewBag.AicteData = Aicte;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,College,DataEntry,FacultyVerification,DataEntry")]
        public ActionResult AicteFaculty(AICTEFacultyClass AICTEFacultyClass, HttpPostedFileBase ExcelFile)
        {
            int userid = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);


            int UserCollegeId = db.jntuh_college_users.Where(u => u.userID == userid).Select(u => u.collegeID).FirstOrDefault();

            List<SelectListItem> Facultytype = new List<SelectListItem>();
            Facultytype.Add(new SelectListItem() { Text = "UG", Value = "1" });
            Facultytype.Add(new SelectListItem() { Text = "PG", Value = "2" });
            ViewBag.FacultyType = Facultytype;

            List<SelectListItem> FacultyJobType = new List<SelectListItem>();
            FacultyJobType.Add(new SelectListItem() { Text = "FT", Value = "1" });
            FacultyJobType.Add(new SelectListItem() { Text = "PT", Value = "2" });
            ViewBag.FacultyJobType = FacultyJobType;

            List<SelectListItem> Programme = new List<SelectListItem>();
            Programme.Add(new SelectListItem() { Text = "ENGINEERING AND TECHNOLOGY", Value = "1" });
            Programme.Add(new SelectListItem() { Text = "PHARMACY", Value = "2" });
            Programme.Add(new SelectListItem() { Text = "MANAGEMENT", Value = "3" });
            ViewBag.Programme = Programme;

            List<SelectListItem> Doctorate = new List<SelectListItem>();
            Doctorate.Add(new SelectListItem() { Text = "YES", Value = "1" });
            Doctorate.Add(new SelectListItem() { Text = "No", Value = "2" });
            ViewBag.doctorate = Doctorate;

            ViewBag.departments = db.jntuh_department.Where(s => s.isActive == true).ToList();
            //ViewBag.designation = db.jntuh_designation.Where(s => s.isActive == true).Take(4).ToList();

            //if (ExcelFile != null)
            //{
            //    #region Excel File Code
            //    string filePath = string.Empty;


            //    dynamic Result;
            //    //  string appfilepath = Server.MapPath("~/Content/AicteFacultyExcelFiles/");
            //    // string appfilepath = 
            //    string appfilename = ExcelFile.FileName;
            //    filePath = appfilename;
            //    string extension = Path.GetExtension(ExcelFile.FileName);
            //    string conString = string.Empty;
            //    switch (extension)
            //    {
            //        case ".xls":
            //            conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
            //            break;
            //        case ".xlsx":
            //            conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
            //            break;
            //    }
            //    conString = string.Format(conString, filePath);
            //    using (OleDbConnection connExcel = new OleDbConnection(conString))
            //    {
            //        using (OleDbCommand cmdExcel = new OleDbCommand())
            //        {
            //            using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
            //            {
            //                DataTable dt = new DataTable();
            //                cmdExcel.Connection = connExcel;
            //                connExcel.Open();
            //                DataTable dtExcelSchema;

            //                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            //                string sheetName = dtExcelSchema.Rows[1]["Table_Name"].ToString();


            //                connExcel.Close();
            //                connExcel.Open();
            //                cmdExcel.CommandText = "SELECT * From [" + sheetName + "]";
            //                odaExcel.SelectCommand = cmdExcel;
            //                odaExcel.Fill(dt);
            //                connExcel.Close();
            //                int i = 1;

            //                var query = from row in dt.AsEnumerable()
            //                            select row;

            //                foreach (DataRow row in dt.Rows)
            //                {


            //                    jntuh_college_aictefaculty jntuh_college_aictefaculty = new jntuh_college_aictefaculty();
            //                    jntuh_college_aictefaculty.CollegeId = UserCollegeId;
            //                    jntuh_college_aictefaculty.FacultyId = row["FacultyId"].ToString();
            //                    jntuh_college_aictefaculty.RegistrationNumber = row["RegistrationNumber"].ToString();
            //                    jntuh_college_aictefaculty.FirstName = row["FirstName"].ToString();
            //                    jntuh_college_aictefaculty.SurName = row["SurName"].ToString();
            //                    jntuh_college_aictefaculty.PanNumber = row["PANNumber"].ToString();
            //                    jntuh_college_aictefaculty.AadhaarNumber = row["AadhaarNumber"].ToString();
            //                    jntuh_college_aictefaculty.Programme = row["Programme"].ToString();
            //                    jntuh_college_aictefaculty.Course = row["Course"].ToString();
            //                    // data.CollegeId = row["CollegeCode"].ToString(); ;

            //                    jntuh_college_aictefaculty.DateOfJoiningTheInstitute = null;
            //                    jntuh_college_aictefaculty.AppointmentType = null;
            //                    jntuh_college_aictefaculty.Doctorate = null;
            //                    jntuh_college_aictefaculty.MastersDegree = null;
            //                    jntuh_college_aictefaculty.BachelorsDegree = null;
            //                    jntuh_college_aictefaculty.OtherQualification = "Test";


            //                    jntuh_college_aictefaculty.IsActive = true;
            //                    jntuh_college_aictefaculty.CreatedBy = userid;
            //                    jntuh_college_aictefaculty.CreatedOn = DateTime.Now;
            //                    jntuh_college_aictefaculty.UpdatedBy = null;
            //                    jntuh_college_aictefaculty.UpdatedOn = null;

            //                    db.jntuh_college_aictefaculty.Add(jntuh_college_aictefaculty);
            //                    db.SaveChanges();
            //                }

            //            }
            //        }
            //    }

            //    #endregion
            //    return RedirectToAction("AicteFaculty");
            //}
            if (ModelState.IsValid == true)
            {
                if (AICTEFacultyClass != null)
                {
                    jntuh_college_aictefaculty jntuh_college_aictefaculty = new jntuh_college_aictefaculty();

                    jntuh_college_aictefaculty.CollegeId = UserCollegeId;
                    jntuh_college_aictefaculty.FacultyId = AICTEFacultyClass.FacultyId;
                    jntuh_college_aictefaculty.Programme = Programme.Where(s => s.Value == AICTEFacultyClass.Programme).Select(s => s.Text).FirstOrDefault();
                    jntuh_college_aictefaculty.Course = AICTEFacultyClass.Course;
                    jntuh_college_aictefaculty.FirstName = AICTEFacultyClass.FirstName;
                    jntuh_college_aictefaculty.SurName = AICTEFacultyClass.SurName;

                    jntuh_college_aictefaculty.RegistrationNumber = AICTEFacultyClass.RegistrationNumber == null ? "" : AICTEFacultyClass.RegistrationNumber;
                    jntuh_college_aictefaculty.PanNumber = AICTEFacultyClass.PanNumber.ToUpper();
                    jntuh_college_aictefaculty.AadhaarNumber = AICTEFacultyClass.AadhaarNumber;
                    // jntuh_college_aictefaculty.AICTEFacultyType =AICTEFacultyClass.FacultyType == "1" ? "UG" :"PG";
                    //jntuh_college_aictefaculty.JobType = AICTEFacultyClass.JobType == "1" ? "FT" :"PT";
                    // jntuh_college_aictefaculty.Email = AICTEFacultyClass.Email;
                    // jntuh_college_aictefaculty.Mobile = AICTEFacultyClass.Mobile;
                    // if(AICTEFacultyClass.ExactDesignation=="4")
                    //{
                    //    jntuh_college_aictefaculty.ExactDesignation = AICTEFacultyClass.OtherDesignation;
                    //}
                    //else{
                    //    jntuh_college_aictefaculty.ExactDesignation = AICTEFacultyClass.ExactDesignation;
                    //}

                    //  jntuh_college_aictefaculty.DateOfJoiningTheInstitute = Convert.ToDateTime(AICTEFacultyClass.DateOfJoiningTheInstitute);
                    //  jntuh_college_aictefaculty.AppointmentType = AICTEFacultyClass.AppionmentType;
                    //  jntuh_college_aictefaculty.Doctorate = AICTEFacultyClass.Doctorate == "1" ? "Yes" : "No";
                    //  jntuh_college_aictefaculty.MastersDegree = AICTEFacultyClass.MastersDegree;
                    //  jntuh_college_aictefaculty.BachelorsDegree = AICTEFacultyClass.BachelorsDegree;
                    // jntuh_college_aictefaculty.OtherQualification = AICTEFacultyClass.OtherQualification;


                    jntuh_college_aictefaculty.AICTEFacultyType = null;
                    jntuh_college_aictefaculty.JobType = null;
                    jntuh_college_aictefaculty.Email = null;
                    jntuh_college_aictefaculty.Mobile = null;
                    jntuh_college_aictefaculty.ExactDesignation = null;


                    jntuh_college_aictefaculty.DateOfJoiningTheInstitute = null;
                    jntuh_college_aictefaculty.AppointmentType = null;
                    jntuh_college_aictefaculty.Doctorate = null;
                    jntuh_college_aictefaculty.MastersDegree = null;
                    jntuh_college_aictefaculty.BachelorsDegree = null;
                    jntuh_college_aictefaculty.OtherQualification = "Test";


                    jntuh_college_aictefaculty.IsActive = true;
                    jntuh_college_aictefaculty.CreatedBy = userid;
                    jntuh_college_aictefaculty.CreatedOn = DateTime.Now;
                    jntuh_college_aictefaculty.UpdatedBy = null;
                    jntuh_college_aictefaculty.UpdatedOn = null;
                    db.jntuh_college_aictefaculty.Add(jntuh_college_aictefaculty);
                    db.SaveChanges();

                    TempData["message"] = "Registration completed successfully.";
                    //ViewBag.message = "Faculty Is Registrated Succesfully ";
                    return RedirectToAction("AicteFaculty");
                }
                else
                {
                    return RedirectToAction("AicteFaculty");
                }
            }

            else
            {

                return RedirectToAction("AicteFaculty");
                //ModelState.AddModelError(string.Empty,"* marks Fields are mandatory");
            }
            // return View();
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult CollegeInactiveFaculty()
        {
            return RedirectToAction("CollegeDashboard", "Dashboard");
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int Collegeid = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //int Collegeid = Convert.ToInt32(UAAAS.Models.Utilities.DecryptString(CollegeId.ToString(), System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"])); ;
            if (Collegeid == 375)
            {
                Collegeid = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            var FacultyRegistrationList = new List<FacultyRegistration>();
            if (Collegeid != null)
            {
                var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                string[] strRegnos = jntuh_college_faculty_registered.Select(cf => cf.RegistrationNumber).ToArray();


                var jntuh_college_faculty_registered_new = db.jntuh_college_principal_registered.Where(cf => cf.collegeId == Collegeid).ToList();
                var principalRegno = jntuh_college_faculty_registered_new.Select(cf => cf.RegistrationNumber).FirstOrDefault();

                var registeredFaculty = principalRegno != null ? db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList()
                   : db.jntuh_registered_faculty.Where(rf => strRegnos.Contains(rf.RegistrationNumber.Trim())).ToList();//&& rf.RegistrationNumber != principalRegno && rf.DepartmentId == departmentid


                //education categoryIds UG,PG,PHD...........
                var jntuh_education_category = db.jntuh_education_category.ToList();

                var jntuh_registered_faculty1 = registeredFaculty.Select(rf => new
                {
                    Id = rf.id,
                    type = rf.type,
                    Absent = rf.Absent != null ? (bool)rf.Absent : false,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE != null ? (bool)rf.NotQualifiedAsperAICTE : false,
                    InvalidPANNo = rf.InvalidPANNumber != null ? (bool)rf.InvalidPANNumber : false,
                    InCompleteCeritificates = rf.IncompleteCertificates != null ? (bool)rf.IncompleteCertificates : false,
                    PANNumber = rf.PANNumber,
                    XeroxcopyofcertificatesFlag = rf.Xeroxcopyofcertificates != null ? (bool)rf.Xeroxcopyofcertificates : false,
                    NOrelevantUgFlag = rf.NoRelevantUG == "Yes" ? true : false,
                    NOrelevantPgFlag = rf.NoRelevantPG == "Yes" ? true : false,
                    NOrelevantPhdFlag = rf.NORelevantPHD == "Yes" ? true : false,
                    BlacklistFaculty = rf.Blacklistfaculy != null ? (bool)rf.Blacklistfaculy : false,
                    NotIdentityFiedForAnyProgramFlag = rf.NotIdentityfiedForanyProgram != null ? (bool)rf.NotIdentityfiedForanyProgram : false,
                    OriginalCertificatesnotshownFlag = rf.OriginalCertificatesNotShown != null ? (bool)rf.OriginalCertificatesNotShown : false,
                    NoSCM = rf.NoSCM != null ? (bool)rf.NoSCM : false,
                    Invaliddegree = rf.Invaliddegree != null ? (bool)(rf.Invaliddegree) : false,
                    BAS = rf.BAS,
                    InvalidAadhaar = rf.InvalidAadhaar,
                    OriginalsVerifiedUG = rf.OriginalsVerifiedUG == true ? true : false,
                    OriginalsVerifiedPHD = rf.OriginalsVerifiedPHD == true ? true : false,
                    VerificationStatus = rf.AbsentforVerification != null ? (bool)rf.AbsentforVerification : false,
                    //PhotocopyofPAN = rf.PhotoCopyofPAN != null ? (bool)rf.PhotoCopyofPAN : false,
                    PhdUndertakingDocumentstatus = rf.PhdUndertakingDocumentstatus != null ? (bool)(rf.PhdUndertakingDocumentstatus) : false,
                    PHDUndertakingDocumentView = rf.PHDUndertakingDocument,
                    PhdUndertakingDocumentText = rf.PhdUndertakingDocumentText,
                    // AppliedPAN = rf.AppliedPAN != null ? (bool)(rf.AppliedPAN) : false,
                    Notin116 = rf.Notin116,
                    Blacklistfaculy = rf.Blacklistfaculy,
                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.jntuh_department != null ? rf.jntuh_department.departmentName : "",
                    HighestDegreeID = rf.jntuh_registered_faculty_education.Count() != 0 ? rf.jntuh_registered_faculty_education.Select(e => e.educationId).Max() : 0,
                    IsApproved = rf.isApproved,
                    PanNumber = rf.PANNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FirstName + rf.MiddleName + rf.LastName,
                    FacultyEducation = rf.jntuh_registered_faculty_education,
                    DegreeId = rf.jntuh_registered_faculty_education.Count(e => e.facultyId == rf.id) > 0 ? rf.jntuh_registered_faculty_education.Where(e => e.facultyId == rf.id).Select(e => e.educationId).Max() : 0,
                    DepartmentId = rf.DepartmentId



                }).ToList();

                var RegistrationNumbersCleared = jntuh_registered_faculty1.Where(rf => rf.type != "Adjunct" && ((rf.NotQualifiedAsperAICTE == false || rf.NotQualifiedAsperAICTE == null) && (rf.NoSCM == false || rf.NoSCM == null) && (rf.PANNumber != null) && (rf.InCompleteCeritificates == false || rf.InCompleteCeritificates == null) && (rf.Blacklistfaculy == false) &&
                                                 rf.NOrelevantUgFlag == false && rf.NOrelevantPgFlag == false && rf.NOrelevantPhdFlag == false && (rf.InvalidPANNo == false || rf.InvalidPANNo == null) && (rf.XeroxcopyofcertificatesFlag == false || rf.XeroxcopyofcertificatesFlag == null) &&  // (rf.AppliedPAN == false || rf.AppliedPAN == null) && && (rf.MultipleReginSamecoll == false || rf.MultipleReginSamecoll == null) && (rf.PhdUndertakingDocumentstatus == true || rf.PhdUndertakingDocumentstatus == null) 
                                                 (rf.Invaliddegree == false || rf.Invaliddegree == null) && (rf.VerificationStatus == false || rf.VerificationStatus == null) && rf.InvalidAadhaar != "Yes" && rf.OriginalsVerifiedUG == false && rf.OriginalsVerifiedPHD == false && (rf.DepartmentId != 61 || rf.DepartmentId != 27) && rf.HighestDegreeID >= 4)).Select(e => e.RegistrationNumber).ToArray();

                var jntuh_registered_faculty = jntuh_registered_faculty1.Where(e => !RegistrationNumbersCleared.Contains(e.RegistrationNumber)).Select(rf => new
                {
                    Id = rf.Id,
                    type = rf.type,
                    Absent = rf.Absent,
                    NotQualifiedAsperAICTE = rf.NotQualifiedAsperAICTE,
                    rf.InCompleteCeritificates,
                    rf.InvalidPANNo,
                    rf.NOrelevantPgFlag,
                    rf.NOrelevantUgFlag,
                    rf.NOrelevantPhdFlag,
                    rf.VerificationStatus,
                    rf.XeroxcopyofcertificatesFlag,
                    NoSCM = rf.NoSCM,
                    PANNumber = rf.PANNumber,
                    rf.NotIdentityFiedForAnyProgramFlag,
                    rf.InvalidAadhaar,
                    rf.BAS,
                    rf.OriginalsVerifiedUG,
                    rf.OriginalsVerifiedPHD,
                    rf.Invaliddegree,
                    rf.OriginalCertificatesnotshownFlag,
                    Blacklistfaculy = rf.Blacklistfaculy,

                    PHDundertakingnotsubmitted = rf.PhdUndertakingDocumentstatus,
                    Notin116 = rf.Notin116,

                    RegistrationNumber = rf.RegistrationNumber,
                    Department = rf.Department,
                    HighestDegree = jntuh_education_category.Where(c => c.id == rf.HighestDegreeID).Select(c => c.educationCategoryName).FirstOrDefault(),
                    Recruitedfor = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.IdentifiedFor).FirstOrDefault(),
                    SpecializationId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.SpecializationId).FirstOrDefault(),
                    DeptId = jntuh_college_faculty_registered.Where(c => c.RegistrationNumber == rf.RegistrationNumber).Select(c => c.DepartmentId).FirstOrDefault(),
                    PanNumber = rf.PanNumber,
                    AadhaarNumber = rf.AadhaarNumber,
                    Photo = rf.Photo,
                    FullName = rf.FullName,
                    faculty_education = rf.FacultyEducation,
                    HighestDegreeID = rf.HighestDegreeID
                }).ToList();


                foreach (var item in jntuh_registered_faculty)
                {
                    string Reason = null;
                    FacultyRegistration facultyregistered = new FacultyRegistration();
                    facultyregistered.id = item.Id;
                    facultyregistered.RegistrationNumber = item.RegistrationNumber;
                    facultyregistered.FirstName = item.FullName;
                    facultyregistered.department = item.Department;
                    facultyregistered.DepartmentId = item.DeptId;
                    facultyregistered.jntuh_registered_faculty_education = item.faculty_education;
                    facultyregistered.facultyPhoto = item.Photo;
                    facultyregistered.Absent = item.Absent != null && (bool)item.Absent;
                    facultyregistered.NOTQualifiedAsPerAICTE = item.NotQualifiedAsperAICTE != null && (bool)item.NotQualifiedAsperAICTE;
                    facultyregistered.NoSCM = item.NoSCM != null && (bool)item.NoSCM;
                    facultyregistered.PANNumber = item.PANNumber;
                    facultyregistered.PHDundertakingnotsubmitted = item.PHDundertakingnotsubmitted != null && (bool)item.PHDundertakingnotsubmitted;
                    facultyregistered.BlacklistFaculty = item.Blacklistfaculy != null && (bool)item.Blacklistfaculy;
                    facultyregistered.DegreeId = item.HighestDegreeID;


                    //if (item.Absent == true)
                    //    Reason += "Absent";
                    if (item.type == "Adjunct")
                    {
                        if (Reason != null)
                            Reason += ",Adjunct Faculty";
                        else
                            Reason += "Adjunct Faculty";
                    }

                    if (item.XeroxcopyofcertificatesFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",Xerox copyof certificates";
                        else
                            Reason += "Xerox copyof certificates";
                    }

                    if (item.NOrelevantUgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant UG";
                        else
                            Reason += "NO Relevant UG";
                    }

                    if (item.NOrelevantPgFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PG";
                        else
                            Reason += "NO Relevant PG";
                    }

                    if (item.NOrelevantPhdFlag == true)
                    {
                        if (Reason != null)
                            Reason += ",NO Relevant PHD";
                        else
                            Reason += "NO Relevant PHD";
                    }

                    if (item.NotQualifiedAsperAICTE == true || item.HighestDegreeID < 4)
                    {
                        if (Reason != null)
                            Reason += ",NOT Qualified AsPerAICTE";
                        else
                            Reason += "NOT Qualified AsPerAICTE";
                    }

                    if (item.InvalidPANNo == true)
                    {
                        if (Reason != null)
                            Reason += ",InvalidPANNumber";
                        else
                            Reason += "InvalidPANNumber";
                    }

                    if (item.InCompleteCeritificates == true)
                    {
                        if (Reason != null)
                            Reason += ",InComplete Ceritificates";
                        else
                            Reason += "InComplete Ceritificates";
                    }

                    if (item.NoSCM == true)
                    {
                        if (Reason != null)
                            Reason += ",NoSCM";
                        else
                            Reason += "NoSCM";
                    }

                    //if (item.OriginalCertificatesnotshownFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",Original Certificates notshown";
                    //    else
                    //        Reason += "Original Certificates notshown";
                    //}

                    if (item.PANNumber == null)
                    {
                        if (Reason != null)
                            Reason += ",No PANNumber";
                        else
                            Reason += "No PANNumber";
                    }

                    //if (item.NotIdentityFiedForAnyProgramFlag == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",NotIdentityFied ForAnyProgram";
                    //    else
                    //        Reason += "NotIdentityFied ForAnyProgram";
                    //}

                    //if (item.SamePANUsedByMultipleFaculty == true)
                    //{
                    //    if (Reason != null)
                    //        Reason += ",SamePANUsedByMultipleFaculty";
                    //    else
                    //        Reason += "SamePANUsedByMultipleFaculty";
                    //}

                    if (item.InvalidAadhaar == "Yes")
                    {
                        if (Reason != null)
                            Reason += ",No/Invalid Aadhaar Document";
                        else
                            Reason += "No/Invalid Aadhaar Document";
                    }

                    //if (item.BASStatusOld == "Yes")
                    //{
                    //    if (Reason != null)
                    //        Reason += ",BAS Flag";
                    //    else
                    //        Reason += "BAS Flag";
                    //}

                    if (item.OriginalsVerifiedUG == true)
                    {
                        if (Reason != null)
                            Reason += ",Complaint PHD Faculty";
                        else
                            Reason += "Complaint PHD Faculty";
                    }

                    if (item.OriginalsVerifiedPHD == true)
                    {
                        if (Reason != null)
                            Reason += ",No Guide Sign in PHD Thesis";
                        else
                            Reason += "No Guide Sign in PHD Thesis";
                    }
                    if (item.Blacklistfaculy == true)
                    {
                        if (Reason != null)
                            Reason += ",Blacklistfaculy";
                        else
                            Reason += "Blacklistfaculy";
                    }
                    if (item.VerificationStatus == true)
                    {
                        if (Reason != null)
                            Reason += ",Absent for Physical Verification";
                        else
                            Reason += "Absent for Physical Verification";
                    }
                    facultyregistered.DeactivationReason = Reason;
                    FacultyRegistrationList.Add(facultyregistered);
                }
            }
            return View(FacultyRegistrationList);
        }


        [Authorize(Roles = "Admin,SuperAdmin,College")]
        [HttpGet]
        public ActionResult InductionProgram()
        {
            // return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {

                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("College", "Dashboard");
                }
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "Induction Program")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var AICTEApprovalLettr =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                ViewBag.AICTEApprovalLettr = AICTEApprovalLettr;
                return View();
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,College")]
        public ActionResult InductionProgram(HttpPostedFileBase fileUploader, string command)
        {
            // return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {
                string InductionPath = "~/Content/Upload/CollegeEnclosures/InductionProgram";
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("LogOn", "Account");
                    // userCollegeID = Convert.ToInt32(collegeId);
                }
                //To Save File in jntuh_college_enclosures
                string fileName = string.Empty;
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "Induction Program")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                jntuh_college_enclosures.collegeID = userCollegeID;
                jntuh_college_enclosures.enclosureId = enclosureId;
                jntuh_college_enclosures.isActive = true;
                if (fileUploader != null)
                {
                    if (!Directory.Exists(Server.MapPath(InductionPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(InductionPath));
                    }

                    string ext = Path.GetExtension(fileUploader.FileName);

                    if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                    {
                        fileName =
                       db.jntuh_college.Where(c => c.id == userCollegeID).Select(c => c.collegeCode).FirstOrDefault() +
                       "_IP_" + enclosureId + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ext;
                        fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/InductionProgram"),
                            fileName));
                        jntuh_college_enclosures.path = fileName;
                    }

                }
                else if (!string.IsNullOrEmpty(college_enclosures.path))
                {
                    fileName = college_enclosures.path;
                    jntuh_college_enclosures.path = fileName;
                }

                if (command == "Delete")
                {
                    if (college_enclosures != null)
                    {
                        db.jntuh_college_enclosures.Remove(college_enclosures);
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (college_enclosures == null)
                    {
                        jntuh_college_enclosures.createdBy = userID;
                        jntuh_college_enclosures.createdOn = DateTime.Now;
                        db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                        db.SaveChanges();
                    }
                    else
                    {
                        college_enclosures.path = fileName;
                        college_enclosures.updatedBy = userID;
                        college_enclosures.updatedOn = DateTime.Now;
                        db.Entry(college_enclosures).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("InductionProgram");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        public ActionResult DownloadInducationProgram()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "Content/Upload/Downloads/";
            byte[] fileBytes = System.IO.File.ReadAllBytes(path + "Induction Program.xlsx");
            string fileName = "Induction Program.xlsx";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public ActionResult GetRegisteredFaculty()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var jntuh_registered_Faculty = db.jntuh_registered_faculty.
                Select(s => new FacultyRegistration
                {
                    id = s.id,
                    RegistrationNumber = s.RegistrationNumber,
                    FirstName = s.FirstName,
                    MiddleName = s.MiddleName,
                    LastName = s.LastName,
                    DepartmentId = s.DepartmentId,
                    DesignationId = s.DesignationId,
                    PANNumber = s.PANNumber,
                    Email = s.Email,
                    Mobile = s.Mobile,
                    BlacklistFaculty = s.Blacklistfaculy
                }).ToList();
            return View(jntuh_registered_Faculty);
        }

        #region 08/08/2019 Create New Screen Upload EXCEL from Colleges Only Enggering Colleges and Pharma.D or Pharma.D (PB) Hospital Data Getting  written by Narayana Reddy
        //[ActionName("Collegefacultydetails")]
        [Authorize(Roles = "College")]
        public ActionResult CollegeFacultyData()
        {
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("College", "Dashboard");
                }
                var specializations = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == 11 && i.proposedIntake != 0).Select(s => s.specializationId).ToArray();
                var departments = db.jntuh_specialization.Where(s => specializations.Contains(s.id)).Select(s => s.departmentId).ToArray();
                var degreeids = db.jntuh_department.Where(r => departments.Contains(r.id)).Select(d => d.degreeId).ToArray();
                if (!degreeids.Contains(4))
                {
                    return RedirectToAction("College", "Dashboard");
                }
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "College Faculty Details")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var facultydetailsfile =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                ViewBag.facultydetails = facultydetailsfile;
                return View();
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
            return View();
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult FacultyDetailsUpload(HttpPostedFileBase fileUploader, string collegeId)
        {
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = Convert.ToInt32(collegeId);
                }
                //To Save File in jntuh_college_enclosures
                string fileName = string.Empty;
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                var specializations = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == academicyearId && i.proposedIntake != 0).Select(s => s.specializationId).ToArray();
                var departments = db.jntuh_specialization.Where(s => specializations.Contains(s.id)).Select(s => s.departmentId).ToArray();
                var degreeids = db.jntuh_department.Where(r => departments.Contains(r.id)).Select(d => d.degreeId).ToArray();
                if (!degreeids.Contains(4))
                {
                    return RedirectToAction("College", "Dashboard");
                }
                int enclosureId =
                    db.jntuh_enclosures.Where(e => e.documentName == "College Faculty Details")
                        .Select(e => e.id)
                        .FirstOrDefault();
                var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                var jntuh_college =
                    db.jntuh_college.Where(c => c.id == userCollegeID && c.isActive == true).Select(c => c).FirstOrDefault();
                jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                jntuh_college_enclosures.collegeID = userCollegeID;
                jntuh_college_enclosures.academicyearId = academicyearId;
                jntuh_college_enclosures.enclosureId = enclosureId;
                jntuh_college_enclosures.isActive = true;
                if (fileUploader != null)
                {
                    string ext = Path.GetExtension(fileUploader.FileName);
                    //DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + faculty.FirstName.Substring(0, 1) + "-" + faculty.LastName.Substring(0, 1)
                    if (college_enclosures != null && !String.IsNullOrEmpty(college_enclosures.path))
                    {
                        fileName = college_enclosures.path;
                    }
                    else
                    {
                        fileName =
                            jntuh_college.collegeCode + "-" + jntuh_college.collegeName +
                            "_CFD_" + enclosureId + ext;
                    }
                    fileUploader.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/College Faculty Details"),
                        fileName));
                    jntuh_college_enclosures.path = fileName;
                }
                else if (!string.IsNullOrEmpty(college_enclosures.path))
                {
                    fileName = college_enclosures.path;
                    jntuh_college_enclosures.path = fileName;
                }

                if (college_enclosures == null)
                {
                    jntuh_college_enclosures.createdBy = userID;
                    jntuh_college_enclosures.createdOn = DateTime.Now;
                    db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                    db.SaveChanges();
                    TempData["Success"] = "College Faculty Details are Updated successfully";
                }
                else
                {
                    college_enclosures.path = fileName;
                    college_enclosures.updatedBy = userID;
                    college_enclosures.updatedOn = DateTime.Now;
                    db.Entry(college_enclosures).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "College Faculty Details are Saved successfully";
                }
                return RedirectToAction("CollegeFacultyData");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
            return RedirectToAction("CollegeFacultyData");
        }

        //09/05/2019 Create New Screen Upload EXCEL and EOU pdf from Colleges Only Pharma D,Pharma D.PB Colleges by Narayana Reddy
        [Authorize(Roles = "College")]
        [HttpGet]
        public ActionResult CollegeHospitalmembers()
        {
            return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {
                pharmacyhospitaldata hospitalmou = new pharmacyhospitaldata();
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0 || userCollegeID == null)
                {
                    return RedirectToAction("College", "Dashboard");
                }
                var specializations = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == 11 && i.proposedIntake != 0).Select(s => s.specializationId).ToArray();
                var departments = db.jntuh_specialization.Where(s => specializations.Contains(s.id)).Select(s => s.departmentId).ToArray();
                var degreeids = db.jntuh_department.Where(r => departments.Contains(r.id)).Select(d => d.degreeId).ToArray();
                if (!degreeids.Contains(9))
                {
                    if (!degreeids.Contains(10))
                    {
                        return RedirectToAction("College", "Dashboard");
                    }

                }
                int enclosureId1 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital MOU")
                        .Select(e => e.id)
                        .FirstOrDefault();
                hospitalmou.hospitaldocumentfile =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId1 && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                int enclosureId2 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital Faculty/Student Data")
                        .Select(e => e.id)
                        .FirstOrDefault();
                hospitalmou.facultyinformationfile =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId2 && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();

                int enclosureId3 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital Time Tables")
                        .Select(e => e.id)
                        .FirstOrDefault();
                hospitalmou.timetablesheetfile =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId3 && e.collegeID == userCollegeID)
                        .OrderByDescending(a => a.id)
                        .Select(e => e.path)
                        .FirstOrDefault();
                return View(hospitalmou);
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
        }

        [Authorize(Roles = "College")]
        [HttpPost]
        public ActionResult CollegeHospitalmembers(pharmacyhospitaldata pharmacy, string collegeId)
        {
            return RedirectToAction("College", "Dashboard");
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID =
                    db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = Convert.ToInt32(collegeId);
                }
                //To Save File in jntuh_college_enclosures
                string fileName = string.Empty;
                int presentAY =
                    db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true)
                        .Select(a => a.actualYear)
                        .FirstOrDefault();
                int academicyearId =
                    db.jntuh_academic_year.Where(a => a.actualYear == (presentAY + 1))
                        .Select(a => a.id)
                        .FirstOrDefault();
                var specializations = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == academicyearId && i.proposedIntake != 0).Select(s => s.specializationId).ToArray();
                var departments = db.jntuh_specialization.Where(s => specializations.Contains(s.id)).Select(s => s.departmentId).ToArray();
                var degreeids = db.jntuh_department.Where(r => departments.Contains(r.id)).Select(d => d.degreeId).ToArray();
                if (!degreeids.Contains(9))
                {
                    if (!degreeids.Contains(10))
                    {
                        return RedirectToAction("College", "Dashboard");
                    }

                }
                var jntuh_college =
                    db.jntuh_college.Where(c => c.id == userCollegeID && c.isActive == true).Select(c => c).FirstOrDefault();
                if (pharmacy.hospitaldocument == null)
                {

                    TempData["ERROR"] = "Files can't be empty.";
                    return RedirectToAction("CollegeHospitalmembers");

                }
                if (pharmacy.facultyinformation == null)
                {

                    TempData["ERROR"] = "Files can't be empty.";
                    return RedirectToAction("CollegeHospitalmembers");

                }
                if (pharmacy.timetablesheet == null)
                {

                    TempData["ERROR"] = "Files can't be empty.";
                    return RedirectToAction("CollegeHospitalmembers");

                }
                if (pharmacy.hospitaldocument != null)
                {
                    int enclosureId =
                   db.jntuh_enclosures.Where(e => e.documentName == "Hospital MOU")
                       .Select(e => e.id)
                       .FirstOrDefault();
                    var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                    jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                    jntuh_college_enclosures.collegeID = userCollegeID;
                    jntuh_college_enclosures.academicyearId = academicyearId;
                    jntuh_college_enclosures.enclosureId = enclosureId;
                    jntuh_college_enclosures.isActive = true;
                    string ext = Path.GetExtension(pharmacy.hospitaldocument.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        if (college_enclosures != null && !String.IsNullOrEmpty(college_enclosures.path))
                        {
                            fileName = college_enclosures.path;
                        }
                        else
                        {
                            fileName =
                                jntuh_college.collegeCode + "-" + jntuh_college.collegeName +
                                "_HMOU_" + enclosureId + ext;
                        }
                        //HospitalMOU Means College Upload pdf Hospital MOU
                        pharmacy.hospitaldocument.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/HospitalMOU"),
                            fileName));
                        jntuh_college_enclosures.path = fileName;
                    }
                    else
                    {
                        TempData["ERROR"] = "File upload in correct format.";
                        return RedirectToAction("CollegeHospitalmembers");

                    }
                    if (college_enclosures == null)
                    {
                        jntuh_college_enclosures.createdBy = userID;
                        jntuh_college_enclosures.createdOn = DateTime.Now;
                        db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                        db.SaveChanges();
                        TempData["Success"] = "Details are Saved successfully";

                    }
                    else
                    {
                        college_enclosures.path = fileName;
                        college_enclosures.updatedBy = userID;
                        college_enclosures.updatedOn = DateTime.Now;
                        db.Entry(college_enclosures).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Details are Updated successfully";
                    }

                }
                if (pharmacy.facultyinformation != null)
                {
                    int enclosureId =
                   db.jntuh_enclosures.Where(e => e.documentName == "Hospital Faculty/Student Data")
                       .Select(e => e.id)
                       .FirstOrDefault();
                    var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                    jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                    jntuh_college_enclosures.collegeID = userCollegeID;
                    jntuh_college_enclosures.academicyearId = academicyearId;
                    jntuh_college_enclosures.enclosureId = enclosureId;
                    jntuh_college_enclosures.isActive = true;
                    string ext = Path.GetExtension(pharmacy.facultyinformation.FileName);
                    if (ext.ToUpper().Equals(".XLS") || ext.ToUpper().Equals(".XLSX"))
                    {
                        if (college_enclosures != null && !String.IsNullOrEmpty(college_enclosures.path))
                        {
                            fileName = college_enclosures.path;
                        }
                        else
                        {
                            fileName =
                                jntuh_college.collegeCode + "-" + jntuh_college.collegeName +
                                "_HFS_" + enclosureId + ext;
                        }
                        //HospitalFS College Upload Excel Faculty/Student Information
                        pharmacy.facultyinformation.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/HospitalFS"),
                            fileName));
                        jntuh_college_enclosures.path = fileName;
                    }
                    else
                    {
                        TempData["ERROR"] = "File upload in correct format.";
                        return RedirectToAction("CollegeHospitalmembers");

                    }
                    if (college_enclosures == null)
                    {
                        jntuh_college_enclosures.createdBy = userID;
                        jntuh_college_enclosures.createdOn = DateTime.Now;
                        db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                        db.SaveChanges();
                        TempData["Success"] = "Details are Saved successfully";

                    }
                    else
                    {
                        college_enclosures.path = fileName;
                        college_enclosures.updatedBy = userID;
                        college_enclosures.updatedOn = DateTime.Now;
                        db.Entry(college_enclosures).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Details are Updated successfully";
                    }
                }
                if (pharmacy.timetablesheet != null)
                {
                    int enclosureId =
                   db.jntuh_enclosures.Where(e => e.documentName == "Hospital Time Tables")
                       .Select(e => e.id)
                       .FirstOrDefault();
                    var college_enclosures =
                    db.jntuh_college_enclosures.Where(e => e.enclosureId == enclosureId && e.collegeID == userCollegeID)
                        .Select(e => e)
                        .FirstOrDefault();
                    jntuh_college_enclosures jntuh_college_enclosures = new jntuh_college_enclosures();
                    jntuh_college_enclosures.collegeID = userCollegeID;
                    jntuh_college_enclosures.academicyearId = academicyearId;
                    jntuh_college_enclosures.enclosureId = enclosureId;
                    jntuh_college_enclosures.isActive = true;
                    string ext = Path.GetExtension(pharmacy.timetablesheet.FileName);
                    if (ext.ToUpper().Equals(".PDF"))
                    {
                        if (college_enclosures != null && !String.IsNullOrEmpty(college_enclosures.path))
                        {
                            fileName = college_enclosures.path;
                        }
                        else
                        {
                            fileName =
                                jntuh_college.collegeCode + "-" + jntuh_college.collegeName +
                                "_HTT_" + enclosureId + ext;
                        }
                        //HospitalTT Means College Upload pdf Hospital Time Tables
                        pharmacy.timetablesheet.SaveAs(string.Format("{0}/{1}", Server.MapPath("~/Content/Upload/CollegeEnclosures/HospitalTT"),
                            fileName));
                        jntuh_college_enclosures.path = fileName;
                    }
                    else
                    {
                        TempData["ERROR"] = "File upload in correct format.";
                        return RedirectToAction("CollegeHospitalmembers");

                    }
                    if (college_enclosures == null)
                    {
                        jntuh_college_enclosures.createdBy = userID;
                        jntuh_college_enclosures.createdOn = DateTime.Now;
                        db.jntuh_college_enclosures.Add(jntuh_college_enclosures);
                        db.SaveChanges();
                        TempData["Success"] = "Details are Saved successfully";

                    }
                    else
                    {
                        college_enclosures.path = fileName;
                        college_enclosures.updatedBy = userID;
                        college_enclosures.updatedOn = DateTime.Now;
                        db.Entry(college_enclosures).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Details are Updated successfully";
                    }
                }

            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }
            return RedirectToAction("CollegeHospitalmembers");

        }

        //16/09/2019 Create New Screen for Admin only See Hospital Data
        [Authorize(Roles = "Admin")]
        public ActionResult AllHospitalMembersData()
        {
            List<pharmacyhospitaldata> pharmacyhospitaldatalist = new List<pharmacyhospitaldata>();
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                var collegeids = (from ie in db.jntuh_college_intake_existing
                                  join s in db.jntuh_specialization on ie.specializationId equals s.id
                                  join d in db.jntuh_department on s.departmentId equals d.id
                                  where ie.academicYearId == 11 && (d.degreeId == 10 || d.degreeId == 9)
                                  select new
                                  {
                                      ie.collegeId
                                  }).Distinct().ToArray();
                int enclosureId1 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital MOU")
                        .Select(e => e.id)
                        .FirstOrDefault();
                int enclosureId2 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital Faculty/Student Data")
                        .Select(e => e.id)
                        .FirstOrDefault();
                int enclosureId3 =
                    db.jntuh_enclosures.Where(e => e.documentName == "Hospital Time Tables")
                        .Select(e => e.id)
                        .FirstOrDefault();

                foreach (var collegeid in collegeids)
                {
                    pharmacyhospitaldata hospitaldata = new pharmacyhospitaldata();
                    int id = Convert.ToInt32(collegeid.collegeId);
                    var enclosures =
                        db.jntuh_college_enclosures.Where(
                            r =>
                                r.collegeID == id &&
                                (r.enclosureId == enclosureId1 || r.enclosureId == enclosureId2 ||
                                 r.enclosureId == enclosureId3)).Select(s => s).ToList();
                    var college = db.jntuh_college.Where(c => c.id == id && c.isActive == true).Select(s => s).FirstOrDefault();
                    hospitaldata.collegecode = college.collegeCode;
                    hospitaldata.collegename = college.collegeName;
                    hospitaldata.hospitaldocumentfile =
                        enclosures.Where(r => r.enclosureId == enclosureId1).Select(s => s.path).FirstOrDefault();
                    hospitaldata.facultyinformationfile =
                        enclosures.Where(r => r.enclosureId == enclosureId2).Select(s => s.path).FirstOrDefault();
                    hospitaldata.timetablesheetfile =
                       enclosures.Where(r => r.enclosureId == enclosureId3).Select(s => s.path).FirstOrDefault();
                    pharmacyhospitaldatalist.Add(hospitaldata);
                }

            }
            return View(pharmacyhospitaldatalist);
        }

        #endregion
    }

    public class pharmacyhospitaldata
    {
        public string collegecode { get; set; }
        public string collegename { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase hospitaldocument { get; set; }
        public string hospitaldocumentfile { get; set; }
        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase facultyinformation { get; set; }
        public string facultyinformationfile { get; set; }

        [Required(ErrorMessage = "*")]
        public HttpPostedFileBase timetablesheet { get; set; }
        public string timetablesheetfile { get; set; }
    }
    public class actionlinks
    {
        public int Id { get; set; }
        public string collegeId { get; set; }
        public int ParentId { get; set; }
        public string Text { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public string Url { get; set; }
        //Id = menu.id,
        //              ParentId = parent,
        //              Text = menu.menuName,
        //              Url = menu.menuControllerName.Equals(string.Empty) ? "#" : string.Format("/{0}/{1}", menu.menuControllerName, menu.menuActionName),
        //              OpenInNewWindow = false,
        //              SortOrder = (int)menu.menuOrder
    }
    public class acdemicFees
    {
        public int AcdemicYearId { get; set; }
        public int Fee { get; set; }

    }

    public class CollegeApprovedList
    {
        public int id { get; set; }
        public int AcademicYearId { get; set; }
        public int collegeId { get; set; }
        public string collegecode { get; set; }
        public string CollegeName { get; set; }
        public string CollegeType { get; set; }
        public int SpecializationId { get; set; }
        public int ShiftId { get; set; }
        public int ApprovedIntake { get; set; }
        public int AdmittedIntake { get; set; }
        public string Coursestatus { get; set; }
        public bool IsActive { get; set; }
        public int degreeId { get; set; }
        public int deptId { get; set; }
        public int SpecilizationId { get; set; }
        public string Department { get; set; }
        public string Degree { get; set; }
        public string SpecializationName { get; set; }
        public int FirstYearFee { get; set; }
        public int SecondYearFee { get; set; }
        public int ThirdYearFee { get; set; }
        public int FourthYearFee { get; set; }
        public int SpecializationwiseSalary { get; set; }


        public int ApprovedIntake1 { get; set; }
        public int AdmittedIntake1 { get; set; }


        public int ApprovedIntake2 { get; set; }
        public int AdmittedIntake2 { get; set; }
        public int LeteralentryIntake2 { get; set; }

        public int ApprovedIntake3 { get; set; }
        public int AdmittedIntake3 { get; set; }
        public int LeteralentryIntake3 { get; set; }

        public int ApprovedIntake4 { get; set; }
        public int AdmittedIntake4 { get; set; }
        public int LeteralentryIntake4 { get; set; }

        public string DisplayOrder { get; set; }


    }

    public class AffiliationFeeCollegeList
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string afrcFee { get; set; }
        public bool? isPending { get; set; }
        public int PendingFee { get; set; }
        public int DDAmount { get; set; }
        public List<CollegeApprovedList> CollegeApprovedList { get; set; }
    }

    public class AutonamusCollegeFee
    {
        public string CollegeName { get; set; }
        public int Fee { get; set; }
    }

    public class NotEnrolledData
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string Dept { get; set; }
        public string HTNo { get; set; }
        public string Name { get; set; }
    }

    public class StudentsAttendance
    {
        public int? CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string HTNo { get; set; }
        public string Name { get; set; }
        public string Dept { get; set; }
        public int October { get; set; }
        public int November { get; set; }
        public int December { get; set; }
        public int? January { get; set; }
        public int February { get; set; }
        public int March { get; set; }
        public int April { get; set; }
        public int? May { get; set; }
        public int? PrsentDays { get; set; }
        public string Percentage { get; set; }

    }

    public class CollegeFacultyNew
    {
        public string RegistrationNumber { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }
        public int? DesignationId { get; set; }
        public string Designation { get; set; }
        public string Specialization { get; set; }
        public string PANNumber { get; set; }
    }


    public class College_Dachboard
    {
        public int collegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string College_eamsetCode { get; set; }

        public string Chairman { get; set; }
        public string ChairmanPhoto { get; set; }
        public string Principal { get; set; }
        public string PrincipalRegNo { get; set; }
        public string PrincipalPhoto { get; set; }
        public string PrincipalMobile { get; set; }
        public string PrincipalEmail { get; set; }

        public int TotalFacultyCount { get; set; }
        public int TotalStudentCount { get; set; }
        public List<DeptWise_Faculty> DeptWise_FacultyCount { get; set; }
        public List<Academic_Year_Student_Count> YearWise_Student_Count { get; set; }
        public List<string> CollegePhotos { get; set; }

        public string CollegeEmail { get; set; }
        public string CollegePhoneNo { get; set; }
        public string CollegeMobile { get; set; }
        public string College_Website { get; set; }

        public string SocietyEmail { get; set; }
        public string SocietyPhoneNo { get; set; }
        public string SocietyMobile { get; set; }

        public string SecretaryEmail { get; set; }
        public string SecretaryPhoneNo { get; set; }
        public string SecretaryMobile { get; set; }

        public string SocietyName { get; set; }
        public string Address { get; set; }

        public string Director { get; set; }
        public string DirectorMobile { get; set; }
        public string DirectorPhoneNo { get; set; }
        public string DirectorEmail { get; set; }
    }

    public class DeptWise_Faculty
    {
        public int? Deptid { get; set; }
        public string Deptname { get; set; }
        public string Department { get; set; }
        public string Degree { get; set; }
        public int FacultyCount { get; set; }
    }

    public class Academic_Year_Student_Count
    {
        public int SpecializationId { get; set; }
        public string CourseName { get; set; }
        public int ApprovedIntake { get; set; }
        public string Year { get; set; }
        public int Approved_Count { get; set; }
        public int Admitted_Count { get; set; }
    }

    public class BASMonthlyReports
    {
        public string title { get; set; }
        public string path { get; set; }
        public DateTime? createdDate { get; set; }
    }

    public class DiaryDetails
    {
        public int collegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegeAddress { get; set; }
        public string College_eamsetCode { get; set; }

        public string Director { get; set; }
        public string DirectorMobile { get; set; }
        public string DirectorPhoneNo { get; set; }
        public string DirectorEmail { get; set; }
        public string DirectorFax { get; set; }

        public string Principal { get; set; }
        public string PrincipalFirstName { get; set; }
        public string PrincipalMiddleName { get; set; }
        public string PrincipalLastName { get; set; }

        // [Required(ErrorMessage = "*")]
        //[Remote("GetRegData", "Dashboard", HttpMethod = "POST", ErrorMessage = "Registration Number is not Exists.")]
        public string PrincipalRegNo { get; set; }

        public string PrincipalMobile { get; set; }
        public string PrincipalEmail { get; set; }

        [Required(ErrorMessage = "*")]
        public string CollegeEmail { get; set; }
        [Required(ErrorMessage = "*")]
        public string CollegePhoneNo { get; set; }
        [Required(ErrorMessage = "*")]
        public string CollegeMobile { get; set; }
        [Required(ErrorMessage = "*")]
        public string CollegeMandal { get; set; }
        [Required(ErrorMessage = "*")]
        public string CollegetownOrCity { get; set; }
        [Required(ErrorMessage = "*")]
        public int CollegeDistrictID { get; set; }
        public string CollegeDistrict { get; set; }


        public string SocietyName { get; set; }
        [Required(ErrorMessage = "*")]
        public string SocietyEmail { get; set; }
        [Required(ErrorMessage = "*")]
        public string SocietyPhoneNo { get; set; }
        [Required(ErrorMessage = "*")]
        public string SocietyMobile { get; set; }
        [Required(ErrorMessage = "*")]
        public string SocietyMandal { get; set; }
        [Required(ErrorMessage = "*")]
        public string SocietytownOrCity { get; set; }
        [Required(ErrorMessage = "*")]
        public int SocietyDistrictID { get; set; }
        public string SocietyDistrict { get; set; }

        public string Chairman { get; set; }
        [Required(ErrorMessage = "*")]
        public string SecretaryEmail { get; set; }
        [Required(ErrorMessage = "*")]
        public string SecretaryPhoneNo { get; set; }
        [Required(ErrorMessage = "*")]
        public string SecretaryMobile { get; set; }
        [Required(ErrorMessage = "*")]
        public string SecretaryMandal { get; set; }
        [Required(ErrorMessage = "*")]
        public string SecretarytownOrCity { get; set; }
        [Required(ErrorMessage = "*")]
        public int SecretaryDistrictID { get; set; }
        public string SecretaryDistrict { get; set; }
    }
}
