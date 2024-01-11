using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class PaymentAPIController : Controller
    {
        //
        // GET: /PaymentAPI/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult FaildTransaction(int? collegeid)
        {
            //List<PaymentDetails> PaymentDetailslist = new List<PaymentDetails>();
            //if (collegeid != null)
            //{
            //    var collegecode =
            //        db.jntuh_college.Where(c => c.id == collegeid).Select(s => s.collegeCode).FirstOrDefault();
            //    var paymentlist =
            //        db.jntuh_paymentrequests.Where(s => s.CollegeCode == collegecode).Select(s => s).ToList();
            //    foreach (var item in paymentlist)
            //    {
            //        PaymentDetails PaymentDetails = new PaymentDetails();
            //        PaymentDetails.collegeName = "ZZ";
            //        PaymentDetails.collegeCode = collegecode;
            //        PaymentDetails.challnNumber = item.ChallanNumber;
            //        PaymentDetails.RequestType = "0122";
            //        PaymentDetails.merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            //        PaymentDetails.Checksumkey = WebConfigurationManager.AppSettings["ChecksumKey"];
            //        PaymentDetails.message = PaymentDetails.RequestType + "|" + PaymentDetails.merchantId + "|" + PaymentDetails.challnNumber + "|" + DateTime.Now.ToString("yyyMMddHHmmss");
            //        PaymentDetails.message += "|" + GetHMACSHA256(PaymentDetails.message, PaymentDetails.Checksumkey).ToUpper();
            //        PaymentDetailslist.Add(PaymentDetails);
            //    }

            //}
            var msg = "";
            var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            //ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            ViewBag.challnNumber = "ZZ20180130110149";
            string RequestType = "0122";
            var Checksumkey = WebConfigurationManager.AppSettings["ChecksumKey"];
            msg = RequestType + "|" + merchantId + "|" + ViewBag.challnNumber + "|" + DateTime.Now.ToString("yyyMMddHHmmss");
            msg += "|" + GetHMACSHA256(msg, Checksumkey).ToUpper();
            ViewBag.msg = msg;
            return View();
            //return View(PaymentDetailslist);
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

    public class PaymentDetails
    {
       public int collegeId { get; set; }
       public string collegeCode { get; set; }
       public  string collegeName { get; set; }
       public string challnNumber { get; set; }
       public string RequestType { get; set; }
       public string merchantId { get; set; }
       public string Checksumkey { get; set; }
       public string message { get; set; }
    }
}
