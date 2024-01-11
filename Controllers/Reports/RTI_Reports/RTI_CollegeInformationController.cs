using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;
using System.Web.Security;
using System.Data;
using System.Web.Configuration;
using System.Configuration;

namespace UAAAS.Controllers.Reports.RTI_Reports
{
    public class RTI_CollegeInformationController : Controller
    {
        private string ReportHeader = null;
        //
        // GET: /RTI_CollegeInformation/
        private uaaasDBContext db = new uaaasDBContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? collegeid)
        {
            ViewBag.Colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { collegeId = c.id, collegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.collegeName).ToList();
            CollegeInformation collegeInformation = new CollegeInformation();
            var jntuh_department = db.jntuh_department.ToList();

            if (collegeid != null)
            {
                DateTime todayDate = DateTime.Now.Date;

                if (collegeid == null)
                {
                    ViewBag.Norecords = true;
                }
                else
                {
                    ViewBag.Norecords = false;

                    jntuh_college jntuh_college = db.jntuh_college.Find(collegeid);
                    if (jntuh_college != null)
                    {
                        collegeInformation.id = jntuh_college.id;
                        collegeInformation.collegeCode = jntuh_college.collegeCode;
                        collegeInformation.collegeName = jntuh_college.collegeName;
                        collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;
                        collegeInformation.eamcetCode = jntuh_college.eamcetCode;
                        collegeInformation.icetCode = jntuh_college.icetCode;
                        collegeInformation.otherCategory = jntuh_college.otherCategory;
                        collegeInformation.createdBy = jntuh_college.createdBy;
                        collegeInformation.createdOn = jntuh_college.createdOn;
                        collegeInformation.updatedBy = jntuh_college.createdBy;
                        collegeInformation.updatedOn = jntuh_college.createdOn;
                    }

                    jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == collegeid && a.addressTye.Equals("COLLEGE")).Select(a => a).ToList().FirstOrDefault();
                    if (jntuh_address != null)
                    {
                        collegeInformation.address = jntuh_address.address;
                        collegeInformation.addressTye = jntuh_address.addressTye;
                        collegeInformation.townOrCity = jntuh_address.townOrCity;
                        collegeInformation.mandal = jntuh_address.mandal;
                        collegeInformation.stateId = jntuh_address.stateId;
                        collegeInformation.districtId = jntuh_address.districtId;
                        collegeInformation.pincode = jntuh_address.pincode;
                        collegeInformation.fax = jntuh_address.fax;
                        collegeInformation.landline = jntuh_address.landline;
                        collegeInformation.mobile = jntuh_address.mobile;
                        collegeInformation.email = jntuh_address.email;
                        collegeInformation.website = jntuh_address.website;
                    }

                    if(jntuh_address == null)
                    {
                        ViewBag.Norecords = true;
                        return View(collegeInformation);
                    }
                    //after postback
                    string[] selectedCollegeAffiliation = jntuh_college.collegeAffiliationTypeID.ToString().Split(' ');
                    List<Item> lstAffiliationType = new List<Item>();
                    foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
                    {
                        string strType = type.id.ToString();
                        lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });

                    }

                    collegeInformation.collegeAffiliationType = lstAffiliationType;

                    string[] selectedCollegeType = jntuh_college.collegeTypeID.ToString().Split(' ');
                    List<Item> lstCollegeType = new List<Item>();
                    foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
                    {
                        string strType = type.id.ToString();
                        lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
                    }

                    collegeInformation.collegeType = lstCollegeType;

