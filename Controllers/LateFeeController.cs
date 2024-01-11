using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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

namespace UAAAS.Controllers
{
    public class LateFeeController : BaseController
    {
        //
        // GET: /LateFee/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult LateFeeDetailsandPayment()
        {
            #region AfterSubmission
            //var currentDate1 = DateTime.Now;
            //if (currentDate1 >= new DateTime(2016, 3, 20, 23, 59, 59))
            //{
            //    return RedirectToAction("College", "Dashboard");

            //}
            //else
            //{
            //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //    //int userCollegeID = 145;
            //    string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //    int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
            //    if (userCollegeID > 0 && userCollegeID != null)
            //    {

            //        //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
            //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
            //        var jntuh_specialization = db.jntuh_specialization;
            //        var jntuh_department = db.jntuh_department;
            //        var jntuh_degree = db.jntuh_degree;
            //        var jntuh_shift = db.jntuh_shift;
            //        List<int> pgdegrees = new List<int>();
            //        List<int> ugdegrees = new List<int>();
            //        List<int> totaldegrees = new List<int>();
            //        long ugSpecializationAmmount = 0;
            //        long pgSpecializationAmmount = 0;
            //        long applicationFee = 0;
            //        int ugCount = 0;
            //        int pgCount = 0;

            //        var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

            //        int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //        int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //        foreach (var item in intakeExisting)
            //        {
            //            jntuh_college_intake_existing details = db.jntuh_college_intake_existing
            //                                                  .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //                                                  .Select(e => e)
            //                                                  .FirstOrDefault();


            //            if (details != null)
            //            {
            //                if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
            //                {


            //                    if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //                    {
            //                        ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                        if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
            //                        {
            //                            ugSpecializationAmmount = 25000;
            //                            totaldegrees.AddRange(ugdegrees);
            //                        }
            //                        else
            //                        {
            //                            ugCount++;
            //                            ugSpecializationAmmount = 25000 + (ugCount * 4000);
            //                            totaldegrees.AddRange(ugdegrees);
            //                        }
            //                    }

            //                }
            //                else
            //                {
            //                    if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //                    {
            //                        pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                        pgCount++;
            //                        pgSpecializationAmmount = pgCount * 12000;
            //                        totaldegrees.AddRange(pgdegrees);
            //                    }

            //                }
            //            }



            //            // totaldegrees.AddRange(pgdegrees);
            //            //totaldegrees.AddRange(ugdegrees);
            //        }
            //        if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
            //            applicationFee = 1000;
            //        else
            //            applicationFee = 750;


            //        // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            //        //ugdegrees.AsEnumerable().Where(r=> new{r.co})
            //        ViewBag.userCollegeID = userCollegeID;
            //        ViewBag.countofUgcourse = ugdegrees.Count;
            //        ViewBag.countofPgcourse = pgdegrees.Count;
            //        var currentYear = DateTime.Now.Year;
            //        var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentType == "Latefee") > 0;
            //        ViewBag.IsLatePaymentDone = isPaid;
            //        var totalFee = 0.0;
            //        if (!isPaid)
            //        {
            //            var amount = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
            //            var currentDate = DateTime.Now;
            //            if (currentDate >= new DateTime(2016, 3, 15, 06, 00, 00) && currentDate <= new DateTime(2016, 3, 16, 23, 59, 59))
            //            {
            //                totalFee = amount / 4.0;
            //            }
            //            if (currentDate >= new DateTime(2016, 3, 17) && currentDate <= new DateTime(2016, 3, 18, 23, 59, 59))
            //            {
            //                totalFee = amount / 2.0;
            //            }
            //            if (currentDate >= new DateTime(2016, 3, 19) && currentDate <= new DateTime(2016, 3, 20, 23, 59, 59))
            //            {
            //                totalFee = amount;
            //            }
            //        }

            //        ViewBag.totalFee = ViewBag.lateFee = totalFee;
            //        ViewBag.collegeCode = clgCode;
            //        ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            //        var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode).OrderByDescending(it => it.TxnDate).ToList();
            //        ViewBag.Payments = payments;
            //        var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl1"];
            //        var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            //        var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            //        var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            //        var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
            //        var msg = "";
            //        if (userCollegeID == 375)
            //        {
            //            msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
            //            var key = WebConfigurationManager.AppSettings["ChecksumKey"];
            //            msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            //        }
            //        else
            //        {
            //            msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
            //            var key = WebConfigurationManager.AppSettings["ChecksumKey"];
            //            msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            //        }

            //        ViewBag.msg = msg;

            //    }
            //    return View();
            //}
            #endregion

            if (Membership.GetUser() == null)
            {
                return RedirectToAction("LogOn", "Account");
            }
            else
            {
                #region BeforeSubmission
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                if (userCollegeID == 375)
                {
                    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
                }

                DateTime todayDate = DateTime.Now.Date;
                int Pagestatus = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                         editStatus.IsCollegeEditable == true &&
                                                                                         editStatus.editFromDate <= todayDate &&
                                                                                         editStatus.editToDate >= todayDate)
                                                                    .Select(editStatus => editStatus.id)
                                                                    .FirstOrDefault();



                if (Pagestatus == 0 && Roles.IsUserInRole("College"))
                {
                    ViewBag.NotUpload = true;
                }
                else
                {
                    ViewBag.NotUpload = false;
                }

                string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
                List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
                int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
                if (userCollegeID > 0 && userCollegeID != null)
                {

                    List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
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

                    int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                    int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                    foreach (var item in intakeExisting)
                    {
                        jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                              .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
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



                        // totaldegrees.AddRange(pgdegrees);
                        //totaldegrees.AddRange(ugdegrees);
                    }
                    if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                        applicationFee = 1000;
                    else
                        applicationFee = 750;


                    // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                    //ugdegrees.AsEnumerable().Where(r=> new{r.co})
                    ViewBag.userCollegeID = userCollegeID;
                    ViewBag.countofUgcourse = ugdegrees.Count;
                    ViewBag.countofPgcourse = pgdegrees.Count;
                    var currentYear = DateTime.Now.Year;
                    var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8) > 0;
                    ViewBag.IsLatePaymentDone = isPaid;
                    var totalFee = 0.0;
                    if (!isPaid)
                    {
                        var amount = ugSpecializationAmmount+DualdegreeSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                        var currentDate = DateTime.Now;
                        if (currentDate >= new DateTime(2023, 4, 16, 00, 00, 00) && currentDate <= new DateTime(2023, 4, 17, 23, 59, 59))
                        {
                            totalFee = amount / 4.0;
                        }
                        if (currentDate >= new DateTime(2023, 4, 18) && currentDate <= new DateTime(2023, 4, 19, 23, 59, 59))
                        {
                            totalFee = amount / 2.0;
                        }
                        if (currentDate >= new DateTime(2023, 4, 20) && currentDate <= new DateTime(2023, 4, 21, 23, 59, 59))
                        {
                            totalFee = amount;
                        }
                    }
                    else //Once Late Fee Paid Checking 
                    {
                        var LateFeePaid = 0.0;
                        var lateFeeData = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8&&it.AcademicYearId==AY0).Select(it => it.TxnAmount).ToList();

                        var lateFeesum = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8&&it.AcademicYearId==AY0).Select(e => e.TxnAmount).Sum();

