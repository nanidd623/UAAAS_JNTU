using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using UAAAS.Models.Admin;

namespace UAAAS.Controllers.Admin
{
    public class PaymentResponseController : BaseController
    {
        //
        // GET: /PaymentResponse/
        uaaasDBContext db = new uaaasDBContext();
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            ViewBag.objpayment = db.jntuh_college_paymentoffee_type.ToList();
            return View("~/Views/PaymentResponse/Index.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SavePaymentResponse(PaymentResponse modelobj)
        {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            jntuh_paymentresponse objpaymentes = new jntuh_paymentresponse();
            objpaymentes.AcademicYearId = ay0;
            objpaymentes.MerchantID = "JAWAHARLAL";
            objpaymentes.ChallanNumber = "ChallanNumber";
            objpaymentes.CurrencyName = "ÏNR";
            objpaymentes.TxnType = "450";
            objpaymentes.ErrorDescription = "Y";
            objpaymentes.SettlementType = "NA";
            objpaymentes.ErrorStatus = "NA";
            objpaymentes.AuthStatus = "0300";
            objpaymentes.noctypeId = null;
            objpaymentes.CollegeId = modelobj.CollegeId.ToUpper();
            objpaymentes.CustomerID = modelobj.CustomerID;
            objpaymentes.TxnReferenceNo = modelobj.TxnReferenceNo;
            objpaymentes.BankReferenceNo = modelobj.BankReferenceNo;
            objpaymentes.TxnAmount = modelobj.TxnAmount;
            objpaymentes.BankID = modelobj.BankID.ToUpper();
            objpaymentes.BankMerchantID = modelobj.BankMerchantID;
            objpaymentes.TxnDate = modelobj.TxnDate;
            objpaymentes.PaymentTypeID = modelobj.PaymentTypeID;
            db.jntuh_paymentresponse.Add(objpaymentes);
            db.SaveChanges();
            TempData["Success"] = "Data Saved Successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult NewCollegecodeCheck(string CustomerID, string CollegeId)
       {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            var CheckingCollegeId = db.jntuh_paymentresponse.Where(e => e.CustomerID.Trim() == CustomerID.Trim() && e.CollegeId.Trim() == CollegeId.ToUpper() && e.AcademicYearId == ay0).Select(e => e.CollegeId).FirstOrDefault();
            if (!string.IsNullOrEmpty(CheckingCollegeId))
            {
                if (CheckingCollegeId.Trim() == CollegeId.Trim())
                {
                    return Json("Customer Id already exits", JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(false);
            }
            return Json(true);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PaymentResponseGridIndex(int? AcademicYearId, int? PaymentTypeID, string command)
        {
            var jntuh_academic_Year = db.jntuh_academic_year.ToList();
            var AcademicYears = jntuh_academic_Year.OrderByDescending(a => a.actualYear).Select(e => e).Take(5).ToList();
            ViewBag.AcademicYears = AcademicYears.Select(e => new
            {
                AcademicYearId = e.id,
                AcademicYear = e.academicYear
            }).ToList();
            ViewBag.objpayment = db.jntuh_college_paymentoffee_type.ToList();
            List<PaymentResponse> CollegeList = new List<PaymentResponse>();
            var CollegeIds = db.jntuh_paymentresponse.Where(a => a.AcademicYearId == AcademicYearId && a.PaymentTypeID == PaymentTypeID && a.CollegeId != "ZZ").ToList();
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
                return PartialView("~/Views/PaymentResponse/PaymentResponseGridDetailsExport.cshtml", CollegeList);
            }
            return View(CollegeList);
        }
    }
}
