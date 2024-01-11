using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class SocietyController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private List<Society> SocietyDetails(Society Society)
        {
            List<Society> SocietyList = new List<Society>();
            SocietyList = (from College in db.jntuh_college
                           join CollegeAddress in db.jntuh_address on College.id equals CollegeAddress.collegeId
                           join CollegeEditStatus in db.jntuh_college_edit_status on College.id equals CollegeEditStatus.collegeId
                           where (College.isActive == true && CollegeAddress.addressTye=="COLLEGE")
                           select new Society
                           {
                               CollegeId = College.id,
                               CollegeCode = College.collegeCode,
                               CollegeName = College.collegeName,
                               IsCollegeEditable = CollegeEditStatus.IsCollegeEditable,
                               DistrictId = CollegeAddress.districtId,
                               CollegeTypeId = College.collegeTypeID
                             }).ToList();
            foreach (var item in SocietyList)
            {
                var AddressDetails = (from Address in db.jntuh_address
                                      join SocietyEstablishment in db.jntuh_college_establishment 
                                      on Address.collegeId equals SocietyEstablishment.collegeId
                                      where(SocietyEstablishment.collegeId==item.CollegeId &&
                                            Address.collegeId == item.CollegeId &&
                                            Address.addressTye=="SOCIETY")
                                      select new {
                                          SocietyAddress=Address.address,
                                          LandLine = Address.landline,
                                          Emailaddress=Address.email,
                                          Name=SocietyEstablishment.societyName
                                      }).FirstOrDefault();
                if (AddressDetails != null)
                {
                    item.SocietyName = AddressDetails.Name;
                    item.SocietyAddress = AddressDetails.SocietyAddress;
                    item.PhoneNumber = AddressDetails.LandLine;
                    item.Email = AddressDetails.Emailaddress;
                }
            }
            if (Society.SelectedCollegeType != null)
            {
                if (Society.SelectedCollegeType.Trim() == "All Colleges" && Society.DistrictId == 0 && Society.CollegeTypeId == 0)
                {
                    SocietyList = SocietyList.ToList();
                }
                else if (Society.SelectedCollegeType.Trim() != "All Colleges" && Society.DistrictId != 0 && Society.CollegeTypeId != 0)
                {
                    if (Society.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Society.IsCollegeEditable = true;
                    }
                    else if (Society.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Society.IsCollegeEditable = false;
                    }
                    SocietyList = SocietyList.Where(c => c.IsCollegeEditable == Society.IsCollegeEditable &&
                                                             c.DistrictId == Society.DistrictId &&
                                                             c.CollegeTypeId == Society.CollegeTypeId)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() != "All Colleges" && Society.DistrictId == 0 && Society.CollegeTypeId == 0)
                {
                    if (Society.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Society.IsCollegeEditable = true;
                    }
                    else if (Society.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Society.IsCollegeEditable = false;
                    }
                    SocietyList = SocietyList.Where(c => c.IsCollegeEditable == Society.IsCollegeEditable)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() == "All Colleges" && Society.DistrictId != 0 && Society.CollegeTypeId == 0)
                {
                    SocietyList = SocietyList.Where(c => c.DistrictId == Society.DistrictId)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() == "All Colleges" && Society.DistrictId == 0 && Society.CollegeTypeId != 0)
                {
                    SocietyList = SocietyList.Where(c => c.CollegeTypeId == Society.CollegeTypeId)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() != "All Colleges" && Society.DistrictId != 0 && Society.CollegeTypeId == 0)
                {
                    if (Society.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Society.IsCollegeEditable = true;
                    }
                    else if (Society.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Society.IsCollegeEditable = false;
                    }
                    SocietyList = SocietyList.Where(c => c.IsCollegeEditable == Society.IsCollegeEditable &&
                                                             c.DistrictId == Society.DistrictId)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() != "All Colleges" && Society.DistrictId == 0 && Society.CollegeTypeId != 0)
                {

                    if (Society.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Society.IsCollegeEditable = true;
                    }
                    else if (Society.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Society.IsCollegeEditable = false;
                    }
                    SocietyList = SocietyList.Where(c => c.IsCollegeEditable == Society.IsCollegeEditable &&
                                                             c.CollegeTypeId == Society.CollegeTypeId)
                                                 .ToList();
                }
                else if (Society.SelectedCollegeType.Trim() == "All Colleges" && Society.DistrictId != 0 && Society.CollegeTypeId != 0)
                {
                    SocietyList = SocietyList.Where(c => c.DistrictId == Society.DistrictId &&
                                                             c.CollegeTypeId == Society.CollegeTypeId)
                                                 .ToList();
                }
            }
            string[] SelectedCollegeTypes = { "All Colleges",
                                              "PendingColleges",
                                              "Submitted Colleges",
                                            };
            ViewBag.Districts = db.jntuh_district
                                  .Where(district => district.isActive == true)
                                  .ToList();
            ViewBag.CollegeTypes = db.jntuh_college_type
                                  .Where(c => c.isActive == true)
                                  .ToList();
            ViewBag.SelectedCollegeTypes = SelectedCollegeTypes;
            ViewBag.SocietyDetails = SocietyList;
            ViewBag.Count = SocietyList.Count();
            return SocietyList;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Society()
        {
            Society Society = new Society();
            SocietyDetails(Society);
            return View("~/Views/Reports/Society.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Society(Society Society, string cmd)
        {
            List<Society> SocietyList = SocietyDetails(Society);
            int Count = SocietyList.Count();
            if (cmd == "Export To Excel")
            {                
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=CollegeSocietyReport.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_Society.cshtml");
            }
            else
            {
                return View("~/Views/Reports/Society.cshtml");
            }            
        }
    }
}
