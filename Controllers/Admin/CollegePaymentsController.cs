using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers.Admin
{
    public class CollegePaymentsController : Controller
    {
        uaaasDBContext db = new uaaasDBContext();
        //
        // GET: /CollegePayments/

        public ActionResult PaymentDetails()
        {
            
            
            List<CollegePaymentsDetails> collegepaymentdetails = new List<CollegePaymentsDetails>();  
             string[] collegeid = db.jntuh_paymentresponse.Where(i=>i.AuthStatus=="0300").Select(i => i.CollegeId).Distinct().ToArray();
             foreach (var item in collegeid)
             {
                // var list = (from i in db.jntuh_college where i.collegeCode == item select new { i.collegeCode,i.collegeName}).ToList();
                 CollegePaymentsDetails collegelistclass = new CollegePaymentsDetails();
                 collegelistclass.collegeCode = (db.jntuh_college.Where(e => e.collegeCode == item).Select(e => e.collegeCode)).FirstOrDefault();
                 collegelistclass.collegeName = (db.jntuh_college.Where(e => e.collegeCode == item).Select(e => e.collegeName)).FirstOrDefault();
                 collegelistclass.TxnAmount = (db.jntuh_paymentresponse).Where(e => e.CollegeId == item).Select(e => e.TxnAmount).FirstOrDefault();

                 int Paymentid = (db.jntuh_paymentresponse).Where(e => e.CollegeId == item).Select(e => e.PaymentTypeID).FirstOrDefault();
                 
                 if(Paymentid == 6)
                 {
                     collegelistclass.Trantype = "Appeal";
                 }
                 else if (Paymentid == 7)
                 {
                     collegelistclass.Trantype = "College&Inspectionfee";
                 }
                 else
                 {
                     collegelistclass.Trantype = "LateFee";
                 }
               
                 collegepaymentdetails.Add(collegelistclass);
             }                    
                        return View(collegepaymentdetails);
        }
        public ActionResult EditPaymentDetails(string id)
        {
            List<jntuh_paymentresponse> jntuhcollegepaymentdetails = new List<jntuh_paymentresponse>();
           
            if (id != null)
            {
                var collegelist = (from i in db.jntuh_paymentresponse join j in db.jntuh_college on i.CollegeId equals j.collegeCode where i.CollegeId==id select new 
                {
                  i.CollegeId,i.CurrencyName,i.CustomerID,i.TxnReferenceNo,i.BankReferenceNo,i.TxnAmount,i.AuthStatus,i.ErrorDescription
                }).ToList();

               
                foreach (var item in collegelist)
                {
                    jntuh_paymentresponse jntupaymentresponce = new jntuh_paymentresponse();
                    jntupaymentresponce.CollegeId = item.CollegeId;
                    jntupaymentresponce.CurrencyName = item.CurrencyName;
                    jntupaymentresponce.CustomerID = item.CustomerID;
                    jntupaymentresponce.TxnReferenceNo = item.TxnReferenceNo;
                    jntupaymentresponce.BankReferenceNo = item.BankReferenceNo;
                    jntupaymentresponce.TxnAmount = item.TxnAmount;
                    jntupaymentresponce.AuthStatus = item.AuthStatus;
                    jntupaymentresponce.ErrorDescription = item.ErrorDescription;
                   // jntupaymentresponce.PaymentType=
                    jntuhcollegepaymentdetails.Add(jntupaymentresponce);
                }         
            }
            ViewBag.allpaymentlist = jntuhcollegepaymentdetails;
                return PartialView("_Edit", jntuhcollegepaymentdetails);
                     
        }
    }
}
