using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace UAAAS.Models
{
    public class LandInformation
    {
        public LandInformation()
        {
        }
        public int id { get; set; }

        public int collegeId { get; set; }

        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^[0-9]\d{0,2}(\.\d{1,2})?%?$", ErrorMessage = "Please enter correct Area In Acres, ex: 999.99")]
        [Display(Name = "Total Land Area")]
        public decimal areaInAcres { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Land Type")]
        public string[] landTypeID { get; set; }
        public IEnumerable<Item> landType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Land Registration Type")]
        public string[] landRegistrationTypeId { get; set; }
        public IEnumerable<Item> landRegistrationType { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Land Category")]
        public string[] landCategoryId { get; set; }
        public IEnumerable<Item> landCategory { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Issued by")]
        public string conversionCertificateIssuedBy { get; set; }

        [Display(Name = "Issued Date")]
        public string conversionCertificateIssuedDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Issued Purpose")]
        public string conversionCertificateIssuedPurpose { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Issued by")]
        public string buildingPlanIssuedBy { get; set; }

        [Display(Name = "Issued Date")]
        public string buildingPlanIssuedDate { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Issued by")]
        public string masterPlanIssuedBy { get; set; }

        [Display(Name = "Issued Date")]
        public string masterPlanIssuedDate { get; set; }


        [Required(ErrorMessage = "Required")]
        [Display(Name = "Compound Wall/Fencing")]
        public bool compoundWall { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Approach Road")]
        public string[] approachRoadId { get; set; }
        public IEnumerable<Item> approachRoad { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Power Supply")]
        public string[] powerSupplyId { get; set; }
        public IEnumerable<Item> powerSupply { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Water Supply")]
        public string[] WaterSupplyId { get; set; }
        public IEnumerable<Item> WaterSupply { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Drinking Water")]
        public string[] drinkingWaterId { get; set; }
        public IEnumerable<Item> DrinkingWater { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Is Water Purified?")]
        public bool IsPurifiedWater { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Potable water ")]
        public int potableWaterPerDay { get; set; }

        public string landConversionFilePath { get; set; }

        [Display(Name = "Upload Land Conversion")]
        public HttpPostedFileBase landConversionFile { get; set; }

        public string buildingPlanFilePath { get; set; }

        [Display(Name = "Upload Building Plan")]
        public HttpPostedFileBase buildingPlanFile { get; set; }

        public string masterPlanFilePath { get; set; }

        [Display(Name = "Upload Master Plan")]
        public HttpPostedFileBase masterplanFile { get; set; }

        public string landRegistrationFilePath { get; set; }

        [Display(Name = "Registration Document")]
        public HttpPostedFileBase landRegistrationFile { get; set; }

        public Nullable<System.DateTime> createdOn { get; set; }
        public Nullable<int> createdBy { get; set; }
        public Nullable<System.DateTime> updatedOn { get; set; }
        public Nullable<int> updatedBy { get; set; }

        public virtual jntuh_approach_road jntuh_approach_road { get; set; }
        public virtual jntuh_college jntuh_college { get; set; }
        public virtual jntuh_land_type jntuh_land_type { get; set; }
        public virtual jntuh_land_registration_type jntuh_land_registration_type { get; set; }
        public virtual jntuh_facility_status jntuh_facility_status { get; set; }
        public virtual jntuh_facility_status jntuh_facility_status1 { get; set; }
        public virtual jntuh_water_type jntuh_water_type { get; set; }
        public virtual my_aspnet_users my_aspnet_users { get; set; }
        public virtual my_aspnet_users my_aspnet_users1 { get; set; }
        public virtual jntuh_land_category jntuh_land_category { get; set; }       
    }
}