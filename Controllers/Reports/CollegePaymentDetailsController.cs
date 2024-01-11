using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using UAAAS.Models;

namespace UAAAS.Controllers.Reports
{
    public class CollegePaymentDetailsController : BaseController
    {
        //
        // GET: /CollegePaymentDetails/
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,DataEntry")]
        public ActionResult Index()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            List<jntuh_paymentresponse> jntuhpaymentresponse =
                db.jntuh_paymentresponse.Where(
                    p => p.AcademicYearId == ay0 && p.AuthStatus == "0300" && p.TxnDate.Year == (actualYear + 1))
                    .Select(s => s)
                    .ToList();
            List<CollegePaymentDetails> CollegePaymentDetailslist= new List<CollegePaymentDetails>();
            List<jntuh_college> jntuhColleges = 
                db.jntuh_college.Where(c => c.isActive == true).Select(s => s).ToList();
            List<jntuh_college_paymentoffee_type> jntuhcollegepaymentoffeetype =
                db.jntuh_college_paymentoffee_type.Select(s => s).ToList();
            foreach (var item in jntuhpaymentresponse)
            {
                CollegePaymentDetails payment=new CollegePaymentDetails();
                payment.CollegeId =
                    jntuhColleges.Where(c => c.collegeCode == item.CollegeId).Select(s => s.id).FirstOrDefault();
                payment.Collegecode = item.CollegeId;
                payment.CollegeName =
                    jntuhColleges.Where(c => c.id == payment.CollegeId).Select(s => s.collegeName).FirstOrDefault();
                payment.ChallanNumber = item.CustomerID.Trim();
                payment.TxnReferenceNo = item.TxnReferenceNo.Trim();
                payment.PaymentAuthstatus = item.AuthStatus;
                payment.Errordescription = item.ErrorDescription;
                payment.FeeTypeId = item.PaymentTypeID;
                if (payment.FeeTypeId != 0)
                    payment.FeeType =
                        jntuhcollegepaymentoffeetype.Where(p => p.id == payment.FeeTypeId)
                            .Select(s => s.FeeType)
                            .FirstOrDefault();
                payment.TxnAmount = item.TxnAmount;
                CollegePaymentDetailslist.Add(payment);
            }
            CollegePaymentDetailslist = CollegePaymentDetailslist.Distinct().ToList();
            return View(CollegePaymentDetailslist.OrderBy(c=>c.CollegeName).ToList());
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CollegewiseInspectionfee()
        {
            List<jntuh_college> jntuhcollege = db.jntuh_college.Where(a => a.isActive == true).Select(s=>s).ToList();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            List<CollegePaymentDetails> CollegePaymentDetailslist= new List<CollegePaymentDetails>();
            foreach (var college in jntuhcollege)
            {

                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == college.id && i.academicYearId == prAy).ToList();
                //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;
                List<int> pgdegrees = new List<int>();
                List<int> dualdegrees = new List<int>();
                List<int> ugdegrees = new List<int>();
                List<int> totaldegrees = new List<int>();
                long ugSpecializationAmmount = 0;
                long pgSpecializationAmmount = 0;
                long DualdegreeSpecializationAmmount = 0;
                long applicationFee = 0;
                int ugCount = 0;
                int dualCount = 0;
                int pgCount = 0;

                var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                foreach (var item in intakeExisting)
                {
                    jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                          .Where(e => e.collegeId == college.id && e.academicYearId == prAy && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
                                                          .Select(e => e)
                                                          .FirstOrDefault();


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
                if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                    applicationFee = 1000;
                else
                    applicationFee = 750;

                var amount = ugSpecializationAmmount + DualdegreeSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                CollegePaymentDetails payment = new CollegePaymentDetails();
                payment.CollegeId = college.id;
                payment.Collegecode = college.collegeCode.Trim();
                payment.CollegeName = college.collegeName.Trim();
                payment.FeeType = "College&Inspectionfee";
                payment.TxnAmount = amount;
                payment.UGdegrees = ugdegrees.Count;
                payment.PGdegrees = pgdegrees.Count;
                payment.ApplFee = applicationFee;
                payment.TwintyfivepercentTxnAmount =amount / 4.0;
                payment.FiftypercentTxnAmount =amount / 2.0;
                payment.HundredpercentTxnAmount =  amount;
                CollegePaymentDetailslist.Add(payment);
            }
            return View(CollegePaymentDetailslist.OrderBy(c=>c.CollegeName).ToList());
        }

        //Excel Download
        public ActionResult PaymentdoneColleges()
        {
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            List<jntuh_paymentresponse> jntuhpaymentresponse =
                db.jntuh_paymentresponse.Where(
                    p => p.AcademicYearId == ay0 && p.AuthStatus == "0300" && p.TxnDate.Year == (actualYear + 1))
                    .Select(s => s)
                    .ToList();
            List<CollegePaymentDetails> CollegePaymentDetailslist = new List<CollegePaymentDetails>();
            List<jntuh_college> jntuhColleges =
                db.jntuh_college.Where(c => c.isActive == true).Select(s => s).ToList();
            List<jntuh_college_paymentoffee_type> jntuhcollegepaymentoffeetype =
                db.jntuh_college_paymentoffee_type.Select(s => s).ToList();
            foreach (var item in jntuhpaymentresponse)
            {
                CollegePaymentDetails payment = new CollegePaymentDetails();
                payment.CollegeId =
                    jntuhColleges.Where(c => c.collegeCode == item.CollegeId).Select(s => s.id).FirstOrDefault();
                payment.Collegecode = item.CollegeId;
                payment.CollegeName =
                    jntuhColleges.Where(c => c.id == payment.CollegeId).Select(s => s.collegeName).FirstOrDefault();
                payment.ChallanNumber = item.CustomerID.Trim();
                payment.TxnReferenceNo = item.TxnReferenceNo.Trim();
                payment.PaymentAuthstatus = item.AuthStatus;
                payment.Paymentdate = item.TxnDate.ToString();
                payment.Errordescription = item.ErrorDescription;
                payment.FeeTypeId = item.PaymentTypeID;
                if (payment.FeeTypeId != 0)
                    payment.FeeType =
                        jntuhcollegepaymentoffeetype.Where(p => p.id == payment.FeeTypeId)
                            .Select(s => s.FeeType)
                            .FirstOrDefault();
                payment.TxnAmount = item.TxnAmount;
                CollegePaymentDetailslist.Add(payment);
            }

            string ReportHeader = "PaymentCompleteColleges.xls";
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportHeader);
            Response.ContentType = "application/vnd.ms-excel";
            return PartialView("~/Views/CollegePaymentDetails/PaymentDetailsExport.cshtml", CollegePaymentDetailslist);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AddPaymentDetalis()
        {
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeCode = c.co.collegeCode, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            ViewBag.PaymentType = db.jntuh_college_paymentoffee_type.Select(c =>c).ToList();
            AddcollegePaymentDetails paymentDetails= new AddcollegePaymentDetails();
            paymentDetails.BankMerchantid = "NA";
            return View(paymentDetails);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult AddPaymentDetalis(AddcollegePaymentDetails paymentDetails)
        {
            jntuh_paymentresponse addpaPaymentresponse= new jntuh_paymentresponse();
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //Adding
            ViewBag.PaymentType = db.jntuh_college_paymentoffee_type.Select(c => c).ToList();
            ViewBag.Colleges = db.jntuh_college.Join(db.jntuh_college_edit_status, co => co.id, e => e.collegeId, (co, e) => new { co = co, e = e }).Where(c => c.co.isActive == true).Select(c => new { collegeId = c.co.id, collegeCode = c.co.collegeCode, collegeName = c.co.collegeCode + "-" + c.co.collegeName }).OrderBy(c => c.collegeName).ToList();
            if (ModelState.IsValid)
            {
                addpaPaymentresponse.AcademicYearId = ay0;
                addpaPaymentresponse.CollegeId = paymentDetails.Collegecode;
                addpaPaymentresponse.MerchantID = "JAWAHARLAL";
                addpaPaymentresponse.CustomerID = paymentDetails.Customerid;
                addpaPaymentresponse.TxnReferenceNo = paymentDetails.TxnReferenceNo;
                addpaPaymentresponse.BankReferenceNo = paymentDetails.BankReferenceNo;
                addpaPaymentresponse.TxnAmount = paymentDetails.TxnAmount;
                addpaPaymentresponse.BankID = paymentDetails.Bankid;
                addpaPaymentresponse.BankMerchantID = paymentDetails.BankMerchantid == null ? "NA" : paymentDetails.BankMerchantid;
                addpaPaymentresponse.TxnType = "01";
                addpaPaymentresponse.CurrencyName = "INR";
                addpaPaymentresponse.TxnDate = Convert.ToDateTime(paymentDetails.TxnDate);
                addpaPaymentresponse.AuthStatus = "0300";
                addpaPaymentresponse.ErrorStatus = "NA";
                addpaPaymentresponse.ErrorDescription = "Y";
                addpaPaymentresponse.SettlementType = "NA";
                addpaPaymentresponse.ChallanNumber = "ChallanNumber";
                addpaPaymentresponse.PaymentTypeID = paymentDetails.Paymenttypeid;
                //addpaPaymentresponse.noctypeId = null;
                if (addpaPaymentresponse.CollegeId != null && addpaPaymentresponse.TxnAmount!=0)
                {
                    db.jntuh_paymentresponse.Add(addpaPaymentresponse);
                    db.SaveChanges();
                    TempData["Success"] = "College Code: "+paymentDetails.Collegecode + " - Payment Details Added Successfully.";
                }
                else
                {
                    TempData["Error"] = "Payment Added Failed";
                }            
            }
            return RedirectToAction("Index");
        }
        
    }
}