                    string[] selectedCollegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == jntuh_college.id && d.isActive == true).Select(d => d.degreeId).ToArray().Select(s => s.ToString()).ToArray();
                    List<Item> lstDegree = new List<Item>();
                    foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
                    {
                        string strType = d.id.ToString();
                        lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
                    }

                    collegeInformation.degree = lstDegree;

                    string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
                    List<Item> lstType = new List<Item>();
                    foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
                    {
                        string strType = t.id.ToString();
                        lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
                    }
                    int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                              .Select(s => s.id).ToArray();
                    int affiliationCount = 1;
                    foreach (var item in selectedAffiliationId)
                    {
                        if (affiliationCount == 1)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected1 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected1 = 2;
                            }
                        }
                        else if (affiliationCount == 2)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected2 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected2 = 2;
                            }
                        }

                        else if (affiliationCount == 3)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected3 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected3 = 2;
                            }
                        }
                        else if (affiliationCount == 4)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Conferred")
                            {
                                collegeInformation.affiliationSelected4 = 1;
                            }
                            else if (statusType == "Applied")
                            {
                                collegeInformation.affiliationSelected4 = 2;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected4 = 3;
                            }
                        }
                        else if (affiliationCount == 5)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                             .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Conferred")
                            {
                                collegeInformation.affiliationSelected5 = 1;
                            }
                            else if (statusType == "Applied")
                            {
                                collegeInformation.affiliationSelected5 = 2;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected5 = 3;
                            }
                        }
                        affiliationCount++;
                    }
                    collegeInformation.affiliationType = lstType;

                    int rowIndex = 1;
                    foreach (var type in lstType)
                    {
                        int affiliationType = type.id;

                        if (rowIndex == 1)
                        {
                            collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();
                            collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();
                            collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        if (rowIndex == 2)
                        {
                            collegeInformation.affiliationFromDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        if (rowIndex == 3)
                        {
                            collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }
                        if (rowIndex == 4)
                        {
                            collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }
                        if (rowIndex == 5)
                        {
                            collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        rowIndex++;
                    }

                    if (collegeInformation.affiliationFromDate1 != null)
                    {
                //        collegeInformation.affiliationFromDate1 = new string[] { 
                //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                //};
                        collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 != null
                            ? Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.ToString())
                            : string.Empty;
                    }

                    if (collegeInformation.affiliationFromDate2 != null)
                    {
                        collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate3 != null)
                    {
                        collegeInformation.affiliationFromDate3 = new string[] { 
                    collegeInformation.affiliationFromDate3.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate4 != null)
                    {
                        collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate5 != null)
                    {
                        collegeInformation.affiliationFromDate5 = new string[] { 
                    collegeInformation.affiliationFromDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()  
                };
                    }
                    if (collegeInformation.affiliationToDate1 != null)
                    {
                //        collegeInformation.affiliationToDate1 = new string[] { 
                //    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                //};
                        collegeInformation.affiliationToDate1 = collegeInformation.affiliationToDate1==null?string.Empty :
                        Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.ToString());
                    }

                    if (collegeInformation.affiliationToDate2 != null)
                    {
                        collegeInformation.affiliationToDate2 = new string[] { 
                    collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate3 != null)
                    {
                        collegeInformation.affiliationToDate3 = new string[] { 
                    collegeInformation.affiliationToDate3.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate4 != null)
                    {
                        collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate5 != null)
                    {
                        collegeInformation.affiliationToDate5 = new string[] { 
                    collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()  
                };
                    }
                    if (collegeInformation.affiliationDuration1 != null)
                    {
                //        collegeInformation.affiliationDuration1 = new string[] { 
                //    collegeInformation.affiliationDuration1.Length == 0 ? string.Empty : collegeInformation.affiliationDuration1.FirstOrDefault()  
                //};
                        collegeInformation.affiliationDuration1 =collegeInformation.affiliationDuration1==null?string.Empty : collegeInformation.affiliationDuration1.ToString();
                    }
                    if (collegeInformation.affiliationDuration2 != null)
                    {
                        collegeInformation.affiliationDuration2 = new string[] { 
                    collegeInformation.affiliationDuration2.Length == 0 ? string.Empty : collegeInformation.affiliationDuration2.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration3 != null)
                    {
                        collegeInformation.affiliationDuration3 = new string[] { 
                    collegeInformation.affiliationDuration3.Length == 0 ? string.Empty : collegeInformation.affiliationDuration3.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration4 != null)
                    {
                        collegeInformation.affiliationDuration4 = new string[] { 
                    collegeInformation.affiliationDuration4.Length == 0 ? string.Empty : collegeInformation.affiliationDuration4.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration5 != null)
                    {
                        collegeInformation.affiliationDuration5 = new string[] { 
                    collegeInformation.affiliationDuration5.Length == 0 ? string.Empty : collegeInformation.affiliationDuration5.FirstOrDefault()  
                };
                    }

                    //if (collegeInformation.affiliationToDate1 != null)
                    //{
                    //    collegeInformation.affiliationToDate1 = new string[] {
                    //        collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate2 != null)
                    //{
                    //    collegeInformation.affiliationToDate2 = new string[] { 
                    //        collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate3 != null)
                    //{
                    //    collegeInformation.affiliationToDate3 = new string[] { 
                    //        collegeInformation.affiliationToDate3.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate4 != null)
                    //{
                    //    collegeInformation.affiliationToDate4 = new string[] { 
                    //        collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate5 != null)
                    //{
                    //    collegeInformation.affiliationToDate5 = new string[] { 
                    //        collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
                    ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                    ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
                    ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();

                    ViewBag.StatusName = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();
                    ViewBag.StateName = db.jntuh_state.Where(s => s.id == collegeInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                    ViewBag.DistrictName = db.jntuh_district.Where(s => s.id == collegeInformation.districtId).Select(s => s.districtName).FirstOrDefault();
                }

                return View(collegeInformation);
            }

            return View(collegeInformation);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult RTI_AllCollegesInformationReport()
        {
            List<jntuh_college> colleges = db.jntuh_college.Where(c => c.isActive == true).Select(c => c).ToList();
            List<CollegeInformation> collegeInformationList = new List<CollegeInformation>();

            foreach (var clgInfo in colleges.ToList())
            {
                CollegeInformation collegeInformation = new CollegeInformation();

                int collegeid = clgInfo.id;

                if (collegeid != 0)
                {

                    jntuh_college jntuh_college = db.jntuh_college.Find(collegeid);
                    if (jntuh_college != null)
                    {
                        collegeInformation.id = jntuh_college.id;
                        collegeInformation.collegeCode = jntuh_college.collegeCode;
                        collegeInformation.collegeName = jntuh_college.collegeName;
                        collegeInformation.collegeStatusID = jntuh_college.collegeStatusID;
                        collegeInformation.eamcetCode = jntuh_college.eamcetCode;
                        collegeInformation.icetCode = jntuh_college.icetCode;
                        collegeInformation.otherCategory = jntuh_college.otherCategory;
                        collegeInformation.createdBy = jntuh_college.createdBy;
                        collegeInformation.createdOn = jntuh_college.createdOn;
                        collegeInformation.updatedBy = jntuh_college.createdBy;
                        collegeInformation.updatedOn = jntuh_college.createdOn;
                    }

                    jntuh_address jntuh_address = db.jntuh_address.Where(a => a.collegeId == jntuh_college.id && a.addressTye.Equals("COLLEGE")).Select(a => a).ToList().FirstOrDefault();
                    if (jntuh_address != null)
                    {
                        collegeInformation.address = jntuh_address.address;
                        collegeInformation.addressTye = jntuh_address.address;
                        collegeInformation.townOrCity = jntuh_address.townOrCity;
                        collegeInformation.mandal = jntuh_address.mandal;
                        collegeInformation.stateId = jntuh_address.stateId;
                        collegeInformation.districtId = jntuh_address.districtId;
                        collegeInformation.pincode = jntuh_address.pincode;
                        collegeInformation.fax = jntuh_address.fax;
                        collegeInformation.landline = jntuh_address.landline;
                        collegeInformation.mobile = jntuh_address.mobile;
                        collegeInformation.email = jntuh_address.email;
                        collegeInformation.website = jntuh_address.website;
                    }

                    //after postback
                    string[] selectedCollegeAffiliation = jntuh_college.collegeAffiliationTypeID.ToString().Split(' ');
                    List<Item> lstAffiliationType = new List<Item>();
                    foreach (var type in db.jntuh_college_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.DisplayOrder).ToList())
                    {
                        string strType = type.id.ToString();
                        lstAffiliationType.Add(new Item { id = type.id, name = type.collegeAffiliationType, selected = selectedCollegeAffiliation.Contains(strType) ? 1 : 0 });

                    }

                    collegeInformation.collegeAffiliationType = lstAffiliationType;

                    string[] selectedCollegeType = jntuh_college.collegeTypeID.ToString().Split(' ');
                    List<Item> lstCollegeType = new List<Item>();
                    foreach (var type in db.jntuh_college_type.Where(s => s.isActive == true))
                    {
                        string strType = type.id.ToString();
                        lstCollegeType.Add(new Item { id = type.id, name = type.collegeType, selected = selectedCollegeType.Contains(strType) ? 1 : 0 });
                    }

                    collegeInformation.collegeType = lstCollegeType;

                    string[] selectedCollegeDegree = db.jntuh_college_degree.Where(d => d.collegeId == jntuh_college.id && d.isActive == true).Select(d => d.degreeId).ToArray().Select(s => s.ToString()).ToArray();
                    List<Item> lstDegree = new List<Item>();
                    foreach (var d in db.jntuh_degree.Where(s => s.isActive == true).OrderBy(s => s.degreeDisplayOrder))
                    {
                        string strType = d.id.ToString();
                        lstDegree.Add(new Item { id = d.id, name = d.degree, selected = selectedCollegeDegree.Contains(strType) ? 1 : 0 });
                    }

                    collegeInformation.degree = lstDegree;

                    string[] selectedAffiliationType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id).OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationTypeId).ToArray().Select(s => s.ToString()).ToArray();
                    List<Item> lstType = new List<Item>();
                    foreach (var t in db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder).ToList())
                    {
                        string strType = t.id.ToString();
                        lstType.Add(new Item { id = t.id, name = t.affiliationType, selected = selectedAffiliationType.Contains(strType) ? 1 : 0 });
                    }
                    int[] selectedAffiliationId = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(s => s.displayOrder)
                                                                              .Select(s => s.id).ToArray();
                    int affiliationCount = 1;
                    foreach (var item in selectedAffiliationId)
                    {
                        if (affiliationCount == 1)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected1 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected1 = 2;
                            }
                        }
                        else if (affiliationCount == 2)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected2 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected2 = 2;
                            }
                        }

                        else if (affiliationCount == 3)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Yes")
                            {
                                collegeInformation.affiliationSelected3 = 1;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected3 = 2;
                            }
                        }
                        else if (affiliationCount == 4)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                            .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Conferred")
                            {
                                collegeInformation.affiliationSelected4 = 1;
                            }
                            else if (statusType == "Applied")
                            {
                                collegeInformation.affiliationSelected4 = 2;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected4 = 3;
                            }
                        }
                        else if (affiliationCount == 5)
                        {
                            string statusType = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == item)
                                                                             .Select(a => a.affiliationStatus).FirstOrDefault();
                            if (statusType == "Conferred")
                            {
                                collegeInformation.affiliationSelected5 = 1;
                            }
                            else if (statusType == "Applied")
                            {
                                collegeInformation.affiliationSelected5 = 2;
                            }
                            else
                            {
                                collegeInformation.affiliationSelected5 = 3;
                            }
                        }
                        affiliationCount++;
                    }
                    collegeInformation.affiliationType = lstType;

                    int rowIndex = 1;
                    foreach (var type in lstType)
                    {
                        int affiliationType = type.id;

                        if (rowIndex == 1)
                        {
                            collegeInformation.affiliationFromDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();
                            collegeInformation.affiliationToDate1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();
                            collegeInformation.affiliationDuration1 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).FirstOrDefault();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        if (rowIndex == 2)
                        {
                            collegeInformation.affiliationFromDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration2 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        if (rowIndex == 3)
                        {
                            collegeInformation.affiliationFromDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration3 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }
                        if (rowIndex == 4)
                        {
                            collegeInformation.affiliationFromDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration4 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }
                        if (rowIndex == 5)
                        {
                            collegeInformation.affiliationFromDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationFromDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationToDate5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationToDate).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();
                            collegeInformation.affiliationDuration5 = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType)
                                                                        .OrderByDescending(a => a.affiliationTypeId).Select(a => a.affiliationDuration).ToArray()
                                                                        .Select(s => s.ToString()).ToArray();

                            collegeInformation.affiliationGrade = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.affiliationGrade).FirstOrDefault();
                            collegeInformation.affiliationCGPA = db.jntuh_college_affiliation.Where(a => a.collegeId == jntuh_college.id && a.affiliationTypeId == affiliationType).Select(a => a.CGPA).FirstOrDefault();
                        }

                        rowIndex++;
                    }

                    if (collegeInformation.affiliationFromDate1 != null)
                    {
                //        collegeInformation.affiliationFromDate1 = new string[] { 
                //    collegeInformation.affiliationFromDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString() 
                //};
                        collegeInformation.affiliationFromDate1 = collegeInformation.affiliationFromDate1 != null
                            ? Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.ToString())
                            : string.Empty;
                    }

                    if (collegeInformation.affiliationFromDate2 != null)
                    {
                        collegeInformation.affiliationFromDate2 = new string[] { 
                    collegeInformation.affiliationFromDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate3 != null)
                    {
                        collegeInformation.affiliationFromDate3 = new string[] { 
                    collegeInformation.affiliationFromDate3.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate4 != null)
                    {
                        collegeInformation.affiliationFromDate4 = new string[] { 
                    collegeInformation.affiliationFromDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationFromDate5 != null)
                    {
                        collegeInformation.affiliationFromDate5 = new string[] { 
                    collegeInformation.affiliationFromDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()  
                };
                    }
                    if (collegeInformation.affiliationToDate1 != null)
                    {
                        collegeInformation.affiliationToDate1 = new string[] { 
                    collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString() 
                };
                    }

                    if (collegeInformation.affiliationToDate2 != null)
                    {
                        collegeInformation.affiliationToDate2 = new string[] { 
                    collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate3 != null)
                    {
                        collegeInformation.affiliationToDate3 = new string[] { 
                    collegeInformation.affiliationToDate3.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate4 != null)
                    {
                        collegeInformation.affiliationToDate4 = new string[] { 
                    collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()  
                };
                    }

                    if (collegeInformation.affiliationToDate5 != null)
                    {
                        collegeInformation.affiliationToDate5 = new string[] { 
                    collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()  
                };
                    }
                    if (collegeInformation.affiliationDuration1 != null)
                    {
                //        collegeInformation.affiliationDuration1 = new string[] { 
                //    collegeInformation.affiliationDuration1.Length == 0 ? string.Empty : collegeInformation.affiliationDuration1.FirstOrDefault()  
                //};
                        collegeInformation.affiliationDuration1 =collegeInformation.affiliationDuration1==null? string.Empty : collegeInformation.affiliationDuration1.ToString();
                    }
                    if (collegeInformation.affiliationDuration2 != null)
                    {
                        collegeInformation.affiliationDuration2 = new string[] { 
                    collegeInformation.affiliationDuration2.Length == 0 ? string.Empty : collegeInformation.affiliationDuration2.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration3 != null)
                    {
                        collegeInformation.affiliationDuration3 = new string[] { 
                    collegeInformation.affiliationDuration3.Length == 0 ? string.Empty : collegeInformation.affiliationDuration3.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration4 != null)
                    {
                        collegeInformation.affiliationDuration4 = new string[] { 
                    collegeInformation.affiliationDuration4.Length == 0 ? string.Empty : collegeInformation.affiliationDuration4.FirstOrDefault()  
                };
                    }
                    if (collegeInformation.affiliationDuration5 != null)
                    {
                        collegeInformation.affiliationDuration5 = new string[] { 
                    collegeInformation.affiliationDuration5.Length == 0 ? string.Empty : collegeInformation.affiliationDuration5.FirstOrDefault()  
                };
                    }

                    //if (collegeInformation.affiliationToDate1 != null)
                    //{
                    //    collegeInformation.affiliationToDate1 = new string[] {
                    //        collegeInformation.affiliationToDate1.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate1.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate1.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate2 != null)
                    //{
                    //    collegeInformation.affiliationToDate2 = new string[] { 
                    //        collegeInformation.affiliationToDate2.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate2.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate2.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate3 != null)
                    //{
                    //    collegeInformation.affiliationToDate3 = new string[] { 
                    //        collegeInformation.affiliationToDate3.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate3.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate3.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate4 != null)
                    //{
                    //    collegeInformation.affiliationToDate4 = new string[] { 
                    //        collegeInformation.affiliationToDate4.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate4.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate4.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //}

                    //if (collegeInformation.affiliationToDate5 != null)
                    //{
                    //    collegeInformation.affiliationToDate5 = new string[] { 
                    //        collegeInformation.affiliationToDate5.Length == 0 ? string.Empty : (Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationToDate5.FirstOrDefault()).ToString().Length - 4, 4)) -
                    //        Convert.ToInt32(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString()
                    //        .Substring(Utilities.MMDDYY2DDMMYY(collegeInformation.affiliationFromDate5.FirstOrDefault()).ToString().Length - 4, 4))).ToString()
                    //    };
                    //} 
                    //ViewBag.Status = db.jntuh_college_status.Where(s => s.isActive == true).ToList();
                    //ViewBag.State = db.jntuh_state.Where(s => s.isActive == true).ToList();
                    //ViewBag.District = db.jntuh_district.Where(s => s.isActive == true).ToList();
                    ViewBag.AffiliationType = db.jntuh_affiliation_type.Where(s => s.isActive == true).OrderBy(f => f.displayOrder).ToList();

                    collegeInformation.statusName = db.jntuh_college_status.Where(s => s.id == collegeInformation.collegeStatusID).Select(s => s.collegeStatus).FirstOrDefault();
                    collegeInformation.stateName = db.jntuh_state.Where(s => s.id == collegeInformation.stateId).Select(s => s.stateName).FirstOrDefault();
                    collegeInformation.districtName = db.jntuh_district.Where(s => s.id == collegeInformation.districtId).Select(s => s.districtName).FirstOrDefault();

                }
                collegeInformationList.Add(collegeInformation);
            }

            int Count = collegeInformationList.Count();

            if (Count != 0)
            {
                Response.ClearContent();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=RTI_AllCollegesInformation.xls");
                Response.ContentType = "application//vnd.ms-excel";
                return PartialView("~/Views/RTI_CollegeInformation/RTI_AllCollegesInformationReport.cshtml", collegeInformationList);
            }
            else
            {
                return View("~/Views/RTI_CollegeInformation/Index.cshtml");
            }
        }


    }
}
