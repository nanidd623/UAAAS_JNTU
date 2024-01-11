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
    public class PrincipalController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        private List<Principal> PrincipalDetails(Principal Principal)
        {            
            List<Principal> PrincipalList = new List<Principal>();
            PrincipalList = (from College in db.jntuh_college
                             join CollegeAddress in db.jntuh_address on College.id equals CollegeAddress.collegeId
                             join CollegeEditStatus in db.jntuh_college_edit_status on College.id equals CollegeEditStatus.collegeId
                             where (College.isActive == true &&
                                    CollegeAddress.addressTye == "COLLEGE")
                             select new Principal
                             {
                                 CollegeId = College.id,
                                 CollegeCode = College.collegeCode,
                                 CollegeName = College.collegeName,
                                 IsCollegeEditable = CollegeEditStatus.IsCollegeEditable,
                                 DistrictId = CollegeAddress.districtId,
                                 CollegeTypeId = College.collegeTypeID
                             }).ToList();
            foreach (var item in PrincipalList)
            {
                jntuh_college_principal_director PrincipalDetails = db.jntuh_college_principal_director
                                                                      .Where(p => p.collegeId == item.CollegeId &&
                                                                                  p.type == "PRINCIPAL")
                                                                      .FirstOrDefault();
                if (PrincipalDetails != null)
                {
                    item.PrincipalName = PrincipalDetails.surname + " " + PrincipalDetails.firstName + " " + PrincipalDetails.lastName;
                    item.PhoneNumber = PrincipalDetails.landline;
                    item.MobileNumber = PrincipalDetails.mobile;
                    item.Email = PrincipalDetails.email;
                }
            }
            if (Principal.SelectedCollegeType != null)
            {
                if (Principal.SelectedCollegeType.Trim() == "All Colleges" && Principal.DistrictId == 0 && Principal.CollegeTypeId == 0)
                {
                    PrincipalList = PrincipalList.ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() != "All Colleges" && Principal.DistrictId != 0 && Principal.CollegeTypeId != 0)
                {
                    if (Principal.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Principal.IsCollegeEditable = true;
                    }
                    else if (Principal.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Principal.IsCollegeEditable = false;
                    }
                    PrincipalList = PrincipalList.Where(c => c.IsCollegeEditable == Principal.IsCollegeEditable &&
                                                             c.DistrictId==Principal.DistrictId &&
                                                             c.CollegeTypeId==Principal.CollegeTypeId)
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() != "All Colleges" && Principal.DistrictId == 0 && Principal.CollegeTypeId == 0)
                {
                    if (Principal.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Principal.IsCollegeEditable = true;
                    }
                    else if (Principal.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Principal.IsCollegeEditable = false;
                    }
                    PrincipalList = PrincipalList.Where(c => c.IsCollegeEditable == Principal.IsCollegeEditable)
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() == "All Colleges" && Principal.DistrictId != 0 && Principal.CollegeTypeId == 0)
                {
                    PrincipalList = PrincipalList.Where(c => c.DistrictId == Principal.DistrictId)
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() == "All Colleges" && Principal.DistrictId == 0 && Principal.CollegeTypeId != 0)
                {
                    PrincipalList = PrincipalList.Where(c => c.CollegeTypeId == Principal.CollegeTypeId)
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() != "All Colleges" && Principal.DistrictId != 0 && Principal.CollegeTypeId == 0)
                {
                    if (Principal.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Principal.IsCollegeEditable = true;
                    }
                    else if (Principal.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Principal.IsCollegeEditable = false;
                    }
                    PrincipalList = PrincipalList.Where(c => c.IsCollegeEditable == Principal.IsCollegeEditable &&
                                                             c.DistrictId == Principal.DistrictId )
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() != "All Colleges" && Principal.DistrictId == 0 && Principal.CollegeTypeId != 0)
                {

                    if (Principal.SelectedCollegeType.Trim() == "PendingColleges")
                    {
                        Principal.IsCollegeEditable = true;
                    }
                    else if (Principal.SelectedCollegeType.Trim() == "Submitted Colleges")
                    {
                        Principal.IsCollegeEditable = false;
                    }
                    PrincipalList = PrincipalList.Where(c => c.IsCollegeEditable == Principal.IsCollegeEditable && 
                                                             c.CollegeTypeId == Principal.CollegeTypeId)
                                                 .ToList();
                }
                else if (Principal.SelectedCollegeType.Trim() == "All Colleges" && Principal.DistrictId != 0 && Principal.CollegeTypeId != 0)
                {
                    PrincipalList = PrincipalList.Where(c => c.DistrictId == Principal.DistrictId &&
                                                             c.CollegeTypeId == Principal.CollegeTypeId)
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
            ViewBag.PrincipalDetails = PrincipalList;
            ViewBag.Count = PrincipalList.Count();
            return PrincipalList;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Principal()
        {
            Principal Principal=new Principal();
            PrincipalDetails(Principal);
            return View("~/Views/Reports/Principal.cshtml");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Principal(Principal Principal,string cmd)
        {
            List<Principal> PrincipalList = PrincipalDetails(Principal);
            int Count = PrincipalList.Count();
            if (cmd == "Export To Excel" && Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=CollegeSocietyReport.xls");
                Response.ContentType = "application/vnd.ms-excel";
                return PartialView("~/Views/Reports/_Principal.cshtml");
            }
            else
            {
                return View("~/Views/Reports/Principal.cshtml");
            }   
        }
    }
}
