using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UAAAS.Models
{
    public class CollegePaymentsDetails
    {
        public int collegeid { get; set; }
        public string collegeName { get; set; }
        public string collegeCode { get; set; }
        public string Trantype { get; set; }
        public decimal TxnAmount { get; set; }
        public virtual List<jntuh_college> collegelist { get; set; }
        public virtual List<jntuh_paymentresponse> paymentresponse { get; set; }
    }
}