using ClosedXML.Excel;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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
    public class CollegesFeeController : BaseController
    {
        //Written by siva.
        // GET: /CollegesFee/
        uaaasDBContext db = new uaaasDBContext();
        List<SelectListItem> Orderlist = new List<SelectListItem>()
        {
            new SelectListItem(){Value = "01",Text = "B.Tech"},
            new SelectListItem(){Value = "02",Text = "B.Pharmacy"},
            new SelectListItem(){Value = "03",Text = "M.Tech"},
            new SelectListItem(){Value = "04",Text = "M.Pharmacy"},
            new SelectListItem(){Value = "05",Text = "MCA"},
            new SelectListItem(){Value = "06",Text = "MBA"},
            new SelectListItem(){Value = "07",Text = "5-Year MBA(Integrated)"},
            new SelectListItem(){Value = "08",Text = "MTM"},
            new SelectListItem(){Value = "09",Text = "Pharm.D"},
            new SelectListItem(){Value = "10",Text = "Pharm.D PB"},
            new SelectListItem(){Value = "07",Text = "MAM"},
        };

        List<SelectListItem> CollegesType = new List<SelectListItem>()
        {
            new SelectListItem(){Value = "Autonomous",Text = "Autonomous"},
            new SelectListItem(){Value = "Non-Autonomous",Text = "Non-Autonomous"}         
        };

        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var jntuh_college_dd_payments = db.jntuh_college_dd_payments.Where(s => s.IsActive == true).ToList();

            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            int date = DateTime.Today.Day;

            DateTime? TodayDate = new DateTime(year, month, date);

            DD_Payments DD = new DD_Payments();
            DD.Today_Transactons = jntuh_college_dd_payments.Where(a => a.CreatedOn == TodayDate).Count();
            DD.Today_Amount = jntuh_college_dd_payments.Where(a => a.CreatedOn == TodayDate).Select(a => a.PaidAmount).Sum();
            DD.TotalDD_Count = jntuh_college_dd_payments.Select(w => w.Id).Count();
            DD.PaymentAmount = jntuh_college_dd_payments.Select(a => a.PaidAmount).Sum();

            return View(DD);
        }

        #region Download Colleges DD Reports

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult DailyReports(int PaymentTypeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["FullMonthName"] = null;

            int year = DateTime.Today.Year; int month = DateTime.Today.Month; int date = DateTime.Today.Day;

            DateTime FromDate = new DateTime(year, month, date);
            var FrommonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(FromDate.Month);
            var FullMonthName = FrommonthName + "(" + year + ")";
            TempData["FullMonthName"] = FullMonthName;
            DateTime? TodayDate = new DateTime(year, month, date);

            var jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                             join cc in db.jntuh_college on dd.collegeId equals cc.id
                                             join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                             join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                             where dd.IsActive == true && dd.PaymentTypeId == PaymentTypeId && dd.CreatedOn >= TodayDate
                                             select new College_DD_Payment_Report
                                             {
                                                 DD_No = dd.Tranaction_Number,
                                                 DD_Date = dd.Payment_Date,
                                                 CollegeName = cc.collegeName,
                                                 CollegeCode = cc.collegeCode,
                                                 FeeTypeId = dd.FeeTypeId,
                                                 SubFeeTypeId = dd.Sub_PurposeId,
                                                 FeeType = type.FeeType,
                                                 BankName = bank.BankName,
                                                 Amount = dd.PaidAmount
                                             }).ToList();

            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename='" + date + "/" + month + "/" + year + "'FeeReport.XLS");
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult PaymentDateWiseReports(int PaymentTypeId)
        {
            MonthlyReport Month = new MonthlyReport();
            Month.PaymentTypeId = PaymentTypeId;
            return View(Month);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult PaymentDateWiseReports(MonthlyReport Monthly, string command)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["FullMonthName"] = null;
            if (Monthly != null)
            {
                var StrFromDate = Monthly.fromdate.Split('/');
                int Fromyear = Convert.ToInt32(StrFromDate[2]);
                int Frommonth = Convert.ToInt32(StrFromDate[1]);
                int Fromdate = Convert.ToInt32(StrFromDate[0]);

                var StrToDate = Monthly.todate.Split('/');
                int Toyear = Convert.ToInt32(StrToDate[2]);
                int Tomonth = Convert.ToInt32(StrToDate[1]);
                int Todate = Convert.ToInt32(StrToDate[0]);

                DateTime FromDate = new DateTime(Fromyear, Frommonth, Fromdate);
                DateTime ToDate = new DateTime(Toyear, Tomonth, Todate);

                var FrommonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(FromDate.Month);
                var TomonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ToDate.Month);

                var From = FrommonthName + " " + Fromdate + " " + Fromyear;
                var To = TomonthName + " " + Todate + " " + Toyear;

                var FullMonthName = string.Empty;
                if (FrommonthName == TomonthName && Fromyear == Toyear)
                    FullMonthName = FrommonthName + "-" + Fromyear;
                else
                    FullMonthName = FrommonthName + "(" + Fromyear + ")" + " - " + TomonthName + "(" + Toyear + ")";

                TempData["FullMonthName"] = FullMonthName;
                var jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                 join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                 join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                 join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                 where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.Payment_Date >= FromDate && dd.Payment_Date <= ToDate
                                                 select new College_DD_Payment_Report
                                                 {
                                                     DD_No = dd.Tranaction_Number,
                                                     DD_Date = dd.Payment_Date,
                                                     CollegeName = cc.collegeName,
                                                     CollegeCode = cc.collegeCode,
                                                     FeeTypeId = dd.FeeTypeId,
                                                     SubFeeTypeId = dd.Sub_PurposeId,
                                                     FeeType = type.FeeType,
                                                     BankName = bank.BankName,
                                                     Amount = dd.PaidAmount
                                                 }).ToList();

                if (command == "Submit")
                {
                    ViewBag.Payments = jntuh_college_dd_payments;
                    return View();
                }
                else
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=" + From + "-" + To + " FeeReport.XLS");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                }
            }
            else
                return RedirectToAction("MonthlyReports");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult MonthlyReports(int PaymentTypeId)
        {
            MonthlyReport Month = new MonthlyReport();
            Month.PaymentTypeId = PaymentTypeId;
            return View(Month);

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult MonthlyReports(MonthlyReport Monthly, string command)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["FullMonthName"] = null;
            if (Monthly != null)
            {
                var StrFromDate = Monthly.fromdate.Split('/');
                int Fromyear = Convert.ToInt32(StrFromDate[2]);
                int Frommonth = Convert.ToInt32(StrFromDate[1]);
                int Fromdate = Convert.ToInt32(StrFromDate[0]);

                var StrToDate = Monthly.todate.Split('/');
                int Toyear = Convert.ToInt32(StrToDate[2]);
                int Tomonth = Convert.ToInt32(StrToDate[1]);
                int Todate = Convert.ToInt32(StrToDate[0]);

                int Hour = DateTime.Now.Hour;
                int Minutes = DateTime.Now.Minute;
                int Seconds = DateTime.Now.Second;

                DateTime FromDate = new DateTime(Fromyear, Frommonth, Fromdate);
                DateTime ToDate = new DateTime(Toyear, Tomonth, Todate, Hour, Minutes, Seconds);

                var FrommonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(FromDate.Month);
                var TomonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ToDate.Month);

                var From = FrommonthName + " " + Fromdate + " " + Fromyear;
                var To = TomonthName + " " + Todate + " " + Toyear;

                var FullMonthName = string.Empty;

                if (FrommonthName == TomonthName && Fromyear == Toyear)
                    FullMonthName = FrommonthName + "-" + Fromyear;
                else
                    FullMonthName = FrommonthName + "(" + Fromyear + ")" + " - " + TomonthName + "(" + Toyear + ")";

                TempData["FullMonthName"] = FullMonthName;

                var jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                 join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                 join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                 join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                 where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.CreatedOn >= FromDate && dd.CreatedOn <= ToDate
                                                 select new College_DD_Payment_Report
                                                 {
                                                     DD_No = dd.Tranaction_Number,
                                                     DD_Date = dd.Payment_Date,
                                                     CollegeName = cc.collegeName,
                                                     CollegeCode = cc.collegeCode,
                                                     FeeTypeId = dd.FeeTypeId,
                                                     SubFeeTypeId = dd.Sub_PurposeId,
                                                     FeeType = type.FeeType,
                                                     BankName = bank.BankName,
                                                     Amount = dd.PaidAmount
                                                 }).ToList();

                if (command == "Submit")
                {
                    ViewBag.Payments = jntuh_college_dd_payments;
                    return View();
                }
                else
                {
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=" + From + "-" + To + " FeeReport.XLS");
                    Response.ContentType = "application/vnd.ms-excel";
                    return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                }
            }
            else
            {
                return RedirectToAction("MonthlyReports");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult CollegeWiseReports(int PaymentTypeId)
        {
            MonthlyReport Month = new MonthlyReport();
            Month.PaymentTypeId = PaymentTypeId;
            var Colleges = db.jntuh_college.Where(s => s.isActive == true).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            ViewBag.Colleges = Colleges;

            return View(Month);

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult CollegeWiseReports(MonthlyReport Monthly, string command)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["FullMonthName"] = null;
            var Colleges = db.jntuh_college.Where(s => s.isActive == true).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            ViewBag.Colleges = Colleges;
            if (Monthly != null)
            {
                if (Monthly.CollegeId != null && Monthly.fromdate != null && Monthly.todate != null)
                {
                    var StrFromDate = Monthly.fromdate.Split('/');
                    int Fromyear = Convert.ToInt32(StrFromDate[2]);
                    int Frommonth = Convert.ToInt32(StrFromDate[1]);
                    int Fromdate = Convert.ToInt32(StrFromDate[0]);

                    var StrToDate = Monthly.todate.Split('/');
                    int Toyear = Convert.ToInt32(StrToDate[2]);
                    int Tomonth = Convert.ToInt32(StrToDate[1]);
                    int Todate = Convert.ToInt32(StrToDate[0]);

                    int Hour = DateTime.Now.Hour;
                    int Minutes = DateTime.Now.Minute;
                    int Seconds = DateTime.Now.Second;

                    DateTime FromDate = new DateTime(Fromyear, Frommonth, Fromdate);
                    DateTime ToDate = new DateTime(Toyear, Tomonth, Todate, Hour, Minutes, Seconds);

                    var FrommonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(FromDate.Month);
                    var TomonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ToDate.Month);

                    var From = FrommonthName + " " + Fromdate + " " + Fromyear;
                    var To = TomonthName + " " + Todate + " " + Toyear;

                    var FullMonthName = string.Empty;
                    if (FrommonthName == TomonthName && Fromyear == Toyear)
                        FullMonthName = FrommonthName + "-" + Fromyear;
                    else
                        FullMonthName = FrommonthName + "(" + Fromyear + ")" + " - " + TomonthName + "(" + Toyear + ")";

                    TempData["FullMonthName"] = FullMonthName;

                    var jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.collegeId == Monthly.CollegeId && dd.CreatedOn >= FromDate && dd.CreatedOn <= ToDate
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();

                    if (command == "Submit")
                    {
                        ViewBag.Payments = jntuh_college_dd_payments;
                        return View();
                    }
                    else
                    {
                        Response.ClearContent();
                        Response.Buffer = true;
                        if (jntuh_college_dd_payments.Count == 0)
                        {
                            string CC = db.jntuh_college.Where(q => q.id == Monthly.CollegeId).Select(q => q.collegeCode).FirstOrDefault();
                            Response.AddHeader("content-disposition", "attachment; filename='" + From + "-" + To + "(" + CC + ")" + "_FeeReport.XLS");
                        }
                        else
                        {
                            Response.AddHeader("content-disposition", "attachment; filename='" + From + "-" + To + "(" + jntuh_college_dd_payments.Select(e => e.CollegeCode + "-" + e.CollegeName).FirstOrDefault() + ")" + "_FeeReport.XLS");
                        }
                        Response.ContentType = "application/vnd.ms-excel";
                        return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                    }
                }
                else if (Monthly.CollegeId != null && Monthly.fromdate == null && Monthly.todate == null)
                {


                    var jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.collegeId == Monthly.CollegeId
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    if (command == "Submit")
                    {
                        ViewBag.Payments = jntuh_college_dd_payments;
                        return View();
                    }
                    else
                    {
                        Response.ClearContent();
                        Response.Buffer = true;
                        if (jntuh_college_dd_payments.Count == 0)
                        {
                            string CC = db.jntuh_college.Where(q => q.id == Monthly.CollegeId).Select(q => q.collegeCode).FirstOrDefault();
                            Response.AddHeader("content-disposition", "attachment; filename=" + CC + "_FeeReport.XLS");
                        }
                        else
                        {
                            Response.AddHeader("content-disposition", "attachment; filename=" + jntuh_college_dd_payments.Select(e => e.CollegeCode + "-" + e.CollegeName).FirstOrDefault() + "_FeeReport.XLS");
                        }
                        Response.ContentType = "application/vnd.ms-excel";
                        return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                    }
                }
                return RedirectToAction("CollegeWiseReports");
            }
            else
            {
                return RedirectToAction("CollegeWiseReports");
            }

        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult BanksWiseReports(int PaymentTypeId)
        {
            MonthlyReport Month = new MonthlyReport();
            Month.PaymentTypeId = PaymentTypeId;
            List<SelectListItem> list = new List<SelectListItem>()
            {
                  new SelectListItem(){Value = "01",Text = "State Bank of India"},
                  new SelectListItem(){Value = "02",Text = "Andhra Bank"},
                  new SelectListItem(){Value = "03",Text = "Others"},
            };
            ViewBag.Banks = list;

            return View(Month);

        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult BanksWiseReports(MonthlyReport Monthly, string command)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            TempData["FullMonthName"] = null;
            List<SelectListItem> list = new List<SelectListItem>()
            {
                  new SelectListItem(){Value = "01",Text = "State Bank of India"},
                  new SelectListItem(){Value = "02",Text = "Andhra Bank"},
                  new SelectListItem(){Value = "03",Text = "Others"},
            };
            ViewBag.Banks = list;
            if (Monthly != null)
            {
                if (Monthly.BankId != null && Monthly.fromdate != null && Monthly.todate != null)
                {
                    var StrFromDate = Monthly.fromdate.Split('/');
                    int Fromyear = Convert.ToInt32(StrFromDate[2]);
                    int Frommonth = Convert.ToInt32(StrFromDate[1]);
                    int Fromdate = Convert.ToInt32(StrFromDate[0]);

                    var StrToDate = Monthly.todate.Split('/');
                    int Toyear = Convert.ToInt32(StrToDate[2]);
                    int Tomonth = Convert.ToInt32(StrToDate[1]);
                    int Todate = Convert.ToInt32(StrToDate[0]);

                    int Hour = DateTime.Now.Hour;
                    int Minutes = DateTime.Now.Minute;
                    int Seconds = DateTime.Now.Second;

                    DateTime FromDate = new DateTime(Fromyear, Frommonth, Fromdate);
                    DateTime ToDate = new DateTime(Toyear, Tomonth, Todate, Hour, Minutes, Seconds);

                    var FrommonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(FromDate.Month);
                    var TomonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ToDate.Month);

                    var From = FrommonthName + " " + Fromdate + " " + Fromyear;
                    var To = TomonthName + " " + Todate + " " + Toyear;

                    var FullMonthName = string.Empty;
                    if (FrommonthName == TomonthName && Fromyear == Toyear)
                        FullMonthName = FrommonthName + "-" + Fromyear;
                    else
                        FullMonthName = FrommonthName + "(" + Fromyear + ")" + " - " + TomonthName + "(" + Toyear + ")";

                    var jntuh_college_dd_payments = new List<College_DD_Payment_Report>();
                    if (Monthly.BankId == 01)
                    {
                        TempData["FullMonthName"] = "SBI " + FullMonthName;

                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.CreatedOn >= FromDate && dd.CreatedOn <= ToDate && dd.BankId == 28
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }
                    else if (Monthly.BankId == 02)
                    {
                        TempData["FullMonthName"] = "Andhra Bank " + FullMonthName;
                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.CreatedOn >= FromDate && dd.CreatedOn <= ToDate && dd.BankId == 5
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }
                    else
                    {
                        TempData["FullMonthName"] = "Other Banks " + FullMonthName;
                        int[] BanksId = new int[] { 5, 28 };
                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.CreatedOn >= FromDate && dd.CreatedOn <= ToDate && !BanksId.Contains(dd.BankId)
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }

                    if (command == "Submit")
                    {
                        ViewBag.Payments = jntuh_college_dd_payments;
                        return View();
                    }
                    else
                    {
                        Response.ClearContent();
                        Response.Buffer = true;
                        if (Monthly.BankId == 01)
                            Response.AddHeader("content-disposition", "attachment; filename='" + From + " To " + To + "'_SBI_FeeReport.XLS");
                        else if (Monthly.BankId == 02)
                            Response.AddHeader("content-disposition", "attachment; filename='" + From + " To " + To + "'_AndhraBank_FeeReport.XLS");
                        else
                            Response.AddHeader("content-disposition", "attachment; filename='" + From + " To " + To + "'_OthersBank_FeeReport.XLS");
                        Response.ContentType = "application/vnd.ms-excel";
                        return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                    }
                }
                else if (Monthly.BankId != null && Monthly.fromdate == null && Monthly.todate == null)
                {
                    var jntuh_college_dd_payments = new List<College_DD_Payment_Report>();
                    if (Monthly.BankId == 01)
                    {
                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.BankId == 28
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }
                    else if (Monthly.BankId == 02)
                    {
                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && dd.BankId == 5
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }
                    else
                    {
                        int[] BanksId = new int[] { 5, 28 };
                        jntuh_college_dd_payments = (from dd in db.jntuh_college_dd_payments
                                                     join cc in db.jntuh_college on dd.collegeId equals cc.id
                                                     join bank in db.jntuh_college_payments_banks on dd.BankId equals bank.Id
                                                     join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                                                     where dd.IsActive == true && dd.PaymentTypeId == Monthly.PaymentTypeId && !BanksId.Contains(dd.BankId)
                                                     select new College_DD_Payment_Report
                                                     {
                                                         DD_No = dd.Tranaction_Number,
                                                         DD_Date = dd.Payment_Date,
                                                         CollegeName = cc.collegeName,
                                                         CollegeCode = cc.collegeCode,
                                                         FeeTypeId = dd.FeeTypeId,
                                                         SubFeeTypeId = dd.Sub_PurposeId,
                                                         FeeType = type.FeeType,
                                                         BankName = bank.BankName,
                                                         Amount = dd.PaidAmount
                                                     }).ToList();
                    }

                    if (command == "Submit")
                    {
                        ViewBag.Payments = jntuh_college_dd_payments;
                        return View();
                    }
                    else
                    {
                        Response.ClearContent();
                        Response.Buffer = true;
                        if (Monthly.BankId == 01)
                            Response.AddHeader("content-disposition", "attachment; filename=SBI_FeeReport.XLS");
                        else if (Monthly.BankId == 02)
                            Response.AddHeader("content-disposition", "attachment; filename=AndhraBank_FeeReport.XLS");
                        else
                            Response.AddHeader("content-disposition", "attachment; filename=OthersBank_FeeReport.XLS");

                        Response.ContentType = "application/vnd.ms-excel";
                        return PartialView("~/Views/CollegesFee/AffiliationFeeReport.cshtml", jntuh_college_dd_payments.OrderBy(e => e.FeeType).ThenBy(a => a.BankName).ToList());
                    }
                }
                return RedirectToAction("BanksWiseReports");
            }
            else
            {
                return RedirectToAction("BanksWiseReports");
            }

        }

        //[HttpGet]
        //[Authorize(Roles = "Admin,Accounts")]
        //public ActionResult DCBReports()
        //{
        //    int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

        //    var jntuh_academic_Year = db.jntuh_academic_year.ToList();
        //    var actualYearNew = jntuh_academic_Year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
        //    var PresentYear = actualYearNew + 1;

        //    int ACY1 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew + 1)).Select(e => e.id).FirstOrDefault();
        //    int ACY2 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew)).Select(e => e.id).FirstOrDefault();
        //    int ACY3 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 1)).Select(e => e.id).FirstOrDefault();
        //    int ACY4 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 2)).Select(e => e.id).FirstOrDefault();
        //    int ACY5 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 3)).Select(e => e.id).FirstOrDefault();
        //    int ACY6 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 4)).Select(e => e.id).FirstOrDefault();

        //    var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();

        //    ViewBag.AcademicYears = AcademicYears.Select(e => new
        //    {
        //        AcademicYearId = e.id,
        //        AcademicYear = e.academicYear
        //    }).ToList();

        //    return View();

        //}

        //[HttpPost]
        //[Authorize(Roles = "Admin,Accounts")]
        //public ActionResult DCBReports(string Download, int? AcademicYearId)
        //{
        //    if (Download == "Download" && AcademicYearId != null)
        //    {
        //        //Pharmacy Colleges 
        //        var Pharmacy_CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235 };

        //        var Submitted_CollegeIds = db.jntuh_college_edit_status.Where(e => e.IsCollegeEditable == false && e.collegeId != 375).Select(w => w.collegeId).ToList();

        //        var jntuh_college = db.jntuh_college.Where(s => Submitted_CollegeIds.Contains(s.id)).Select(e => e).ToList();

        //        var collegeIds = jntuh_college.Select(e => e.id).ToList();

        //        var jntuh_college_dd_payments = db.jntuh_college_dd_payments.Where(d => d.IsActive == true).ToList();

        //        var jntuh_college_affiliationfee = db.jntuh_college_affiliationfee.Where(d => d.IsActive == true).ToList();

        //        var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
        //        int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.id == AcademicYearId).Select(a => a.actualYear).FirstOrDefault();

        //        TempData["FirstYear"] = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //        TempData["SecondYear"] = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //        TempData["ThirdYear"] = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //        TempData["FouthYear"] = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
        //        TempData["FifthYear"] = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

        //        int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear - 1).Select(e => e.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();

        //        int[] YearIds = new int[] { AY1, AY2, AY3, AY4 };

        //        List<DCB_Reports> DCB = new List<DCB_Reports>();

        //        foreach (var item in collegeIds)
        //        {
        //            DCB_Reports dcb_report = new DCB_Reports();
        //            dcb_report.CollegeId = item;
        //            dcb_report.CollegeCode = jntuh_college.Where(e => e.id == item).Select(a => a.collegeCode).FirstOrDefault();
        //            dcb_report.CollegeName = jntuh_college.Where(e => e.id == item).Select(a => a.collegeName).FirstOrDefault();


        //            dcb_report.FirstYearDemand = jntuh_college_affiliationfee.Where(e => e.collegeId == item && e.AcademicYearId == AY1).Select(e => e.Amount).Sum();
        //            dcb_report.FirstYearCollection = jntuh_college_dd_payments.Where(e => e.collegeId == item && e.AcademicYearId == AY1).Select(e => e.PaidAmount).Sum();
        //            dcb_report.FirstYearBalance = (dcb_report.FirstYearDemand - dcb_report.FirstYearCollection);

        //            dcb_report.SecondYearDemand = jntuh_college_affiliationfee.Where(e => e.collegeId == item && e.AcademicYearId == AY2).Select(e => e.Amount).Sum();
        //            dcb_report.SecondYearCollection = jntuh_college_dd_payments.Where(e => e.collegeId == item && e.AcademicYearId == AY2).Select(e => e.PaidAmount).Sum();
        //            dcb_report.SecondYearBalance = (dcb_report.SecondYearDemand - dcb_report.SecondYearCollection);

        //            dcb_report.ThridYearDemand = jntuh_college_affiliationfee.Where(e => e.collegeId == item && e.AcademicYearId == AY3).Select(e => e.Amount).Sum();
        //            dcb_report.ThridYearCollection = jntuh_college_dd_payments.Where(e => e.collegeId == item && e.AcademicYearId == AY3).Select(e => e.PaidAmount).Sum();
        //            dcb_report.ThridYearBalance = (dcb_report.ThridYearDemand - dcb_report.ThridYearCollection);

        //            dcb_report.FouthYearDemand = jntuh_college_affiliationfee.Where(e => e.collegeId == item && e.AcademicYearId == AY4).Select(e => e.Amount).Sum();
        //            dcb_report.FouthYearCollection = jntuh_college_dd_payments.Where(e => e.collegeId == item && e.AcademicYearId == AY4).Select(e => e.PaidAmount).Sum();
        //            dcb_report.FouthYearBalance = (dcb_report.FouthYearDemand - dcb_report.FouthYearCollection);

        //            DCB.Add(dcb_report);
        //        }

        //        Response.ClearContent();
        //        Response.Buffer = true;
        //        Response.AddHeader("content-disposition", "attachment; filename= ALL_DCB_Report.XLS");
        //        Response.ContentType = "application/vnd.ms-excel";
        //        return PartialView("~/Views/CollegesFee/YearWiseDCBReport.cshtml", DCB);
        //    }
        //    return RedirectToAction("DCBReports");
        //}

        #endregion

        #region DataEntry Screens

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult GetAllTransactions()
        {
            var AllDetails = (from payment in db.jntuh_college_dd_payments
                              join cc in db.jntuh_college on payment.collegeId equals cc.id
                              join fee in db.jntuh_college_paymentoffee_type on payment.FeeTypeId equals fee.id
                              join sub in db.jntuh_college_payments_subpurpose on payment.Sub_PurposeId equals sub.Id
                              join bank in db.jntuh_college_payments_banks on payment.BankId equals bank.Id
                              join year in db.jntuh_academic_year on payment.AcademicYearId equals year.id
                              join type in db.jntuh_college_accounts_payment_type on payment.PaymentTypeId equals type.Id
                              where payment.IsActive == true
                              select new CollegePaymentDetails
                              {
                                  Id = payment.Id,
                                  CollegeName = cc.collegeName + "(" + cc.collegeCode + ")",
                                  AcademicYear = year.academicYear,
                                  FeeTypeId = fee.id,
                                  FeeType = fee.FeeType,
                                  Sub_PurposeId = sub.Id,
                                  Sub_Purpose = sub.Sub_PurposeType,
                                  BankId = bank.Id,
                                  Bank = bank.BackCode + "-" + bank.BankName,
                                  PaidAmount = payment.PaidAmount,
                                  DepartmentIds = payment.DepartmentsIds,
                                  TypeId = payment.PaymentTypeId,
                                  Type = type.Payment_type,
                                  DDdate_New = payment.Payment_Date,
                                  Trans_Number = payment.Tranaction_Number
                              }).ToList();

            return View(AllDetails);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Payment_DataEntry(int? collegeId)
        {
            CollegePaymentDetails College_Payment = new CollegePaymentDetails();
            List<SelectListItem> AddDepartments = new List<SelectListItem>();
            var Banks = db.jntuh_college_payments_banks.Where(s => s.IsActive == true).Select(e => new { BankId = e.Id, BankName = e.BankName }).OrderBy(a => a.BankName).ToList();
            ViewBag.BankNames = Banks;

            var Colleges = db.jntuh_college.Where(s => s.isActive == true).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            ViewBag.Colleges = Colleges;

            var FeeTypes = db.jntuh_college_paymentoffee_type.Select(e => new { FeeId = e.id, FeeType = e.FeeType }).OrderBy(q => q.FeeType).ToList();
            ViewBag.FeeTypes = FeeTypes;

            var SubPurposes = db.jntuh_college_payments_subpurpose.Select(e => new { SubPurposeId = e.Id, SubPurposeType = e.Sub_PurposeType }).ToList();
            ViewBag.SubPurposes = SubPurposes;

            var ug_degreeIds = new int[] { 3, 4, 5, 6 };
            var jntuh_departments = (from d in db.jntuh_department
                                     join de in db.jntuh_degree on d.degreeId equals de.id
                                     where d.isActive == true && de.isActive == true && ug_degreeIds.Contains(de.id)
                                     select new
                                     {
                                         id = d.id,
                                         departmentName = de.degree + "-" + d.departmentName
                                     }).ToList();

            foreach (var item in jntuh_departments)
            {
                AddDepartments.Add(new SelectListItem { Value = item.id.ToString(), Text = item.departmentName });
            }

            College_Payment.Departments = AddDepartments;

            var data = jntuh_departments.Select(e => new
            {
                Id = e.id,
                DeptName = e.departmentName
            }).ToList();

            var jntuh_college_account_payment_type = db.jntuh_college_accounts_payment_type.Where(e => e.IsActive == true).Select(a => new { Id = a.Id, payment_Type = a.Payment_type }).ToList();
            ViewBag.Types = jntuh_college_account_payment_type;

            var jntuh_academic_Year = db.jntuh_academic_year.ToList();

            var actualYear = jntuh_academic_Year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
            var PresentYear = actualYear + 1;

            int AY1 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
            int AY2 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
            int AY3 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
            int AY4 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
            int AY5 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();
            int AY6 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 4)).Select(e => e.id).FirstOrDefault();

            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();

            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();

            College_Payment.AcademicYearId = AY1;

            if (collegeId != null)
            {
                var AllDetails = (from payment in db.jntuh_college_dd_payments
                                  join cc in db.jntuh_college on payment.collegeId equals cc.id
                                  join fee in db.jntuh_college_paymentoffee_type on payment.FeeTypeId equals fee.id
                                  join sub in db.jntuh_college_payments_subpurpose on payment.Sub_PurposeId equals sub.Id
                                  join bank in db.jntuh_college_payments_banks on payment.BankId equals bank.Id
                                  join year in db.jntuh_academic_year on payment.AcademicYearId equals year.id
                                  join type in db.jntuh_college_accounts_payment_type on payment.PaymentTypeId equals type.Id
                                  where payment.collegeId == collegeId
                                  select new CollegePaymentDetails
                                  {
                                      Id = payment.Id,
                                      AcademicYear = year.academicYear,
                                      FeeTypeId = fee.id,
                                      FeeType = fee.FeeType,
                                      Sub_PurposeId = sub.Id,
                                      Sub_Purpose = sub.Sub_PurposeType,
                                      BankId = bank.Id,
                                      Bank = bank.BackCode + "-" + bank.BankName,
                                      PaidAmount = payment.PaidAmount,
                                      DepartmentIds = payment.DepartmentsIds,
                                      TypeId = payment.PaymentTypeId,
                                      Type = type.Payment_type,
                                      //DD_Date = UAAAS.Models.Utilities.MMDDYY2DDMMYY(payment.Payment_Date.ToString()),
                                      //DD_Date = Convert.ToString(payment.Payment_Date),
                                      DDdate_New = payment.Payment_Date,
                                      Trans_Number = payment.Tranaction_Number
                                  }).ToList();

                ViewBag.Alldetails = AllDetails;
                return View(College_Payment);
            }
            else
            {
                return View(College_Payment);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Payment_DataEntry(CollegePaymentDetails Payment, string SelectedDepats, string OtherBank)
        {
            int UserId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var jntuh_college_account_payment_type = db.jntuh_college_accounts_payment_type.Where(e => e.IsActive == true).Select(a => new { Id = a.Id, payment_Type = a.Payment_type }).ToList();
            ViewBag.Types = jntuh_college_account_payment_type;

            if (Payment != null)
            {
                var ID = db.jntuh_college_dd_payments.Where(a => a.Tranaction_Number == Payment.Trans_Number && a.BankId == Payment.BankId).Select(a => a.collegeId).FirstOrDefault();
                if (ID == 0 || ID == null)
                { }
                else
                {
                    var collegeinformation = db.jntuh_college.Where(a => a.id == ID).Select(e => e.collegeName + "(" + e.collegeCode + ")").FirstOrDefault();
                    TempData["ERROR"] = "This Payment is Already Added to " + collegeinformation;
                    return RedirectToAction("Payment_DataEntry", new { collegeId = Payment.CollegeId });
                }

                jntuh_college_dd_payments dd_payments = new jntuh_college_dd_payments();

                dd_payments.AcademicYearId = Payment.AcademicYearId;
                dd_payments.collegeId = Payment.CollegeId;
                dd_payments.FeeTypeId = Payment.FeeTypeId;
                dd_payments.Sub_PurposeId = Payment.Sub_PurposeId != 0 ? Payment.Sub_PurposeId : 0;
                dd_payments.BankId = Payment.BankId;
                dd_payments.DepartmentsIds = SelectedDepats == "" ? "(NULL)" : SelectedDepats;
                dd_payments.PaidAmount = Payment.PaidAmount;
                dd_payments.PaymentTypeId = Payment.TypeId;
                dd_payments.Tranaction_Number = Payment.Trans_Number;
                if (Payment.DD_Date != null)
                    dd_payments.Payment_Date = Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(Payment.DD_Date));
                dd_payments.IsActive = true;
                dd_payments.CreatedOn = DateTime.Now;
                dd_payments.CreatedBy = UserId;
                dd_payments.UpdatedOn = null;
                dd_payments.UpdatedBy = null;
                db.jntuh_college_dd_payments.Add(dd_payments);
                db.SaveChanges();
                TempData["Success"] = "College Data-Entry Payment is Added Successfully.";
            }
            return RedirectToAction("Payment_DataEntry", new { collegeId = Payment.CollegeId });
        }

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Edit_DataEntry_Scenes(int? collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            List<SelectListItem> AddDepartments = new List<SelectListItem>();
            var Banks = db.jntuh_college_payments_banks.Where(s => s.IsActive == true).Select(e => new { BankId = e.Id, BankName = e.BankName }).OrderBy(z => z.BankName).ToList();
            ViewBag.BankNames = Banks;

            var Colleges = db.jntuh_college.Where(s => s.isActive == true).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            ViewBag.Colleges = Colleges;

            var jntuh_college_account_payment_type = db.jntuh_college_accounts_payment_type.Where(e => e.IsActive == true).Select(a => new { Id = a.Id, payment_Type = a.Payment_type }).ToList();
            ViewBag.Types = jntuh_college_account_payment_type;

            var ug_degreeIds = new int[] { 3, 4, 5, 6 };
            var jntuh_departments = (from d in db.jntuh_department
                                     join de in db.jntuh_degree on d.degreeId equals de.id
                                     where d.isActive == true && de.isActive == true && ug_degreeIds.Contains(de.id)
                                     select new
                                     {
                                         id = d.id,
                                         departmentName = de.degree + "-" + d.departmentName
                                     }).ToList();

            foreach (var item in jntuh_departments)
            {
                AddDepartments.Add(new SelectListItem { Value = item.id.ToString(), Text = item.departmentName });
            }

            ViewBag.Departments = AddDepartments;

            var FeeTypes = db.jntuh_college_paymentoffee_type.Select(e => new { FeeId = e.id, FeeType = e.FeeType }).OrderBy(z => z.FeeType).ToList();
            ViewBag.FeeTypes = FeeTypes;

            var SubPurposes = db.jntuh_college_payments_subpurpose.Select(e => new { SubPurposeId = e.Id, SubPurposeType = e.Sub_PurposeType }).Select(e => e).ToList();
            ViewBag.SubPurposes = SubPurposes;

            var jntuh_academic_Year = db.jntuh_academic_year.ToList();

            var actualYear = jntuh_academic_Year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
            var PresentYear = actualYear + 1;

            int AY1 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
            int AY2 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
            int AY3 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
            int AY4 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
            int AY5 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();
            int AY6 = jntuh_academic_Year.Where(e => e.actualYear == (actualYear - 4)).Select(e => e.id).FirstOrDefault();

            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();

            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();

            CollegePaymentDetails College_Payment = new CollegePaymentDetails();
            College_Payment.AcademicYearId = AY1;

            if (collegeId != null)
            {
                var AllDetails = (from payment in db.jntuh_college_dd_payments
                                  join cc in db.jntuh_college on payment.collegeId equals cc.id
                                  join fee in db.jntuh_college_paymentoffee_type on payment.FeeTypeId equals fee.id
                                  join sub in db.jntuh_college_payments_subpurpose on payment.Sub_PurposeId equals sub.Id
                                  join bank in db.jntuh_college_payments_banks on payment.BankId equals bank.Id
                                  join year in db.jntuh_academic_year on payment.AcademicYearId equals year.id
                                  join type in db.jntuh_college_accounts_payment_type on payment.PaymentTypeId equals type.Id
                                  where payment.collegeId == collegeId
                                  select new CollegePaymentDetails
                                  {
                                      Id = payment.Id,
                                      AcademicYearId = year.id,
                                      AcademicYear = year.academicYear,
                                      FeeTypeId = fee.id,
                                      FeeType = fee.FeeType,
                                      Sub_PurposeId = sub.Id,
                                      Sub_Purpose = sub.Sub_PurposeType,
                                      BankId = bank.Id,
                                      Bank = bank.BackCode + "-" + bank.BankName,
                                      PaidAmount = payment.PaidAmount,
                                      TypeId = payment.PaymentTypeId,
                                      Type = type.Payment_type,
                                      Trans_Number = payment.Tranaction_Number,
                                      // DD_Date = UAAAS.Models.Utilities.MMDDYY2DDMMYY(payment.Payment_Date.ToString()),
                                      DDdate_New = payment.Payment_Date,
                                      DepartmentIds = payment.DepartmentsIds,
                                      CreatedOn = payment.CreatedOn,
                                      CreatedBy = payment.CreatedBy

                                  }).ToList();

                ViewBag.Alldetails = AllDetails;
                return View(College_Payment);
            }
            else
            {
                return View(College_Payment);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Edit_DataEntry_Scenes(CollegePaymentDetails Payment, string SelectedDepats)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

            var jntuh_college_account_payment_type = db.jntuh_college_accounts_payment_type.Where(e => e.IsActive == true).Select(a => new { Id = a.Id, payment_Type = a.Payment_type }).ToList();
            ViewBag.Types = jntuh_college_account_payment_type;

            if (Payment != null)
            {
                var ID = db.jntuh_college_dd_payments.Where(a => a.Tranaction_Number == Payment.Trans_Number && a.BankId == Payment.BankId).Select(a => a.Id).FirstOrDefault();
                if (ID == 0 || ID == null)
                { }
                else if (Payment.Id == ID)
                { }
                else
                {
                    var collegeinformation = db.jntuh_college.Where(a => a.id == ID).Select(e => e.collegeName + "(" + e.collegeCode + ")").FirstOrDefault();
                    TempData["ERROR"] = "This Payment is Already Added to " + collegeinformation;
                    return RedirectToAction("Edit_DataEntry_Scenes", new { collegeId = Payment.CollegeId });
                }

                jntuh_college_dd_payments dd_payments = new jntuh_college_dd_payments();
                dd_payments.Id = Payment.Id;
                dd_payments.AcademicYearId = Payment.AcademicYearId;
                dd_payments.collegeId = Payment.CollegeId;
                dd_payments.FeeTypeId = Payment.FeeTypeId;
                dd_payments.Sub_PurposeId = Payment.Sub_PurposeId != 0 ? Payment.Sub_PurposeId : 0;
                dd_payments.BankId = Payment.BankId;

                if (dd_payments.FeeTypeId == 9 && dd_payments.Sub_PurposeId == 2)
                {
                    dd_payments.DepartmentsIds = SelectedDepats == "" ? "(NULL)" : SelectedDepats;
                }
                else
                {
                    dd_payments.DepartmentsIds = "(NULL)";
                }
                dd_payments.PaidAmount = Payment.PaidAmount;
                dd_payments.PaymentTypeId = Payment.TypeId;
                dd_payments.Tranaction_Number = Payment.Trans_Number;
                if (Payment.DD_Date != null)
                {
                    dd_payments.Payment_Date = Convert.ToDateTime(UAAAS.Models.Utilities.MMDDYY2DDMMYY(Payment.DD_Date.ToString()));
                    // string fff = Payment.DD_Date.Trim().ToString();
                    // dd_payments.Payment_Date = UAAAS.Models.Utilities.DDMMYY2MMDDYY(fff);
                }
                dd_payments.IsActive = true;
                dd_payments.CreatedOn = Payment.CreatedOn;
                dd_payments.CreatedBy = Payment.CreatedBy;
                dd_payments.UpdatedOn = DateTime.Now;
                dd_payments.UpdatedBy = userId;
                db.Entry(dd_payments).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "College Data-Entry Payment is Updated Successfully";
                return RedirectToAction("Edit_DataEntry_Scenes", new { collegeId = Payment.CollegeId });
            }
            else
            {
                TempData["Error"] = "Payment Object Null value Try Again";
                return RedirectToAction("Edit_DataEntry_Scenes");
            }
        }

        public ActionResult Delete_Payment(int? id)
        {
            if (id != null)
            {
                var college_payment = db.jntuh_college_dd_payments.Where(w => w.Id == id).Select(q => q).FirstOrDefault();
                if (college_payment != null)
                {
                    db.jntuh_college_dd_payments.Remove(college_payment);
                    db.SaveChanges();
                    return RedirectToAction("Payment_DataEntry", new { collegeId = college_payment.collegeId });
                }
            }
            return RedirectToAction("Payment_DataEntry");
        }

        #endregion

        #region Ajax && Json Getting Methods

        public ActionResult Get_Bank_DD_Payments()
        {

            var jntuh_college_dd_payments = db.jntuh_college_dd_payments.Where(s => s.IsActive == true).ToList();

            var BanksIds = jntuh_college_dd_payments.GroupBy(e => new { e.BankId }).Select(a => a.First()).ToList();

            var BanksIdsnew = jntuh_college_dd_payments.GroupBy(e => e.BankId).ToArray();

            List<Bank_DD_Payments_count> Count = new List<Bank_DD_Payments_count>();
            foreach (var item in BanksIdsnew)
            {
                Bank_DD_Payments_count singleCount = new Bank_DD_Payments_count();
                singleCount.BankName = db.jntuh_college_payments_banks.Where(w => w.Id == item.Key).Select(e => e.BankName).FirstOrDefault();
                singleCount.Count = jntuh_college_dd_payments.Where(a => a.BankId == item.Key).Select(e => e.Id).Count();
                Count.Add(singleCount);
            }

            var response = Count.Select(e => new
            {
                BankName = e.BankName,
                BankDDCount = e.Count
            }).ToList();


            return Json(new { response = response }, "application/json", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Get_SubPurposes(int? FeeTypeId)
        {
            var Payment_SubPurposes = db.jntuh_college_payments_subpurpose.Where(e => e.FeeTypeId == FeeTypeId).Select(a => new { SubPurposeId = a.Id, SubPurposeType = a.Sub_PurposeType }).ToList();
            var data = Payment_SubPurposes.Select(e => new
            {
                Sub_Id = e.SubPurposeId,
                Sub_type = e.SubPurposeType
            }).ToList();

            return Json(new { Data = data }, "application/json", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAfrcFee(int? collegeId, int? AcademicYearId)
        {
            if ((collegeId != null && collegeId != 0) && (AcademicYearId != null && AcademicYearId != 0))
            {
                var Fees = db.jntuh_afrc_fee.Where(q => q.collegeId == collegeId && q.academicyearId == AcademicYearId).Select(w => w).FirstOrDefault();
                return Json(new { data = Fees }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("CollegesAll");
            }
        }

        public ActionResult GetPreviousYearFee(int? collegeId, int? AcademicYearId)
        {
            if ((collegeId != null && collegeId != 0) && (AcademicYearId != null && AcademicYearId != 0))
            {
                var dues = db.jntuh_college_affiliationfee_dues.Where(q => q.collegeId == collegeId && q.academicyearId == AcademicYearId).Select(w => w).FirstOrDefault();
                return Json(new { data = dues }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return RedirectToAction("CollegesAll");
            }
        }
        #endregion

        #region College_Wise_UG_PG_Affiliation_Fees

        //[Authorize(Roles = "Admin,Accounts")]
        //public ActionResult InspectionAndAffiliationFeeReport(int? CollegeId)
        //{
        //    List<AffiliationFeeCollegeList> CollegesList = new List<AffiliationFeeCollegeList>();
        //    //Pharmacy Colleges 
        //    var Pharmacy_CollegeIds = new int[] { 448, 180, 159, 376, 428, 445, 24, 117, 202, 213, 395, 364, 27, 30, 6, 34, 55, 58, 54, 52, 9, 60, 65, 78, 150, 204, 219, 90, 97, 104, 110, 139, 105, 107, 114, 379, 118, 39, 120, 389, 127, 237, 135, 136, 47, 252, 169, 253, 262, 442, 301, 436, 370, 263, 267, 206, 140, 44, 290, 454, 42, 95, 297, 384, 75, 348, 298, 295, 303, 302, 283, 332, 410, 392, 234, 146, 315, 320, 317, 314, 318, 319, 353, 313, 235 };

        //    int[] AtanamousColleges = new int[] { 9, 11, 26, 32, 38, 39, 42, 68, 70, 75, 108, 109, 134, 140, 171, 179, 180, 183, 192, 196, 198, 335, 364, 367, 374, 399 };
        //    if (CollegeId == null)
        //    {
        //        var CollegeIds = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == 9).GroupBy(e => e.collegeId).Select(e => e.Key).ToArray();
        //        CollegesList = db.jntuh_college.Where(e => CollegeIds.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
        //        {
        //            CollegeId = e.id,
        //            CollegeCode = e.collegeCode,
        //            CollegeName = e.collegeName
        //        }).OrderBy(e => e.CollegeCode).ToList();
        //    }
        //    else
        //    {
        //        CollegesList = db.jntuh_college.Where(e => e.id == CollegeId && !AtanamousColleges.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
        //        {
        //            CollegeId = e.id,
        //            CollegeCode = e.collegeCode,
        //            CollegeName = e.collegeName
        //        }).ToList();
        //    }
        //    CollegesList = CollegesList.ToList();

        //    List<Get_College_Fee> College_Fee = new List<Get_College_Fee>();
        //    int[] UGDegreeIds = new int[] { 4, 5 };

        //    foreach (var item in CollegesList)
        //    {
        //        item.CollegeApprovedList = Generate_InspectionAndAffiliationReport(item.CollegeId);
        //        if (AtanamousColleges.Contains(item.CollegeId))
        //        {
        //            Get_College_Fee Fee = new Get_College_Fee();
        //            Fee.CollegeId = item.CollegeApprovedList.Select(e => e.collegeId).FirstOrDefault();
        //            Fee.CollegeCode = item.CollegeApprovedList.Select(e => e.collegecode).FirstOrDefault();
        //            Fee.CollegeName = item.CollegeApprovedList.Select(e => e.CollegeName).FirstOrDefault();
        //            Fee.InspectionFee = db.jntuh_afrc_fee.Where(s => s.collegeId == item.CollegeId).Select(s => s.afrcFee).FirstOrDefault();
        //            Fee.UGAffiFee = null;
        //            Fee.PGAffiFee = null;
        //            College_Fee.Add(Fee);
        //        }
        //        else
        //        {
        //            Get_College_Fee Fee = new Get_College_Fee();
        //            Fee.CollegeId = item.CollegeApprovedList.Select(e => e.collegeId).FirstOrDefault();
        //            Fee.CollegeCode = item.CollegeApprovedList.Select(e => e.collegecode).FirstOrDefault();
        //            Fee.CollegeName = item.CollegeApprovedList.Select(e => e.CollegeName).FirstOrDefault();
        //            Fee.UGAffiFee = item.CollegeApprovedList.Where(e => e.collegeId == item.CollegeId && UGDegreeIds.Contains(e.degreeId)).Select(e => e.SpecializationwiseSalary).FirstOrDefault().ToString();
        //            Fee.PGAffiFee = item.CollegeApprovedList.Where(e => e.collegeId == item.CollegeId && !UGDegreeIds.Contains(e.degreeId)).Select(e => e.SpecializationwiseSalary).Sum().ToString();
        //            Fee.InspectionFee = null;
        //            College_Fee.Add(Fee);
        //        }
        //    }

        //    var Engineering_Colleges = College_Fee.Where(e => !Pharmacy_CollegeIds.Contains(e.CollegeId)).Select(s => s).ToList();
        //    var Pharmacy_Colleges = College_Fee.Where(e => Pharmacy_CollegeIds.Contains(e.CollegeId)).Select(s => s).ToList();

        //    XLWorkbook wb = new XLWorkbook();
        //    DataTable Table1 = ConvertToDataTable(Engineering_Colleges);
        //    Table1.TableName = "Engineering";

        //    DataTable Table2 = ConvertToDataTable(Pharmacy_Colleges);
        //    Table2.TableName = "Pharmacy";

        //    //Add DataTable as Worksheet.
        //    wb.Worksheets.Add(Table1);
        //    wb.Worksheets.Add(Table2);


        //    //Export the Excel file.
        //    Response.Clear();
        //    Response.Buffer = true;
        //    Response.Charset = "";
        //    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    Response.AddHeader("content-disposition", "attachment;filename=DataSet.xls");
        //    using (MemoryStream MyMemoryStream = new MemoryStream())
        //    {
        //        wb.SaveAs(MyMemoryStream);
        //        MyMemoryStream.WriteTo(Response.OutputStream);
        //        Response.Flush();
        //        Response.End();
        //    }
        //    return RedirectToAction("Index");
        //}

        //public DataTable ConvertToDataTable<T>(IList<T> data)
        //{
        //    PropertyDescriptorCollection properties =
        //       TypeDescriptor.GetProperties(typeof(T));
        //    DataTable table = new DataTable();
        //    foreach (PropertyDescriptor prop in properties)
        //        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        //    foreach (T item in data)
        //    {
        //        DataRow row = table.NewRow();
        //        foreach (PropertyDescriptor prop in properties)
        //            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
        //        table.Rows.Add(row);
        //    }
        //    return table;
        //}

        //public List<CollegeApprovedList> Generate_InspectionAndAffiliationReport(int collegeId)
        //{
        //    List<CollegeApprovedList> collegeApproved = new List<CollegeApprovedList>();
        //    if (collegeId != 0)
        //    {
        //        collegeApproved = (from app in db.jntuh_approvedadmitted_intake
        //                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
        //                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                           where app.collegeId == collegeId
        //                           select new CollegeApprovedList()
        //                           {
        //                               AcademicYearId = app.AcademicYearId,
        //                               ShiftId = app.ShiftId,
        //                               collegeId = app.collegeId,
        //                               ApprovedIntake = app.ApprovedIntake,
        //                               AdmittedIntake = app.AdmittedIntake,
        //                               SpecializationId = app.SpecializationId,
        //                               IsActive = app.IsActive,
        //                               degreeId = Deg.id,
        //                               deptId = Dept.id,
        //                               Department = Dept.departmentName,
        //                               Degree = Deg.degree,
        //                               SpecializationName = Spec.specializationName
        //                           }).ToList();

        //        var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
        //        int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //        actualYear = actualYear - 1;
        //        //Suresh
        //        ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //        ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //        ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //        ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //        ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

        //        int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
        //        int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear).Select(e => e.id).FirstOrDefault();
        //        int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
        //        int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
        //        collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();
        //        var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == collegeId).Select(e => e.afrcFee).FirstOrDefault();
        //        //First Year Calculation
        //        int afrcFee = 0;
        //        if (afrcAmount != 0)
        //        {
        //            afrcFee = Convert.ToInt32(afrcAmount);
        //        }

        //        var jntuh_secialization = db.jntuh_specialization.ToList();
        //        var jntuh_college = db.jntuh_college.ToList();

        //        foreach (var item in collegeApproved)
        //        {
        //            int FirstYearFee = 0;
        //            int UGSecondYearFee = 0;
        //            int UGThirdYearFee = 0;
        //            int UGFourYearFee = 0;

        //            item.collegecode = jntuh_college.Where(s => s.id == item.collegeId).Select(e => e.collegeCode).FirstOrDefault();
        //            item.CollegeName = jntuh_college.Where(s => s.id == item.collegeId).Select(e => e.collegeName).FirstOrDefault();

        //            item.degreeId = item.degreeId;
        //            item.Degree = jntuh_secialization.Where(e => e.id == item.SpecializationId).Select(a => a.jntuh_department.jntuh_degree.degree).FirstOrDefault();
        //            item.deptId = item.deptId;
        //            item.Department = jntuh_secialization.Where(e => e.id == item.SpecializationId).Select(a => a.jntuh_department.departmentName).FirstOrDefault();

        //            item.SpecializationName = jntuh_secialization.Where(e => e.id == item.SpecializationId).Select(a => a.specializationName).FirstOrDefault();

        //            item.ApprovedIntake1 = GetIntake(collegeId, AY1, item.SpecializationId, item.ShiftId, 1);
        //            item.ApprovedIntake2 = GetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
        //            item.ApprovedIntake3 = GetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
        //            item.ApprovedIntake4 = GetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
        //            item.AdmittedIntake1 = GetIntake(collegeId, AY1, item.SpecializationId, item.ShiftId, 0);
        //            item.AdmittedIntake2 = GetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 0);
        //            item.AdmittedIntake3 = GetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 0);
        //            item.AdmittedIntake4 = GetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 0);

        //            item.LeteralentryIntake2 = LeteralGetIntake(collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
        //            item.LeteralentryIntake3 = LeteralGetIntake(collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
        //            item.LeteralentryIntake4 = LeteralGetIntake(collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
        //            item.AdmittedIntake2 += item.LeteralentryIntake2;
        //            item.AdmittedIntake3 += item.LeteralentryIntake3;
        //            item.AdmittedIntake4 += item.LeteralentryIntake4;

        //            item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
        //            item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
        //            item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
        //            item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
        //            item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
        //            item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
        //        }
        //        return collegeApproved.OrderBy(e => e.DisplayOrder).ToList();
        //    }
        //    else
        //    {
        //        return collegeApproved;
        //    }
        //}
        #endregion

        #region Colleges Affiliation Fee Calculated Methods

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult AllColleges()
        {
            int[] collegeIds = db.jntuh_approvedadmitted_intake.GroupBy(e => e).Select(e => e.Key.collegeId).ToArray();
            List<jntuh_college> colleges = db.jntuh_college.Where(e => collegeIds.Contains(e.id)).Select(e => e).ToList();
            ViewBag.CollegeTypes = CollegesType;
            var AcademicYears = db.jntuh_academic_year.OrderByDescending(a => a.actualYear).Select(e => e).Take(8).ToList();
            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();
            return View(colleges);
        }

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult FeeCalculation(int? collegeid, int? AcademicYearId)
        {
            List<CollegeApprovedList> collegeApproved = new List<CollegeApprovedList>();

            var Colleges = db.jntuh_college.Where(s => s.isActive == true).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            ViewBag.Colleges = Colleges;

            var jntuh_academic_Year = db.jntuh_academic_year.ToList();

            var actualYearNew = jntuh_academic_Year.Where(e => e.isPresentAcademicYear == true && e.isActive == true).Select(e => e.actualYear).FirstOrDefault();
            var PresentYear = actualYearNew + 1;

            int ACY1 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew + 1)).Select(e => e.id).FirstOrDefault();
            int ACY2 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew)).Select(e => e.id).FirstOrDefault();
            int ACY3 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 1)).Select(e => e.id).FirstOrDefault();
            int ACY4 = jntuh_academic_Year.Where(e => e.actualYear == (actualYearNew - 2)).Select(e => e.id).FirstOrDefault();

            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(8).ToList();

            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();

            TempData["YearId"] = AcademicYearId;

            if (collegeid != null && AcademicYearId != null)
            {
                var CollegeData = db.jntuh_college.Where(e => e.id == collegeid).Select(e => new { Name = e.collegeCode + "-" + e.collegeName }).FirstOrDefault();
                var AtanamousColleges = db.jntuh_college_affiliation.Where(s => s.affiliationTypeId == 7 && s.affiliationStatus == "Yes").Select(a => a.collegeId).ToList();

                string CollegeName = string.Empty;
                if (CollegeData != null)
                {
                    CollegeName = CollegeData.Name;
                }

                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.id == AcademicYearId).Select(a => a.actualYear).FirstOrDefault();

                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear - 1).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();

                int[] YearIds = new int[] { AY1, AY2, AY3, AY4 };

                collegeApproved = (from app in db.jntuh_approvedadmitted_intake
                                   join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                   where app.collegeId == collegeid && YearIds.Contains(app.AcademicYearId)
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

                collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();

                var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == AcademicYearId && e.collegeId == collegeid).Select(e => e.afrcFee).FirstOrDefault();
                var afrcAmount1 = db.jntuh_afrc_fee.Where(e => e.collegeId == collegeid).Select(e => new { e.afrcFee, e.academicyearId }).ToList();
                int afrcFee = Convert.ToInt32(afrcAmount);
                if (AcademicYearId == 11 || AcademicYearId == 12 || AcademicYearId == 13 || AcademicYearId == 14 || AcademicYearId == 15)
                    TempData["afrcFee"] = afrcAmount1.Where(a => a.academicyearId == 14).Select(a => a.afrcFee).FirstOrDefault() + " , " + afrcAmount1.Where(a => a.academicyearId == 12).Select(a => a.afrcFee).FirstOrDefault();
                else
                    TempData["afrcFee"] = afrcFee;

                var GroupofInstitutions = db.jntuh_afrc_fee.Where(a => a.academicyearId == AcademicYearId && a.groupofInstitutionsafrcFee != null).Select(w => w.collegeId).ToList();
                int CID = Convert.ToInt32(collegeid);
                if (GroupofInstitutions.Contains(CID))
                {
                    var Bpharmacy_afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == AcademicYearId && e.collegeId == CID).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                    var Bpharmacy_afrcAmount1 = db.jntuh_afrc_fee.Where(e => e.academicyearId == AcademicYearId - 1 && e.collegeId == CID).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                    int Bpharmacy_afrcFee = Convert.ToInt32(Bpharmacy_afrcAmount);
                    int Bpharmacy_afrcFee1 = Convert.ToInt32(Bpharmacy_afrcAmount1);
                    TempData["Bpharmacy_afrcFee"] = Bpharmacy_afrcFee + " , " + Bpharmacy_afrcFee1;
                }

                ViewBag.DuesFee = db.jntuh_college_paymentoffee.Where(s => s.collegeId == collegeid && s.FeeTypeID == 5).Select(s => s.duesAmount).FirstOrDefault();

                foreach (var item in collegeApproved)
                {
                    int FirstYearFee = 0;
                    int UGSecondYearFee = 0;
                    int UGThirdYearFee = 0;
                    int UGFourYearFee = 0;
                    item.ApprovedIntake1 = GetIntake(item.collegeId, AY1, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake2 = GetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake3 = GetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.ApprovedIntake4 = GetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake1 = GetIntake(item.collegeId, AY1, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake2 = GetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake3 = GetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 0);
                    item.AdmittedIntake4 = GetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 0);
                    item.LeteralentryIntake2 = LeteralGetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake3 = LeteralGetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                    item.LeteralentryIntake4 = LeteralGetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                    item.AdmittedIntake2 += item.LeteralentryIntake2;
                    item.AdmittedIntake3 += item.LeteralentryIntake3;
                    item.AdmittedIntake4 += item.LeteralentryIntake4;

                    item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
                    if (AtanamousColleges.Contains(item.collegeId))
                    {
                        item.FirstYearFee = AutonomousColleges_FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                        item.SecondYearFee = AutonomousColleges_FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                        item.ThirdYearFee = AutonomousColleges_FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                        item.FourthYearFee = AutonomousColleges_FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                        item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                        item.CollegeType = "Autonomous";
                    }
                    else
                    {
                        item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                        item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                        item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                        item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                        item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                        item.CollegeType = "Non-Autonomous";
                    }
                }
                ViewBag.TotalAmount = collegeApproved.Sum(e => e.SpecializationwiseSalary);
                return View(collegeApproved.OrderBy(e => e.DisplayOrder).ToList());
            }
            else
            {
                return View();
            }
        }

        public int AutonomousColleges_FeeCalculationYearWise(int Year, int DegreeId, int collegeId, int SpecializationId, int flag, int afrcFee, int shift)
        {
            int Fee = 0;
            int Academic_Year = Convert.ToInt32(TempData["YearId"]);
            var GroupofInstitutions = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year && a.groupofInstitutionsafrcFee != null).Select(w => w.collegeId).ToList();
            int collegeafrcfee = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year && a.collegeId == collegeId).Select(a => a.afrcFee).FirstOrDefault();
            if (DegreeId == 4 || DegreeId == 5)
            {
                List<jntuh_approvedadmitted_intake> collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                                                           join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                                                           join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                                                           join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                                                           where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                                                           select app).ToList();
                if (flag == 1 || flag == 2 || flag == 3)
                {
                    if (collegeapprovedList.Count() != 0)
                    {
                        if (GroupofInstitutions.Contains(collegeId))
                        {
                            int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                            int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);
                            //Get the Bpharmacy Course AFRC Fee....
                            var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == Year && e.collegeId == collegeId).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                            int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);

                            //BTech Fee Calculation
                            double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                            int FirstFee = (int)Math.Ceiling(percentahevalue);
                            var Btech_Fee = FirstFee * Btech_ApprovedIntake;

                            //BPharmcy Fee Calculation
                            double BPharmacy_percentahevalue = (double)(BpharmacyafrcFee * 0.5) / 100;
                            int BPharmcy_FirstFee = (int)Math.Ceiling(BPharmacy_percentahevalue);
                            var Bpharmcy_Fee = BPharmcy_FirstFee * BpharmacyApprovedIntake;

                            Fee = Btech_Fee + Bpharmcy_Fee;
                        }
                        else
                        {
                            if (flag != 3)
                            {
                                int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                                double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                                int FirstFee = (int)Math.Ceiling(percentahevalue);
                                Fee = FirstFee * ApprovedIntake;
                            }
                            else
                            {
                                int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                                double percentahevalue = 0;
                                //if (Academic_Year == 9)
                                //    percentahevalue = (double)175;
                                //else if (Year == 3)
                                //    percentahevalue = (double)175;
                                //else
                                //    percentahevalue = (double)(collegeafrcfee * 0.5) / 100;

                                if (Year < 8)
                                    percentahevalue = 175;
                                else
                                    percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                                int FirstFee = (int)Math.Ceiling(percentahevalue);
                                Fee = FirstFee * ApprovedIntake;
                            }
                        }
                    }
                }
                else if (flag == 4)
                {
                    if (collegeapprovedList.Count() != 0)
                    {
                        if (GroupofInstitutions.Contains(collegeId))
                        {
                            int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                            int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);

                            //BTech Fee Calculation
                            //int FirstFee = 175;
                            double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                            int FirstFee = (int)Math.Ceiling(percentahevalue);
                            var Btech_Fee = FirstFee * Btech_ApprovedIntake;

                            //BPharmcy Fee Calculation
                            //int BPharmcy_FirstFee = 175;
                            var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == Year && e.collegeId == collegeId).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                            int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);
                            double BPharmacy_percentahevalue = (double)(BpharmacyafrcFee * 0.5) / 100;

                            int BPharmcy_FirstFee = (int)Math.Ceiling(BPharmacy_percentahevalue);
                            var Bpharmcy_Fee = BPharmcy_FirstFee * BpharmacyApprovedIntake;

                            Fee = Btech_Fee + Bpharmcy_Fee;
                        }
                        else
                        {
                            int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                            //int FirstFee = 175;
                            double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                            int FirstFee = (int)Math.Ceiling(percentahevalue);
                            Fee = FirstFee * ApprovedIntake;
                        }
                    }
                }
            }

            jntuh_approvedadmitted_intake collegeapprovedList1 = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == Year && e.collegeId == collegeId && e.SpecializationId == SpecializationId && e.ShiftId == shift).Select(e => e).FirstOrDefault();
            if (collegeapprovedList1 != null)
            {
                if ((DegreeId == 1 || DegreeId == 2 || DegreeId == 3 || DegreeId == 6 || DegreeId == 9 || DegreeId == 10) && flag == 1)
                {
                    Fee = 30000;
                }
                if ((DegreeId == 7 || DegreeId == 8) && flag == 1)
                {
                    Fee = 40000;
                }
            }
            return Fee;
        }

        public int FeeCalculationYearWise(int Year, int DegreeId, int collegeId, int SpecializationId, int flag, int afrcFee, int shift)
        {
            int Fee = 0;

            int Academic_Year = Convert.ToInt32(TempData["YearId"]);
            int collegeafrcfee = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year && a.collegeId == collegeId).Select(a => a.afrcFee).FirstOrDefault();
            //if (collegeafrcfee == 0)
            //    collegeafrcfee = 175;
            List<jntuh_approvedadmitted_intake> collegeapprovedList = new List<jntuh_approvedadmitted_intake>();

            var GroupofInstitutions = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year && a.groupofInstitutionsafrcFee != null).Select(w => w.collegeId).ToList();

            if ((DegreeId == 4 || DegreeId == 5) && flag == 1)
            {
                collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                       join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                       join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                       join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                       where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                       select app).ToList();

                if (collegeapprovedList.Count() != 0)
                {
                    if (GroupofInstitutions.Contains(collegeId))
                    {
                        int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                        int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);
                        //Get the Bpharmacy Course AFRC Fee....
                        var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == Year && e.collegeId == collegeId).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                        int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);
                        //if (BpharmacyafrcFee == 0)
                        //    BpharmacyafrcFee = 175;
                        //BTech Fee Calculation
                        //int collegeafrcfee = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year).Select(a => a.afrcFee).FirstOrDefault();
                        double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                        int FirstFee = (int)Math.Ceiling(percentahevalue);
                        var Btech_Fee = FirstFee * Btech_ApprovedIntake;

                        //BPharmcy Fee Calculation
                        //int? collegeBpharmacyafrcFee = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year).Select(a => a.groupofInstitutionsafrcFee).FirstOrDefault();
                        //if (collegeBpharmacyafrcFee == null)
                        //    collegeBpharmacyafrcFee = 0;
                        double BPharmacy_percentahevalue = (double)(BpharmacyafrcFee * 0.5) / 100;
                        int BPharmcy_FirstFee = (int)Math.Ceiling(BPharmacy_percentahevalue);
                        var Bpharmcy_Fee = BPharmcy_FirstFee * BpharmacyApprovedIntake;

                        Fee = Btech_Fee + Bpharmcy_Fee;
                    }
                    else
                    {
                        //int collegeafrcfee = db.jntuh_afrc_fee.Where(a => a.academicyearId == Year).Select(a => a.afrcFee).FirstOrDefault();
                        int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                        double percentahevalue = (double)(collegeafrcfee * 0.5) / 100;
                        int FirstFee = (int)Math.Ceiling(percentahevalue);
                        Fee = FirstFee * ApprovedIntake;
                    }
                }
            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 2)
            {
                collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                       join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                       join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                       join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                       where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                       select app).ToList();

                if (collegeapprovedList.Count() != 0)
                {
                    if (GroupofInstitutions.Contains(collegeId))
                    {
                        int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                        int Btech_AdmittedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.AdmittedIntake);
                        int Btech_LateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.LateralentryIntake));
                        Btech_AdmittedIntake += Btech_LateralIntake;

                        int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);
                        int BpharmacyAddmittedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.AdmittedIntake);
                        int BpharmacyLateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.LateralentryIntake));
                        BpharmacyAddmittedIntake += BpharmacyLateralIntake;

                        //Get the Bpharmacy Course AFRC Fee....
                        var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == collegeId && e.academicyearId == Year).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                        int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);

                        //BTech Fee Calculation
                        double percentahevalue = ((double)Btech_AdmittedIntake / (double)Btech_ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * Btech_ApprovedIntake) / 100;
                        int initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        var Btech_Fee = initialFee * ApprovedPercentage;

                        //BPharmcy Fee Calculation
                        double BPharmacy_percentahevalue = ((double)BpharmacyAddmittedIntake / (double)BpharmacyApprovedIntake) * 100;
                        int Bpharmacy_admittedpercentage = (int)Math.Ceiling(BPharmacy_percentahevalue);
                        int Bpharmacy_calculatepercentage = slap(Bpharmacy_admittedpercentage);
                        int Bpharmacy_ApprovedPercentage = (Bpharmacy_calculatepercentage * BpharmacyApprovedIntake) / 100;
                        int Bpharmacy_initialFee = Convert.ToInt32((BpharmacyafrcFee * 0.5) / 100);
                        var Bpharmcy_Fee = Bpharmacy_initialFee * Bpharmacy_ApprovedPercentage;

                        Fee = Btech_Fee + Bpharmcy_Fee;
                    }
                    else
                    {
                        int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                        int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                        int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                        AdmittedIntake += LateralentryIntake;
                        double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                        int initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        Fee = (initialFee * ApprovedPercentage);
                    }
                }
            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 3)
            {
                collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                       join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                       join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                       join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                       where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                       select app).ToList();
                if (collegeapprovedList.Count() != 0)
                {
                    if (GroupofInstitutions.Contains(collegeId))
                    {
                        int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                        int Btech_AdmittedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.AdmittedIntake);
                        int Btech_LateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.LateralentryIntake));
                        Btech_AdmittedIntake += Btech_LateralIntake;

                        int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);
                        int BpharmacyAddmittedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.AdmittedIntake);
                        int BpharmacyLateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.LateralentryIntake));
                        BpharmacyAddmittedIntake += BpharmacyLateralIntake;

                        //Get the Bpharmacy Course AFRC Fee....
                        var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == Year && e.collegeId == collegeId).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                        int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);

                        //BTech Fee Calculation
                        double percentahevalue = ((double)Btech_AdmittedIntake / (double)Btech_ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * Btech_ApprovedIntake) / 100;
                        int initialFee = 0;
                        if (Year < 8)
                            initialFee = 175;
                        else
                            initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);

                        //if (Academic_Year == 9)
                        //    initialFee = 175;
                        //else if (Year == 3)
                        //    initialFee = 175;
                        //else
                        //    initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);

                        var Btech_Fee = initialFee * ApprovedPercentage;

                        //BPharmcy Fee Calculation
                        double BPharmacy_percentahevalue = ((double)BpharmacyAddmittedIntake / (double)BpharmacyApprovedIntake) * 100;
                        int Bpharmacy_admittedpercentage = (int)Math.Ceiling(BPharmacy_percentahevalue);
                        int Bpharmacy_calculatepercentage = slap(Bpharmacy_admittedpercentage);
                        int Bpharmacy_ApprovedPercentage = (Bpharmacy_calculatepercentage * BpharmacyApprovedIntake) / 100;
                        int Bpharmacy_initialFee = 0;
                        //if (Academic_Year == 9)
                        //    Bpharmacy_initialFee = 175;
                        //else if (Year == 3)
                        //    Bpharmacy_initialFee = 175;
                        //else
                        //    Bpharmacy_initialFee = Convert.ToInt32((BpharmacyafrcFee * 0.5) / 100);

                        if (Year < 8)
                            Bpharmacy_initialFee = 175;
                        else
                            Bpharmacy_initialFee = Convert.ToInt32((BpharmacyafrcFee * 0.5) / 100);

                        var Bpharmcy_Fee = Bpharmacy_initialFee * Bpharmacy_ApprovedPercentage;

                        Fee = Btech_Fee + Bpharmcy_Fee;
                    }
                    else
                    {
                        int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                        int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                        int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                        AdmittedIntake += LateralentryIntake;
                        double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                        int initialFee = 0;

                        //if (Academic_Year == 9)
                        //    initialFee = 175;
                        //else if (Year == 3)
                        //    initialFee = 175;
                        //else
                        //    initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        if (Year < 8)
                            Fee = (175 * ApprovedPercentage);
                        else
                            initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        Fee = (initialFee * ApprovedPercentage);
                    }
                }

            }
            if ((DegreeId == 4 || DegreeId == 5) && flag == 4)
            {
                collegeapprovedList = (from app in db.jntuh_approvedadmitted_intake
                                       join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                       join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                       join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                       where (Deg.id == 4 || Deg.id == 5) && app.AcademicYearId == Year && app.collegeId == collegeId
                                       select app).ToList();
                if (collegeapprovedList.Count() != 0)
                {
                    if (GroupofInstitutions.Contains(collegeId))
                    {
                        int Btech_ApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.ApprovedIntake);
                        int Btech_AdmittedIntake = collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.AdmittedIntake);
                        int Btech_LateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId != 12).Sum(e => e.LateralentryIntake));
                        Btech_AdmittedIntake += Btech_LateralIntake;

                        int BpharmacyApprovedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.ApprovedIntake);
                        int BpharmacyAddmittedIntake = collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.AdmittedIntake);
                        int BpharmacyLateralIntake = Convert.ToInt32(collegeapprovedList.Where(q => q.SpecializationId == 12).Sum(e => e.LateralentryIntake));
                        BpharmacyAddmittedIntake += BpharmacyLateralIntake;

                        //Get the Bpharmacy Course AFRC Fee....
                        var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == Year && e.collegeId == collegeId).Select(e => e.groupofInstitutionsafrcFee).FirstOrDefault();
                        int BpharmacyafrcFee = Convert.ToInt32(afrcAmount);

                        //BTech Fee Calculation
                        double percentahevalue = ((double)Btech_AdmittedIntake / (double)Btech_ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * Btech_ApprovedIntake) / 100;
                        int initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        //int initialFee = 175;
                        var Btech_Fee = initialFee * ApprovedPercentage;

                        //BPharmcy Fee Calculation
                        double BPharmacy_percentahevalue = ((double)BpharmacyAddmittedIntake / (double)BpharmacyApprovedIntake) * 100;
                        int Bpharmacy_admittedpercentage = (int)Math.Ceiling(BPharmacy_percentahevalue);
                        int Bpharmacy_calculatepercentage = slap(Bpharmacy_admittedpercentage);
                        int Bpharmacy_ApprovedPercentage = (Bpharmacy_calculatepercentage * BpharmacyApprovedIntake) / 100;
                        //  int Bpharmacy_initialFee = Convert.ToInt32((BpharmacyafrcFee * 0.5) / 100);
                        int Bpharmacy_initialFee = 0;
                        if (Year < 8)
                            Bpharmacy_initialFee = 175;
                        else
                            Bpharmacy_initialFee = Convert.ToInt32((BpharmacyafrcFee * 0.5) / 100); ;
                        var Bpharmcy_Fee = Bpharmacy_initialFee * Bpharmacy_ApprovedPercentage;

                        Fee = Btech_Fee + Bpharmcy_Fee;
                    }
                    else
                    {
                        int AdmittedIntake = collegeapprovedList.Sum(e => e.AdmittedIntake);
                        int ApprovedIntake = collegeapprovedList.Sum(e => e.ApprovedIntake);
                        int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
                        AdmittedIntake += LateralentryIntake;
                        double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
                        int admittedpercentage = (int)Math.Ceiling(percentahevalue);
                        int calculatepercentage = slap(admittedpercentage);
                        int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
                        int initialFee = Convert.ToInt32((collegeafrcfee * 0.5) / 100);
                        if (Year < 8)
                            Fee = (175 * ApprovedPercentage);
                        else
                            Fee = (initialFee * ApprovedPercentage);
                    }
                }
            }

            jntuh_approvedadmitted_intake collegeapprovedList1 = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == Year && e.collegeId == collegeId && e.SpecializationId == SpecializationId && e.ShiftId == shift).Select(e => e).FirstOrDefault();
            if (collegeapprovedList1 != null)
            {
                if ((DegreeId == 1 || DegreeId == 2 || DegreeId == 3 || DegreeId == 6 || DegreeId == 9 || DegreeId == 10) && flag == 1)
                {
                    Fee = 30000;
                }
                if ((DegreeId == 7 || DegreeId == 8) && flag == 1)
                {
                    Fee = 40000;
                }
            }
            return Fee;
        }

        private int GetIntake(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        {
            int intake = 0;
            if (flag == 1) //approved
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

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult AffiliationFeeReport(int? CollegeId, string type, int? academicYearId, bool? IsPending)
        {
            List<AffiliationFeeCollegeList> CollegesList = new List<AffiliationFeeCollegeList>();
            int collegeID = 0;
            var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.id == academicYearId).Select(a => a.actualYear).FirstOrDefault();
            //Siva
            ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
            ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
            ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
            ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
            ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

            var afrcFee = db.jntuh_afrc_fee.Select(e => e).ToList();
            var AutonamousColleges = db.jntuh_college_affiliation.Where(s => s.affiliationTypeId == 7 && s.affiliationStatus == "Yes").Select(a => a.collegeId).ToList();

            if (CollegeId == null)
            {
                var CollegeIds = db.jntuh_approvedadmitted_intake.Where(e => e.AcademicYearId == academicYearId).GroupBy(e => e.collegeId).Select(e => e.Key).ToArray();
                if (type == "Autonomous")
                {
                    // ViewBag.DuesFee = db.jntuh_college_paymentoffee.Where(s => s.collegeId == CollegeId && s.FeeTypeID == 5).Select(s => s.duesAmount).FirstOrDefault();
                    CollegesList = db.jntuh_college.Where(e => CollegeIds.Contains(e.id) && AutonamousColleges.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
                    {
                        CollegeId = e.id,
                        CollegeCode = e.collegeCode,
                        CollegeName = e.collegeName
                    }).OrderBy(e => e.CollegeCode).ToList();
                }
                else
                {
                    CollegesList = db.jntuh_college.Where(e => CollegeIds.Contains(e.id) && !AutonamousColleges.Contains(e.id)).Select(e => new AffiliationFeeCollegeList
                    {
                        CollegeId = e.id,
                        CollegeCode = e.collegeCode,
                        CollegeName = e.collegeName
                    }).OrderBy(e => e.CollegeCode).ToList();
                }
            }
            else
            {
                collegeID = Convert.ToInt32(CollegeId);
                CollegesList = db.jntuh_college.Where(e => e.id == CollegeId).Select(e => new AffiliationFeeCollegeList
                {
                    CollegeId = e.id,
                    CollegeCode = e.collegeCode,
                    CollegeName = e.collegeName
                }).ToList();

                if (AutonamousColleges.Contains(collegeID))
                    type = "Autonomous";
                else
                    type = "Non-Autonomous";
            }
            CollegesList = CollegesList.ToList();

            var GroupofInstitutions = db.jntuh_afrc_fee.Where(a => a.academicyearId == academicYearId && a.groupofInstitutionsafrcFee != null).Select(w => w.collegeId).ToList();

            foreach (var item in CollegesList)
            {
                if (GroupofInstitutions.Contains(item.CollegeId))
                {
                    if (academicYearId == 15)
                    {
                        //item.afrcFee += " (18-19) " + afrcFee.Where(a => a.academicyearId == 10 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        //item.afrcFee += " (19-20) " + afrcFee.Where(a => a.academicyearId == 11 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        item.afrcFee += " (20-21) " + afrcFee.Where(a => a.academicyearId == 12 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        item.afrcFee += " (21-22) " + afrcFee.Where(a => a.academicyearId == 13 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        item.afrcFee += " (22-23) " + afrcFee.Where(a => a.academicyearId == 14 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        item.afrcFee += " (23-24) " + afrcFee.Where(a => a.academicyearId == 15 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                    }
                    else
                    {
                        item.afrcFee = afrcFee.Where(a => a.academicyearId == academicYearId && a.collegeId == item.CollegeId).Select(a => a.afrcFee + " && " + a.groupofInstitutionsafrcFee + "(Pharmacy)").FirstOrDefault();
                    }
                }
                else
                {
                    if (academicYearId == 15)
                    {
                        //item.afrcFee += " (18-19) " + afrcFee.Where(a => a.academicyearId == 10 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault().ToString();
                        //item.afrcFee += " (19-20) " + afrcFee.Where(a => a.academicyearId == 11 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault().ToString();
                        item.afrcFee += " (20-21) " + afrcFee.Where(a => a.academicyearId == 12 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault().ToString();
                        item.afrcFee += " (21-22) " + afrcFee.Where(a => a.academicyearId == 13 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault().ToString();
                        item.afrcFee += " (22-23) " + afrcFee.Where(a => a.academicyearId == 14 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();
                        item.afrcFee += " (23-24) " + afrcFee.Where(a => a.academicyearId == 15 && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault();

                    }
                    else
                    {
                        item.afrcFee = afrcFee.Where(a => a.academicyearId == academicYearId && a.collegeId == item.CollegeId).Select(a => a.afrcFee).FirstOrDefault().ToString();
                    }
                }

                item.CollegeApprovedList = GenerateReportApprovedLists(item.CollegeId, academicYearId);
                item.isPending = IsPending;
                if (item.isPending == true)
                {
                    item.PendingFee = Convert.ToInt32(db.jntuh_college_affiliationfee_dues.Where(e => e.collegeId == item.CollegeId && e.academicyearId == academicYearId).Select(z => z.prevoiusyearDues).FirstOrDefault());
                    var AYear = jntuh_academic_years.Where(w => w.id == academicYearId).Select(r => r.academicYear).FirstOrDefault();
                    var AffiliationFeetypesId = db.jntuh_college_paymentoffee_type.Where(w => w.FeeType.Contains("Affiliation Fee Due Upto A.Y " + AYear)).Select(q => q.id).ToList();
                    item.DDAmount = Convert.ToInt32(db.jntuh_college_dd_payments.Where(e => e.collegeId == item.CollegeId && AffiliationFeetypesId.Contains(e.FeeTypeId)).Select(z => z.PaidAmount).Sum());
                }
            }

            if (type == "Autonomous")
            {
                Response.ClearContent();
                Response.Buffer = true;
                if (CollegeId == null)
                    Response.AddHeader("content-disposition", "attachment; filename=AutonomousColleges_AffiliationFeeReport.XLS");
                else
                    Response.AddHeader("content-disposition", "attachment; filename=" + CollegesList.Select(a => a.CollegeCode).FirstOrDefault() + "_AffiliationFeeReport.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/CollegesFee/Autonomous_AffiliationFeeReport.cshtml", CollegesList.OrderBy(s => s.CollegeName).ToList());
            }
            else
            {
                Response.ClearContent();
                Response.Buffer = true;
                if (CollegeId == null)
                    Response.AddHeader("content-disposition", "attachment; filename=Non_Autonomous_AffiliationFeeReport.XLS");
                else
                    Response.AddHeader("content-disposition", "attachment; filename=" + CollegesList.Select(a => a.CollegeCode).FirstOrDefault() + "_AffiliationFeeReport.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/CollegesFee/Non_Autonomous_AffiliationFeeReport.cshtml", CollegesList.OrderBy(s => s.CollegeName).ToList());
            }
        }

        public List<CollegeApprovedList> GenerateReportApprovedLists(int collegeId, int? academicYearId)
        {
            List<CollegeApprovedList> collegeApproved = new List<CollegeApprovedList>();
            if (collegeId != 0)
            {
                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.id == academicYearId).Select(a => a.actualYear).FirstOrDefault();

                //Siva
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear - 1).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();

                int[] YearIds = new int[] { AY1, AY2, AY3, AY4 };

                var AtanamousColleges = db.jntuh_college_affiliation.Where(s => s.affiliationTypeId == 7 && s.affiliationStatus == "Yes").Select(a => a.collegeId).ToList();
                var jntuh_afrc_fee = db.jntuh_afrc_fee.ToList();
                collegeApproved = (from app in db.jntuh_approvedadmitted_intake
                                   join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                   where app.collegeId == collegeId && YearIds.Contains(app.AcademicYearId)
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

                collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();

                var afrcAmount = db.jntuh_afrc_fee.Where(e => e.academicyearId == academicYearId && e.collegeId == collegeId).Select(e => e.afrcFee).FirstOrDefault();
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

                    if (AtanamousColleges.Contains(item.collegeId))
                    {
                        item.FirstYearFee = AutonomousColleges_FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                        item.SecondYearFee = AutonomousColleges_FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                        item.ThirdYearFee = AutonomousColleges_FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                        item.FourthYearFee = AutonomousColleges_FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                        item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                    }
                    else
                    {
                        item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                        item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                        item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                        item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                        item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                    }
                }
                return collegeApproved.OrderBy(e => e.DisplayOrder).ToList();
            }
            else
            {
                return collegeApproved;
            }
        }

        #endregion

        #region Affiliation Fee Inserting in DB based on college and specialization

        //[Authorize(Roles = "Admin,Accounts")]
        //public ActionResult AY_AffiliationFeeDBInserting()
        //{
        //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
        //    decimal paidamount = 0;
        //    var colleges = db.jntuh_college_edit_status.Join(db.jntuh_college, co => co.collegeId, c => c.id, (co, c) => new { co = co, c = c }).Where(e => e.co.IsCollegeEditable == false && e.c.isActive == true).Select(e => e.c.id).ToArray();

        //    //int[] colleges = {7};

        //    var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
        //    int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
        //    //actualYear = actualYear - 1;
        //    //Suresh
        //    ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear + 1).ToString(), (actualYear + 2).ToString().Substring(2, 2));
        //    ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
        //    ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
        //    ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
        //    ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));

        //    int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear + 1)).Select(e => e.id).FirstOrDefault();
        //    int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear).Select(e => e.id).FirstOrDefault();
        //    int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 1)).Select(e => e.id).FirstOrDefault();
        //    int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();

        //    var jntuh_afrc_fee = db.jntuh_afrc_fee.ToList();
        //    var Payment_Type_Id = db.jntuh_college_paymentoffee_type.Where(s => s.FeeType == "Affiliation Fee (for UAAC)").Select(e => e.id).FirstOrDefault();

        //    var jntuh_approvedadmittedIntake = db.jntuh_approvedadmitted_intake.ToList();

        //    foreach (var item2 in colleges)
        //    {
        //        //get current user CollegeId
        //        int userCollegeID = item2;

        //        int[] AtanamousColleges = new int[] { 9, 11, 26, 32, 38, 39, 42, 68, 70, 75, 108, 109, 134, 140, 171, 179, 180, 183, 192, 196, 198, 335, 364, 367, 374, 399 };
        //        if (AtanamousColleges.Contains(userCollegeID))
        //        {
        //            string CollegeName = string.Empty;
        //            var afrcAmount = jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.fee).FirstOrDefault();
        //            int afrcFee = Convert.ToInt32(afrcAmount);

        //            AutonamusCollegeFee collegeFee = new AutonamusCollegeFee();
        //            collegeFee.CollegeName = CollegeName;
        //            collegeFee.Fee = afrcFee;

        //            jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();
        //            fee.collegeId = item2;
        //            fee.AcademicYearId = AY1;
        //            fee.FeeTypeId = Payment_Type_Id;
        //            fee.SpecializationId = 174;
        //            fee.shiftId = 1;
        //            fee.Amount = afrcFee;
        //            fee.IsActive = true;
        //            fee.CreatedOn = DateTime.Now;
        //            fee.CreatedBy = userID;
        //            fee.UpdatedOn = null;
        //            fee.UpdatedBy = null;
        //            db.jntuh_college_affiliationfee.Add(fee);
        //            db.SaveChanges();
        //        }
        //        else
        //        {
        //            string CollegeName = string.Empty;
        //            var CollegeSpecializationIds = jntuh_approvedadmittedIntake.Where(e => e.collegeId == userCollegeID).Select(e => e.SpecializationId).Distinct().ToList();

        //            List<CollegeApprovedList> collegeApproved = (from app in db.jntuh_college_intake_existing
        //                                                         join Spec in db.jntuh_specialization on app.specializationId equals Spec.id
        //                                                         join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                                                         join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                                                         where app.collegeId == userCollegeID && app.approvedIntake != 0 && app.courseStatus != "Closure" && CollegeSpecializationIds.Contains(app.specializationId)
        //                                                         select new CollegeApprovedList()
        //                                                         {
        //                                                             AcademicYearId = app.academicYearId,
        //                                                             ShiftId = app.shiftId,
        //                                                             CollegeName = CollegeName,
        //                                                             collegeId = app.collegeId,
        //                                                             ApprovedIntake = app.approvedIntake,
        //                                                             AdmittedIntake = app.admittedIntake,
        //                                                             SpecializationId = app.specializationId,
        //                                                             IsActive = app.isActive,
        //                                                             degreeId = Deg.id,
        //                                                             deptId = Dept.id,
        //                                                             Department = Dept.departmentName,
        //                                                             Degree = Deg.degree,
        //                                                             SpecializationName = Spec.specializationName
        //                                                         }).ToList();

        //            collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();
        //            var afrcAmount = jntuh_afrc_fee.Where(e => e.collegeId == userCollegeID).Select(e => e.fee).FirstOrDefault();

        //            int afrcFee = Convert.ToInt32(afrcAmount);

        //            var MtechSpect = collegeApproved.Where(e => e.degreeId == 1).Select(e => e.SpecializationId).Distinct().ToArray();
        //            var MPharSpect = collegeApproved.Where(e => e.degreeId == 2).Select(e => e.SpecializationId).Distinct().ToArray();

        //            foreach (var item in collegeApproved)
        //            {
        //                int FirstYearFee = 0;
        //                int UGSecondYearFee = 0;
        //                int UGThirdYearFee = 0;
        //                int UGFourYearFee = 0;
        //                item.ApprovedIntake1 = GetIntakeNew(userCollegeID, AY1, item.SpecializationId, item.ShiftId, 1);
        //                item.ApprovedIntake2 = GetIntakeNew(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 1);
        //                item.ApprovedIntake3 = GetIntakeNew(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 1);
        //                item.ApprovedIntake4 = GetIntakeNew(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 1);
        //                item.AdmittedIntake1 = GetIntakeNew(userCollegeID, AY1, item.SpecializationId, item.ShiftId, 0);
        //                item.AdmittedIntake2 = GetIntakeNew(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 0);
        //                item.AdmittedIntake3 = GetIntakeNew(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 0);
        //                item.AdmittedIntake4 = GetIntakeNew(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 0);
        //                item.LeteralentryIntake2 = LeteralGetIntake(userCollegeID, AY2, item.SpecializationId, item.ShiftId, 1);
        //                item.LeteralentryIntake3 = LeteralGetIntake(userCollegeID, AY3, item.SpecializationId, item.ShiftId, 1);
        //                item.LeteralentryIntake4 = LeteralGetIntake(userCollegeID, AY4, item.SpecializationId, item.ShiftId, 1);
        //                item.AdmittedIntake2 += item.LeteralentryIntake2;
        //                item.AdmittedIntake3 += item.LeteralentryIntake3;
        //                item.AdmittedIntake4 += item.LeteralentryIntake4;

        //                item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
        //                item.FirstYearFee = FeeCalculationYearWiseNew(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
        //                item.SecondYearFee = FeeCalculationYearWiseNew(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
        //                item.ThirdYearFee = FeeCalculationYearWiseNew(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
        //                item.FourthYearFee = FeeCalculationYearWiseNew(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
        //                item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;

        //                if (item.degreeId == 4 || item.degreeId == 5)
        //                {
        //                    for (int i = 0; i < 4; i++)
        //                    {
        //                        jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();
        //                        fee.collegeId = item.collegeId;
        //                        fee.FeeTypeId = Payment_Type_Id;
        //                        fee.SpecializationId = item.SpecializationId;
        //                        fee.shiftId = item.ShiftId;
        //                        if (i == 0)
        //                        {
        //                            fee.AcademicYearId = AY1;
        //                            fee.Amount = item.FirstYearFee;
        //                        }
        //                        else if (i == 1)
        //                        {
        //                            fee.AcademicYearId = AY2;
        //                            fee.Amount = item.SecondYearFee;
        //                        }
        //                        else if (i == 2)
        //                        {
        //                            fee.AcademicYearId = AY3;
        //                            fee.Amount = item.ThirdYearFee;
        //                        }
        //                        else if (i == 3)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }

        //                        // fee.Amount = item.SpecializationwiseSalary ;
        //                        fee.IsActive = true;
        //                        fee.CreatedOn = DateTime.Now;
        //                        fee.CreatedBy = userID;
        //                        fee.UpdatedOn = null;
        //                        fee.UpdatedBy = null;
        //                        db.jntuh_college_affiliationfee.Add(fee);
        //                        db.SaveChanges();
        //                    }

        //                }
        //                else if (item.degreeId == 1 || item.degreeId == 2 || item.degreeId == 6)
        //                {
        //                    if (item.FirstYearFee != 0)
        //                    {
        //                        for (int i = 0; i < 2; i++)
        //                        {
        //                            jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();
        //                            fee.collegeId = item.collegeId;
        //                            fee.FeeTypeId = Payment_Type_Id;
        //                            fee.SpecializationId = item.SpecializationId;
        //                            fee.shiftId = item.ShiftId;
        //                            if (i == 0)
        //                            {
        //                                fee.AcademicYearId = AY1;
        //                                fee.Amount = item.FirstYearFee;
        //                            }
        //                            else if (i == 1)
        //                            {
        //                                fee.AcademicYearId = AY2;
        //                                fee.Amount = item.SecondYearFee;
        //                            }
        //                            fee.IsActive = true;
        //                            fee.CreatedOn = DateTime.Now;
        //                            fee.CreatedBy = userID;
        //                            fee.UpdatedOn = null;
        //                            fee.UpdatedBy = null;
        //                            db.jntuh_college_affiliationfee.Add(fee);
        //                            db.SaveChanges();
        //                        }
        //                    }
        //                }
        //                else if (item.degreeId == 3)
        //                {
        //                    if (item.FirstYearFee != 0)
        //                    {
        //                        for (int i = 0; i < 3; i++)
        //                        {
        //                            jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();
        //                            fee.collegeId = item.collegeId;
        //                            fee.FeeTypeId = Payment_Type_Id;
        //                            fee.SpecializationId = item.SpecializationId;
        //                            fee.shiftId = item.ShiftId;
        //                            if (i == 0)
        //                            {
        //                                fee.AcademicYearId = AY1;
        //                                fee.Amount = item.FirstYearFee;
        //                            }
        //                            else if (i == 1)
        //                            {
        //                                fee.AcademicYearId = AY2;
        //                                fee.Amount = item.SecondYearFee;
        //                            }
        //                            else if (i == 2)
        //                            {
        //                                fee.AcademicYearId = AY3;
        //                                fee.Amount = item.ThirdYearFee;
        //                            }
        //                            fee.IsActive = true;
        //                            fee.CreatedOn = DateTime.Now;
        //                            fee.CreatedBy = userID;
        //                            fee.UpdatedOn = null;
        //                            fee.UpdatedBy = null;

        //                            db.jntuh_college_affiliationfee.Add(fee);
        //                            db.SaveChanges();
        //                        }
        //                    }
        //                }
        //                else if (item.degreeId == 7 || item.degreeId == 8 || item.degreeId == 9)
        //                {
        //                    for (int i = 0; i < 5; i++)
        //                    {
        //                        jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();
        //                        fee.collegeId = item.collegeId;
        //                        fee.FeeTypeId = Payment_Type_Id;
        //                        fee.SpecializationId = item.SpecializationId;
        //                        fee.shiftId = item.ShiftId;
        //                        if (i == 0)
        //                        {
        //                            fee.AcademicYearId = AY1;
        //                            fee.Amount = item.FirstYearFee;
        //                        }
        //                        else if (i == 1)
        //                        {
        //                            fee.AcademicYearId = AY2;
        //                            fee.Amount = item.SecondYearFee;
        //                        }
        //                        else if (i == 2)
        //                        {
        //                            fee.AcademicYearId = AY3;
        //                            fee.Amount = item.ThirdYearFee;
        //                        }
        //                        else if (i == 3)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }
        //                        else if (i == 4)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }
        //                        fee.IsActive = true;
        //                        fee.CreatedOn = DateTime.Now;
        //                        fee.CreatedBy = userID;
        //                        fee.UpdatedOn = null;
        //                        fee.UpdatedBy = null;

        //                        db.jntuh_college_affiliationfee.Add(fee);
        //                        db.SaveChanges();
        //                    }
        //                }
        //                else if (item.degreeId == 10)
        //                {
        //                    for (int i = 0; i < 6; i++)
        //                    {
        //                        jntuh_college_affiliationfee fee = new jntuh_college_affiliationfee();

        //                        fee.collegeId = item.collegeId;
        //                        fee.FeeTypeId = Payment_Type_Id;
        //                        fee.SpecializationId = item.SpecializationId;
        //                        fee.shiftId = item.ShiftId;
        //                        if (i == 0)
        //                        {
        //                            fee.AcademicYearId = AY1;
        //                            fee.Amount = item.FirstYearFee;
        //                        }
        //                        else if (i == 1)
        //                        {
        //                            fee.AcademicYearId = AY2;
        //                            fee.Amount = item.SecondYearFee;
        //                        }
        //                        else if (i == 2)
        //                        {
        //                            fee.AcademicYearId = AY3;
        //                            fee.Amount = item.ThirdYearFee;
        //                        }
        //                        else if (i == 3)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }
        //                        else if (i == 4)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }
        //                        else if (i == 5)
        //                        {
        //                            fee.AcademicYearId = AY4;
        //                            fee.Amount = item.FourthYearFee;
        //                        }
        //                        fee.IsActive = true;
        //                        fee.CreatedOn = DateTime.Now;
        //                        fee.CreatedBy = userID;
        //                        fee.UpdatedOn = null;
        //                        fee.UpdatedBy = null;

        //                        db.jntuh_college_affiliationfee.Add(fee);
        //                        db.SaveChanges();
        //                    }
        //                }
        //            }
        //        }
        //        TempData["Success"] = "AffiliationFee is Inserted Successfully";
        //    }
        //    return RedirectToAction("Index");
        //}

        //public int FeeCalculationYearWiseNew(int Year, int DegreeId, int collegeId, int SpecializationId, int flag, int afrcFee, int shift)
        //{
        //    int Fee = 0;
        //    if ((DegreeId == 4 || DegreeId == 5) && flag == 1)
        //    {
        //        List<jntuh_college_intake_existing> collegeapprovedList = (from app in db.jntuh_college_intake_existing
        //                                                                   join Spec in db.jntuh_specialization on app.specializationId equals Spec.id
        //                                                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                                                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                                                                   where (Deg.id == 4 || Deg.id == 5) && app.academicYearId == Year && app.collegeId == collegeId && app.specializationId == SpecializationId && app.approvedIntake != 0
        //                                                                   select app).ToList();
        //        if (collegeapprovedList.Count() != 0)
        //        {
        //            int ApprovedIntake = collegeapprovedList.Sum(e => e.approvedIntake);
        //            double percentahevalue = (double)(afrcFee * 0.5) / 100;
        //            int FirstFee = (int)Math.Ceiling(percentahevalue);
        //            Fee = FirstFee * ApprovedIntake;
        //        }
        //    }
        //    if ((DegreeId == 4 || DegreeId == 5) && flag == 2)
        //    {
        //        List<jntuh_college_intake_existing> collegeapprovedList = (from app in db.jntuh_college_intake_existing
        //                                                                   join Spec in db.jntuh_specialization on app.specializationId equals Spec.id
        //                                                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                                                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                                                                   where (Deg.id == 4 || Deg.id == 5) && app.academicYearId == Year && app.collegeId == collegeId && app.specializationId == SpecializationId && app.approvedIntake != 0
        //                                                                   select app).ToList();

        //        if (collegeapprovedList.Count() != 0)
        //        {
        //            int AdmittedIntake = collegeapprovedList.Sum(e => e.admittedIntake);
        //            int ApprovedIntake = collegeapprovedList.Sum(e => e.approvedIntake);
        //            int LateralentryIntake = LeteralGetIntake(collegeId, Year, collegeapprovedList.Select(e => e.specializationId).FirstOrDefault(), collegeapprovedList.Select(e => e.shiftId).FirstOrDefault(), 1);
        //            // int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
        //            AdmittedIntake += LateralentryIntake;
        //            double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
        //            int admittedpercentage = (int)Math.Ceiling(percentahevalue);
        //            int calculatepercentage = slap(admittedpercentage);
        //            int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
        //            int initialFee = Convert.ToInt32((afrcFee * 0.5) / 100);
        //            Fee = (initialFee * ApprovedPercentage);
        //        }



        //    }
        //    if ((DegreeId == 4 || DegreeId == 5) && flag == 3)
        //    {
        //        List<jntuh_college_intake_existing> collegeapprovedList = (from app in db.jntuh_college_intake_existing
        //                                                                   join Spec in db.jntuh_specialization on app.specializationId equals Spec.id
        //                                                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                                                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                                                                   where (Deg.id == 4 || Deg.id == 5) && app.academicYearId == Year && app.collegeId == collegeId && app.specializationId == SpecializationId && app.approvedIntake != 0
        //                                                                   select app).ToList();
        //        if (collegeapprovedList.Count() != 0)
        //        {
        //            int AdmittedIntake = collegeapprovedList.Sum(e => e.admittedIntake);
        //            int ApprovedIntake = collegeapprovedList.Sum(e => e.approvedIntake);

        //            int LateralentryIntake = LeteralGetIntake(collegeId, Year, collegeapprovedList.Select(e => e.specializationId).FirstOrDefault(), collegeapprovedList.Select(e => e.shiftId).FirstOrDefault(), 1);
        //            // int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
        //            AdmittedIntake += LateralentryIntake;
        //            double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
        //            int admittedpercentage = (int)Math.Ceiling(percentahevalue);
        //            int calculatepercentage = slap(admittedpercentage);
        //            int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
        //            const int initialFee = 175; //Convert.ToInt32((afrcFee * 0.5) / 100);
        //            Fee = (initialFee * ApprovedPercentage);
        //        }

        //    }
        //    if ((DegreeId == 4 || DegreeId == 5) && flag == 4)
        //    {
        //        List<jntuh_college_intake_existing> collegeapprovedList = (from app in db.jntuh_college_intake_existing
        //                                                                   join Spec in db.jntuh_specialization on app.specializationId equals Spec.id
        //                                                                   join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
        //                                                                   join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
        //                                                                   where (Deg.id == 4 || Deg.id == 5) && app.academicYearId == Year && app.collegeId == collegeId && app.specializationId == SpecializationId && app.approvedIntake != 0
        //                                                                   select app).ToList();
        //        if (collegeapprovedList.Count() != 0)
        //        {
        //            int AdmittedIntake = collegeapprovedList.Sum(e => e.admittedIntake);
        //            int ApprovedIntake = collegeapprovedList.Sum(e => e.approvedIntake);
        //            int LateralentryIntake = LeteralGetIntake(collegeId, Year, collegeapprovedList.Select(e => e.specializationId).FirstOrDefault(), collegeapprovedList.Select(e => e.shiftId).FirstOrDefault(), 1);
        //            // int LateralentryIntake = Convert.ToInt32(collegeapprovedList.Sum(e => e.LateralentryIntake));
        //            AdmittedIntake += LateralentryIntake;
        //            double percentahevalue = ((double)AdmittedIntake / (double)ApprovedIntake) * 100;
        //            int admittedpercentage = (int)Math.Ceiling(percentahevalue);
        //            int calculatepercentage = slap(admittedpercentage);
        //            int ApprovedPercentage = (calculatepercentage * ApprovedIntake) / 100;
        //            const int initialFee = 175; //Convert.ToInt32((afrcFee * 0.5) / 100);
        //            Fee = (initialFee * ApprovedPercentage);
        //        }
        //    }

        //    jntuh_college_intake_existing collegeapprovedList1 = db.jntuh_college_intake_existing.Where(e => e.academicYearId == Year && e.collegeId == collegeId && e.specializationId == SpecializationId && e.shiftId == shift && e.approvedIntake != 0).Select(e => e).FirstOrDefault();
        //    if (collegeapprovedList1 != null)
        //    {
        //        if ((DegreeId == 1 || DegreeId == 2 || DegreeId == 3 || DegreeId == 6 || DegreeId == 9 || DegreeId == 10) && flag == 1)
        //        {
        //            Fee = 30000;
        //        }
        //        if ((DegreeId == 7 || DegreeId == 8) && flag == 1)
        //        {
        //            Fee = 40000;
        //        }
        //    }
        //    return Fee;
        //}

        //private int GetIntakeNew(int collegeId, int academicYearId, int specializationId, int shiftId, int flag)
        //{
        //    int intake = 0;
        //    //approved
        //    if (flag == 1)
        //    {
        //        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.approvedIntake).FirstOrDefault();
        //    }
        //    else //admitted
        //    {
        //        intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == collegeId && i.academicYearId == academicYearId && i.specializationId == specializationId && i.shiftId == shiftId).Select(i => i.admittedIntake).FirstOrDefault();
        //    }
        //    return intake;
        //}

        #endregion

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult AY_Colleges_AffiliationFee_Inserting_DB()
        {
            var approvedintakecollegeIds = db.jntuh_approvedadmitted_intake.Select(q => q.collegeId).Distinct().ToList();
            var Colleges = db.jntuh_college.Where(s => approvedintakecollegeIds.Contains(s.id)).Select(e => new { CollegeId = e.id, CollegeName = e.collegeCode + "-" + e.collegeName }).ToList();
            Colleges.Add(new { CollegeId = -1, CollegeName = "Active Login Colleges" });
            Colleges.Add(new { CollegeId = 0, CollegeName = "All Submission Colleges" });
            ViewBag.Colleges = Colleges.OrderBy(z => z.CollegeId).ToList(); ;
            var approvedacademicyearids = db.jntuh_approvedadmitted_intake.Select(q => q.AcademicYearId).Distinct().ToList();
            AffiliationPendingFee Fee = new AffiliationPendingFee();
            var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            var YearsData = jntuh_academic_years.Where(q => approvedacademicyearids.Contains(q.id)).Select(a => new { Id = a.id, Year = a.academicYear }).OrderByDescending(e => e.Id).ToList();
            ViewBag.Years = YearsData;
            return View(Fee);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult AY_Colleges_AffiliationFee_Inserting_DB(AffiliationPendingFee Fee)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userID == 0)
                return RedirectToAction("AY_Colleges_AffiliationFee_Inserting_DB");
            decimal paidamount = 0;
            List<jntuh_college> colleges = new List<jntuh_college>();
            if (Fee.CollegeId != null && Fee.AcademicYearId != null)
            {
                TempData["YearId"] = Fee.AcademicYearId;
                var AtanamousColleges = db.jntuh_college_affiliation.Where(s => s.affiliationTypeId == 7 && s.affiliationStatus == "Yes").Select(a => a.collegeId).ToList();
                var ActiveLoginCollegeIds = db.jntuh_college.Where(a => a.isActive == true).Select(z => z.id).ToList();
                var SubmittedCollegeIds = db.jntuh_approvedadmitted_intake.Where(a => a.AcademicYearId == Fee.AcademicYearId).Select(z => z.collegeId).Distinct().ToList();

                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(w => w.id == Fee.AcademicYearId).Select(a => a.actualYear).FirstOrDefault();

                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear - 1).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();
                int[] YearIds = new int[] { AY1, AY2, AY3, AY4 };

                var jntuh_afrc_fee = db.jntuh_afrc_fee.ToList();
                var Payment_Type_Id = db.jntuh_college_paymentoffee_type.Where(s => s.FeeType == "Affiliation Fee (for UAAC)").Select(e => e.id).FirstOrDefault();

                if (Fee.CollegeId == -1)
                {
                    colleges = db.jntuh_college.Where(q => ActiveLoginCollegeIds.Contains(q.id)).Select(z => z).ToList();
                }
                else if (Fee.CollegeId == 0)
                {
                    colleges = db.jntuh_college.Where(q => SubmittedCollegeIds.Contains(q.id)).Select(z => z).ToList();
                }
                else
                {
                    colleges = db.jntuh_college.Where(q => q.id == Fee.CollegeId).Select(z => z).ToList();
                }

                foreach (var Cid in colleges)
                {
                    List<CollegeApprovedList> collegeApproved = new List<CollegeApprovedList>();
                    collegeApproved = (from app in db.jntuh_approvedadmitted_intake
                                       join Spec in db.jntuh_specialization on app.SpecializationId equals Spec.id
                                       join Dept in db.jntuh_department on Spec.departmentId equals Dept.id
                                       join Deg in db.jntuh_degree on Dept.degreeId equals Deg.id
                                       where app.collegeId == Cid.id && YearIds.Contains(app.AcademicYearId)
                                       select new CollegeApprovedList()
                                       {
                                           AcademicYearId = app.AcademicYearId,
                                           collegeId = app.collegeId,
                                           collegecode = Cid.collegeCode,
                                           CollegeName = Cid.collegeName,
                                           ApprovedIntake = app.ApprovedIntake,
                                           AdmittedIntake = app.AdmittedIntake,
                                           IsActive = app.IsActive,
                                           degreeId = Deg.id,
                                           Degree = Deg.degree,
                                           deptId = Dept.id,
                                           Department = Dept.departmentName,
                                           SpecializationId = app.SpecializationId,
                                           SpecializationName = Spec.specializationName,
                                           ShiftId = app.ShiftId
                                       }).ToList();

                    collegeApproved = collegeApproved.AsEnumerable().GroupBy(r => new { r.SpecializationId, r.ShiftId }).Select(r => r.First()).ToList();

                    var afrcAmount = db.jntuh_afrc_fee.Where(e => e.collegeId == Cid.id).Select(e => e.afrcFee).FirstOrDefault();
                    int afrcFee = Convert.ToInt32(afrcAmount);
                    TempData["afrcFee"] = afrcFee;

                    foreach (var item in collegeApproved)
                    {
                        int FirstYearFee = 0;
                        int UGSecondYearFee = 0;
                        int UGThirdYearFee = 0;
                        int UGFourYearFee = 0;
                        item.ApprovedIntake1 = GetIntake(item.collegeId, AY1, item.SpecializationId, item.ShiftId, 1);
                        item.ApprovedIntake2 = GetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                        item.ApprovedIntake3 = GetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                        item.ApprovedIntake4 = GetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                        item.AdmittedIntake1 = GetIntake(item.collegeId, AY1, item.SpecializationId, item.ShiftId, 0);
                        item.AdmittedIntake2 = GetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 0);
                        item.AdmittedIntake3 = GetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 0);
                        item.AdmittedIntake4 = GetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 0);
                        item.LeteralentryIntake2 = LeteralGetIntake(item.collegeId, AY2, item.SpecializationId, item.ShiftId, 1);
                        item.LeteralentryIntake3 = LeteralGetIntake(item.collegeId, AY3, item.SpecializationId, item.ShiftId, 1);
                        item.LeteralentryIntake4 = LeteralGetIntake(item.collegeId, AY4, item.SpecializationId, item.ShiftId, 1);
                        item.AdmittedIntake2 += item.LeteralentryIntake2;
                        item.AdmittedIntake3 += item.LeteralentryIntake3;
                        item.AdmittedIntake4 += item.LeteralentryIntake4;

                        item.DisplayOrder = Orderlist.Where(e => e.Text == item.Degree).Select(e => e.Value).First();
                        if (AtanamousColleges.Contains(item.collegeId))
                        {
                            item.FirstYearFee = AutonomousColleges_FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                            item.SecondYearFee = AutonomousColleges_FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                            item.ThirdYearFee = AutonomousColleges_FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                            item.FourthYearFee = AutonomousColleges_FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                            item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                        }
                        else
                        {
                            item.FirstYearFee = FeeCalculationYearWise(AY1, item.degreeId, item.collegeId, item.SpecializationId, 1, afrcFee, item.ShiftId);
                            item.SecondYearFee = FeeCalculationYearWise(AY2, item.degreeId, item.collegeId, item.SpecializationId, 2, afrcFee, item.ShiftId);
                            item.ThirdYearFee = FeeCalculationYearWise(AY3, item.degreeId, item.collegeId, item.SpecializationId, 3, afrcFee, item.ShiftId);
                            item.FourthYearFee = FeeCalculationYearWise(AY4, item.degreeId, item.collegeId, item.SpecializationId, 4, afrcFee, item.ShiftId);
                            item.SpecializationwiseSalary = item.FirstYearFee + item.SecondYearFee + item.ThirdYearFee + item.FourthYearFee;
                        }
                    }
                    var ugamount = collegeApproved.Where(w => w.degreeId == 4 || w.degreeId == 5).Select(q => q.SpecializationwiseSalary).FirstOrDefault();
                    var pgamount = collegeApproved.Where(w => w.degreeId != 4 && w.degreeId != 5 && w.SpecializationwiseSalary != 0).Sum(q => q.SpecializationwiseSalary);
                    var toal = ugamount + pgamount;

                    jntuh_college_affiliationfee_dues due = new jntuh_college_affiliationfee_dues();
                    due.collegeId = Cid.id;
                    due.academicyearId = Convert.ToInt32(Fee.AcademicYearId);
                    due.prevoiusyearDues = null;
                    due.currentyearDues = toal;
                    due.paymentsoncurrentYear = null;
                    due.isActive = true;
                    due.createdOn = DateTime.Now;
                    due.createdBy = userID;
                    db.jntuh_college_affiliationfee_dues.Add(due);
                    db.SaveChanges();
                }
                TempData["Success"] = "Affiliation Fee is Inserted Successfully.";
                return RedirectToAction("AY_Colleges_AffiliationFee_Inserting_DB");
            }
            TempData["Error"] = "Something Went Wrong ,Try Again.";
            return RedirectToAction("AY_Colleges_AffiliationFee_Inserting_DB");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult CollegesAll()
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userId == 0 || userId == null)
                return RedirectToAction("LogOn", "Account");

            var Allcolleges = db.jntuh_college.ToList();
            ViewBag.Colleges = Allcolleges;
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult EditCollegssAfrcFee(int? collegeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userId == 0 || userId == null)
                return RedirectToAction("LogOn", "Account");

            if (collegeId != null && collegeId != 0)
            {
                var approvedacademicyearids = db.jntuh_afrc_fee.Select(q => q.academicyearId).Distinct().ToList();
                var jntuh_academic_year = db.jntuh_academic_year.ToList();
                var YearsData = jntuh_academic_year.Where(q => approvedacademicyearids.Contains(q.id)).Select(a => new { id = a.id, Year = a.academicYear }).OrderByDescending(e => e.id).ToList();
                var actualYear = jntuh_academic_year.Where(e => e.isActive == true && e.isPresentAcademicYear == true).Select(e => e.actualYear).FirstOrDefault();
                var CurrentYear = jntuh_academic_year.Where(e => e.actualYear == (actualYear + 1)).Select(q => new { q.id, q.academicYear }).FirstOrDefault();
                YearsData.Add(new { id = CurrentYear.id, Year = CurrentYear.academicYear });
                ViewBag.Years = YearsData.OrderByDescending(w => w.Year);

                var afrcfee = db.jntuh_afrc_fee.Where(q => q.collegeId == collegeId).Select(w => w).FirstOrDefault();
                AFRCFee Fee = new AFRCFee();
                if (afrcfee != null)
                {
                    Fee.CollegeId = collegeId;
                    Fee.AcademicyearId = afrcfee.academicyearId;
                    Fee.college = db.jntuh_college.Where(q => q.id == Fee.CollegeId).Select(q => q.collegeName + "(" + q.collegeCode + ")").FirstOrDefault();
                    Fee.EngineeringFee = Convert.ToInt32(afrcfee.afrcFee);
                    Fee.PharmacyFee = Convert.ToInt32(afrcfee.groupofInstitutionsafrcFee);
                    Fee.Message = null;
                    return PartialView("~/Views/CollegesFee/_afrcFeeedit.cshtml", Fee);
                }
                else
                {
                    Fee.Message = "Affiliation Fee is Does't exists to this college";
                    return PartialView("~/Views/CollegesFee/_afrcFeeedit.cshtml", Fee);
                }

            }
            TempData["Error"] = "Something went wrong.";
            return RedirectToAction("CollegesAll");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult EditCollegssAfrcFee(AFRCFee fee)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userId == 0 || userId == null)
                return RedirectToAction("LogOn", "Account");

            if (fee.CollegeId != null && fee.EngineeringFee != null && (fee.AcademicyearId != null && fee.AcademicyearId != 0))
            {
                var obj = db.jntuh_afrc_fee.Where(q => q.academicyearId == fee.AcademicyearId && q.collegeId == fee.CollegeId).Select(q => q).FirstOrDefault();
                if (obj != null)
                {
                    obj.afrcFee = Convert.ToInt32(fee.EngineeringFee);
                    obj.groupofInstitutionsafrcFee = fee.PharmacyFee;
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "Afrc Fee is Updated Successfully.";
                }
                else
                {
                    jntuh_afrc_fee Fee = new jntuh_afrc_fee();
                    Fee.academicyearId = fee.AcademicyearId;
                    Fee.collegeId = Convert.ToInt32(fee.CollegeId);
                    Fee.afrcFee = Convert.ToInt32(fee.EngineeringFee);
                    Fee.groupofInstitutionsafrcFee = fee.PharmacyFee;
                    db.jntuh_afrc_fee.Add(Fee);
                    db.SaveChanges();
                    TempData["Success"] = "Afrc Fee is Added Successfully.";
                }
            }
            return RedirectToAction("CollegesAll");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult EditCollegssPendingFee(int? collegeId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userId == 0 || userId == null)
                return RedirectToAction("LogOn", "Account");

            var approvedacademicyearids = db.jntuh_approvedadmitted_intake.Select(q => q.AcademicYearId).Distinct().ToList();
            var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
            var YearsData = jntuh_academic_years.Where(q => approvedacademicyearids.Contains(q.id)).Select(a => new { Id = a.id, Year = a.academicYear }).OrderByDescending(e => e.Id).ToList();
            ViewBag.Years = YearsData;

            jntuh_college_affiliationfee_dues dues = new jntuh_college_affiliationfee_dues();
            if (collegeId != null && collegeId != 0)
            {
                dues = db.jntuh_college_affiliationfee_dues.Where(q => q.collegeId == collegeId).Select(w => w).FirstOrDefault();
                AffiliationPendingFee Fee = new AffiliationPendingFee();
                if (dues != null)
                {
                    Fee.CollegeId = collegeId;
                    Fee.college = db.jntuh_college.Where(q => q.id == Fee.CollegeId).Select(q => q.collegeName + "(" + q.collegeCode + ")").FirstOrDefault();
                    Fee.AcademicYearId = dues.academicyearId;
                    Fee.PreviousFee = dues.prevoiusyearDues;
                    Fee.AffiliationFee = dues.currentyearDues;
                    Fee.Message = null;
                    return PartialView("~/Views/CollegesFee/_affiliationFeeedit.cshtml", Fee);
                }
                else
                {
                    Fee.Message = "Affiliation Fee is Does't exists to this college";
                    return PartialView("~/Views/CollegesFee/_affiliationFeeedit.cshtml", Fee);
                }
            }
            TempData["Error"] = "Something went wrong.";
            return RedirectToAction("CollegesAll");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult EditCollegssPendingFee(AffiliationPendingFee fee)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (userId == 0 || userId == null)
                return RedirectToAction("LogOn", "Account");

            if (fee.CollegeId != null && fee.AcademicYearId != null)
            {
                var obj = db.jntuh_college_affiliationfee_dues.Where(q => q.collegeId == fee.CollegeId && q.academicyearId == fee.AcademicYearId).Select(q => q).FirstOrDefault();
                //if(obj == null)
                //{
                //    jntuh_college_affiliationfee_dues adddues = new jntuh_college_affiliationfee_dues();
                //    adddues.collegeId = fee.CollegeId;
                //    adddues.academicyearId = fee.AcademicYearId;
                //    adddues.prevoiusyearDues = fee.PreviousFee;
                //    adddues.isActive = true;
                //    adddues.createdOn = DateTime.Now;
                //    adddues.createdBy = userId;
                //    adddues.updatedOn = null;
                //    adddues.updatedBy = null;
                //    db.jntuh_college_affiliationfee_dues.Add(adddues);
                //    db.SaveChanges();
                //    TempData["Success"] = "College Affiliation Fee is Added Successfully.";
                //}
                //else 
                if (obj != null)
                {
                    obj.prevoiusyearDues = fee.PreviousFee;
                    obj.updatedOn = DateTime.Now;
                    obj.updatedBy = userId;
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Success"] = "College Affiliation Fee is Updated Successfully.";
                }
                else
                {
                    TempData["Error"] = "This Year College Affiliation Fee is Does't exists";
                }
            }
            return RedirectToAction("CollegesAll");
        }

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult FourYearsAffiliationFeeDues(int? AcademicYearId)
        {
            int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            List<CollegeBalanceFee> AffiliationFeeDetails = new List<CollegeBalanceFee>();
            if (AcademicYearId != null)
            {
                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                int actualYear = jntuh_academic_years.Where(a => a.isActive == true && a.id == AcademicYearId).Select(a => a.actualYear).FirstOrDefault();
                ViewBag.FirstYear = String.Format("{0}-{1}", (actualYear).ToString(), (actualYear + 1).ToString().Substring(2, 2));
                ViewBag.SecondYear = String.Format("{0}-{1}", (actualYear - 1).ToString(), (actualYear).ToString().Substring(2, 2));
                ViewBag.ThirdYear = String.Format("{0}-{1}", (actualYear - 2).ToString(), (actualYear - 1).ToString().Substring(2, 2));
                ViewBag.FourthYear = String.Format("{0}-{1}", (actualYear - 3).ToString(), (actualYear - 2).ToString().Substring(2, 2));
                ViewBag.FifthYear = String.Format("{0}-{1}", (actualYear - 4).ToString(), (actualYear - 3).ToString().Substring(2, 2));

                int AY1 = jntuh_academic_years.Where(e => e.actualYear == (actualYear)).Select(e => e.id).FirstOrDefault();
                int AY2 = jntuh_academic_years.Where(e => e.actualYear == actualYear - 1).Select(e => e.id).FirstOrDefault();
                int AY3 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 2)).Select(e => e.id).FirstOrDefault();
                int AY4 = jntuh_academic_years.Where(e => e.actualYear == (actualYear - 3)).Select(e => e.id).FirstOrDefault();

                int[] YearsIds = new int[] { AY1, AY2, AY3, AY4 };

                var affiliationFeeDues = db.jntuh_college_affiliationfee_dues.Where(w => YearsIds.Contains(w.academicyearId)).Select(e => e).ToList();
                var CollegesIds = affiliationFeeDues.Select(e => e.collegeId).Distinct().ToList();
                var jntuhColleges = db.jntuh_college.Where(q => CollegesIds.Contains(q.id)).Select(x => x).ToList();

                var AffiliationFeetypesId = db.jntuh_college_paymentoffee_type.Where(w => w.FeeType.Contains("Affiliation Fee")).Select(q => q.id).ToList();
                foreach (var item in CollegesIds)
                {
                    CollegeBalanceFee Fee = new CollegeBalanceFee();
                    var collegeaffiliationdue = affiliationFeeDues.Where(q => q.collegeId == item).Select(q => q).OrderByDescending(x => x.academicyearId).ToList();
                    if (collegeaffiliationdue.Count != 0)
                    {
                        Fee.CollegeId = collegeaffiliationdue.Select(e => e.collegeId).FirstOrDefault();
                        Fee.Collegecode = jntuhColleges.Where(q => q.id == Fee.CollegeId).Select(a => a.collegeCode).FirstOrDefault();
                        Fee.CollegeName = jntuhColleges.Where(q => q.id == Fee.CollegeId).Select(a => a.collegeName).FirstOrDefault();
                        Fee.FirstYear = AY1;
                        Fee.FirstYearAffiliationFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY1).Select(e => e.currentyearDues).FirstOrDefault());
                        Fee.FirstYearFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY1).Select(e => e.prevoiusyearDues).FirstOrDefault());
                        Fee.FirstYear_PaidFee = db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY1 && AffiliationFeetypesId.Contains(e.FeeTypeId)).Select(w => w.PaidAmount).Sum() == null ? 0 : db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY1 && e.FeeTypeId == 1).Select(w => w.PaidAmount).Sum();
                        Fee.FirstYear_BalanceFee = (Fee.FirstYearFee + Fee.FirstYearAffiliationFee) - Fee.FirstYear_PaidFee;
                        Fee.SecondYear = AY2;
                        Fee.SecondYearAffiliationFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY2).Select(e => e.currentyearDues).FirstOrDefault());
                        Fee.SecondYearFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY2).Select(e => e.prevoiusyearDues).FirstOrDefault());
                        Fee.SecondYear_PaidFee = db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY2 && AffiliationFeetypesId.Contains(e.FeeTypeId)).Select(w => w.PaidAmount).Sum() == null ? 0 : db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY2 && e.FeeTypeId == 1).Select(w => w.PaidAmount).Sum();
                        Fee.SecondYear_BalanceFee = (Fee.SecondYearFee + Fee.SecondYearAffiliationFee) - Fee.SecondYear_PaidFee;
                        Fee.ThirdYear = AY3;
                        Fee.ThridYearAffiliationFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY3).Select(e => e.currentyearDues).FirstOrDefault());
                        Fee.ThridYearFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY3).Select(e => e.prevoiusyearDues).FirstOrDefault());
                        Fee.ThridYear_PaidFee = db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY3 && AffiliationFeetypesId.Contains(e.FeeTypeId)).Select(w => w.PaidAmount).Sum() == null ? 0 : db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY3 && e.FeeTypeId == 1).Select(w => w.PaidAmount).Sum();
                        Fee.ThridYear_BalanceFee = (Fee.ThridYearFee + Fee.ThridYearAffiliationFee) - Fee.ThridYear_PaidFee;
                        Fee.FourthYear = AY4;
                        Fee.FourthYearAffiliationFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY4).Select(e => e.currentyearDues).FirstOrDefault());
                        Fee.FourthYearFee = Convert.ToDecimal(collegeaffiliationdue.Where(s => s.academicyearId == AY4).Select(e => e.prevoiusyearDues).FirstOrDefault());
                        Fee.FourthYear_PaidFee = db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY4 && AffiliationFeetypesId.Contains(e.FeeTypeId)).Select(w => w.PaidAmount).Sum() == null ? 0 : db.jntuh_college_dd_payments.Where(e => e.collegeId == Fee.CollegeId && e.AcademicYearId == AY4 && e.FeeTypeId == 1).Select(w => w.PaidAmount).Sum();
                        Fee.FourthYear_BalanceFee = (Fee.FourthYearFee + Fee.FourthYearAffiliationFee) - Fee.FourthYear_PaidFee;
                        Fee.Total_BalanceFee = Convert.ToDecimal(Fee.FirstYear_BalanceFee + Fee.SecondYear_BalanceFee + Fee.ThridYear_BalanceFee + Fee.FourthYear_BalanceFee);
                        AffiliationFeeDetails.Add(Fee);
                    }
                }

                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=Affiliation Fee Dues for 4 Years.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/CollegesFee/_CollegesAffiliationFeeDuesfor4Years.cshtml", AffiliationFeeDetails.ToList());
            }
            else
            {
                var approvedacademicyearids = db.jntuh_approvedadmitted_intake.Select(q => q.AcademicYearId).Distinct().ToList();
                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                var YearsData = jntuh_academic_years.Where(q => approvedacademicyearids.Contains(q.id)).Select(a => new { Id = a.id, Year = a.academicYear }).OrderByDescending(e => e.Id).ToList();
                ViewBag.Years = YearsData;
                return PartialView("~/Views/CollegesFee/FourYearsAffiliationFeeDues.cshtml");
            }
            return RedirectToAction("CollegesAll");
        }

        [HttpGet]
        public ActionResult Getacademicyears()
        {
            var yearsId = db.jntuh_approvedadmitted_intake.Select(e => e.AcademicYearId).Distinct().ToList();
            var Years = db.jntuh_academic_year.Where(w => yearsId.Contains(w.id)).Select(e => new { id = e.id, Year = e.academicYear }).ToList();
            ViewBag.Years = Years;
            return PartialView("~/Views/CollegesFee/_Getacademicyears.cshtml");
        }

        [HttpGet]
        public ActionResult GetPaidAmountDetails(int? Collegeid, int? academicyearid)
        {
            if (Collegeid != null && academicyearid != null)
            {
                var AffiliationFeetypesId = db.jntuh_college_paymentoffee_type.Where(w => w.FeeType.Contains("Affiliation Fee")).Select(q => q.id).ToList();
                var college_DataEntry_Payment = db.jntuh_college_dd_payments.Where(s => s.collegeId == Collegeid && s.AcademicYearId == academicyearid && AffiliationFeetypesId.Contains(s.FeeTypeId)).Select(a => a).ToList();
                return PartialView("_GetPaidAmountDetails", college_DataEntry_Payment);
            }
            else
            {
                return PartialView("_GetPaidAmountDetails", null);
            }
        }

        [HttpGet]
        public ActionResult Getreports()
        {
            var FeetypeIds = new int[] { 1, 8, 9, 10, 11 };
            var FeeTypes = db.jntuh_college_paymentoffee_type.Where(s => FeetypeIds.Contains(s.id)).Select(w => new { Id = w.id, feeType = w.FeeType }).ToList();
            ViewBag.FeeTypes = FeeTypes;
            var YearIds = db.jntuh_college_dd_payments.Where(s => FeetypeIds.Contains(s.FeeTypeId)).Select(w => w.AcademicYearId).Distinct().ToList();
            var AcademicYears = db.jntuh_academic_year.Where(q => YearIds.Contains(q.id)).Select(e => e).ToList();
            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Getreports(int? fType, int? academicId)
        {
            if (fType != null && fType != 0 && academicId != null && academicId != 0)
            {
                List<int?> FTypeId = new List<int?>();
                if (fType == 1)
                {
                    var AffiliationFeetypesId = db.jntuh_college_paymentoffee_type.Where(w => w.FeeType.Contains("Affiliation Fee")).Select(q => q.id).ToList();
                    foreach (var item in AffiliationFeetypesId)
                        FTypeId.Add(item);
                }
                else
                {
                    FTypeId.Add(fType);
                }

                ViewBag.Year = db.jntuh_academic_year.Where(w => w.id == academicId).Select(e => e.academicYear).FirstOrDefault();
                var data = (from dd in db.jntuh_college_dd_payments
                            join cc in db.jntuh_college on dd.collegeId equals cc.id
                            join type in db.jntuh_college_paymentoffee_type on dd.FeeTypeId equals type.id
                            join stype in db.jntuh_college_payments_subpurpose on dd.Sub_PurposeId equals stype.Id
                            where dd.IsActive == true && dd.AcademicYearId == academicId && FTypeId.Contains(dd.FeeTypeId)
                            select new College_DD_Payment_Report
                            {
                                DD_No = dd.Tranaction_Number,
                                DD_Date = dd.Payment_Date,
                                CollegeName = cc.collegeName,
                                CollegeCode = cc.collegeCode,
                                FeeTypeId = type.id,
                                FeeType = type.FeeType,
                                SubFeeTypeId = stype.Id,
                                SubFeeType = stype.Sub_PurposeType,
                                Amount = dd.PaidAmount
                            }).ToList();

                Response.ClearContent();
                Response.Buffer = true;
                if (data.Count == 0)
                    Response.AddHeader("content-disposition", "attachment; filename=NoRecords_Report.XLS");
                else
                    Response.AddHeader("content-disposition", "attachment; filename=" + data.Select(a => a.FeeType).FirstOrDefault() + "_Report.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/CollegesFee/_GetDbReports.cshtml", data);
            }
            else
            {
                TempData["Error"] = "Please Select the Values Properly";
            }
            return View();
        }

        #region All Colleges YearWise Inspection Fee Code wrritten by siva

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult AllColleges_InspectionFeeDetails(int? AcademicYearId)
        {
            if (AcademicYearId != null)
            {
                List<jntuh_college_edit_status> editlist = new List<jntuh_college_edit_status>();
                if (AcademicYearId == 11)
                    editlist = db.jntuh_college_edit_status.Where(q => q.academicyearId == AcademicYearId && q.isSubmitted == true).Select(a => a).ToList();
                else
                    editlist = db.jntuh_college_edit_status.Where(q => q.academicyearId == AcademicYearId && q.IsCollegeEditable == false).Select(a => a).ToList();

                var submissionIds = editlist.Select(e => e.collegeId).ToList();
                var Colleges = db.jntuh_college.Where(s => submissionIds.Contains(s.id)).Select(a => new { CollegeId = a.id, Collegecode = a.collegeCode, CollegeName = a.collegeName }).OrderBy(q => q.CollegeName).ToList();
                List<CollegesInspectionFee> CollegesList = new List<CollegesInspectionFee>();
                foreach (var item in Colleges)
                {
                    CollegesInspectionFee College = FeeDetailsandPayment(item.CollegeId, AcademicYearId);
                    CollegesList.Add(College);
                }

                var ActualYear = db.jntuh_academic_year.Where(w => w.id == AcademicYearId).Select(z => z.academicYear).FirstOrDefault();
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ActualYear + "_All Colleges Inspection Fee Details.XLS");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/CollegesFee/_CollegesInspectionFee.cshtml", CollegesList.OrderBy(a => a.CollegeName).ToList());
            }
            else
            {
                var ademicyearids = db.jntuh_college_edit_status.Select(q => q.academicyearId).Distinct().ToList();
                var jntuh_academic_years = db.jntuh_academic_year.AsNoTracking().ToList();
                var YearsData = jntuh_academic_years.Where(q => ademicyearids.Contains(q.id)).Select(a => new { Id = a.id, Year = a.academicYear }).OrderByDescending(e => e.Id).ToList();
                ViewBag.Years = YearsData;
                return View();
            }
        }

        [Authorize(Roles = "Admin,Accounts")]
        public CollegesInspectionFee FeeDetailsandPayment(int collegeID, int? academicyearId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = 0;
            if (collegeID != null)
                userCollegeID = Convert.ToInt32(collegeID);
            else
                userCollegeID = 0;

            var clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID);
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            CollegesInspectionFee FeeDetails = new CollegesInspectionFee();

            decimal totalFee = 0;

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int PresentYear = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            int AY0 = Convert.ToInt32(academicyearId);

            if (userCollegeID > 0 && userCollegeID != null)
            {
                List<jntuh_college_intake_existing> intake = new List<jntuh_college_intake_existing>();

                if (AY0 == PresentYear)
                    intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == AY0).ToList();
                else if (AY0 == 10 || AY0 == 9 || AY0 == 8)
                {
                    string constr = string.Empty;
                    MySqlConnection con = new MySqlConnection();
                    MySqlCommand cmd;
                    MySqlDataReader dr = null;
                    if (AY0 == 10)
                    {
                        constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=uaaas20180316;";
                        con = new MySqlConnection(constr);
                        string query = "SELECT * FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + "";
                        con.Open();
                        cmd = new MySqlCommand(query, con);
                        dr = cmd.ExecuteReader();
                    }
                    else if (AY0 == 9)
                    {
                        constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=dataentry24032017;";
                        con = new MySqlConnection(constr);
                        string query = "SELECT * FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + "";
                        con.Open();
                        cmd = new MySqlCommand(query, con);
                        dr = cmd.ExecuteReader();
                    }
                    else if (AY0 == 8)
                    {
                        constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=uaaas05312016;";
                        con = new MySqlConnection(constr);
                        string query = "SELECT * FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + "";
                        con.Open();
                        cmd = new MySqlCommand(query, con);
                        dr = cmd.ExecuteReader();
                    }

                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            jntuh_college_intake_existing Obj1718 = new jntuh_college_intake_existing();
                            Obj1718.id = Convert.ToInt32(dr["id"]);
                            Obj1718.academicYearId = Convert.ToInt32(dr["academicYearId"]);
                            Obj1718.collegeId = Convert.ToInt32(dr["collegeId"]);
                            Obj1718.specializationId = Convert.ToInt32(dr["specializationId"]);
                            Obj1718.shiftId = Convert.ToInt32(dr["shiftId"]);
                            Obj1718.approvedIntake = Convert.ToInt32(dr["approvedIntake"]);
                            Obj1718.admittedIntake = Convert.ToInt32(dr["admittedIntake"]);
                            Obj1718.proposedIntake = string.IsNullOrEmpty(dr["proposedIntake"].ToString()) ? (int?)null : Convert.ToInt32(dr["proposedIntake"]);
                            Obj1718.approvalLetter = dr["approvalLetter"].ToString();
                            Obj1718.isActive = Convert.ToBoolean(dr["isActive"]);
                            Obj1718.createdBy = Convert.ToInt32(dr["createdBy"]);
                            Obj1718.createdOn = Convert.ToDateTime(dr["createdOn"]);
                            Obj1718.courseStatus = dr["courseStatus"].ToString();
                            Obj1718.jntuh_specialization = db.jntuh_specialization.Where(q => q.id == Obj1718.specializationId).Select(w => w).FirstOrDefault();
                            Obj1718.jntuh_shift = db.jntuh_shift.Where(q => q.id == Obj1718.shiftId).Select(w => w).FirstOrDefault();
                            Obj1718.jntuh_academic_year = db.jntuh_academic_year.Where(q => q.id == Obj1718.academicYearId).Select(w => w).FirstOrDefault();
                            Obj1718.jntuh_college = db.jntuh_college.Where(q => q.id == Obj1718.collegeId).Select(w => w).FirstOrDefault();
                            intake.Add(Obj1718);
                        }
                    }
                    con.Close();
                }
                else
                {
                    var intake_proposed = db.jntuh_college_intake_proposed.Where(i => i.collegeId == userCollegeID && i.academicYearId == AY0).ToList();
                    foreach (var log in intake_proposed)
                    {
                        jntuh_college_intake_existing LogItem = new jntuh_college_intake_existing();
                        LogItem.id = log.id;
                        LogItem.academicYearId = log.academicYearId;
                        LogItem.collegeId = log.collegeId;
                        LogItem.specializationId = log.specializationId;
                        LogItem.shiftId = log.shiftId;
                        LogItem.proposedIntake = log.proposedIntake;
                        LogItem.isActive = log.isActive;
                        LogItem.createdBy = log.createdBy;
                        LogItem.createdOn = log.createdOn;
                        LogItem.updatedBy = log.updatedBy;
                        LogItem.updatedOn = log.updatedOn;
                        LogItem.courseStatus = log.courseAffiliationStatusCodeId == 1 ? "Closure" :
                            log.courseAffiliationStatusCodeId == 2 ? "Increase" :
                            log.courseAffiliationStatusCodeId == 3 ? "Decrease" :
                            log.courseAffiliationStatusCodeId == 4 ? "NoChange" :
                            log.courseAffiliationStatusCodeId == 5 ? "New" :
                            log.courseAffiliationStatusCodeId == 6 ? "NewIncrease" :
                            log.courseAffiliationStatusCodeId == 7 ? "NewReduce" :
                            log.courseAffiliationStatusCodeId == 8 ? "OldIncrease" :
                            log.courseAffiliationStatusCodeId == 9 ? "OldReduce" :
                            log.courseAffiliationStatusCodeId == 10 ? "P" :
                            log.courseAffiliationStatusCodeId == 11 ? "TwoYears" : "";
                        LogItem.jntuh_specialization = db.jntuh_specialization.Where(q => q.id == log.specializationId).Select(w => w).FirstOrDefault();
                        LogItem.jntuh_shift = db.jntuh_shift.Where(q => q.id == log.shiftId).Select(w => w).FirstOrDefault();
                        LogItem.jntuh_academic_year = db.jntuh_academic_year.Where(q => q.id == log.academicYearId).Select(w => w).FirstOrDefault();
                        LogItem.jntuh_college = db.jntuh_college.Where(q => q.id == log.collegeId).Select(w => w).FirstOrDefault();
                        intake.Add(LogItem);
                    }
                }

                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;
                List<int> dualdegrees = new List<int>();
                List<int> pgdegrees = new List<int>();
                List<int> ugdegrees = new List<int>();
                List<int> totaldegrees = new List<int>();
                long DualdegreeSpecializationAmmount = 0;
                long ugSpecializationAmmount = 0;
                long pgSpecializationAmmount = 0;
                long applicationFee = 0;
                int ugCount = 0;
                int pgCount = 0;
                int dualCount = 0;

                var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                foreach (var item in intakeExisting)
                {
                    jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                  .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                                  .Select(e => e)
                                  .FirstOrDefault();

                    if (AY0 == PresentYear)
                    {
                        if (details != null)
                        {
                            if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                    {
                                        ugSpecializationAmmount = 25000;
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                    else
                                    {
                                        ugCount++;
                                        ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                }
                            }
                            else if (item.jntuh_specialization.jntuh_department.degreeId == 7)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    dualdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    dualCount++;
                                    DualdegreeSpecializationAmmount = dualCount * 40000;
                                    totaldegrees.AddRange(dualdegrees);
                                }
                            }
                            else
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    pgCount++;
                                    pgSpecializationAmmount = pgCount * 12000;
                                    totaldegrees.AddRange(pgdegrees);
                                }
                            }
                        }
                    }
                    //else if (AY0 == 10)
                    //{
                    //    if (details != null)
                    //    {
                    //        if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
                    //        {
                    //            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                    //            {
                    //                ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                    //                if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                    //                {
                    //                    ugSpecializationAmmount = 25000;
                    //                    totaldegrees.AddRange(ugdegrees);
                    //                }
                    //                else
                    //                {
                    //                    ugCount++;
                    //                    ugSpecializationAmmount = 25000 + (ugCount * 4000);
                    //                    totaldegrees.AddRange(ugdegrees);
                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                    //            {
                    //                pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                    //                pgCount++;
                    //                pgSpecializationAmmount = pgCount * 12000;
                    //                totaldegrees.AddRange(pgdegrees);
                    //            }
                    //        }
                    //    }
                    //}
                    else if (AY0 == 10 || AY0 == 9 || AY0 == 8)
                    {
                        int ExistingId = 0;
                        if (AY0 == 10)
                        {
                            string constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=uaaas20180316;";
                            MySqlConnection con = new MySqlConnection(constr);
                            string query = "SELECT id FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + " and academicYearId = " + AY0 + " && specializationId = " + item.specializationId + " && shiftId = " + item.shiftId + "";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand(query, con);
                            ExistingId = Convert.ToInt32(cmd.ExecuteScalar());
                            con.Close();
                        }
                        else if (AY0 == 9)
                        {
                            string constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=dataentry24032017;";
                            MySqlConnection con = new MySqlConnection(constr);
                            string query = "SELECT id FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + " and academicYearId = " + AY0 + " && specializationId = " + item.specializationId + " && shiftId = " + item.shiftId + "";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand(query, con);
                            ExistingId = Convert.ToInt32(cmd.ExecuteScalar());
                            con.Close();
                        }
                        else if (AY0 == 8)
                        {
                            string constr = "Data Source=10.10.10.108;user id=root;password=jntu123;database=uaaas05312016;";
                            MySqlConnection con = new MySqlConnection(constr);
                            string query = "SELECT id FROM jntuh_college_intake_existing where collegeId = " + userCollegeID + " and academicYearId = " + AY0 + " && specializationId = " + item.specializationId + " && shiftId = " + item.shiftId + "";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand(query, con);
                            ExistingId = Convert.ToInt32(cmd.ExecuteScalar());
                            con.Close();
                        }

                        if (ExistingId != 0)
                        {
                            if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
                            {
                                if ((item.proposedIntake != 0 && item.courseStatus != "Closure"))
                                {
                                    ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                    {
                                        ugSpecializationAmmount = 25000;
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                    else
                                    {
                                        ugCount++;
                                        ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                }
                            }
                            else
                            {
                                if ((item.proposedIntake != 0 && item.courseStatus != "Closure"))
                                {
                                    pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    pgCount++;
                                    pgSpecializationAmmount = pgCount * 12000;
                                    totaldegrees.AddRange(pgdegrees);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (details != null)
                        {
                            if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
                                    {
                                        ugSpecializationAmmount = 25000;
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                    else
                                    {
                                        ugCount++;
                                        ugSpecializationAmmount = 25000 + (ugCount * 4000);
                                        totaldegrees.AddRange(ugdegrees);
                                    }
                                }
                            }
                            else if (item.jntuh_specialization.jntuh_department.degreeId == 7)
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    dualdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    dualCount++;
                                    DualdegreeSpecializationAmmount = dualCount * 40000;
                                    totaldegrees.AddRange(dualdegrees);
                                }
                            }
                            else
                            {
                                if (item.proposedIntake != 0 && item.courseStatus != "Closure")
                                {
                                    pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
                                    pgCount++;
                                    pgSpecializationAmmount = pgCount * 12000;
                                    totaldegrees.AddRange(pgdegrees);
                                }
                            }
                        }
                    }
                }
                if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                    applicationFee = 1000;
                else
                    applicationFee = 750;

                FeeDetails.CollegeId = clgCode.id;
                FeeDetails.Collegecode = clgCode.collegeCode;
                FeeDetails.CollegeName = clgCode.collegeName;
                FeeDetails.CountofUGDegree = ugdegrees.Count;
                FeeDetails.CountofPGDegree = pgdegrees.Count;
                FeeDetails.CountofDualDegree = dualdegrees.Count;
                FeeDetails.CountofDegree = ugdegrees.Count + pgdegrees.Count + dualdegrees.Count;
                FeeDetails.SumofUGAmount = ugSpecializationAmmount;
                FeeDetails.SumofPGAmount = pgSpecializationAmmount;
                FeeDetails.SumofDualAmount = DualdegreeSpecializationAmmount;
                FeeDetails.SumofAmount = ugSpecializationAmmount + pgSpecializationAmmount + DualdegreeSpecializationAmmount;
                FeeDetails.ApplicationFee = applicationFee;
                FeeDetails.TotalFee = FeeDetails.SumofAmount + FeeDetails.ApplicationFee;
                var DuplicateAmounts = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == AY0 && a.CollegeId == FeeDetails.Collegecode && (a.PaymentTypeID == 7) && a.AuthStatus == "0300").Select(e => e).ToList();
                if (DuplicateAmounts.Count > 1)
                    FeeDetails.DuplicateAmount = DuplicateAmounts.Select(e => e.TxnAmount).FirstOrDefault();
                FeeDetails.BilldeskAmount = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == AY0 && a.CollegeId == FeeDetails.Collegecode && (a.PaymentTypeID == 7 || a.PaymentTypeID == 8) && a.AuthStatus == "0300").Select(e => e.TxnAmount).Sum();
                var TwentyFivePercentLateFeeCase = FeeDetails.TotalFee / 4.0;
                var FiftyPercentLateFeeCase = FeeDetails.TotalFee / 2.0;
                var HundredPercentLateFeeCase = FeeDetails.TotalFee;
                var LateFees = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == AY0 && a.CollegeId == FeeDetails.Collegecode && a.PaymentTypeID == 8 && a.AuthStatus == "0300").Select(e => e).ToList();
                FeeDetails.LateFee = LateFees.Select(e => e.TxnAmount).Sum();
                //foreach (var LateFee in LateFees)
                //{
                //    if (LateFee.TxnAmount == Convert.ToDecimal(Math.Round(TwentyFivePercentLateFeeCase)))
                //        FeeDetails.TwentyFivePercentLateFee = LateFee.TxnAmount;
                //    if (LateFee.TxnAmount == Convert.ToDecimal(Math.Round(FiftyPercentLateFeeCase)))
                //        FeeDetails.FiftyPercentLateFee = LateFee.TxnAmount;
                //    if (LateFee.TxnAmount == Convert.ToDecimal(HundredPercentLateFeeCase))
                //        FeeDetails.HundredPercentLateFee = LateFee.TxnAmount;
                //}
            }
            return FeeDetails;
        }

        #endregion

    }
}