                        foreach (var late in lateFeeData)
                        {
                            if (late != null)
                                LateFeePaid += (double)late;
                        }
                        var amount = ugSpecializationAmmount+DualdegreeSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                        var currentDate = DateTime.Now;
                        if (currentDate >= new DateTime(2023, 4, 16, 00, 00, 00) && currentDate <= new DateTime(2023, 4, 17, 23, 59, 59))
                        {
                            totalFee = amount / 4.0;
                        }
                        if (currentDate >= new DateTime(2023, 4, 18) && currentDate <= new DateTime(2023, 4, 19, 23, 59, 59))
                        {
                            totalFee = amount / 2.0;
                        }
                        if (currentDate >= new DateTime(2023, 4, 20) && currentDate <= new DateTime(2023, 4, 21, 23, 59, 59))
                        {
                            totalFee = amount;
                        }
                        if (LateFeePaid >= totalFee)
                        {
                            totalFee = 0;
                        }
                        else
                        {
                            totalFee = totalFee - LateFeePaid;
                        }              
                    }

                    ViewBag.totalFee = ViewBag.lateFee = totalFee;
                    ViewBag.collegeCode = clgCode;
                    ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                    var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode&&it.AcademicYearId==AY0&&it.AuthStatus=="0300" &&it.PaymentTypeID==8).OrderByDescending(it => it.TxnDate).ToList();
                    ViewBag.Payments = payments;
                    var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl1"];
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
                return View();
                #endregion
            }          

        }

        [HttpPost]
        public ActionResult LateFeeDetailsandPayment(string msg)
        {
            SaveResponse(msg, "ChallanNumber");
            return RedirectToAction("LateFeeDetailsandPayment");
        }

        [HttpPost]
        public ActionResult SavePaymentRequest(string challanNumber, decimal txnAmount, string collegeCode)
        {

            var appSettings = WebConfigurationManager.AppSettings;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var req = new jntuh_paymentrequests();
            req.TxnAmount = txnAmount;
            req.AcademicYearId = prAy;
            req.CollegeCode = collegeCode;
            req.ChallanNumber = challanNumber;
            req.MerchantID = appSettings["MerchantID"];
            req.CustomerID = appSettings["CustomerID"];
            req.SecurityID = appSettings["SecurityID"];
            req.CurrencyType = appSettings["CurrencyType"];
            req.TxnDate = DateTime.Now;
            req.PaymentTypeID = 8;
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
                clgCode = cid.Substring(0,2);
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
             //Response Message Saving
            temp_aeronautical temp_aeronautical = new temp_aeronautical();
            temp_aeronautical.Degree = responseMsg.Length < 255
                ? responseMsg
                : responseMsg.Substring(0, 254); ;
            temp_aeronautical.Department = "Latefee";
            temp_aeronautical.Specialization = clgCode;
            temp_aeronautical.DegreeId = userCollegeID;
            temp_aeronautical.LabCode = DateTime.Now.ToString();
            db.temp_aeronautical.Add(temp_aeronautical);
            db.SaveChanges();
           
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            var dbResponse = new UAAAS.Models.jntuh_paymentresponse();
            dbResponse.AcademicYearId = prAy;
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
            dbResponse.PaymentTypeID = 8;
            db.jntuh_paymentresponse.Add(dbResponse);

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
            db.SaveChanges();
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



        public ActionResult LateFeeDetailsandPaymentView(string id)
        {
            #region AfterSubmission
            //var currentDate1 = DateTime.Now;
            //if (currentDate1 >= new DateTime(2016, 3, 20, 23, 59, 59))
            //{
            //    return RedirectToAction("College", "Dashboard");

            //}
            //else
            //{
            //    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //    //int userCollegeID = 145;
            //    string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            //    List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            //    int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
            //    if (userCollegeID > 0 && userCollegeID != null)
            //    {

            //        //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
            //        List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
            //        var jntuh_specialization = db.jntuh_specialization;
            //        var jntuh_department = db.jntuh_department;
            //        var jntuh_degree = db.jntuh_degree;
            //        var jntuh_shift = db.jntuh_shift;
            //        List<int> pgdegrees = new List<int>();
            //        List<int> ugdegrees = new List<int>();
            //        List<int> totaldegrees = new List<int>();
            //        long ugSpecializationAmmount = 0;
            //        long pgSpecializationAmmount = 0;
            //        long applicationFee = 0;
            //        int ugCount = 0;
            //        int pgCount = 0;

            //        var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

            //        int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            //        int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            //        foreach (var item in intakeExisting)
            //        {
            //            jntuh_college_intake_existing details = db.jntuh_college_intake_existing
            //                                                  .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
            //                                                  .Select(e => e)
            //                                                  .FirstOrDefault();


            //            if (details != null)
            //            {
            //                if (item.jntuh_specialization.jntuh_department.degreeId == 5 || item.jntuh_specialization.jntuh_department.degreeId == 4)
            //                {


            //                    if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //                    {
            //                        ugdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                        if (ugdegrees.Count != 0 && ugdegrees.Count <= 4)
            //                        {
            //                            ugSpecializationAmmount = 25000;
            //                            totaldegrees.AddRange(ugdegrees);
            //                        }
            //                        else
            //                        {
            //                            ugCount++;
            //                            ugSpecializationAmmount = 25000 + (ugCount * 4000);
            //                            totaldegrees.AddRange(ugdegrees);
            //                        }
            //                    }

            //                }
            //                else
            //                {
            //                    if (item.proposedIntake != 0 && item.courseStatus != "Closure")
            //                    {
            //                        pgdegrees.Add(item.jntuh_specialization.jntuh_department.degreeId);
            //                        pgCount++;
            //                        pgSpecializationAmmount = pgCount * 12000;
            //                        totaldegrees.AddRange(pgdegrees);
            //                    }

            //                }
            //            }



            //            // totaldegrees.AddRange(pgdegrees);
            //            //totaldegrees.AddRange(ugdegrees);
            //        }
            //        if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
            //            applicationFee = 1000;
            //        else
            //            applicationFee = 750;


            //        // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
            //        //ugdegrees.AsEnumerable().Where(r=> new{r.co})
            //        ViewBag.userCollegeID = userCollegeID;
            //        ViewBag.countofUgcourse = ugdegrees.Count;
            //        ViewBag.countofPgcourse = pgdegrees.Count;
            //        var currentYear = DateTime.Now.Year;
            //        var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentType == "Latefee") > 0;
            //        ViewBag.IsLatePaymentDone = isPaid;
            //        var totalFee = 0.0;
            //        if (!isPaid)
            //        {
            //            var amount = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
            //            var currentDate = DateTime.Now;
            //            if (currentDate >= new DateTime(2016, 3, 15, 06, 00, 00) && currentDate <= new DateTime(2016, 3, 16, 23, 59, 59))
            //            {
            //                totalFee = amount / 4.0;
            //            }
            //            if (currentDate >= new DateTime(2016, 3, 17) && currentDate <= new DateTime(2016, 3, 18, 23, 59, 59))
            //            {
            //                totalFee = amount / 2.0;
            //            }
            //            if (currentDate >= new DateTime(2016, 3, 19) && currentDate <= new DateTime(2016, 3, 20, 23, 59, 59))
            //            {
            //                totalFee = amount;
            //            }
            //        }

            //        ViewBag.totalFee = ViewBag.lateFee = totalFee;
            //        ViewBag.collegeCode = clgCode;
            //        ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
            //        var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode).OrderByDescending(it => it.TxnDate).ToList();
            //        ViewBag.Payments = payments;
            //        var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl1"];
            //        var merchantId = WebConfigurationManager.AppSettings["MerchantID"];
            //        var securityId = WebConfigurationManager.AppSettings["SecurityID"];
            //        var typefield1 = WebConfigurationManager.AppSettings["TypeField1"];
            //        var typefield2 = WebConfigurationManager.AppSettings["TypeField2"];
            //        var msg = "";
            //        if (userCollegeID == 375)
            //        {
            //            msg = merchantId + "|" + ViewBag.challnNumber + "|NA|2.00|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
            //            var key = WebConfigurationManager.AppSettings["ChecksumKey"];
            //            msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            //        }
            //        else
            //        {
            //            msg = merchantId + "|" + ViewBag.challnNumber + "|NA|" + ViewBag.totalFee + "|NA|NA|NA|INR|NA|" + typefield1 + "|" + securityId + "|NA|NA|" + typefield2 + "|NA|NA|NA|NA|NA|NA|NA|" + returnUrl;
            //            var key = WebConfigurationManager.AppSettings["ChecksumKey"];
            //            msg += "|" + GetHMACSHA256(msg, key).ToUpper();
            //        }

            //        ViewBag.msg = msg;

            //    }
            //    return View();
            //}
            #endregion



            #region BeforeSubmission
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]));
            }

            DateTime todayDate = DateTime.Now.Date;
            int Pagestatus = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                     editStatus.IsCollegeEditable == true &&
                                                                                     editStatus.editFromDate <= todayDate &&
                                                                                     editStatus.editToDate >= todayDate)
                                                                .Select(editStatus => editStatus.id)
                                                                .FirstOrDefault();



            if (Pagestatus == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            string clgCode = db.jntuh_college.SingleOrDefault(a => a.id == userCollegeID).collegeCode;
            List<CollegeIntakeExisting> collegeIntakeExisting = new List<CollegeIntakeExisting>();
            int PresentAcademicYear = db.jntuh_college_intake_existing.Where(a => a.isActive == true).OrderByDescending(a => a.academicYearId).Select(a => a.academicYearId).FirstOrDefault();
            if (userCollegeID > 0 && userCollegeID != null)
            {

                //List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID && i.academicYearId == PresentAcademicYear).ToList();
                List<jntuh_college_intake_existing> intake = db.jntuh_college_intake_existing.Where(i => i.collegeId == userCollegeID).ToList();
                var jntuh_specialization = db.jntuh_specialization;
                var jntuh_department = db.jntuh_department;
                var jntuh_degree = db.jntuh_degree;
                var jntuh_shift = db.jntuh_shift;
                List<int> pgdegrees = new List<int>();
                List<int> ugdegrees = new List<int>();
                List<int> totaldegrees = new List<int>();
                long ugSpecializationAmmount = 0;
                long pgSpecializationAmmount = 0;
                long applicationFee = 0;
                int ugCount = 0;
                int pgCount = 0;

                var intakeExisting = intake.GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();

                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int AY0 = db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
                foreach (var item in intakeExisting)
                {
                    jntuh_college_intake_existing details = db.jntuh_college_intake_existing
                                                          .Where(e => e.collegeId == userCollegeID && e.academicYearId == AY0 && e.specializationId == item.specializationId && e.shiftId == item.shiftId)
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



                    // totaldegrees.AddRange(pgdegrees);
                    //totaldegrees.AddRange(ugdegrees);
                }
                if (pgdegrees.Count > 0 && ugdegrees.Count > 0)
                    applicationFee = 1000;
                else
                    applicationFee = 750;


                // collegeIntakeExisting = collegeIntakeExisting.AsEnumerable().GroupBy(r => new { r.specializationId, r.shiftId }).Select(r => r.First()).ToList();
                //ugdegrees.AsEnumerable().Where(r=> new{r.co})
                ViewBag.userCollegeID = userCollegeID;
                ViewBag.countofUgcourse = ugdegrees.Count;
                ViewBag.countofPgcourse = pgdegrees.Count;
                var currentYear = DateTime.Now.Year;
                var isPaid = db.jntuh_paymentresponse.Count(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8) > 0;
                ViewBag.IsLatePaymentDone = isPaid;
                var totalFee = 0.0;
                if (!isPaid)
                {
                    var amount = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                    var currentDate = DateTime.Now;
                    //if (currentDate >= new DateTime(2021, 8, 18, 00, 00, 00) && currentDate <= new DateTime(2021, 8, 19, 23, 59, 59))
                    //{
                    //    totalFee = amount / 4.0;
                    //}
                    //if (currentDate >= new DateTime(2021, 8, 20) && currentDate <= new DateTime(2021, 8, 21, 23, 59, 59))
                    //{
                    //    totalFee = amount / 2.0;
                    //}
                    //if (currentDate >= new DateTime(2021, 8, 22) && currentDate <= new DateTime(2021, 8, 23, 23, 59, 59))
                    //{
                    //    totalFee = amount;
                    //}

                    if (currentDate >= new DateTime(2023, 4, 16, 00, 00, 00) && currentDate <= new DateTime(2023, 4, 17, 23, 59, 59))
                    {
                        totalFee = amount / 4.0;
                    }
                    if (currentDate >= new DateTime(2023, 4, 18) && currentDate <= new DateTime(2023, 4, 19, 23, 59, 59))
                    {
                        totalFee = amount / 2.0;
                    }
                    if (currentDate >= new DateTime(2023, 4, 20) && currentDate <= new DateTime(2023, 4, 21, 23, 59, 59))
                    {
                        totalFee = amount;
                    }
                }
                else //Once Late Fee Paid Checking 
                {
                    var LateFeePaid = 0.0;
                    var lateFeeData = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8).Select(it => it.TxnAmount).ToList();

                    var lateFeesum = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode && it.TxnDate.Year == currentYear && it.AuthStatus == "0300" && it.PaymentTypeID == 8).Select(e => e.TxnAmount).Sum();

                    foreach (var late in lateFeeData)
                    {
                        if (late != null)
                            LateFeePaid += (double)late;
                    }
                    var amount = ugSpecializationAmmount + pgSpecializationAmmount + applicationFee;
                    var currentDate = DateTime.Now;
                    //if (currentDate >= new DateTime(2021, 8, 18, 00, 30, 00) && currentDate <= new DateTime(2021, 8, 19, 23, 59, 59))
                    //{
                    //    totalFee = amount / 4.0;
                    //}
                    //if (currentDate >= new DateTime(2021, 8, 20) && currentDate <= new DateTime(2021, 8, 21, 23, 59, 59))
                    //{
                    //    totalFee = amount / 2.0;
                    //}
                    //if (currentDate >= new DateTime(2021, 8, 22) && currentDate <= new DateTime(2021, 8, 23, 23, 59, 59))
                    //{
                    //    totalFee = amount;
                    //}

                    if (currentDate >= new DateTime(2023, 4, 16, 00, 00, 00) && currentDate <= new DateTime(2023, 4, 17, 23, 59, 59))
                    {
                        totalFee = amount / 4.0;
                    }
                    if (currentDate >= new DateTime(2023, 4, 18) && currentDate <= new DateTime(2023, 4, 19, 23, 59, 59))
                    {
                        totalFee = amount / 2.0;
                    }
                    if (currentDate >= new DateTime(2023, 4, 20) && currentDate <= new DateTime(2023, 4, 21, 23, 59, 59))
                    {
                        totalFee = amount;
                    }

                    totalFee = totalFee - LateFeePaid;
                }
                ViewBag.totalFee = ViewBag.lateFee = totalFee;
                ViewBag.collegeCode = clgCode;
                ViewBag.challnNumber = clgCode + DateTime.Now.ToString("yyyMMddHHmmss");
                var payments = db.jntuh_paymentresponse.Where(it => it.CollegeId == clgCode&&it.AuthStatus=="0300"&&it.AcademicYearId==AY0&&it.PaymentTypeID==8).OrderByDescending(it => it.TxnDate).ToList();
                ViewBag.Payments = payments;
                var returnUrl = WebConfigurationManager.AppSettings["ReturnUrl1"];
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
            return View();
            #endregion



        }

    }
}
