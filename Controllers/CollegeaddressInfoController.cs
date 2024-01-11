using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using UAAAS.Models;

namespace UAAAS.Controllers
{
  
    public class CollegeaddressInfoController : Controller
    {
        //
        // GET: /CollegeaddressInfo/
        private uaaasDBContext db = new uaaasDBContext();
        public ActionResult Index()
        {
            return View();
        }
       
        public ActionResult CollegeInfo(string collegeId)
        {
            //int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            //int userCollegeID = db.jntuh_college_users.Where(u => u.userID == userID).Select(u => u.collegeID).FirstOrDefault();
            //if (userCollegeID == 0)
            //{
            //    if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("DataEntry"))
            //    {
            //        if (collegeId != null)
            //        {
            //            userCollegeID = Convert.ToInt32(Utilities.DecryptString(collegeId, WebConfigurationManager.AppSettings["CryptoKey"]));
            //        }
            //    }
            //}
            //if (userCollegeID == 375)
            //{
            //    userCollegeID = Convert.ToInt32(ConfigurationManager.AppSettings["appCollegeId"]);
            //}

            jntuh_college collegeinfo = db.jntuh_college.Where(e => e.id == 375).Select(e => e).FirstOrDefault();
            CollegeInformation clginfo = new CollegeInformation();
            clginfo.collegeCode = collegeinfo.collegeCode;
            clginfo.collegeName = collegeinfo.collegeName;
            clginfo.id = 375; // here id is college id
            


            ViewBag.State = db.jntuh_state.Where(state => state.isActive == true).ToList();
            ViewBag.District = db.jntuh_district.Where(district => district.isActive == true).ToList();
        
            return View(clginfo);


        }
        [HttpPost]
        public ActionResult CollegeInfo(CollegeInformation clginfo)
        {
            try
            {
                jntuh_address_log addlog = new jntuh_address_log();
                addlog.id = 132;
                addlog.collegeId = clginfo.id;
                addlog.address = clginfo.address;
                addlog.addressTye = "COLLEGE";
                addlog.townOrCity = clginfo.townOrCity;
                addlog.mandal = clginfo.mandal;
                addlog.stateId = clginfo.stateId;
                addlog.districtId = clginfo.districtId;
                addlog.pincode = clginfo.pincode;
                addlog.landline = clginfo.landline;
                addlog.email = "jntu";
                addlog.mobile = "jntu";
                addlog.fax = "jntu ";
                addlog.website = " jntu";
                addlog.createdBy = clginfo.id;
                addlog.createdOn = DateTime.Now;
                addlog.updatedOn = DateTime.Now;
                addlog.academicYearid = 11;

                db.jntuh_address_log.Add(addlog);
                db.SaveChanges();

              

                TempData["Success"] = "Updated successfully!";
          
            }
            catch (DbEntityValidationException dbEx)
            {

                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }
               
            return View();
        }

        [HttpGet]
        public ActionResult GoverningBody()
        {
            List<jntuh_governingbodydesignations> governingbodydesignationslist = new List<Models.jntuh_governingbodydesignations>();
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id=1,
                designation = "Chairman"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 2,
                designation = "Member to be nominated by Registered Society / Trust"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 3,
                designation = "Member to be nominated by Registered Society / Trust"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 4,
                designation = "Member to be nominated by Registered Society / Trust"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 5,
                designation = "Member to be nominated by Registered Society / Trust"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 6,
                designation = "Eminent Professional"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 7,
                designation = "Eminent Professional"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 8,
                designation = "Academician"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 9,
                designation = "Academician"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 10,
                designation = "University Nominee"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 11,
                designation = "Member Secretary"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 12,
                designation = "Others1"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 13,
                designation = "Others2"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 14,
                designation = "Others3"
            });
            governingbodydesignationslist.Add(new jntuh_governingbodydesignations
            {
                id = 15,
                designation = "Others4"
            });
            ViewBag.governingbodydes = governingbodydesignationslist;


            //List<jntuh_college_governingbodyclass> governingbodylist = new List<jntuh_college_governingbodyclass>();

