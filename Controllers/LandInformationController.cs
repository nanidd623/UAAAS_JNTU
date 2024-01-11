using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class LandInformationController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();

        // GET: /LandInformation/Create
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Create(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            else if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }   
            int userLandInformationID = db.jntuh_college_land.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
           
           
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (userCollegeID > 0 && userLandInformationID > 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("View", "LandInformation");
            }
            if (userCollegeID > 0 && userLandInformationID > 0 && status > 0 && Roles.IsUserInRole("College"))
            {
                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //    return RedirectToAction("View", "LandInformation");
                //}
                //else
                //{
                //    return RedirectToAction("Edit", "LandInformation");
                //}
                return RedirectToAction("Edit", "LandInformation");
            }
            if (userCollegeID > 0 && userLandInformationID > 0 && (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry")))
            {
                return RedirectToAction("Edit", "LandInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            if (userLandInformationID == 0 && status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.NotUpload = true;
            }
            else
            {
                ViewBag.NotUpload = false;
            }

            bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

            if (isPageEditable)
            {
                ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "LandInformation");
            }

            ViewBag.State = db.jntuh_state.Where(state => state.isActive == true).ToList();

            LandInformation landInformation = new LandInformation();
            landInformation.collegeId = userCollegeID;
            string[] strSelected = new string[] { };//new string[] { "8", "10" };

            List<Item> lstLandType = new List<Item>();
            foreach (var type in db.jntuh_land_type.Where(landType => landType.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandType.Add(new Item { id = type.id, name = type.landType, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landType = lstLandType;

            List<Item> lstLandRegistrationType = new List<Item>();
            foreach (var type in db.jntuh_land_registration_type.Where(landRegistrationType => landRegistrationType.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandRegistrationType.Add(new Item { id = type.id, name = type.landRegistrationType, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landRegistrationType = lstLandRegistrationType;

            List<Item> lstLandCategory = new List<Item>();
            foreach (var type in db.jntuh_land_category.Where(landCategory => landCategory.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }
            landInformation.landCategory = lstLandCategory;

            List<Item> lstApproachRoad = new List<Item>();
            foreach (var type in db.jntuh_approach_road.Where(approachRoad => approachRoad.isActive == true))
            {
                string strtype = type.id.ToString();
                lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.approachRoad = lstApproachRoad;

            List<Item> lstPowerSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(facultyStatus => facultyStatus.isActive == true))
            {
                string strtype = type.id.ToString();
                lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.powerSupply = lstPowerSupply;

            List<Item> lstWaterSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(facultyStatus => facultyStatus.isActive == true))
            {
                string strtype = type.id.ToString();
                lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.WaterSupply = lstWaterSupply;

            List<Item> lstDrinkingWater = new List<Item>();
            foreach (var type in db.jntuh_water_type.Where(waterType => waterType.isActive == true))
            {
                string strtype = type.id.ToString();
                lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = strSelected.Contains(strtype) ? 1 : 0 });
            }

            landInformation.DrinkingWater = lstDrinkingWater;

            var regDetails = db.jntuh_college_land_registration.Where(landRegistration => landRegistration.isActive == true &&
                                                                                   landRegistration.collegeId == userCollegeID).ToList();
            if (regDetails.Count() == 0 && status == 0)
            {
                ViewBag.regDetailsNotUpload = true;
            }
            else
            {
                ViewBag.regDetailsNotUpload = false;
            }
            ViewBag.RegistrationDetails = regDetails;
            ViewBag.Count = regDetails.Count();
            if (regDetails.Count() == 0 && status == 0)
            {
                ViewBag.RegDetailsNotUpload = true;
            }
            else
            {
                ViewBag.RegDetailsNotUpload = false;
            }
            return View(landInformation);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Create(LandInformation landInformation)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = landInformation.collegeId;
            }
            SaveLandInformation(landInformation);

            ViewBag.RegistrationDetails = db.jntuh_college_land_registration.Where(landRegistration => landRegistration.isActive == true &&
                                                                                   landRegistration.collegeId == userCollegeID).ToList();
            return View(landInformation);
        }

        private void SaveLandInformation(LandInformation landInformation)
        {
            var resultMeassage = string.Empty;
            if (ModelState.IsValid)
            {
                //get current logged in user id
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                int userCollegeID = db.jntuh_college_users.Where(collegeUser => collegeUser.userID == userID).Select(collegeUser => collegeUser.collegeID).FirstOrDefault();
                string collegeCode = db.jntuh_college.Where(u => u.id == userCollegeID).Select(u => u.collegeCode).FirstOrDefault();
                if (userCollegeID == 0)
                {
                    userCollegeID = landInformation.collegeId;
                }
                //Create College Land Object
                jntuh_college_land jntuh_college_land = new jntuh_college_land();

                jntuh_college_land.id = landInformation.id;
                jntuh_college_land.collegeId = userCollegeID;
                jntuh_college_land.areaInAcres = landInformation.areaInAcres;
                jntuh_college_land.landTypeID = Convert.ToInt32(landInformation.landTypeID
                                                             .Where(landType => !string.IsNullOrWhiteSpace(landType))
                                                             .Select(landType => landType).FirstOrDefault());
                jntuh_college_land.landRegistrationTypeId = Convert.ToInt32(landInformation.landRegistrationTypeId
                                                                         .Where(landRegistrationType => !string.IsNullOrWhiteSpace(landRegistrationType))
                                                                         .Select(landRegistrationType => landRegistrationType).FirstOrDefault());
                jntuh_college_land.landCategoryId = Convert.ToInt32(landInformation.landCategoryId
                                                                 .Where(landCategory => !string.IsNullOrWhiteSpace(landCategory))
                                                                 .Select(landCategory => landCategory).FirstOrDefault());
                jntuh_college_land.conversioncertificateissuedBy = landInformation.conversionCertificateIssuedBy;

                if (landInformation.conversionCertificateIssuedDate != null)
                {
                    jntuh_college_land.conversionCertificateIssuedDate = Utilities.DDMMYY2MMDDYY(landInformation.conversionCertificateIssuedDate);
                }
                else
                {
                    jntuh_college_land.conversionCertificateIssuedDate = null;
                }

                jntuh_college_land.conversionCertificateIssuedPurpose = landInformation.conversionCertificateIssuedPurpose;

                #region Land Conversion Certificate

                if (landInformation.landConversionFile != null)
                {
                    string land_conversionfile = "~/Content/Upload/College/LandInformation/LandConversion";
                    if (!Directory.Exists(Server.MapPath(land_conversionfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(land_conversionfile));
                    }
                    var ext = Path.GetExtension(landInformation.landConversionFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (landInformation.landConversionFilePath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            landInformation.landConversionFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(land_conversionfile), fileName, ext));
                            landInformation.landConversionFilePath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_land.landconversiondocument = landInformation.landConversionFilePath;
                        }
                        else
                        {
                            landInformation.landConversionFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(land_conversionfile), landInformation.landConversionFilePath));
                            jntuh_college_land.landconversiondocument = landInformation.landConversionFilePath;
                        }
                    }
                }
                else
                {
                    jntuh_college_land.landconversiondocument = landInformation.landConversionFilePath;
                }

                #endregion

                #region Buiding Plan Certificate

                if (landInformation.buildingPlanFile != null)
                {
                    string buildingPlan_file = "~/Content/Upload/College/LandInformation/BuildingPlan";
                    if (!Directory.Exists(Server.MapPath(buildingPlan_file)))
                    {
                        Directory.CreateDirectory(Server.MapPath(buildingPlan_file));
                    }
                    var ext = Path.GetExtension(landInformation.buildingPlanFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (landInformation.buildingPlanFilePath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            landInformation.buildingPlanFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(buildingPlan_file), fileName, ext));
                            landInformation.buildingPlanFilePath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_land.approvedbuildingplans = landInformation.buildingPlanFilePath;
                        }
                        else
                        {
                            landInformation.buildingPlanFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(buildingPlan_file), landInformation.buildingPlanFilePath));
                            jntuh_college_land.approvedbuildingplans = landInformation.buildingPlanFilePath;
                        }
                    }
                }
                else
                {
                    jntuh_college_land.approvedbuildingplans = landInformation.buildingPlanFilePath;
                }

                #endregion

                #region Master Plan Certificate

                if (landInformation.masterplanFile != null)
                {
                    string masterPlan_file = "~/Content/Upload/College/LandInformation/MasterPlan";
                    if (!Directory.Exists(Server.MapPath(masterPlan_file)))
                    {
                        Directory.CreateDirectory(Server.MapPath(masterPlan_file));
                    }
                    var ext = Path.GetExtension(landInformation.masterplanFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (landInformation.masterPlanFilePath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            landInformation.masterplanFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(masterPlan_file), fileName, ext));
                            landInformation.masterPlanFilePath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_land.masterplanofthecampus = landInformation.masterPlanFilePath;
                        }
                        else
                        {
                            landInformation.masterplanFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(masterPlan_file), landInformation.masterPlanFilePath));
                            jntuh_college_land.masterplanofthecampus = landInformation.masterPlanFilePath;
                        }
                    }
                }
                else
                {
                    jntuh_college_land.masterplanofthecampus = landInformation.masterPlanFilePath;
                }

                #endregion

                #region Land Registration Certificate

                if (landInformation.landRegistrationFile != null)
                {
                    string land_registrationfile = "~/Content/Upload/College/LandInformation/LandRegistration";
                    if (!Directory.Exists(Server.MapPath(land_registrationfile)))
                    {
                        Directory.CreateDirectory(Server.MapPath(land_registrationfile));
                    }
                    var ext = Path.GetExtension(landInformation.landRegistrationFile.FileName);
                    if (ext.ToUpper().Equals(".PDF") || ext.ToUpper().Equals(".PDF"))
                    {
                        if (landInformation.landRegistrationFilePath == null)
                        {
                            string fileName = collegeCode + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmssffffff") + "-" + "JNTUH";
                            landInformation.landRegistrationFile.SaveAs(string.Format("{0}/{1}{2}", Server.MapPath(land_registrationfile), fileName, ext));
                            landInformation.landRegistrationFilePath = string.Format("{0}{1}", fileName, ext);
                            jntuh_college_land.landregistrationdocument = landInformation.landRegistrationFilePath;
                        }
                        else
                        {
                            landInformation.landRegistrationFile.SaveAs(string.Format("{0}/{1}", Server.MapPath(land_registrationfile), landInformation.landRegistrationFilePath));
                            jntuh_college_land.landregistrationdocument = landInformation.landRegistrationFilePath;
                        }
                    }
                }
                else
                {
                    jntuh_college_land.landregistrationdocument = landInformation.landRegistrationFilePath;
                }

                #endregion

                jntuh_college_land.buildingPlanIssuedBy = landInformation.buildingPlanIssuedBy;

                if (landInformation.buildingPlanIssuedDate != null)
                {
                    jntuh_college_land.buildingPlanIssuedDate = Utilities.DDMMYY2MMDDYY(landInformation.buildingPlanIssuedDate);
                }
                else
                {
                    jntuh_college_land.buildingPlanIssuedDate = null;
                }

                jntuh_college_land.masterPlanIssuedBy = landInformation.masterPlanIssuedBy;

                if (landInformation.masterPlanIssuedDate != null)
                {
                    jntuh_college_land.masterPlanIssuedDate = Utilities.DDMMYY2MMDDYY(landInformation.masterPlanIssuedDate);
                }
                else
                {
                    jntuh_college_land.masterPlanIssuedDate = null;
                }

                jntuh_college_land.compoundWall = landInformation.compoundWall;
                jntuh_college_land.approachRoadId = Convert.ToInt32(landInformation.approachRoadId
                                                                 .Where(approachRoad => !string.IsNullOrWhiteSpace(approachRoad))
                                                                 .Select(approachRoad => approachRoad)
                                                                 .FirstOrDefault());
                jntuh_college_land.powerSupplyId = Convert.ToInt32(landInformation.powerSupplyId
                                                                .Where(powerSupply => !string.IsNullOrWhiteSpace(powerSupply))
                                                                .Select(powerSupply => powerSupply)
                                                                .FirstOrDefault());
                jntuh_college_land.WaterSupplyId = Convert.ToInt32(landInformation.WaterSupplyId
                                                                .Where(waterSupply => !string.IsNullOrWhiteSpace(waterSupply))
                                                                .Select(waterSupply => waterSupply)
                                                                .FirstOrDefault());
                jntuh_college_land.drinkingWaterId = Convert.ToInt32(landInformation.drinkingWaterId
                                                                  .Where(drinkingWater => !string.IsNullOrWhiteSpace(drinkingWater))
                                                                  .Select(drinkingWater => drinkingWater)
                                                                  .FirstOrDefault());

                jntuh_college_land.IsPurifiedWater = landInformation.IsPurifiedWater;

                jntuh_college_land.potableWaterPerDay = landInformation.potableWaterPerDay;

                int existLandId = db.jntuh_college_land.Where(collegeLand => collegeLand.collegeId == userCollegeID)
                                                     .Select(collegeLand => collegeLand.id)
                                                     .FirstOrDefault();

                if (existLandId == 0) // If college land id and college id is zero then insert the College land records
                {
                    jntuh_college_land.createdBy = userID;
                    jntuh_college_land.createdOn = DateTime.Now;
                    db.jntuh_college_land.Add(jntuh_college_land);
                    resultMeassage = "Save";
                }

                else // If college land id and college id is not equal to zero then modify the existing the College land records
                {
                    jntuh_college_land.id = existLandId;
                    jntuh_college_land.createdBy = db.jntuh_college_land.Where(collegeLand => collegeLand.id == existLandId)
                                                                        .Select(collegeLand => collegeLand.createdBy)
                                                                        .FirstOrDefault();
                    jntuh_college_land.createdOn = db.jntuh_college_land.Where(collegeLand => collegeLand.id == existLandId)
                                                                        .Select(collegeLand => collegeLand.createdOn)
                                                                        .FirstOrDefault();
                    jntuh_college_land.updatedBy = userID;
                    jntuh_college_land.updatedOn = DateTime.Now;
                    db.Entry(jntuh_college_land).State = EntityState.Modified;
                    resultMeassage = "Update";
                }

                db.SaveChanges();
                if (resultMeassage == "Update")
                {
                    TempData["Success"] = "College Land Information Details Updated successfully.";
                }
                else
                {
                    TempData["Success"] = "College Land Information Details Added successfully.";
                }

            }

            //after postback

            int userName = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userNameCollegeID = db.jntuh_college_users.Where(u => u.userID == userName).Select(u => u.collegeID).FirstOrDefault();
            string[] selectedLandType = landInformation.landTypeID.Where(landType => !string.IsNullOrWhiteSpace(landType)).Select(landType => landType).ToArray();
            List<Item> lstLandType = new List<Item>();
            foreach (var type in db.jntuh_land_type.Where(landType => landType.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandType.Add(new Item { id = type.id, name = type.landType, selected = selectedLandType.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landType = lstLandType;

            string[] selectedLandRegistrationType = landInformation.landRegistrationTypeId.Where(landRegistrationType => !string.IsNullOrWhiteSpace(landRegistrationType)).Select(landRegistrationType => landRegistrationType).ToArray();
            List<Item> lstLandRegistrationtype = new List<Item>();
            foreach (var type in db.jntuh_land_registration_type.Where(landRegistrationType => landRegistrationType.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandRegistrationtype.Add(new Item { id = type.id, name = type.landRegistrationType, selected = selectedLandRegistrationType.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landRegistrationType = lstLandRegistrationtype;

            string[] selectedLandCategory = landInformation.landCategoryId.Where(landCategory => !string.IsNullOrWhiteSpace(landCategory)).Select(landCategory => landCategory).ToArray();
            List<Item> lstLandCategory = new List<Item>();
            foreach (var type in db.jntuh_land_category.Where(landCategory => landCategory.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = selectedLandCategory.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landCategory = lstLandCategory;

            var college_land = db.jntuh_college_land.Where(l => l.collegeId == userNameCollegeID).FirstOrDefault();


            landInformation.landConversionFilePath = college_land.landconversiondocument;
            landInformation.buildingPlanFilePath = college_land.approvedbuildingplans;
            landInformation.masterPlanFilePath = college_land.masterplanofthecampus;
            landInformation.landRegistrationFilePath = college_land.landregistrationdocument;

            string[] selectedApproachRoad = landInformation.approachRoadId.Where(approachRoad => !string.IsNullOrWhiteSpace(approachRoad)).Select(approachRoad => approachRoad).ToArray();
            List<Item> lstApproachRoad = new List<Item>();
            foreach (var type in db.jntuh_approach_road.Where(approachRoad => approachRoad.isActive == true))
            {
                string strtype = type.id.ToString();
                lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = selectedApproachRoad.Contains(strtype) ? 1 : 0 });
            }

            landInformation.approachRoad = lstApproachRoad;

            string[] selectedPowerSupply = landInformation.powerSupplyId.Where(powerSupply => !string.IsNullOrWhiteSpace(powerSupply)).Select(powerSupply => powerSupply).ToArray();
            List<Item> lstPowerSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(powerSupply => powerSupply.isActive == true))
            {
                string strtype = type.id.ToString();
                lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedPowerSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.powerSupply = lstPowerSupply;

            string[] selectedWaterSupply = landInformation.WaterSupplyId.Where(waterSupply => !string.IsNullOrWhiteSpace(waterSupply)).Select(waterSupply => waterSupply).ToArray();
            List<Item> lstWaterSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(waterSupply => waterSupply.isActive == true))
            {
                string strtype = type.id.ToString();
                lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedWaterSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.WaterSupply = lstWaterSupply;

            string[] selectedDrinkingwater = landInformation.drinkingWaterId.Where(drinkingWater => !string.IsNullOrWhiteSpace(drinkingWater)).Select(drinkingWater => drinkingWater).ToArray();
            List<Item> lstDrinkingWater = new List<Item>();
            foreach (var type in db.jntuh_water_type.Where(d => d.isActive == true))
            {
                string strtype = type.id.ToString();
                lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = selectedDrinkingwater.Contains(strtype) ? 1 : 0 });
            }
            ViewBag.RegistrationDetails = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                               r.collegeId == userNameCollegeID).ToList();
            landInformation.DrinkingWater = lstDrinkingWater;


        }

        // GET: /LandInformation/Edit/1
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Edit(string collegeId)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0 && collegeId!=null)
            {
                if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                {
                    userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                }
            }
            ViewBag.collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString();
            int userLandID = db.jntuh_college_land.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
            if (userCollegeID == 0 && Roles.IsUserInRole("College"))
            {
                return RedirectToAction("Create", "CollegeInformation");
            }
            if (userCollegeID == 0 && (Roles.IsUserInRole("DataEntry") || Roles.IsUserInRole("Admin")))
            {
                return RedirectToAction("Create", "CollegeInformation", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

            if (userLandID == 0)
            {
                return RedirectToAction("Create", "LandInformation",new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }
            DateTime todayDate = DateTime.Now.Date;
            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status == 0 && Roles.IsUserInRole("College"))
            {
                ViewBag.IsEditable = false;
                return RedirectToAction("View", "LandInformation");
            }
            else
            {
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                    return RedirectToAction("View", "LandInformation");
                }
                //ViewBag.IsEditable = true;
            }
         

            ////RAMESH:To-DisableEdit
            //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
            //{
            //    ViewBag.IsEditable = false;
            //    return RedirectToAction("View", "LandInformation");
            //}
            //else
            //{
            //    ViewBag.IsEditable = true;
            //}

            LandInformation landInformation = new LandInformation();
            jntuh_college_land jntuh_college_land = db.jntuh_college_land.Find(userLandID);
            landInformation.id = jntuh_college_land.id;
            landInformation.collegeId = jntuh_college_land.collegeId;
            landInformation.areaInAcres = jntuh_college_land.areaInAcres;

            string[] selectedLandType = jntuh_college_land.landTypeID.ToString().Split(' ');
            List<Item> lstLandType = new List<Item>();
            foreach (var type in db.jntuh_land_type.Where(l => l.isActive == true))
            {
                string strType = type.id.ToString();
                lstLandType.Add(new Item { id = type.id, name = type.landType, selected = selectedLandType.Contains(strType) ? 1 : 0 });
            }

            landInformation.landType = lstLandType;

            string[] selectedLandRegistrationType = jntuh_college_land.landRegistrationTypeId.ToString().Split(' ');
            List<Item> lstLandRegistrationtype = new List<Item>();
            foreach (var type in db.jntuh_land_registration_type.Where(r => r.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandRegistrationtype.Add(new Item { id = type.id, name = type.landRegistrationType, selected = selectedLandRegistrationType.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landRegistrationType = lstLandRegistrationtype;

            string[] selectedLandCategory = jntuh_college_land.landCategoryId.ToString().Split(' ');
            List<Item> lstLandCategory = new List<Item>();
            foreach (var type in db.jntuh_land_category.Where(c => c.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = selectedLandCategory.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landCategory = lstLandCategory;

            landInformation.landConversionFilePath = jntuh_college_land.landconversiondocument;
            landInformation.buildingPlanFilePath = jntuh_college_land.approvedbuildingplans;
            landInformation.masterPlanFilePath = jntuh_college_land.masterplanofthecampus;
            landInformation.landRegistrationFilePath = jntuh_college_land.landregistrationdocument;

            landInformation.conversionCertificateIssuedBy = jntuh_college_land.conversioncertificateissuedBy;
            if (jntuh_college_land.conversionCertificateIssuedDate != null)
            {
                landInformation.conversionCertificateIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.conversionCertificateIssuedDate.ToString());
            }
            else
            {
                landInformation.conversionCertificateIssuedDate = null;
            }
            landInformation.conversionCertificateIssuedPurpose = jntuh_college_land.conversionCertificateIssuedPurpose;
            landInformation.buildingPlanIssuedBy = jntuh_college_land.buildingPlanIssuedBy;
            if (jntuh_college_land.buildingPlanIssuedDate != null)
            {
                landInformation.buildingPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.buildingPlanIssuedDate.ToString());
            }
            else
            {
                landInformation.buildingPlanIssuedDate = null;
            }
            landInformation.masterPlanIssuedBy = jntuh_college_land.masterPlanIssuedBy;
            if (jntuh_college_land.masterPlanIssuedDate != null)
            {
                landInformation.masterPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.masterPlanIssuedDate.ToString());
            }
            else
            {
                landInformation.masterPlanIssuedDate = null;
            }
            landInformation.compoundWall = jntuh_college_land.compoundWall;
            string[] selectedApproachRoad = jntuh_college_land.approachRoadId.ToString().Split(' ');
            List<Item> lstApproachRoad = new List<Item>();
            foreach (var type in db.jntuh_approach_road.Where(a => a.isActive == true))
            {
                string strtype = type.id.ToString();
                lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = selectedApproachRoad.Contains(strtype) ? 1 : 0 });
            }

            landInformation.approachRoad = lstApproachRoad;

            string[] selectedPowerSupply = jntuh_college_land.powerSupplyId.ToString().Split(' ');
            List<Item> lstPowerSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(p => p.isActive == true))
            {
                string strtype = type.id.ToString();
                lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedPowerSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.powerSupply = lstPowerSupply;

            string[] selectedWaterSupply = jntuh_college_land.WaterSupplyId.ToString().Split(' ');
            List<Item> lstWaterSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(w => w.isActive == true))
            {
                string strtype = type.id.ToString();
                lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedWaterSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.WaterSupply = lstWaterSupply;

            string[] selectedDrinkingwater = jntuh_college_land.drinkingWaterId.ToString().Split(' ');
            List<Item> lstDrinkingWater = new List<Item>();
            foreach (var type in db.jntuh_water_type.Where(d => d.isActive == true))
            {
                string strtype = type.id.ToString();
                lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = selectedDrinkingwater.Contains(strtype) ? 1 : 0 });
            }
            landInformation.DrinkingWater = lstDrinkingWater;

            landInformation.IsPurifiedWater = jntuh_college_land.IsPurifiedWater;
            landInformation.potableWaterPerDay = jntuh_college_land.potableWaterPerDay;

            landInformation.createdBy = jntuh_college_land.createdBy;
            landInformation.createdOn = jntuh_college_land.createdOn;
            landInformation.updatedBy = jntuh_college_land.updatedBy;
            landInformation.updatedOn = jntuh_college_land.updatedOn;


            ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
            ViewBag.RegistrationDetails = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                               r.collegeId == userCollegeID).ToList();
            return View("Create", landInformation);

        }

        // POST: /CollegeInformation/Edit
        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult Edit(LandInformation landInformation)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = landInformation.collegeId;
            }
            SaveLandInformation(landInformation);

            ViewBag.RegistrationDetails = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
              
                                                                 r.collegeId == userCollegeID).ToList();
            //return View("Create", landInformation);
            return View("View", landInformation);
        }

        [Authorize(Roles = "Admin,College")]
        public ActionResult View(string id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            }
            int userLandID = db.jntuh_college_land.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
            DateTime todayDate = DateTime.Now.Date;

            int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
            int prAy =
                db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
            int status = db.jntuh_college_edit_status.Where(editStatus => editStatus.collegeId == userCollegeID &&
                                                                                      editStatus.IsCollegeEditable == true &&
                                                                                      editStatus.editFromDate <= todayDate &&
                                                                                      editStatus.editToDate >= todayDate)
                                                                 .Select(editStatus => editStatus.id)
                                                                 .FirstOrDefault();
            if (status > 0)
            {
                ViewBag.Status = true;
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.Status = true;
                }
                else
                {
                    ViewBag.Status = false;
                }
            }
            else
            {
                ViewBag.Status = false;
            }
            if (status > 0 && Roles.IsUserInRole("College"))
            {
                ////RAMESH:To-DisableEdit
                //if (ConfigurationManager.AppSettings["ShouldFormsBeDisabled"].ToString() == "true")
                //{
                //    ViewBag.IsEditable = false;
                //}
                //else
                //{
                //    ViewBag.IsEditable = true;
                //}
                bool isPageEditable = db.jntuh_college_screens_assigned.Where(a => a.jntuh_college_screens.ScreenCode.Equals("LA") && a.CollegeId == userCollegeID).Select(a => a.IsEditable).FirstOrDefault();

                if (isPageEditable)
                {
                    ViewBag.IsEditable = true;
                }
                else
                {
                    ViewBag.IsEditable = false;
                }
                //ViewBag.IsEditable = true;
            }
            else
            {
                ViewBag.IsEditable = false;
            }
            LandInformation landInformation = new LandInformation();
            if (userLandID == 0)
            {
                ViewBag.Norecords = true;
            }
            else
            {
                var regDetails = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                                       r.collegeId == userCollegeID).ToList();
                ViewBag.RegistrationDetails = regDetails;
                //if (status > 0 && Roles.IsUserInRole("College") && regDetails.Count() > 0)
                //{
                //}

                if (userLandID == 0)
                {
                    ViewBag.Norecords = true;
                }
                else
                {

                    jntuh_college_land jntuh_college_land = db.jntuh_college_land.Find(userLandID);
                    landInformation.id = jntuh_college_land.id;
                    landInformation.collegeId = jntuh_college_land.collegeId;
                    landInformation.areaInAcres = jntuh_college_land.areaInAcres;

                    string[] selectedLandType = jntuh_college_land.landTypeID.ToString().Split(' ');
                    List<Item> lstLandType = new List<Item>();
                    foreach (var type in db.jntuh_land_type.Where(l => l.isActive == true))
                    {
                        string strType = type.id.ToString();
                        lstLandType.Add(new Item { id = type.id, name = type.landType, selected = selectedLandType.Contains(strType) ? 1 : 0 });
                    }

                    landInformation.landType = lstLandType;

                    string[] selectedLandRegistrationType = jntuh_college_land.landRegistrationTypeId.ToString().Split(' ');
                    List<Item> lstLandRegistrationtype = new List<Item>();
                    foreach (var type in db.jntuh_land_registration_type.Where(r => r.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstLandRegistrationtype.Add(new Item { id = type.id, name = type.landRegistrationType, selected = selectedLandRegistrationType.Contains(strtype) ? 1 : 0 });
                    }

                    landInformation.landRegistrationType = lstLandRegistrationtype;

                    string[] selectedLandCategory = jntuh_college_land.landCategoryId.ToString().Split(' ');
                    List<Item> lstLandCategory = new List<Item>();
                    foreach (var type in db.jntuh_land_category.Where(c => c.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = selectedLandCategory.Contains(strtype) ? 1 : 0 });
                    }

                    landInformation.landCategory = lstLandCategory;

                    landInformation.landConversionFilePath = jntuh_college_land.landconversiondocument;
                    landInformation.buildingPlanFilePath = jntuh_college_land.approvedbuildingplans;
                    landInformation.masterPlanFilePath = jntuh_college_land.masterplanofthecampus;
                    landInformation.landRegistrationFilePath = jntuh_college_land.landregistrationdocument;

                    landInformation.conversionCertificateIssuedBy = jntuh_college_land.conversioncertificateissuedBy;
                    if (jntuh_college_land.conversionCertificateIssuedDate != null)
                    {
                        landInformation.conversionCertificateIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.conversionCertificateIssuedDate.ToString());
                    }
                    else
                    {
                        landInformation.conversionCertificateIssuedDate = null;
                    }
                    landInformation.conversionCertificateIssuedPurpose = jntuh_college_land.conversionCertificateIssuedPurpose;
                    landInformation.buildingPlanIssuedBy = jntuh_college_land.buildingPlanIssuedBy;
                    if (jntuh_college_land.buildingPlanIssuedDate != null)
                    {
                        landInformation.buildingPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.buildingPlanIssuedDate.ToString());
                    }
                    else
                    {
                        landInformation.buildingPlanIssuedDate = null;
                    }
                    landInformation.masterPlanIssuedBy = jntuh_college_land.masterPlanIssuedBy;
                    if (jntuh_college_land.masterPlanIssuedDate != null)
                    {
                        landInformation.masterPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.masterPlanIssuedDate.ToString());
                    }
                    else
                    {
                        landInformation.masterPlanIssuedDate = null;
                    }
                    landInformation.compoundWall = jntuh_college_land.compoundWall;
                    string[] selectedApproachRoad = jntuh_college_land.approachRoadId.ToString().Split(' ');
                    List<Item> lstApproachRoad = new List<Item>();
                    foreach (var type in db.jntuh_approach_road.Where(a => a.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = selectedApproachRoad.Contains(strtype) ? 1 : 0 });
                    }

                    landInformation.approachRoad = lstApproachRoad;

                    string[] selectedPowerSupply = jntuh_college_land.powerSupplyId.ToString().Split(' ');
                    List<Item> lstPowerSupply = new List<Item>();
                    foreach (var type in db.jntuh_facility_status.Where(p => p.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedPowerSupply.Contains(strtype) ? 1 : 0 });
                    }
                    landInformation.powerSupply = lstPowerSupply;

                    string[] selectedWaterSupply = jntuh_college_land.WaterSupplyId.ToString().Split(' ');
                    List<Item> lstWaterSupply = new List<Item>();
                    foreach (var type in db.jntuh_facility_status.Where(w => w.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedWaterSupply.Contains(strtype) ? 1 : 0 });
                    }
                    landInformation.WaterSupply = lstWaterSupply;

                    string[] selectedDrinkingwater = jntuh_college_land.drinkingWaterId.ToString().Split(' ');
                    List<Item> lstDrinkingWater = new List<Item>();
                    foreach (var type in db.jntuh_water_type.Where(d => d.isActive == true))
                    {
                        string strtype = type.id.ToString();
                        lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = selectedDrinkingwater.Contains(strtype) ? 1 : 0 });
                    }
                    landInformation.DrinkingWater = lstDrinkingWater;

                    landInformation.IsPurifiedWater = jntuh_college_land.IsPurifiedWater;
                    landInformation.potableWaterPerDay = jntuh_college_land.potableWaterPerDay;

                    landInformation.createdBy = jntuh_college_land.createdBy;
                    landInformation.createdOn = jntuh_college_land.createdOn;
                    landInformation.updatedBy = jntuh_college_land.updatedBy;
                    landInformation.updatedOn = jntuh_college_land.updatedOn;


                    ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                    ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
                }
            }
            return View("View", landInformation);
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpGet]
        public ActionResult AddEditRecord(int? id, string collegeId)
        {
            if (Request.IsAjaxRequest())
            {
                if (id != null)
                {
                    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                    if (userCollegeID == 0)
                    {
                        if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
                        {
                            if (collegeId != null)
                            {
                                userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                            }
                            else
                            {
                                userCollegeID = db.jntuh_college_land_registration.Where(r => r.isActive == true && r.id == id).Select(r => r.collegeId).FirstOrDefault();
                            }
                        }
                    }
                    ViewBag.IsUpdate = true;

                    LandRegistrationDetails landRegistrationDetails = new LandRegistrationDetails();

                    jntuh_college_land_registration jntuh_college_land_registration = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                                      r.collegeId == userCollegeID &&
                                                                                      r.id == id).FirstOrDefault();
                    landRegistrationDetails.id = jntuh_college_land_registration.id;
                    landRegistrationDetails.collegeId = jntuh_college_land_registration.collegeId;
                    if (jntuh_college_land_registration.landRegistraionDate != null)
                    {
                        landRegistrationDetails.landRegistraionDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land_registration.landRegistraionDate.ToString());
                    }
                    else
                    {
                        landRegistrationDetails.landRegistraionDate = null;
                    }
                    landRegistrationDetails.landAreaInAcres = jntuh_college_land_registration.landAreaInAcres;
                    landRegistrationDetails.landDocumentNumber = jntuh_college_land_registration.landDocumentNumber;
                    landRegistrationDetails.landSurveyNumber = jntuh_college_land_registration.landSurveyNumber;
                    landRegistrationDetails.landLocation = jntuh_college_land_registration.landLocation;
                    landRegistrationDetails.createdOn = jntuh_college_land_registration.createdOn;
                    landRegistrationDetails.createdBy = jntuh_college_land_registration.createdBy;
                    landRegistrationDetails.updatedBy = jntuh_college_land_registration.updatedBy;
                    landRegistrationDetails.updatedOn = jntuh_college_land_registration.updatedOn;

                    return PartialView("_CreateLandRegistration", landRegistrationDetails);
                }
                else
                {
                    LandRegistrationDetails landRegistrationDetails = new LandRegistrationDetails();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        landRegistrationDetails.collegeId = userCollegeID;
                    }                   

                    ViewBag.IsUpdate = false;
                    return PartialView("_CreateLandRegistration",landRegistrationDetails);
                }
            }
            else
            {
                if (id != null)
                {
                    int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
                    if (userCollegeID == 0)
                    {
                        if (collegeId != null)
                        {
                            userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        }
                        else
                        {
                            userCollegeID = db.jntuh_college_land_registration.Where(r => r.isActive == true && r.id == id).Select(r => r.collegeId).FirstOrDefault();
                        }
                    }
                    ViewBag.IsUpdate = true;
                    LandRegistrationDetails landRegistrationDetails = new LandRegistrationDetails();
                    jntuh_college_land_registration jntuh_college_land_registration = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                                      r.collegeId == userCollegeID &&
                                                                                      r.id == id).FirstOrDefault();
                    landRegistrationDetails.id = jntuh_college_land_registration.id;
                    landRegistrationDetails.collegeId = jntuh_college_land_registration.collegeId;
                    if (jntuh_college_land_registration.landRegistraionDate != null)
                    {
                        landRegistrationDetails.landRegistraionDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land_registration.landRegistraionDate.ToString());
                    }
                    else
                    {
                        landRegistrationDetails.landRegistraionDate = null;
                    }
                    landRegistrationDetails.landAreaInAcres = jntuh_college_land_registration.landAreaInAcres;
                    landRegistrationDetails.landDocumentNumber = jntuh_college_land_registration.landDocumentNumber;
                    landRegistrationDetails.landSurveyNumber = jntuh_college_land_registration.landSurveyNumber;
                    landRegistrationDetails.landLocation = jntuh_college_land_registration.landLocation;
                    landRegistrationDetails.createdOn = jntuh_college_land_registration.createdOn;
                    landRegistrationDetails.createdBy = jntuh_college_land_registration.createdBy;
                    landRegistrationDetails.updatedBy = jntuh_college_land_registration.updatedBy;
                    landRegistrationDetails.updatedOn = jntuh_college_land_registration.updatedOn;
                    return View("CreateLandRegistration", landRegistrationDetails);
                }
                else
                {
                    LandRegistrationDetails landRegistrationDetails = new LandRegistrationDetails();
                    if (collegeId != null)
                    {
                        int userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
                        landRegistrationDetails.collegeId = userCollegeID;
                    }
                    ViewBag.IsUpdate = false;
                    return View("CreateLandRegistration",landRegistrationDetails);
                }
            }
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        [HttpPost]
        public ActionResult AddEditRecord(LandRegistrationDetails landRegistrationDetails, string cmd)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = landRegistrationDetails.collegeId;
            }
            if (ModelState.IsValid)
            {               
                landRegistrationDetails.collegeId = userCollegeID;
                if (cmd == "Add")
                {
                    var id = db.jntuh_college_land_registration.Where(r => r.landDocumentNumber == landRegistrationDetails.landDocumentNumber &&
                                                                      r.collegeId == landRegistrationDetails.collegeId).
                                                                      Select(r => r.id).FirstOrDefault();

                    if (id > 0)
                    {
                        TempData["RegistrationError"] = "Document Number is already exists . Please enter a different Document Number";
                        return RedirectToAction("Create");
                    }
                    else
                    {
                        jntuh_college_land_registration jntuh_college_land_registration = new jntuh_college_land_registration();
                        jntuh_college_land_registration.id = landRegistrationDetails.id;
                        jntuh_college_land_registration.collegeId = landRegistrationDetails.collegeId;
                        jntuh_college_land_registration.landRegistraionDate = Utilities.DDMMYY2MMDDYY(landRegistrationDetails.landRegistraionDate);
                        jntuh_college_land_registration.landAreaInAcres = landRegistrationDetails.landAreaInAcres;
                        jntuh_college_land_registration.landDocumentNumber = landRegistrationDetails.landDocumentNumber;
                        jntuh_college_land_registration.landSurveyNumber = landRegistrationDetails.landSurveyNumber;
                        jntuh_college_land_registration.landLocation = landRegistrationDetails.landLocation;
                        jntuh_college_land_registration.isActive = true;
                        jntuh_college_land_registration.createdBy = userID;
                        jntuh_college_land_registration.createdOn = DateTime.Now;
                        db.jntuh_college_land_registration.Add(jntuh_college_land_registration);
                        db.SaveChanges();
                        TempData["RegistrationSuccess"] = "Land Registration Details are Added successfully.";
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                }
                else
                {
                    var IdUpdate =
                        db.jntuh_college_land_registration.Where(r => r.landDocumentNumber == landRegistrationDetails.landDocumentNumber &&
                                                                      r.collegeId == landRegistrationDetails.collegeId &&
                                                                      r.id != landRegistrationDetails.id).
                                                                      Select(r => r.id).FirstOrDefault();

                    if (IdUpdate > 0)
                    {
                        TempData["RegistrationError"] = "Document Number is already exists . Please enter a different Document Number";
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }
                    else
                    {
                        jntuh_college_land_registration jntuh_college_land_registration = new jntuh_college_land_registration();
                        jntuh_college_land_registration.id = landRegistrationDetails.id;
                        jntuh_college_land_registration.collegeId = landRegistrationDetails.collegeId;
                        jntuh_college_land_registration.landRegistraionDate = Utilities.DDMMYY2MMDDYY(landRegistrationDetails.landRegistraionDate);
                        jntuh_college_land_registration.landAreaInAcres = landRegistrationDetails.landAreaInAcres;
                        jntuh_college_land_registration.landDocumentNumber = landRegistrationDetails.landDocumentNumber;
                        jntuh_college_land_registration.landSurveyNumber = landRegistrationDetails.landSurveyNumber;
                        jntuh_college_land_registration.landLocation = landRegistrationDetails.landLocation;
                        jntuh_college_land_registration.isActive = true;
                        jntuh_college_land_registration.createdBy = landRegistrationDetails.createdBy;
                        jntuh_college_land_registration.createdOn = landRegistrationDetails.createdOn;
                        jntuh_college_land_registration.updatedBy = userID;
                        jntuh_college_land_registration.updatedOn = DateTime.Now;
                        db.Entry(jntuh_college_land_registration).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["RegistrationSuccess"] = "Land Registration Details are Updated successfully.";
                        return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
                    }

                }

            }
            else
            {
                return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
            }

        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult DeleteRecord(int id)
        {
            jntuh_college_land_registration jntuh_college_land_registration = db.jntuh_college_land_registration.Where(r => r.id == id).FirstOrDefault();
            int usercollegeId = db.jntuh_college_land_registration.Where(r => r.id == id).Select(r=>r.collegeId).FirstOrDefault();
            if (jntuh_college_land_registration != null)
            {
                db.jntuh_college_land_registration.Remove(jntuh_college_land_registration);
                db.SaveChanges();
                TempData["RegistrationSuccess"] = "Land Registration Details are Deleted successfully";
            }
            return RedirectToAction("Create", new { collegeId = Utilities.EncryptString(usercollegeId.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });
        }

        [Authorize(Roles = "Admin,SuperAdmin,Committee,College,DataEntry")]
        public ActionResult Details(int id)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            if (userCollegeID == 0)
            {
                userCollegeID = db.jntuh_college_land_registration.Where(r => r.id == id).Select(r => r.collegeId).FirstOrDefault();
            }
            LandRegistrationDetails landRegistrationDetails = new LandRegistrationDetails();
            jntuh_college_land_registration jntuh_college_land_registration = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                               r.id == id).FirstOrDefault();

            landRegistrationDetails.id = jntuh_college_land_registration.id;
            landRegistrationDetails.collegeId = jntuh_college_land_registration.collegeId;
            if (jntuh_college_land_registration.landRegistraionDate != null)
            {
                landRegistrationDetails.landRegistraionDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land_registration.landRegistraionDate.ToString());
            }
            else
            {
                landRegistrationDetails.landRegistraionDate = null;
            }
            landRegistrationDetails.landAreaInAcres = jntuh_college_land_registration.landAreaInAcres;
            landRegistrationDetails.landDocumentNumber = jntuh_college_land_registration.landDocumentNumber;
            landRegistrationDetails.landSurveyNumber = jntuh_college_land_registration.landSurveyNumber;
            landRegistrationDetails.landLocation = jntuh_college_land_registration.landLocation;
            landRegistrationDetails.createdBy = jntuh_college_land_registration.createdBy;
            landRegistrationDetails.createdOn = jntuh_college_land_registration.createdOn;
            landRegistrationDetails.updatedBy = jntuh_college_land_registration.updatedBy;
            landRegistrationDetails.updatedOn = jntuh_college_land_registration.updatedOn;
            if (landRegistrationDetails != null)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_LandRegistrationDetails", landRegistrationDetails);
                }
                else
                {
                    return View("LandRegistrationDetails", landRegistrationDetails);
                }
            }
            return View("Create",new { collegeId = Utilities.EncryptString(userCollegeID.ToString(), WebConfigurationManager.AppSettings["CryptoKey"]).ToString() });

        }

        public ActionResult UserView(string id)
        {
            int userCollegeID = Convert.ToInt32(Utilities.DecryptString(id, WebConfigurationManager.AppSettings["CryptoKey"]));
            int userLandID = db.jntuh_college_land.Where(e => e.collegeId == userCollegeID).Select(e => e.id).FirstOrDefault();
            LandInformation landInformation = new LandInformation();
            var regDetails = db.jntuh_college_land_registration.Where(r => r.isActive == true &&
                                                                                       r.collegeId == userCollegeID).ToList();
            ViewBag.RegistrationDetails = regDetails;
            ViewBag.Count = regDetails.Count();
            jntuh_college_land jntuh_college_land = db.jntuh_college_land.Find(userLandID);
            landInformation.id = jntuh_college_land.id;
            landInformation.collegeId = jntuh_college_land.collegeId;
            landInformation.areaInAcres = jntuh_college_land.areaInAcres;

            string[] selectedLandType = jntuh_college_land.landTypeID.ToString().Split(' ');
            List<Item> lstLandType = new List<Item>();
            foreach (var type in db.jntuh_land_type.Where(l => l.isActive == true))
            {
                string strType = type.id.ToString();
                lstLandType.Add(new Item { id = type.id, name = type.landType, selected = selectedLandType.Contains(strType) ? 1 : 0 });
            }

            landInformation.landType = lstLandType;

            string[] selectedLandRegistrationType = jntuh_college_land.landRegistrationTypeId.ToString().Split(' ');
            List<Item> lstLandRegistrationtype = new List<Item>();
            foreach (var type in db.jntuh_land_registration_type.Where(r => r.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandRegistrationtype.Add(new Item { id = type.id, name = type.landRegistrationType, selected = selectedLandRegistrationType.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landRegistrationType = lstLandRegistrationtype;

            string[] selectedLandCategory = jntuh_college_land.landCategoryId.ToString().Split(' ');
            List<Item> lstLandCategory = new List<Item>();
            foreach (var type in db.jntuh_land_category.Where(c => c.isActive == true))
            {
                string strtype = type.id.ToString();
                lstLandCategory.Add(new Item { id = type.id, name = type.landCategory, selected = selectedLandCategory.Contains(strtype) ? 1 : 0 });
            }

            landInformation.landCategory = lstLandCategory;

            landInformation.conversionCertificateIssuedBy = jntuh_college_land.conversioncertificateissuedBy;
            if (jntuh_college_land.conversionCertificateIssuedDate != null)
            {
                landInformation.conversionCertificateIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.conversionCertificateIssuedDate.ToString());
            }
            else
            {
                landInformation.conversionCertificateIssuedDate = null;
            }
            landInformation.conversionCertificateIssuedPurpose = jntuh_college_land.conversionCertificateIssuedPurpose;
            landInformation.buildingPlanIssuedBy = jntuh_college_land.buildingPlanIssuedBy;
            if (jntuh_college_land.buildingPlanIssuedDate != null)
            {
                landInformation.buildingPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.buildingPlanIssuedDate.ToString());
            }
            else
            {
                landInformation.buildingPlanIssuedDate = null;
            }
            landInformation.masterPlanIssuedBy = jntuh_college_land.masterPlanIssuedBy;
            if (jntuh_college_land.masterPlanIssuedDate != null)
            {
                landInformation.masterPlanIssuedDate = Utilities.MMDDYY2DDMMYY(jntuh_college_land.masterPlanIssuedDate.ToString());
            }
            else
            {
                landInformation.masterPlanIssuedDate = null;
            }
            landInformation.compoundWall = jntuh_college_land.compoundWall;
            string[] selectedApproachRoad = jntuh_college_land.approachRoadId.ToString().Split(' ');
            List<Item> lstApproachRoad = new List<Item>();
            foreach (var type in db.jntuh_approach_road.Where(a => a.isActive == true))
            {
                string strtype = type.id.ToString();
                lstApproachRoad.Add(new Item { id = type.id, name = type.approachRoadType, selected = selectedApproachRoad.Contains(strtype) ? 1 : 0 });
            }

            landInformation.approachRoad = lstApproachRoad;

            string[] selectedPowerSupply = jntuh_college_land.powerSupplyId.ToString().Split(' ');
            List<Item> lstPowerSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(p => p.isActive == true))
            {
                string strtype = type.id.ToString();
                lstPowerSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedPowerSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.powerSupply = lstPowerSupply;

            string[] selectedWaterSupply = jntuh_college_land.WaterSupplyId.ToString().Split(' ');
            List<Item> lstWaterSupply = new List<Item>();
            foreach (var type in db.jntuh_facility_status.Where(w => w.isActive == true))
            {
                string strtype = type.id.ToString();
                lstWaterSupply.Add(new Item { id = type.id, name = type.facilityStatus, selected = selectedWaterSupply.Contains(strtype) ? 1 : 0 });
            }
            landInformation.WaterSupply = lstWaterSupply;

            string[] selectedDrinkingwater = jntuh_college_land.drinkingWaterId.ToString().Split(' ');
            List<Item> lstDrinkingWater = new List<Item>();
            foreach (var type in db.jntuh_water_type.Where(d => d.isActive == true))
            {
                string strtype = type.id.ToString();
                lstDrinkingWater.Add(new Item { id = type.id, name = type.waterType, selected = selectedDrinkingwater.Contains(strtype) ? 1 : 0 });
            }
            landInformation.DrinkingWater = lstDrinkingWater;
            landInformation.IsPurifiedWater = jntuh_college_land.IsPurifiedWater;
            landInformation.potableWaterPerDay = jntuh_college_land.potableWaterPerDay;
            if (jntuh_college_land == null)
            {
                ViewBag.NoRecords = true;
            }
            return View("UserView", landInformation);
        }
    }
}