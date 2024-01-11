using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace UAAAS.Models
{
    public class CollegePlayground
    {
        public int id { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of Playgrounds")]
        public int totalPlaygrounds { get; set; }

        public string PlaygroundType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Playground(s) Type")]
        public string[] PlayGroundTypeId { get; set; }

        public IEnumerable<PlayGroundTypeModel> GroundTypes { get; set; }

        public string modeOfTransport { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Mode of Transport to reach the Institute")]
        public string[] modeOfTransportId { get; set; }

        public IEnumerable<ModeOfTransportModel> TransportModes { get; set; }

        [Display(Name = "Number of bus by college")]
        public int numberOfBus { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Number of other transport vehicles by college")]
        public int numberOfOtherVehicles { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Mode of Payment of Salary")]
        public string[] modeOfPaymentId { get; set; }

        public string modeOfPayment { get; set; }

        public IEnumerable<ModeOfPaymentModel> PaymentModes { get; set; }

        
        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_college jntuh_college { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_sports_type jntuh_sports_type { get; set; }
    }

    public class PlayGroundTypeModel
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Checked { get; set; }
    }

    public class ModeOfPaymentModel
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Checked { get; set; }
    }

    public class ModeOfTransportModel
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int Checked { get; set; }
    }

}