using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class CollegePaymentDetails
    {
        public int Id { get; set; }
        public int CollegeId { get; set; }
        public string CollegeName { get; set; }
        public string Collegecode { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public int AcademicYearId { get; set; }
        public string AcademicYear { get; set; }
        public int FeeTypeId { get; set; }
        public string FeeType { get; set; }
        public int Sub_PurposeId { get; set; }
        public string Sub_Purpose { get; set; }
        public List<SelectListItem> Departments { get; set; }
        public string DepartmentIds { get; set; }
        public int BankId { get; set; }
        public string Bank { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? DueAmount { get; set; }
        public string Trans_Number { get; set; }
        public string DD_Date { get; set; }
        public DateTime? DDdate_New { get; set; }
        public bool isActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int  UpdatedBy { get; set; }

        public decimal TxnAmount { get; set; }
        public double TwintyfivepercentTxnAmount { get; set; }
        public double FiftypercentTxnAmount { get; set; }
        public double HundredpercentTxnAmount { get; set; }
        public string ChallanNumber { get; set; }
        public string TxnReferenceNo { get; set; }
        public string PaymentAuthstatus { get; set; }
        public string Errordescription { get; set; }
        public string Paymentdate { get; set; }
        public int PGdegrees { get; set; }
        public int UGdegrees { get; set; }
        public decimal ApplFee { get; set; }
    }

    public class AddcollegePaymentDetails
    {
        //[Display(Name = "")]
        public int AcademicYearid { get; set; }
        [Required(ErrorMessage = "*")]
        public string Collegecode { get; set; }
        public string Collegename { get; set; }
       
        public int Collegeid { get; set; }
        public string Merchantid { get; set; }
        [Required(ErrorMessage = "*")]
        public string Customerid { get; set; }
         [Required(ErrorMessage = "*")]
        public string TxnReferenceNo { get; set; }
         [Required(ErrorMessage = "*")]
        public string BankReferenceNo { get; set; }
        [Required(ErrorMessage = "*")]
        public decimal TxnAmount { get; set; }
         [Required(ErrorMessage = "*")]
        public string Bankid { get; set; }
        public string BankMerchantid { get; set; }
        public string Txntype { get; set; }
        public string Currencyname { get; set; }
        [Required(ErrorMessage = "*")]
        public string TxnDate { get; set; }
        public string Authstatus { get; set; }
        public string Settlementtype { get; set; }
        public string ErrorStatus { get; set; }
        public string Errordescription { get; set; }
        public string ChallanNumber { get; set; }
         [Required(ErrorMessage = "*")]
        public int Paymenttypeid { get; set; }
        public string Paymentype { get; set; }
    }
}