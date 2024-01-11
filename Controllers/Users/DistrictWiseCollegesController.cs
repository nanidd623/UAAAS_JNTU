using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UAAAS.Models;

namespace UAAAS.Controllers
{
    [ErrorHandling]
    public class DistrictWiseCollegesController : BaseController
    {
        private uaaasDBContext db = new uaaasDBContext();
        
        //
        // GET: /DistrictWiseCollegesIndex/
        public ActionResult DistrictWiseCollegesIndex(string districtName)
        {
            //Get districtid based on district name
            int districtId = db.jntuh_district.Where(district => district.districtName == districtName)
                                              .Select(district => district.id)
                                              .FirstOrDefault();
            //Get collegeIds based on districtId
            int[] collegeId = db.jntuh_address.Where(address => address.districtId == districtId)
                                              .Select(address => address.collegeId)
                                              .ToArray();
            //Get District Wise college list based on college id
            List<jntuh_college> districtWiseCollegeDetails = db.jntuh_college.Where(college => collegeId.Contains(college.id))
                                                                             .ToList();
            ViewBag.Count = districtWiseCollegeDetails.Count();
            ViewBag.DistrictWise = true;
            ViewBag.Districtname = districtName;
            return View("~/Views/Users/DistrictWiseCollegesIndex.cshtml", districtWiseCollegeDetails);
        }

        //
        // GET: /DisciplineWiseCollegesIndex/
        public ActionResult DisciplineWiseCollegesIndex(string degreeName)
        {
            //Get degreeId based on degree Name
            int degreeId = db.jntuh_degree.Where(degree => degree.degree == degreeName)
                                              .Select(degree => degree.id)
                                              .FirstOrDefault();
            //Get collegeIds based on degreeId
            int[] collegeId = db.jntuh_college_degree.Where(degree => degree.degreeId == degreeId)
                                              .Select(degree => degree.collegeId)
                                              .ToArray();
            //Get Degree Wise college list based on college id
            List<jntuh_college> disciplineWiseCollegeDetails = db.jntuh_college.Where(college => collegeId.Contains(college.id))
                                                                             .ToList();
            ViewBag.Count = disciplineWiseCollegeDetails.Count();
            ViewBag.DistrictWise = false;
            ViewBag.DegreeName = degreeName;
            return View("~/Views/Users/DistrictWiseCollegesIndex.cshtml", disciplineWiseCollegeDetails);
        }

    }
}
