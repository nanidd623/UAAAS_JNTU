using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    public class CollegeFeesListController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Index()
        {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            List<CollegeFeesListModel> collegedata = new List<CollegeFeesListModel>();
            var fee = db.jntuh_college_paymentoffee.AsNoTracking().ToList();
            var colleges = db.jntuh_college.AsNoTracking().Where(i => i.isActive).ToList();

            foreach (var fff in colleges)
            {
                var AffiliationFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 5).Select(pa => pa).ToList();
                var CommonServiceFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 3).Select(pa => pa).ToList();

                if (AffiliationFee.Count > 0)
                {
                    foreach (var item in AffiliationFee)
                    {
                        var AcademicYear = jntuh_academic_year.FirstOrDefault(i => i.id == item.academicyearId).academicYear;
                        var curCmnSrFee = CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId) != null ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0";
                        collegedata.Add(new CollegeFeesListModel()
                        {
                            PaymentOfFeeId = item.id,
                            CollegeId = fff.id,
                            CollegeCode = fff.collegeCode,
                            CollegeName = fff.collegeName,
                            academicYearId = item.academicyearId,
                            year = AcademicYear,
                            CommonServiceFee = CommonServiceFee.Count > 0 ? curCmnSrFee : "0",
                            AffiliationFee = item.duesAmount,
                            TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(curCmnSrFee)),
                            GstAmount = item.gstamount
                            //AffiliationFee = item.duesAmount,
                            //CommonServiceFee = CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
                            //TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
                        });
                    }

                }

            }

            //foreach (var fff in colleges)
            //{
            //    var AffiliationFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 5).Select(pa => pa).ToList();
            //    var CommonServiceFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 3).Select(pa => pa).ToList();


            //    if (CommonServiceFee.Count > 0)
            //    {
            //        foreach (var item in CommonServiceFee)
            //        {
            //            var AcademicYear = jntuh_academic_year.FirstOrDefault(i => i.id == item.academicyearId).academicYear;
            //            var curAffFee = AffiliationFee.FirstOrDefault(i => i.academicyearId == item.academicyearId) != null ? AffiliationFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0";
            //            collegedata.Add(new CollegeFeesListModel()
            //            {
            //                PaymentOfFeeId = item.id,
            //                CollegeId = fff.id,
            //                CollegeCode = fff.collegeCode,
            //                CollegeName = fff.collegeName,
            //                academicYearId = item.academicyearId,
            //                year = AcademicYear,
            //                AffiliationFee = AffiliationFee.Count > 0 ? curAffFee : "0",
            //                CommonServiceFee = item.duesAmount,
            //                TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(curAffFee))
            //                //AffiliationFee = item.duesAmount,
            //                //CommonServiceFee = CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
            //                //TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
            //            });
            //        }

            //    }

            //}

            //collegedata = collegedata.AsEnumerable().GroupBy(p => new { p.CollegeId }).Select(p => p.FirstOrDefault()).ToList();
            return View(collegedata);
        }

        [Authorize(Roles = "Admin,Accounts")]
        public ActionResult Edit(CollegeFeesListModel collegedata)
        {
            return PartialView("_Edit", collegedata);
        }

        [Authorize(Roles = "Admin,Accounts")]
        [HttpPost]
        public ActionResult Update(CollegeFeesListModel collegedata)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();

            var AffiliationFee = db.jntuh_college_paymentoffee.Where(pa => pa.collegeId == collegedata.CollegeId && pa.FeeTypeID == 5 && pa.academicyearId == collegedata.academicYearId).Select(pa => pa).FirstOrDefault();
            var CommonServiceFee = db.jntuh_college_paymentoffee.Where(pa => pa.collegeId == collegedata.CollegeId && pa.FeeTypeID == 3 && pa.academicyearId == collegedata.academicYearId).Select(pa => pa).FirstOrDefault();

            if (AffiliationFee != null)
            {
                //update
                AffiliationFee.duesAmount = collegedata.AffiliationFee;
                AffiliationFee.gstamount = collegedata.GstAmount;
                AffiliationFee.updatedOn = DateTime.Now;
                AffiliationFee.updatedBy = userID;
                db.Entry(AffiliationFee).State = EntityState.Modified;
            }
            else
            {
                //Add
                jntuh_college_paymentoffee affiliationfeeobj = new jntuh_college_paymentoffee();
                affiliationfeeobj.collegeId = collegedata.CollegeId;
                affiliationfeeobj.academicyearId = ay0;
                affiliationfeeobj.FeeTypeID = 5;
                affiliationfeeobj.duesAmount = collegedata.AffiliationFee;
                affiliationfeeobj.gstamount = collegedata.GstAmount;
                affiliationfeeobj.createdOn = DateTime.Now;
                affiliationfeeobj.createdBy = userID;
                db.jntuh_college_paymentoffee.Add(affiliationfeeobj);
            }

            if (CommonServiceFee != null)
            {
                //update
                CommonServiceFee.duesAmount = collegedata.CommonServiceFee;
                CommonServiceFee.gstamount = collegedata.GstAmount;
                CommonServiceFee.updatedOn = DateTime.Now;
                CommonServiceFee.updatedBy = userID;
                db.Entry(CommonServiceFee).State = EntityState.Modified;
            }
            else
            {
                //Add
                jntuh_college_paymentoffee commonservicefeeobj = new jntuh_college_paymentoffee();
                commonservicefeeobj.collegeId = collegedata.CollegeId;
                commonservicefeeobj.academicyearId = ay0;
                commonservicefeeobj.FeeTypeID = 3;
                commonservicefeeobj.duesAmount = collegedata.CommonServiceFee;
                commonservicefeeobj.gstamount = collegedata.GstAmount;
                commonservicefeeobj.createdOn = DateTime.Now;
                commonservicefeeobj.createdBy = userID;
                db.jntuh_college_paymentoffee.Add(commonservicefeeobj);
            }

            db.SaveChanges();
            TempData["Success"] = "Updated Successfully";
            return RedirectToAction("Index");
        }

        public ActionResult CollegeFeesListExcel()
        {
            var gv = new GridView();
            gv.DataSource = GetCollegesFeesListData();
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            string reportName = "Colleges Fees List";
            Response.AddHeader("content-disposition", "attachment; filename=" + reportName + " " + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter objStringWriter = new StringWriter();
            HtmlTextWriter objHtmlTextWriter = new HtmlTextWriter(objStringWriter);
            gv.RenderControl(objHtmlTextWriter);
            Response.Output.Write(objStringWriter.ToString());
            Response.Flush();
            Response.End();
            return View();
        }

        public List<CollegesFeesListExport> GetCollegesFeesListData()
        {
            var jntuh_academic_year = db.jntuh_academic_year.Select(s => s).ToList();
            int actualYear = jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int ay0 = jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(a => a.id).FirstOrDefault();
            List<CollegeFeesListModel> collegedata = new List<CollegeFeesListModel>();
            var fee = db.jntuh_college_paymentoffee.AsNoTracking().ToList();
            var colleges = db.jntuh_college.AsNoTracking().Where(i => i.id != 375 && i.isActive).ToList();
            //foreach (var fff in colleges)
            //{
            //    //var AffiliationFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 5 && pa.academicyearId == (ay0 - 1)).Select(pa => pa).FirstOrDefault();
            //    // var CommonServiceFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 3 && pa.academicyearId == (ay0 - 1)).Select(pa => pa).FirstOrDefault();
            //    //if (AffiliationFee != null)
            //    //{
            //    //    collegedata.Add(new CollegeFeesListModel()
            //    //    {
            //    //        PaymentOfFeeId = AffiliationFee.id,
            //    //        CollegeId = fff.id,
            //    //        CollegeCode = fff.collegeCode,
            //    //        CollegeName = fff.collegeName,
            //    //        AffiliationFee = AffiliationFee.duesAmount,
            //    //        CommonServiceFee = CommonServiceFee != null ? CommonServiceFee.duesAmount : "0",
            //    //        TotalFee = Convert.ToString(Convert.ToInt64(AffiliationFee.duesAmount) + Convert.ToInt64(CommonServiceFee != null ? CommonServiceFee.duesAmount : "0"))
            //    //    });
            //    //}
            //    var AffiliationFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 5).Select(pa => pa).ToList();
            //    var CommonServiceFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 3).Select(pa => pa).ToList();
            //    //if (AffiliationFee.Count > 0)
            //    //{
            //    //    foreach (var item in AffiliationFee)
            //    //    {
            //    //        collegedata.Add(new CollegeFeesListModel()
            //    //        {
            //    //            PaymentOfFeeId = item.id,
            //    //            CollegeId = fff.id,
            //    //            CollegeCode = fff.collegeCode,
            //    //            CollegeName = fff.collegeName,
            //    //            academicYearId = item.academicyearId,
            //    //            year = item.academicyearId == (ay0) ? "2021-22" : "2020-21",
            //    //            AffiliationFee = item.duesAmount,
            //    //            CommonServiceFee = CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
            //    //            TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
            //    //        });
            //    //    }
            //    //}
            //    if (CommonServiceFee.Count > 0)
            //    {
            //        foreach (var item in CommonServiceFee)
            //        {
            //            collegedata.Add(new CollegeFeesListModel()
            //            {
            //                PaymentOfFeeId = item.id,
            //                CollegeId = fff.id,
            //                CollegeCode = fff.collegeCode,
            //                CollegeName = fff.collegeName,
            //                academicYearId = item.academicyearId,
            //                year = item.academicyearId == (ay0) ? "2021-22" : "2020-21",
            //                AffiliationFee = AffiliationFee.Count > 0 ? AffiliationFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
            //                CommonServiceFee = item.duesAmount,
            //                TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(AffiliationFee.Count > 0 ? AffiliationFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
            //                //AffiliationFee = item.duesAmount,
            //                //CommonServiceFee = CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
            //                //TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
            //            });
            //        }

            //    }
            //}
            foreach (var fff in colleges)
            {
                var AffiliationFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 5).Select(pa => pa).ToList();
                var CommonServiceFee = fee.Where(pa => pa.collegeId == fff.id && pa.FeeTypeID == 3).Select(pa => pa).ToList();

                if (AffiliationFee.Count > 0)
                {
                    foreach (var item in AffiliationFee)
                    {
                        var AcademicYear = jntuh_academic_year.FirstOrDefault(i => i.id == item.academicyearId).academicYear;
                        var curCmnSrFee = CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId) != null ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0";
                        collegedata.Add(new CollegeFeesListModel()
                        {
                            PaymentOfFeeId = item.id,
                            CollegeId = fff.id,
                            CollegeCode = fff.collegeCode,
                            CollegeName = fff.collegeName,
                            academicYearId = item.academicyearId,
                            year = AcademicYear,
                            CommonServiceFee = CommonServiceFee.Count > 0 ? curCmnSrFee : "0",
                            AffiliationFee = item.duesAmount,
                            TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(curCmnSrFee)),
                            GstAmount = item.gstamount
                            //AffiliationFee = item.duesAmount,
                            //CommonServiceFee = CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0",
                            //TotalFee = Convert.ToString(Convert.ToInt64(item.duesAmount) + Convert.ToInt64(CommonServiceFee.Count > 0 ? CommonServiceFee.FirstOrDefault(i => i.academicyearId == item.academicyearId).duesAmount : "0"))
                        });
                    }

                }

            }
            // collegedata = collegedata.AsEnumerable().GroupBy(p => new { p.CollegeId }).Select(p => p.FirstOrDefault()).ToList();
            var feesList = collegedata.Select(x => new CollegesFeesListExport
            {
                CollegeCode = x.CollegeCode,
                CollegeName = x.CollegeName,
                AcademicYear = x.year,
                AffiliationFee = x.AffiliationFee,
                CommonServiceFee = x.CommonServiceFee,
                GstAmount = x.GstAmount,
                TotalFee = x.TotalFee
            }).ToList();
            return feesList;
        }
    }
}
