using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace UAAAS.Models.Admin
{
    public class PaymentResponse
    {
        public long Id { get; set; }
        public Nullable<int> AcademicYearId { get; set; }
        public string CollegeId { get; set; }
        public string FeeType  { get; set; }
        public string CollegeName { get; set; }
        public string PaymentTypeName  { get; set; }
        [Remote("NewCollegecodeCheck", "PaymentResponse", AdditionalFields = "CollegeId", HttpMethod = "POST", ErrorMessage = "Customer Number Already Exists.")]
        public string  CustomerID { get; set; }
        public string MerchantID { get; set; }
        public string TxnReferenceNo { get; set; }
        public string BankReferenceNo { get; set; }
        public decimal TxnAmount { get; set; }
        public string BankID { get; set; }
        public string BankMerchantID { get; set; }
        public string TxnType { get; set; }
        public string CurrencyName { get; set; }
        public System.DateTime TxnDate { get; set; }
        public string AuthStatus { get; set; }
        public string ErrorStatus { get; set; }
        public string ErrorDescription { get; set; }
        public string SettlementType { get; set; }
        public string ChallanNumber { get; set; }
        public int PaymentTypeID { get; set; }
        public Nullable<int> noctypeId { get; set; }
    }
}