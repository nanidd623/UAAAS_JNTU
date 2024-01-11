using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers.Permanent_Affiliation
{
    public class PermanentaffiliationpaymentDetailsController : Controller
    {
        //
        // GET: /OnlineAppeal/

        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult PApaymentDetails()
        {
            var currentDate = DateTime.Now;
            DateTime EditFromDate;
            DateTime EditTODate;
            bool PageEdible = false;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 375)
            {
                userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            }
            DateTime todayDate = DateTime.Now;
            ViewBag.NotUpload = db.jntuh_appeal_college_edit_status.Where(i => i.collegeId == userCollegeID).Select(i => i.IsCollegeEditable).FirstOrDefault();
            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            var CollegeDetails = db.jntuh_pa_college_edit_status.Where(C => C.collegeId == userCollegeID).Select(C => new { C.collegeId, C.editFromDate, C.editToDate, C.IsCollegeEditable }).FirstOrDefault();
            if (CollegeDetails != null)
            {
                EditFromDate = Convert.ToDateTime(CollegeDetails.editFromDate);
                EditTODate = Convert.ToDateTime(CollegeDetails.editToDate);
                PageEdible = Convert.ToBoolean(CollegeDetails.IsCollegeEditable);
                if (currentDate >= EditFromDate && currentDate <= EditTODate)
                {

                }
                else
                {
                    return RedirectToAction("CollegeDashboard", "Dashboard");
                }
                if (userCollegeID > 0 && userCollegeID != null && currentDate >= EditFromDate && currentDate <= EditTODate)
                {

                    int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                    var currentYear = DateTime.Now.Year;
                    var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 6) > 0;
                    ViewBag.IsLatePaymentDone = isPaid;
                    var totalFee = 120000.0;
                    ViewBag.totalFee = ViewBag.lateFee = totalFee;
                    ViewBag.collegeCode = clgCode;
                    ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                    var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.AcademicYearId == AY0 && it.TxnDate.Year == currentYear && it.PaymentTypeID == 6 && it.AuthStatus == "0300").OrderByDescending(it => it.TxnDate).ToList();
                    ViewBag.Payments = payments;
                    var returnUrl = WebConfigurationManager.AppSettings["PAReturnUrl"];
                    var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
                    var securityId = WebConfigurationManager.AppSettings["SecurityID"];
                    var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
                    var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
                    var msg = "";
                    if (userCollegeID == 375)
                    {
                        msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                        var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                        msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                    }
                    else
                    {
                        msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
                        var key = WebConfigurationManager.AppSettings["ChecksumKey"];
                        msg += "|" + GetHMACSHA256(msg, key).ToUpper();
                    }

                    ViewBag.msg = msg;




                }
                else
                {
                    if (currentDate >= EditFromDate && currentDate <= EditTODate)
                    {

                        // return RedirectToAction("CollegeDashboard", "Dashboard");

                    }
                }
            }
            else
            {

                // return RedirectToAction("College", "Dashboard");
            }

            return View();
        }

        [HttpPost]
        public ActionResult PApaymentDetails(string msg)
        {
            SaveResponse(msg, "ChallanNumber");
            return RedirectToAction("OnlineAppealFeeDetailsandPayment");
        }

        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {

            var appSettings = WebConfigurationManager.AppSettings;
            var req = new jntuh_paymentrequests();
            req.TxnAmount = txnAmount;
            req.CollegeCode = collegeCode;
            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = 6;
            db.jntuh_paymentrequests.Add(req);

            db.SaveChanges();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void SaveResponse(string responseMsg, string challanno)
        {
            var tokens = responseMsg.Split('|');
            int userID = 0;
            int userCollegeID = 0;
            string clgCode = string.Empty;
            if (Membership.GetUser() == null)
            {
                //return RedirectToAction("LogOn", "Account");
                string cid = tokens[1];
                clgCode = cid.Substring(0, 2);
                userCollegeID =
                    db.jntuh_college.Where(c => c.collegeCode == clgCode.Trim()).Select(s => s.id).FirstOrDefault();
                userID = db.jntuh_college_users.Where(u => u.collegeID == userCollegeID).Select(u => u.userID).FirstOrDefault();
            }
            else
            {
                userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            }
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            //var tokens = responseMsg.Split('|');
            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.MerchantID = tokens[0];
            dbResponse.CustomerID = tokens[1];
            dbResponse.TxnReferenceNo = tokens[2];
            dbResponse.BankReferenceNo = tokens[3];
            dbResponse.TxnAmount = decimal.Parse(tokens[4]);
            dbResponse.BankID = tokens[5];
            dbResponse.BankMerchantID = tokens[6];
            dbResponse.TxnType = tokens[7];
            dbResponse.CurrencyName = tokens[8];
            dbResponse.TxnDate = DateTime.Now;
            dbResponse.AuthStatus = tokens[14];
            dbResponse.SettlementType = tokens[15];
            dbResponse.ErrorStatus = tokens[23];
            dbResponse.ErrorDescription = tokens[24];
            dbResponse.CollegeId = clgCode;
            dbResponse.ChallanNumber = challanno;
            dbResponse.AcademicYearId = 11;
            dbResponse.PaymentTypeID = 6;
            db.jntuh_paymentresponse.Add(dbResponse);
            db.SaveChanges();
            //Log file paymentresponse
            //var filename = DateTime.Now.Date + ".txt";
            //string paymentpath = "~/Content/Payment/PaymentResponses";
            //FileStream fs = null;
            //if (!Directory.Exists(Server.MapPath(paymentpath)))
            //{
            //    Directory.CreateDirectory(Server.MapPath(paymentpath));
            //}
            //string filepath = Server.MapPath(paymentpath) + "\\" + @"LogFile" + DateTime.Now.ToString("CL_yyyyMMdd") + ".txt";
            //fs = new FileStream(filepath, FileMode.Append);
            //var log = new StreamWriter(fs, Encoding.UTF8);
            //log.WriteLine("Date : {0} Time : {1}", DateTime.Now.ToString("MMMM dd, yyyy"), DateTime.Now.ToString("hh:mm:ss"));
            //log.WriteLine("Message : {0} ", "College Code :- " + clgCode + ". Challan No :- " + challanno + ". MerchantID :- " + tokens[6] + ". CustomerID :- " + tokens[1] + ". Transaction Date :- " + DateTime.Now + ". Transaction Amount :- " + decimal.Parse(tokens[4]));
            //log.WriteLine("==============================================================================================================================================================");
            //log.Close();
            //fs.Close();
            //Log file paymentrequest

            //mail

            var collegename = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeName;
            var membershipmailid = db.my_aspnet_membership.Where(i => i.userId == userID).FirstOrDefault().Email;
            IUserMailer mailer = new UserMailer();
            mailer.PaymentResponse(membershipmailid, "Payment Response", dbResponse.CollegeId + " / " + collegename, dbResponse.CustomerID, dbResponse.TxnReferenceNo, dbResponse.BankReferenceNo, dbResponse.TxnAmount, dbResponse.TxnDate.ToString(), dbResponse.ErrorDescription, dbResponse.ChallanNumber, "Payment Response", "JNTUH-AAC-ONLINE APPLICATION PAYMENT STATUS").SendAsync();

        }

        public static string GetHMACSHA256(string text, string key)
        {
            UTF8Encoding encoder = new UTF8Encoding();

            byte[] hashValue;
            byte[] keybyt = encoder.GetBytes(key);
            byte[] message = encoder.GetBytes(text);

            HMACSHA256 hashString = new HMACSHA256(keybyt);
            string hex = "";

            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

    }
}
