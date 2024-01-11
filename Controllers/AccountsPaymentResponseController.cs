using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using UAAAS.Models.Admin;

namespace UAAAS.Controllers
{
    public class AccountsPaymentResponseController : BaseController
    {
        uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Accounts")]
        public ActionResult PaymentResponseGridIndex(int? AcademicYearId, string command)
        {
            var jntuh_academic_Year = db.jntuh_academic_year.ToList();
            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();
            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();
            //ViewBag.objpayment = db.jntuh_college_paymentoffee_type.ToList();
            List<PaymentResponse> CollegeList = new List<PaymentResponse>();
            var CollegeIds = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == AcademicYearId && a.CollegeId != "ZZ" && a.AuthStatus == "0300").ToList();
            foreach (var item in CollegeIds)
            {
                PaymentResponse College = new PaymentResponse();
                College.CollegeId = item.CollegeId;
                College.CollegeName = db.jntuh_college.Where(e => e.collegeCode == item.CollegeId).Select(e => e.collegeName).FirstOrDefault();
                College.TxnAmount = item.TxnAmount;
                College.CustomerID = item.CustomerID;
                College.TxnReferenceNo = item.TxnReferenceNo;
                College.PaymentTypeName = db.jntuh_college_paymentoffee_type.Where(e => e.id == item.PaymentTypeID).Select(e => e.FeeType).FirstOrDefault();
                College.TxnDate = item.TxnDate;
                College.AuthStatus = item.AuthStatus;
                College.ErrorDescription = item.AuthStatus + "(" + item.ErrorDescription + ")";
                CollegeList.Add(College);
            }
            if (command == "Export")
            {
                string ReportHeader = "PaymentResponseGridDetails.xls";
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/AccountsPaymentResponse/AccountsPaymentResponseGridDetailsExport.cshtml", CollegeList);
            }
            return View(CollegeList);
        }

    }
}
