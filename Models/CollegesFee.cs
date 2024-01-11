using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegesFee
    {
    }

    public class College_DD_Payment_Report
    {
        public int Collegeid { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }

        public int AcademicYear { get; set; }
        public string DD_No { get; set; }
        public DateTime? DD_Date { get; set; }
        public int FeeTypeId { get; set; }
        public int SubFeeTypeId { get; set; }
        public string FeeType { get; set; }
        public string SubFeeType { get; set; }
        public string BankName { get; set; }
        public decimal? Amount { get; set; }
    }

    public class DD_Payments
    {
        public int TotalDD_Count { get; set; }
        public int Today_Transactons { get; set; }
        public decimal? Today_Amount { get; set; }
        public decimal? PaymentAmount { get; set; }
    }

    public class Bank_DD_Payments_count
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyReport
    {
        public int? BankId { get; set; }
        public int? CollegeId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? PaymentTypeId { get; set; }
        public string fromdate { get; set; }
        public string todate { get; set; }
    }

    public class DCB_Reports
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string FirstYear { get; set; }
        public string SecondYear { get; set; }
        public string ThridYear { get; set; }
        public string FouthYear { get; set; }
        public decimal? FirstYearDemand { get; set; }
        public decimal? FirstYearCollection { get; set; }
        public decimal? FirstYearBalance { get; set; }
        public decimal? SecondYearDemand { get; set; }
        public decimal? SecondYearCollection { get; set; }
        public decimal? SecondYearBalance { get; set; }
        public decimal? ThridYearDemand { get; set; }
        public decimal? ThridYearCollection { get; set; }
        public decimal? ThridYearBalance { get; set; }
        public decimal? FouthYearDemand { get; set; }
        public decimal? FouthYearCollection { get; set; }
        public decimal? FouthYearBalance { get; set; }

    }

    public class Get_College_Fee
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string InspectionFee { get; set; }
        public string UGAffiFee { get; set; }
        public string PGAffiFee { get; set; }
    }

    public class CollegeBalanceFee
    {
        public int CollegeId { get; set; }
        public string Collegecode { get; set; }
        public string CollegeName { get; set; }
        public bool isAutonomous { get; set; }
        public bool isActive { get; set; }
        public int FirstYearApprovedIntake { get; set; }
        public int FirstYear { get; set; }
        public decimal FirstYearFee { get; set; }
        public decimal FirstYearAffiliationFee { get; set; }
        public decimal? FirstYear_PaidFee { get; set; }
        public decimal? FirstYear_BalanceFee { get; set; }
        public int SecondYearApprovedIntake { get; set; }
        public int SecondYear { get; set; }
        public decimal SecondYearFee { get; set; }
        public decimal SecondYearAffiliationFee { get; set; }
        public decimal? SecondYear_PaidFee { get; set; }
        public decimal? SecondYear_BalanceFee { get; set; }
        public int ThirdYear { get; set; }
        public decimal ThridYearFee { get; set; }
        public decimal ThridYearAffiliationFee { get; set; }
        public decimal? ThridYear_PaidFee { get; set; }
        public decimal? ThridYear_BalanceFee { get; set; }
        public int FourthYear { get; set; }
        public decimal FourthYearFee { get; set; }
        public decimal FourthYearAffiliationFee { get; set; }
        public decimal? FourthYear_PaidFee { get; set; }
        public decimal? FourthYear_BalanceFee { get; set; }
        public decimal TotalFee { get; set; }
        public decimal Total_PaidFee { get; set; }
        public decimal Total_BalanceFee { get; set; }
    }

    public class CollegesInspectionFee
    {
        public int CollegeId { get; set; }
        public string Collegecode { get; set; }
        public string CollegeName { get; set; }
        public int CountofUGDegree { get; set; }
        public int CountofPGDegree { get; set; }
        public int CountofDualDegree { get; set; }
        public int CountofDegree { get; set; }
        public long SumofUGAmount { get; set; }
        public long SumofPGAmount { get; set; }
        public long SumofDualAmount { get; set; }
        public long SumofAmount { get; set; }
        public long ApplicationFee { get; set; }
        public long TotalFee { get; set; }
        public decimal TwentyFivePercentLateFee { get; set; }
        public decimal FiftyPercentLateFee { get; set; }
        public decimal HundredPercentLateFee { get; set; }
        public decimal LateFee { get; set; }
        public decimal BilldeskAmount { get; set; }
        public decimal DuplicateAmount { get; set; }
    }

    public class AffiliationPendingFee
    {
        public int? CollegeId { get; set; }
        public string college { get; set; }
        public int? AcademicYearId { get; set; }
        public decimal? PreviousFee { get; set; }
        public decimal? AffiliationFee { get; set; }
        public decimal? CurrentYearPayment { get; set; }

        public string Message { get; set; }
    }

    public class AFRCFee
    {
        public int? AcademicyearId { get; set; }
        public int? CollegeId { get; set; }
        public string college { get; set; }
        public int? PharmacyFee { get; set; }
        [Required]
        public int? EngineeringFee { get; set; }

        public string Message { get; set; }
    }
}