           // jntuh_college_governingbody governingbody = new jntuh_college_governingbody();



            //foreach (var item in governingbodydesignationslist)
            //{
            //    jntuh_college_governingbodyclass governingbody = new jntuh_college_governingbodyclass();
            //      governingbody.id = item.id;
            //      governingbody.GoverningBodyMemberDesignation = item.designation;
            //      governingbodylist.Add(governingbody);
            //}

            //governingbodylist.Add(governingbodylist);



            return View();
        }

        //[HttpPost]
        //public ActionResult GoverningBody(FormCollection formCollection,IList<jntuh_college_governingbodyclass> listobj)
        //{
        //      jntuh_college_governingbody governingbodyobject=new jntuh_college_governingbody();

        //         governingbodyobject.nameofthemember=formCollection["1_nameofthemember"];
        //         governingbodyobject.memberdesignation=1;
        //           governingbodyobject.organizationworking=  formCollection["1_organizationworking"];

        //             governingbodyobject.organizationdesignation=  formCollection[ "1_organizationdesignation"];

        //             if (!string.IsNullOrEmpty(formCollection["1_dateofappointment"]))
        //             {
        //                 var date = UAAAS.Models.Utilities.MMDDYY2DDMMYY(formCollection["1_dateofappointment"]);
        //                 governingbodyobject.dateofappointment = Convert.ToDateTime(date);
        //             }
        //            governingbodyobject.supportingdocument = formCollection["1_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();




        //            governingbodyobject.nameofthemember = formCollection["2_nameofthemember"];
        //            governingbodyobject.memberdesignation = 2;
        //            governingbodyobject.organizationworking = formCollection["2_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["2_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["2_dateofappointment"]))
        //            {
        //                var date1 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(formCollection["2_dateofappointment"]);
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date1);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["2_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);
        //            db.SaveChanges();




        //            governingbodyobject.nameofthemember = formCollection["3_nameofthemember"];
        //            governingbodyobject.memberdesignation = 3;
        //            governingbodyobject.organizationworking = formCollection["3_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["3_organizationdesignation"];

        //    if(!string.IsNullOrEmpty( formCollection["3_dateofappointment"]))
        //    {
        //        var date3 = UAAAS.Models.Utilities.MMDDYY2DDMMYY(formCollection["3_dateofappointment"]);
        //        governingbodyobject.dateofappointment = Convert.ToDateTime(date3);
        //    }
                  

        //            governingbodyobject.supportingdocument = formCollection["3_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();


        //            governingbodyobject.nameofthemember = formCollection["4_nameofthemember"];
        //            governingbodyobject.memberdesignation = 4;
        //            governingbodyobject.organizationworking = formCollection["4_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["4_organizationdesignation"];



        //            if (!string.IsNullOrEmpty(formCollection["4_dateofappointment"]))
        //            {
        //                var date4 = formCollection["4_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date4);

        //            }
        //            governingbodyobject.supportingdocument = formCollection["4_supportingdocument"];

        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);
        //            db.SaveChanges();




        //            governingbodyobject.nameofthemember = formCollection["5_nameofthemember"];
        //            governingbodyobject.memberdesignation = 5;
        //            governingbodyobject.organizationworking = formCollection["5_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["5_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["5_dateofappointment"] ))
        //            {
        //                var date5 = formCollection["5_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date5);

        //            }
        //            governingbodyobject.supportingdocument = formCollection["5_supportingdocument"];

        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;

        //            db.jntuh_college_governingbody.Add(governingbodyobject);
        //            db.SaveChanges();


        //            governingbodyobject.nameofthemember = formCollection["6_nameofthemember"];
        //            governingbodyobject.memberdesignation = 6;
        //            governingbodyobject.organizationworking = formCollection["6_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["6_organizationdesignation"];


        //            if (!string.IsNullOrEmpty(formCollection["6_dateofappointment"]))
        //            {
        //                var date6 = formCollection["6_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date6);

        //            }
        //            governingbodyobject.supportingdocument = formCollection["6_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();


        //            governingbodyobject.nameofthemember = formCollection["7_nameofthemember"];
        //            governingbodyobject.memberdesignation = 7;
        //            governingbodyobject.organizationworking = formCollection["7_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["7_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["7_dateofappointment"]))
        //            {
        //                var date7 = formCollection["7_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date7);
        //            }

        //            governingbodyobject.supportingdocument = formCollection["7_supportingdocument"];

        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;

        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();

        //            governingbodyobject.nameofthemember = formCollection["8_nameofthemember"];
        //            governingbodyobject.memberdesignation = 8;
        //            governingbodyobject.organizationworking = formCollection["8_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["8_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["8_dateofappointment"]))
        //            {
        //                var date8 = formCollection["8_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date8);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["8_supportingdocument"];

        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;

        //            db.jntuh_college_governingbody.Add(governingbodyobject);
        //            db.SaveChanges();

        //            governingbodyobject.nameofthemember = formCollection["9_nameofthemember"];
        //            governingbodyobject.memberdesignation = 9;
        //            governingbodyobject.organizationworking = formCollection["9_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["9_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["9_dateofappointment"]))
        //            {
        //                var date9 = formCollection["9_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date9);

        //            }
        //            governingbodyobject.supportingdocument = formCollection["9_supportingdocument"];

        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;

        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();



        //            governingbodyobject.nameofthemember = formCollection["10_nameofthemember"];
        //            governingbodyobject.memberdesignation = 10;
        //            governingbodyobject.organizationworking = formCollection["10_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["10_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["10_dateofappointment"]))
        //            {
        //                var date10 = formCollection["10_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date10);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["10_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();


        //            governingbodyobject.nameofthemember = formCollection["11_nameofthemember"];
        //            governingbodyobject.memberdesignation = 11;
        //            governingbodyobject.organizationworking = formCollection["11_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["11_organizationdesignation"];


        //            if (!string.IsNullOrEmpty(formCollection["11_dateofappointment"]))
        //            {

        //                var date11 = formCollection["11_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date11);

        //            }
        //            governingbodyobject.supportingdocument = formCollection["11_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();





        //            governingbodyobject.nameofthemember = formCollection["12_nameofthemember"];
        //            governingbodyobject.memberdesignation = 12;
        //            governingbodyobject.organizationworking = formCollection["12_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["12_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["12_dateofappointment"]))
        //            {
        //                var date12 = formCollection["12_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date12);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["12_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();




        //            governingbodyobject.nameofthemember = formCollection["13_nameofthemember"];
        //            governingbodyobject.memberdesignation = 13;
        //            governingbodyobject.organizationworking = formCollection["13_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["13_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["13_dateofappointment"]))
        //            {
        //                var date13 = formCollection["13_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date13);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["13_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();

        //            governingbodyobject.nameofthemember = formCollection["14_nameofthemember"];
        //            governingbodyobject.memberdesignation = 14;
        //            governingbodyobject.organizationworking = formCollection["14_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["14_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["14_dateofappointment"]))
        //            {
        //                var date14 = formCollection["14_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date14);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["14_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);
        //            db.SaveChanges();

        //            governingbodyobject.nameofthemember = formCollection["15_nameofthemember"];
        //            governingbodyobject.memberdesignation = 15;
        //            governingbodyobject.organizationworking = formCollection["15_organizationworking"];

        //            governingbodyobject.organizationdesignation = formCollection["15_organizationdesignation"];

        //            if (!string.IsNullOrEmpty(formCollection["15_dateofappointment"]))
        //            {
        //                var date15 = formCollection["15_dateofappointment"];
        //                governingbodyobject.dateofappointment = Convert.ToDateTime(date15);
        //            }
        //            governingbodyobject.supportingdocument = formCollection["15_supportingdocument"];
        //            governingbodyobject.collegeid = 375;
        //            governingbodyobject.academicyearid = 12;
        //            governingbodyobject.createdon = DateTime.Now;
        //            governingbodyobject.isactive = true;
        //            db.jntuh_college_governingbody.Add(governingbodyobject);

        //            db.SaveChanges();

        //            TempData["Success"] = "saved successfully!";

        //    return View();
        //}
    }
}
