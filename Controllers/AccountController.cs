using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using UAAAS.Models;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using WebMatrix.WebData;


namespace UAAAS.Controllers
{

    public class AccountController : BaseController
    {
        private ErrorLOG Error = new ErrorLOG();
        private uaaasDBContext db = new uaaasDBContext();
        int adminCollegeId = 0;
        private uaaasDBContext db1 = new uaaasDBContext();
        private uaaasDBContext db2 = new uaaasDBContext();

        public ActionResult Applicationmirrors()
        {
            return View();

        }

        public ActionResult LogOn()
        {
            if (Membership.GetUser() != null)
            {
                int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
                int[] FacultyDataEntryUserIds = { 113279, 113280, 113281, 113282, 113283, 113284, 113285, 113286, 113287, 113288, 113289, 113290, 113291, 113292, 113293, 113294, 113295, 113296, 113297, 113298 };
                if (userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()) || userRoles.Contains(
                                       db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry"))
                                           .Select(r => r.id)
                                           .FirstOrDefault()))
                {
                    string strHostName = System.Net.Dns.GetHostName();
                    string sipaddress = string.Empty;
                    IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                    foreach (IPAddress ipAddress in ipEntry.AddressList)
                    {
                        if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                        {
                            sipaddress = ipAddress.ToString();
                        }
                    }
                    if (sipaddress == "10.10.10.5")
                    {
                        TempData["Error"] = "You are not authorized to access this service,so contact your Administrator";
                        return View();
                    }
                }
                if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("College"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    // return RedirectToAction("College","OnlineRegistration");
                    return RedirectToAction("CollegeDashboard", "Dashboard");

                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Committee"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "Committee");
                }
                else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry")).Select(r => r.id).FirstOrDefault()))
                {
                    //if (FacultyDataEntryUserIds.Contains(userID))
                    //{
                    //    return RedirectToAction("SCMFacultyCertitficatesVerification", "FacultyVerificationDENew");
                    //}

                    //return RedirectToAction("Index", "FacultyVerificationDENew");
                    return RedirectToAction("Welcome", "UnderConstruction");

                }
                else if (
               userRoles.Contains(
                   db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                       .Select(r => r.id)
                       .FirstOrDefault()))
                {
                    //Added Faculty Verification Instructions Page in 419
                    return RedirectToAction("FacultyVericicationPreamble", "FacultyVerification");
                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Faculty"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    int facultyId =
                        db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                            .Select(f => f.id)
                            .FirstOrDefault();
                    string FacultyType =
                        db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                            .Select(f => f.type)
                            .FirstOrDefault();
                    string fid = Utilities.EncryptString(facultyId.ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);

                    if (FacultyType != "Adjunct")
                        return RedirectToAction("Index", "NewOnlineRegistration", new { id = fid });
                    else
                        return RedirectToAction("AdjunctFacty", "OnlineRegistration",
                            new { fid = fid });
                    //TempData["Error"] = "This Service is unavilable for short time";
                }
                else if (
                    userRoles.Contains(
                        db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                            .Select(r => r.id)
                            .FirstOrDefault()))
                {
                    return RedirectToAction("Admin", "Dashboard");
                    return RedirectToAction("Admin", "Dashboard");
                }
                else if (
                           userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("Complaints"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "CollegeComplaints");

                }
                else if (
                           userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("Accounts"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "CollegesFee");

                }
                else if (
                       userRoles.Contains(
                           db.my_aspnet_roles.Where(r => r.name.Equals("Operations"))
                               .Select(r => r.id)
                               .FirstOrDefault()))
                {
                    return RedirectToAction("Index", "FacultyVerificationDENew");

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            TempData["Error"] = null;
            try
            {
                if (ModelState.IsValid)
                {
                    //MembershipUserCollection allUsers = Membership.GetAllUsers();
                    MembershipUser userdata = Membership.GetUser(model.UserName);
                    bool isOnline = true;
                    if (userdata == null)
                    {
                        TempData["Error"] = "The username or password provided is incorrect.";
                        return View();
                    }
                    //foreach (MembershipUser userdata in allUsers)
                    //{
                    // if user is currently online, add to gridview list
                    //Single User Login Code Is Comment on 31-01-2018
                    if (userdata.IsOnline == isOnline && userdata.UserName == model.UserName && Roles.GetRolesForUser(userdata.UserName)[0] == "College")
                    {
                        //if (userdata.UserName != "cstl")
                        //{
                        //ViewBag.Multiple = true;
                        //return View(model);
                        //}
                    }
                    //}
                    // int userID1 = Convert.ToInt32(Membership.GetUser(model.UserName).ProviderUserKey);

                    // Autonomus and MBA            112.133.193.233:75   10.10.10.16
                    //int[] LoginUserIds = { 6, 14, 17, 68, 71, 83, 93, 102, 120, 175, 247, 249, 280, 297, 326, 344, 346, 356, 359, 360, 387, 395, 402, 409, 411, 413, 419, 421, 424, 430, 431, 451, 473, 10, 12, 27, 33, 39, 69, 109, 110, 135, 172, 180, 181, 184, 193, 197, 199, 336, 368, 375, 399, 450, 376, 8214 };
                    //if (!LoginUserIds.Contains(userID1))
                    //{
                    //    ViewBag.NotLogin = true;
                    //    return View();
                    //}


                    ////Pharmacy                      112.133.193.232:75     10.10.10.15
                    //int[] LoginUserIds = { 7, 25, 28, 31, 34, 35, 45, 46, 48, 53, 55, 56, 59, 61, 66, 67, 79, 84, 91, 96, 98, 105, 106, 108, 111, 115, 118, 119, 121, 128, 136, 137, 140, 147, 151, 160, 170, 203, 205, 207, 214, 220, 235, 238, 240, 253, 254, 256, 263, 264, 268, 276, 284, 291, 296, 298, 299, 302, 303, 304, 314, 315, 316, 318, 319, 320, 321, 349, 354, 363, 371, 377, 378, 380, 385, 390, 393, 396, 410, 427, 428, 433, 436, 442, 445, 446, 449, 471, 376, 450 };
                    //if (!LoginUserIds.Contains(userID1))
                    //{
                    //    ViewBag.NotLogin = true;
                    //    return View();
                    //}

                    // Some Intigrated Colleges                      112.133.193.235:75     10.10.10.17
                    //int[] LoginUserIds = { 2, 5, 8, 9, 15, 18, 19, 21, 22, 23, 24, 29, 36, 40, 41, 42, 43, 44, 47, 49, 51, 60, 70, 73, 74, 75, 78, 81, 82, 86, 87, 92, 103, 104, 114, 116, 117, 122, 123, 124, 127, 131, 133, 138, 139, 141, 142, 144, 146, 149, 153, 156, 157, 159, 164, 166, 167, 168, 171, 173, 174, 176, 177, 179, 182, 185, 186, 187, 188, 190, 194, 195, 196, 198, 202, 204, 208, 209, 216, 218, 219, 223, 224, 231, 239, 244, 246, 248, 250, 255, 260, 261, 262, 267, 270, 272, 274, 277, 281, 288, 292, 294, 300, 301, 306, 310, 311, 317, 323, 325, 330, 331, 335, 337, 339, 350, 351, 353, 361, 364, 365, 366, 367, 369, 374, 381, 383, 389, 398, 406, 416, 417, 420, 422, 429, 439, 440, 441, 443, 444, 447, 376, 450, 236 };
                    //if (!LoginUserIds.Contains(userID1))
                    //{
                    //    ViewBag.NotLogin = true;
                    //    return View();
                    //}


                    //// Intigrated and Faculty Edit Option                   jntuhaac.in:72    10.10.10.5
                    // int[] LoginUserIds = { 7, 25, 28, 31, 34, 35, 45, 46, 48, 53, 55, 56, 59, 61, 66, 67, 79, 84, 91, 96, 98, 105, 106, 108, 111, 115, 118, 119, 121, 128, 136, 137, 140, 147, 151, 160, 170, 203, 205, 207, 214, 220, 235, 238, 240, 253, 254, 256, 263, 264, 268, 276, 284, 291, 296, 298, 299, 302, 303, 304, 314, 315, 316, 318, 319, 320, 321, 349, 354, 363, 371, 377, 378, 380, 385, 390, 393, 396, 410, 427, 428, 433, 436, 442, 445, 446, 449, 471, 6, 14, 17, 68, 83, 93, 102, 120, 175, 247, 249, 280, 297, 326, 344, 346, 356, 359, 360, 387, 395, 402, 409, 411, 413, 419, 421, 424, 430, 431, 451, 473, 10, 12, 27, 33, 39, 69, 109, 110, 135, 172, 180, 181, 184, 193, 197, 199, 336, 368, 375, 399, 2, 5, 8, 9, 15, 18, 19, 21, 22, 23, 24, 29, 36, 40, 41, 42, 43, 44, 47, 49, 51, 60, 70, 73, 74, 75, 78, 81, 82, 86, 87, 92, 103, 104, 114, 116, 117, 122, 123, 124, 127, 131, 133, 138, 139, 141, 142, 144, 146, 149, 153, 156, 157, 159, 164, 166, 167, 168, 171, 173, 174, 176, 177, 179, 182, 185, 186, 187, 188, 190, 194, 195, 196, 198, 202, 204, 208, 209, 216, 218, 219, 223, 224, 231, 239, 244, 246, 248, 250, 255, 260, 261, 262, 267, 270, 272, 274, 277, 281, 288, 292, 294, 300, 301, 306, 310, 311, 317, 323, 325, 330, 331, 335, 337, 339, 350, 351, 353, 361, 364, 365, 366, 367, 369, 374, 381, 383, 389, 398, 406, 416, 417, 420, 422, 429, 439, 440, 441, 443, 444, 447 };
                    //int[] LoginUserIds = { 6, 14, 17, 68, 71, 83, 93, 102, 120, 175, 247, 249, 280, 297, 326, 344, 346, 356, 359, 360, 387, 395, 402, 409, 411, 413, 419, 421, 424, 430, 431, 451, 473, 10, 12, 27, 33, 39, 69, 109, 110, 135, 172, 180, 181, 184, 193, 197, 199, 336, 368, 375, 399, 8214, 7, 25, 28, 31, 34, 35, 45, 46, 48, 53, 55, 56, 59, 61, 66, 67, 79, 84, 91, 96, 98, 105, 106, 108, 111, 115, 118, 119, 121, 128, 136, 137, 140, 147, 151, 160, 170, 203, 205, 207, 214, 220, 235, 238, 240, 253, 254, 256, 263, 264, 268, 276, 284, 291, 296, 298, 299, 302, 303, 304, 314, 315, 316, 318, 319, 320, 321, 349, 354, 363, 371, 377, 378, 380, 385, 390, 393, 396, 410, 427, 428, 433, 436, 442, 445, 446, 449, 471, 2, 5, 8, 9, 15, 18, 19, 21, 22, 23, 24, 29, 36, 40, 41, 42, 43, 44, 47, 49, 51, 60, 70, 73, 74, 75, 78, 81, 82, 86, 87, 92, 103, 104, 114, 116, 117, 122, 123, 124, 127, 131, 133, 138, 139, 141, 142, 144, 146, 149, 153, 156, 157, 159, 164, 166, 167, 168, 171, 173, 174, 176, 177, 179, 182, 185, 186, 187, 188, 190, 194, 195, 196, 198, 202, 204, 208, 209, 216, 218, 219, 223, 224, 231, 239, 244, 246, 248, 250, 255, 260, 261, 262, 267, 270, 272, 274, 277, 281, 288, 292, 294, 300, 301, 306, 310, 311, 317, 323, 325, 330, 331, 335, 337, 339, 350, 351, 353, 361, 364, 365, 366, 367, 369, 374, 381, 383, 389, 398, 406, 416, 417, 420, 422, 429, 439, 440, 441, 443, 444, 447, 236 };
                    //if (!LoginUserIds.Contains(userID1))
                    //{
                    //    if (userID1 > 500)
                    //    {
                    //        ViewBag.NotLogin = true;
                    //        return View();
                    //    }
                    //}
                    //else
                    //{
                    //    ViewBag.NotLogin = true;
                    //    return View();
                    //}

                    //Faculty Edit Option Only
                    //int[] LoginUserIds = { 199, 381, 442, 406, 75, 25, 34, 46, 62, 380, 341, 269, 264, 233, 430, 27, 184, 42, 188, 131, 86, 330, 395, 6, 342, 399, 177, 153, 37, 216, 302, 298, 440, 300, 340, 357, 168, 244, 366, 306, 122, 15, 385, 11, 425, 383, 23, 29, 50, 355, 292, 239, 202, 288, 135, 171, 127, 336, 179, 65, 87, 367, 103, 116, 124, 416, 379, 227, 138, 191, 246, 447, 252, 391, 281, 412, 260, 167, 22, 443, 232, 195, 418, 312, 384, 7, 67, 59, 115, 19, 26, 365, 36, 38, 44, 224, 40, 18, 222, 149, 369, 114, 144, 197, 374, 51, 141, 175, 402, 142, 373, 146, 60, 150, 358, 368, 338, 382, 375, 282, 329, 433, 315, 210, 352, 339, 218, 422, 419, 267, 236, 95, 83, 247, 249, 387, 396, 409, 445, 431, 316, 303, 105, 9, 364, 426, 63, 286, 81, 73, 270, 33, 398, 335, 187, 274, 194, 317, 311, 106, 262, 172, 310, 176, 12, 294, 223, 49, 272, 219, 108, 113, 118, 121, 128, 285, 238, 136, 48, 378, 43, 348, 353, 70, 69, 198, 248, 201, 77, 21, 96, 304, 354, 209, 79, 205, 371, 123, 24, 208, 157, 41, 185, 2, 323, 147, 393, 256, 203, 363, 132, 78, 321, 427, 284, 35, 253, 4, 100, 217, 291, 349, 276, 296, 275, 254, 299, 104, 137, 28, 174, 279, 428, 263, 53, 240, 278, 337, 64, 47, 180, 181, 133, 314, 182, 140, 58, 186, 204, 82, 110, 389, 439, 99, 161, 331, 444, 250, 193, 231, 166, 196, 350, 301, 139, 92, 156, 190, 74, 164, 261, 255, 420, 351, 417, 117, 109, 362, 324, 325, 159, 277, 206, 332, 5, 441, 361, 8, 173, 20, 429, 213, 145, 403, 30, 243, 32, 54, 163, 52, 72, 408, 152, 228, 435, 80, 423, 155, 221, 154, 85, 215, 71, 39, 242, 107, 404, 290, 305, 88, 343, 225, 392, 245, 13, 226, 134, 273, 101, 386, 125, 130, 388, 112, 372, 251, 57, 377, 66, 151, 390, 207, 31, 111, 446, 212, 126, 189, 407, 434, 143, 192, 480, 241, 76, 230, 89, 414, 265, 97, 293, 401, 165, 268, 394, 183, 148, 415, 271, 295, 169, 308, 307, 309, 322, 158, 257, 234, 200, 328, 237, 327, 477, 345, 356, 280, 360, 359, 432, 283, 16, 162, 98, 410, 119, 320, 14, 17, 397, 405, 470, 424, 258, 3, 448, 370, 211, 400, 297, 259, 411, 346, 313, 178, 10, 94, 333, 334, 45, 235, 326, 68, 437, 438, 344, 451, 120, 129, 229, 287, 90, 289, 492, 93, 102, 413, 266, 436, 347, 472, 474, 473, 471, 421, 478, 479, 475, 84, 449, 318, 319, 170, 160, 220, 214, 56, 55, 61, 91, 376 };
                    //if (LoginUserIds.Contains(userID1))
                    //{
                    //    //if (userID1 < 500)
                    //    //{
                    //    ViewBag.NotLogin = true;
                    //    return View();
                    //    //}
                    //}

                    //Checking Phd Faculty Edit option for Uploading PHD Undertaking Document
                    #region Phd Email Ids for Uploading Phd Undertaking Document.


                    string[] phdemailIds = new string[]
                    {
                        "palsasuresh1@gmail.com", "sonapoppy@gmail.com", "shriramoju.suman@gmail.com",
                        "rihanaftsana@gmail.com", "neelimabatta81@gmail.com", "parimaladevi2006@gmail.com",
                        "raja.haranath@gmail.com", "pharmaraj1981@gmail.com", "jothicbcp@gmail.com",
                        "satyajithpharma09@gmail.com", "satyasunderanalysis@gmail.com", "nithinjadav1@gmail.com",
                        "vangarars123@gmail.com", "hemnath.elango@gmail.com", "sreekanthpharma@gmail.com",
                        "deepthiswcp@gmail.com", "aogcpk07@gmail.com", "mohanty_vision2007@gmail.com"
                    };
                    #endregion

                    //if (phdemailIds.Contains(model.UserName))
                    //{
                    //    var facultyData =db.jntuh_registered_faculty.Where(e => e.Email == model.UserName && e.Mobile.Substring(3,e.Mobile.Length-1) == model.Password).Select(e => e).FirstOrDefault();
                    //    if (facultyData != null)
                    //    {
                    //        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    //        return RedirectToAction("PhdFacultyUploadingPhdDocument", "OnlineRegistration", new { UserName = UAAAS.Models.Utilities.EncryptString(facultyData.Email, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]), Password = UAAAS.Models.Utilities.EncryptString(facultyData.Mobile, System.Web.Configuration.WebConfigurationManager.AppSettings["CryptoKey"]) });
                    //    }
                    //    else
                    //    {
                    //        TempData["Error"] = "The username or password provided is incorrect.";
                    //    }
                    //}
                    //else
                    //{
                    if (Membership.ValidateUser(model.UserName.TrimEnd(' '), model.Password.TrimEnd(' ')))
                    {
                        int userID = Convert.ToInt32(Membership.GetUser(model.UserName).ProviderUserKey);
                        //College Id 413,XD Is Active on 16-02-2018
                        //Restrict In Active Users
                        int[] inactiveCollegeIds = db.jntuh_college.Where(c => c.isActive == false).Select(s => s.id).ToArray();
                        int[] inactiveCollegeUserIds = db.jntuh_college_users.Where(u => inactiveCollegeIds.Contains(u.collegeID)).Select(s => s.userID).ToArray();
                        int[] FacultyDataEntryUserIds = { 113279, 113280, 113281, 113282, 113283, 113284, 113285, 113286, 113287, 113288, 113289, 113290, 113291, 113292, 113293, 113294, 113295, 113296, 113297, 113298 };
                        if (inactiveCollegeUserIds.Contains(userID))
                        {
                            ViewBag.Inactive = true;
                            return View();
                        }
                        //Checking User role befor login
                        List<int> userRoles = db.my_aspnet_usersinroles.Where(u => u.userId == userID).Select(u => u.roleId).ToList();
                        if (userRoles.Contains(
                            db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                                .Select(r => r.id)
                                .FirstOrDefault()) || userRoles.Contains(
                                    db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry"))
                                        .Select(r => r.id)
                                        .FirstOrDefault()))
                        {
                            //string strHostName = System.Net.Dns.GetHostName();
                            //string sipaddress = string.Empty;
                            //IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
                            //foreach (IPAddress ipAddress in ipEntry.AddressList)
                            //{
                            //    if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                            //    {
                            //        sipaddress = ipAddress.ToString();
                            //    }
                            //}
                            //if (sipaddress == "10.10.10.5")
                            //{
                            //    TempData["Error"] = "You are not authorized to access this service,so contact your Administrator";
                            //    return View(model);
                            //}
                        }

                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);


                        MembershipUser user = Membership.GetUser(false);

                        //login log
                        string _sessionId = Guid.NewGuid().ToString();
                        user_login_logout userLog = new user_login_logout();
                        userLog.UserId = userID;
                        userLog.IPAddress = Request.UserHostAddress;
                        userLog.SessionId = _sessionId;
                        userLog.Login = DateTime.Now;
                        userLog.Logout = null;
                        userLog.isOnline = true;
                        userLog.isActive = true;
                        userLog.createdBy = userID;
                        userLog.createdOn = userLog.Login;
                        db.user_login_logout.Add(userLog);
                        db.SaveChanges();
                        db.Database.Connection.Close();
                        Session["LOGIN_GUID"] = _sessionId;

                        //Get browser details
                        var browser = Request.Browser;
                        user_browsers uBrowser = new user_browsers();
                        uBrowser.UserId = userID;
                        uBrowser.IPAddress = Request.UserHostAddress;
                        uBrowser.Type = browser.Type;
                        uBrowser.Name = browser.Browser;
                        uBrowser.Version = browser.Version;
                        uBrowser.MajorVersion = browser.MajorVersion.ToString();
                        uBrowser.MinorVersion = browser.MinorVersion.ToString();
                        uBrowser.Platform = browser.Platform;
                        uBrowser.IsBeta = browser.Beta.Equals(true) ? "True" : "False";
                        uBrowser.IsCrawler = browser.Crawler.Equals(true) ? "True" : "False";
                        uBrowser.IsAOL = browser.AOL.Equals(true) ? "True" : "False";
                        uBrowser.IsWin16 = browser.Win16.Equals(true) ? "True" : "False";
                        uBrowser.IsWin32 = browser.Win32.Equals(true) ? "True" : "False";
                        uBrowser.SupportsFrames = browser.Frames.Equals(true) ? "True" : "False";
                        uBrowser.SupportsTables = browser.Tables.Equals(true) ? "True" : "False";
                        uBrowser.SupportsCookies = browser.Cookies.Equals(true) ? "True" : "False";
                        uBrowser.SupportsVBScript = browser.VBScript.Equals(true) ? "True" : "False";
                        uBrowser.SupportsJavaScript = browser.EcmaScriptVersion.ToString();
                        uBrowser.SupportsJavaApplets = browser.JavaApplets.Equals(true) ? "True" : "False";
                        uBrowser.SupportsActiveXControls = browser.ActiveXControls.Equals(true) ? "True" : "False";
                        uBrowser.SupportsJavaScriptVersion = browser["JavaScriptVersion"].ToString();
                        uBrowser.createdBy = userID;
                        uBrowser.createdOn = DateTime.Now;
                        db.user_browsers.Add(uBrowser);
                        db.SaveChanges();
                        db.Database.Connection.Close();
                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {

                            if (
                                userRoles.Contains(
                                    db.my_aspnet_roles.Where(r => r.name.Equals("College"))
                                        .Select(r => r.id)
                                        .FirstOrDefault()))
                            {
                                // return RedirectToAction("College","OnlineRegistration");
                                return RedirectToAction("CollegeDashboard", "Dashboard");

                            }
                            else if (
                            userRoles.Contains(
                                db.my_aspnet_roles.Where(r => r.name.Equals("Complaints"))
                                    .Select(r => r.id)
                                    .FirstOrDefault()))
                            {
                                // return RedirectToAction("College","OnlineRegistration");
                                return RedirectToAction("Index", "CollegeComplaints");

                            }
                            else if (
                                userRoles.Contains(
                                    db.my_aspnet_roles.Where(r => r.name.Equals("Committee"))
                                        .Select(r => r.id)
                                        .FirstOrDefault()))
                            {
                                return RedirectToAction("Index", "Committee");
                            }
                            else if (userRoles.Contains(db.my_aspnet_roles.Where(r => r.name.Equals("DataEntry")).Select(r => r.id).FirstOrDefault()))
                            {
                                //if (FacultyDataEntryUserIds.Contains(userID))
                                //{
                                //    return RedirectToAction("SCMFacultyCertitficatesVerification", "FacultyVerificationDENew");
                                //}
                                //else
                                //{
                                //return RedirectToAction("DataEntryAssignedColleges", "DataEntryAssignedColleges");
                                return RedirectToAction("Welcome", "UnderConstruction");
                                //}

                            }
                            else if (
                           userRoles.Contains(
                               db.my_aspnet_roles.Where(r => r.name.Equals("FacultyVerification"))
                                   .Select(r => r.id)
                                   .FirstOrDefault()))
                            {
                                //Added Faculty Verification Instructions Page in 419
                                return RedirectToAction("FacultyVericicationPreamble", "FacultyVerification");
                            }
                            else if (
                                userRoles.Contains(
                                    db.my_aspnet_roles.Where(r => r.name.Equals("Faculty"))
                                        .Select(r => r.id)
                                        .FirstOrDefault()))
                            {
                                int facultyId =
                                    db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                                        .Select(f => f.id)
                                        .FirstOrDefault();
                                string FacultyType =
                                    db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                                        .Select(f => f.type)
                                        .FirstOrDefault();
                                string fid = Utilities.EncryptString(facultyId.ToString(),
                                    WebConfigurationManager.AppSettings["CryptoKey"]);

                                if (FacultyType != "Adjunct")
                                    return RedirectToAction("Index", "NewOnlineRegistration", new { id = fid });
                                else
                                    return RedirectToAction("AdjunctFacty", "OnlineRegistration",
                                        new { fid = fid });
                                //TempData["Error"] = "This Service is unavilable for short time";
                            }
                            else if (
                                userRoles.Contains(
                                    db.my_aspnet_roles.Where(r => r.name.Equals("Admin"))
                                        .Select(r => r.id)
                                        .FirstOrDefault()))
                            {
                                return RedirectToAction("Admin", "Dashboard");
                                return RedirectToAction("Admin", "Dashboard");
                            }
                            else if (
                            userRoles.Contains(
                                db.my_aspnet_roles.Where(r => r.name.Equals("Accounts"))
                                    .Select(r => r.id)
                                    .FirstOrDefault()))
                            {
                                return RedirectToAction("Index", "CollegesFee");

                            }
                            else if (
                        userRoles.Contains(
                            db.my_aspnet_roles.Where(r => r.name.Equals("Operations"))
                                .Select(r => r.id)
                                .FirstOrDefault()))
                            {
                                return RedirectToAction("FindRegistrationNumber", "Account");

                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {

                        TempData["Error"] = "The username or password provided is incorrect.";
                    }
                    //  }






                }
                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }



        }

        public ActionResult LogOff()
        {
            try
            {
                MembershipUser user = Membership.GetUser(false);
                if (user != null)
                {
                    user.LastActivityDate = DateTime.UtcNow.AddMinutes(-(Membership.UserIsOnlineTimeWindow + 1));
                    Membership.UpdateUser(user);

                    //int userID = Convert.ToInt32(Membership.GetUser(user.UserName).ProviderUserKey);
                }

                string _sessionID = string.Empty;

                if (Session["LOGIN_GUID"] != null)
                {
                    _sessionID = Session["LOGIN_GUID"].ToString();
                }

                //logout log //l.UserId == userID && 
                var oldLog =
                    db.user_login_logout.AsNoTracking()
                        .Where(l => l.isOnline == true && l.SessionId == _sessionID)
                        .Select(l => l)
                        .FirstOrDefault();

                if (oldLog != null)
                {
                    user_login_logout userLog = new user_login_logout();
                    userLog.id = oldLog.id;
                    userLog.UserId = oldLog.createdBy;
                    userLog.IPAddress = oldLog.IPAddress;
                    userLog.SessionId = oldLog.SessionId;
                    userLog.Login = oldLog.Login;
                    userLog.Logout = DateTime.Now;
                    userLog.isOnline = false;
                    userLog.isActive = oldLog.isActive;
                    userLog.createdBy = oldLog.createdBy;
                    userLog.createdOn = oldLog.createdOn;
                    userLog.updatedBy = oldLog.createdBy;
                    userLog.updatedOn = userLog.Logout;
                    db.Entry(userLog).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Database.Connection.Close();
                }

                FormsAuthentication.SignOut();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }


        }

        [Authorize(Roles = "Admin, SuperAdmin")]
        public ActionResult Register()
        {
            GetAllRoles();
            GetNotAssignedColleges();
            return View();
        }

        public void GetAllRoles()
        {
            try
            {
                var values = db.my_aspnet_roles.ToList()
               .Select(x => new SelectListItem
               {
                   Value = x.id.ToString(),
                   Text = x.name.ToString()
               });

                var menus = new SelectList(values, "Value", "Text");
                ViewData["Roles"] = menus;
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }

        }

        public void GetNotAssignedColleges()
        {
            try
            {
                var values = db.jntuh_college.Where(c => !db.jntuh_college_users.Select(u => u.collegeID).Contains(c.id))
               .ToList()
               .Select(x => new SelectListItem
               {
                   Value = x.id.ToString(),
                   Text = string.Format("{0} - {1}", x.collegeCode.ToString(), x.collegeName.ToString())
               });

                var menus = new SelectList(values, "Value", "Text");
                ViewData["Colleges"] = menus;
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }

        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null,
                        out createStatus);

                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        //add user role to my_aspnet_usersinroles table
                        my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
                        roleModel.roleId = Convert.ToInt32(model.SelectedRole);
                        roleModel.userId =
                            db.my_aspnet_users.Where(u => u.name == model.UserName).Select(u => u.id).FirstOrDefault();
                        db.my_aspnet_usersinroles.Add(roleModel);
                        db.SaveChanges();

                        if (model.selectedCollege != null)
                        {
                            jntuh_college_users collegeUsers = new jntuh_college_users();
                            collegeUsers.userID =
                                db.my_aspnet_users.Where(u => u.name == model.UserName).Select(u => u.id).FirstOrDefault();
                            collegeUsers.collegeID = Convert.ToInt32(model.selectedCollege);
                            collegeUsers.createdBy = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                            collegeUsers.createdOn = DateTime.Now;
                            db.jntuh_college_users.Add(collegeUsers);
                            db.SaveChanges();
                        }

                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.Welcome(model.Email, "LoginInformation", "JNTUH Account Login Details", model.UserName,
                            model.Password, string.Empty, string.Empty).SendAsync();
                        mailer.Welcome("supportaac@jntuh.ac.in", "LoginInformation", "JNTUH Account Login Details",
                            model.UserName, model.Password, string.Empty, string.Empty).SendAsync();

                        GetAllRoles();
                        GetNotAssignedColleges();
                        TempData["Success"] = "User created successfully";
                        //FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                        //return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        GetAllRoles();
                        GetNotAssignedColleges();
                        //ModelState.AddModelError("", ErrorCodeToString(createStatus));
                        TempData["Error"] = ErrorCodeToString(createStatus);
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }

        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                        changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword.TrimEnd(' '),
                            model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        my_aspnet_users my_aspnet_users = db.my_aspnet_users
                            .Find(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                        my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership
                            .Find(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                        //send email
                        IUserMailer mailer = new UserMailer();
                        mailer.Welcome(my_aspnet_membership.Email, "PasswordChange", "JNTUH Account password changed",
                            my_aspnet_users.name, model.NewPassword, string.Empty, string.Empty).SendAsync();
                        //mailer.Welcome("aac.do.not.reply@gmail.com", "PasswordChange", "JNTUH Account password changed", my_aspnet_users.name, model.NewPassword, string.Empty, string.Empty).SendAsync();

                        return RedirectToAction("ChangePasswordSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }

        }

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    string newPassword = "";

                    try
                    {
                        var userdata = db.jntuh_registered_faculty.Where(e => e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Email == model.UserName).Select(e => e.id).FirstOrDefault();
                        if (userdata != 0)
                        {
                            newPassword = Membership.Provider.ResetPassword(model.UserName, null);
                        }
                        else
                        {
                            ModelState.Clear();
                            TempData["Error"] = "Password reset failed. Please reenter your correct values and try again.";
                            return View(model);
                        }

                    }
                    catch (NotSupportedException e)
                    {
                        TempData["Error"] = "An error has occurred resetting your password: " + e.Message + "." +
                                            "Please check your values and try again.";
                    }
                    catch (MembershipPasswordException e)
                    {
                        TempData["Error"] = "Invalid password answer. Please reenter the answer and try again.";
                    }
                    catch (System.Configuration.Provider.ProviderException e)
                    {
                        TempData["Error"] = "The specified user name does not exist. Please check your value and try again.";
                    }

                    if (newPassword != "")
                    {
                        my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.UserName).ProviderUserKey);
                        //send email

                        //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                        //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                        //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                        //              .SendAsync();
                        //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();
                        //10/12/2018 comment by Narayana
                        //IUserMailer mailer = new UserMailer();
                        //mailer.Welcome(my_aspnet_membership.Email, "LoginInformation", "JNTUH Account password reset",
                        //    model.UserName, newPassword, string.Empty, string.Empty).SendAsync();




                        string MailBody = string.Empty;
                        MailBody += "<p>Dear " + my_aspnet_membership.Email + "</p><br/>";
                        MailBody += "<p>You can now login to www.jntuhaac.in with the following credentials.</p>";
                        MailBody += "<p><b>UserName: " + model.UserName + "</b></p>";
                        MailBody += "<p><b>Password: " + newPassword + "</b></p><br/>";
                        MailBody += "<p>Thanks & Regards</p>";
                        MailBody += "<p>Director, AAC,</p>";
                        MailBody += "<p>JNTUH, Hyderabad</p>";


                        MailMessage message = new MailMessage();
                        message.To.Add(my_aspnet_membership.Email);
                        message.Subject = "JNTUH Account password reset";
                        message.Body = MailBody;
                        message.IsBodyHtml = true;
                        //  message.Attachments.Add(new Attachment(filepath));
                        //  message.Attachments.Add(new Attachment(filepathsecond));
                        var smtp = new SmtpClient();
                        smtp.Credentials = new NetworkCredential("supportaac@jntuh.ac.in", "uaaac@aac");
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.EnableSsl = true;
                        smtp.Send(message);



                        string[] email = my_aspnet_membership.Email.Split('@');
                        string maskEmail = email[0].Substring(0, 2) +
                                           new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                           email[0].Substring(email[0].Length - 1, 1);

                        ModelState.Clear();
                        TempData["Success"] = "Password has been reset & sent successfully to " + maskEmail + "@" + email[1];
                    }
                    else
                    {
                        ModelState.Clear();
                        TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                    }
                }

                // If we got this far, something failed, redisplay form
                return View();
            }
            catch (Exception)
            {
                ExceptionContext context = new ExceptionContext();
                Error.LogException(context);
                throw;
            }

        }

        public ActionResult DisallowUserName(string UserName)
        {
            string existingUser = db.my_aspnet_users.Where(u => u.name == UserName).Select(u => u.name).FirstOrDefault();

            if (existingUser == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(string.Format("{0} is already exists", UserName), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DisallowEditUserName(string UserName, int id)
        {
            string existingUser = string.Empty;
            existingUser =
                db.my_aspnet_users.Where(u => u.name == UserName && u.id == id).Select(u => u.name).FirstOrDefault();

            if (existingUser == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }

            return Json(string.Format("{0} is already exists", UserName), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ResetPasswordLogin()
        {
            if (Request.Params["rstu"] != null && Request.Params["rstp"] != null)
            {
                string username = QueryStringEncryption.Encryption64.Decrypt(Request.Params["rstu"].ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
                string password = QueryStringEncryption.Encryption64.Decrypt(Request.Params["rstp"].ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);
                if (Membership.ValidateUser(username, password))
                {
                    FormsAuthentication.SetAuthCookie(username, false);
                }
                else
                {
                    return View("~/Views/Home/Index");
                    //TempData["Error"] = "The user name or password provided is incorrect.";
                }

                ChangePasswordModel ChangePasswordModel = new ChangePasswordModel();
                ChangePasswordModel.OldPassword =
                    QueryStringEncryption.Encryption64.Decrypt(Request.Params["rstp"].ToString(),
                        WebConfigurationManager.AppSettings["CryptoKey"]);
                return View(ChangePasswordModel);
            }
            else
            {
                return View("~/Views/Home/Index");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult ResetPasswordLogin(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    my_aspnet_users my_aspnet_users = db.my_aspnet_users
                        .Find(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership
                        .Find(Membership.GetUser(User.Identity.Name).ProviderUserKey);
                    //send email
                    IUserMailer mailer = new UserMailer();
                    mailer.Welcome(my_aspnet_membership.Email, "PasswordChange", "JNTUH Account password changed",
                        my_aspnet_users.name, model.NewPassword, string.Empty, string.Empty).SendAsync();

                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        #region Status Codes

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return
                        "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return
                        "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return
                        "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return
                        "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion

        public ActionResult FacultyForgotEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult FacultyForgotEmail(FacultyForgotEmail model)
        {
            if (ModelState.IsValid)
            {

                TempData["Email"] =
                    db.jntuh_registered_faculty.Where(
                        e =>
                            e.RegistrationNumber == model.RegistrationNumber && e.PANNumber == model.PanNumber &&
                            e.DateOfBirth == model.DateofBirth).Select(e => e.Email).FirstOrDefault();
                if (TempData["Email"] == null)
                {
                    TempData["Error"] = "Your Details Doesn't Match. Please Provide Valid Details.";
                }

            }
            else
            {
                TempData["Error"] = "Please Enter Mandatory Fields.";
            }
            return View(model);
        }

        public ActionResult GeneratePassword()
        {
            TempData["error"] = "";
            ViewBag.Institutions =
                db.jntuh_college.Where(c => c.isActive == true)
                    .Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName })
                    .OrderBy(c => c.CollegeName)
                    .ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GeneratePassword(GeneratePasswordModel model)
        {
            ViewBag.Institutions = db.jntuh_college.Where(c => c.isActive == true).Select(c => new { CollegeId = c.id, CollegeName = c.collegeCode + "-" + c.collegeName }).OrderBy(c => c.CollegeName).ToList();
            if (ModelState.IsValid)
            {
                var jntuh_registered_facultys = db.jntuh_registered_faculty.ToList();
                try
                {
                    var userId = jntuh_registered_facultys.Where(e => e.RegistrationNumber == model.RegistrationNumber && e.Email == model.Email &&
                                e.DateOfBirth == model.DateOfBirth && e.Mobile == model.Mobile
                                && e.PANNumber == model.PANNumber).FirstOrDefault();//&& e.collegeId == model.CollegeId
                    //&& e.MotherName == model.MotherName
                    if (userId != null)
                    {
                        var id = userId.id;
                        var passed10year =
                            db.jntuh_registered_faculty_education.Where(i => i.facultyId == id && i.educationId == 1)
                                .Select(i => i.passedYear)
                                .FirstOrDefault();
                        var passedUGyear =
                            db.jntuh_registered_faculty_education.Where(i => i.facultyId == id && i.educationId == 3)
                                .Select(i => i.passedYear)
                                .FirstOrDefault();
                        if (id > 0 && passed10year == model.TenthYear && passedUGyear == model.UGYear)
                        {
                            var pass = Membership.Provider.ResetPassword(model.Email, null);
                            // TempData["newpassword"] = pass;
                            if (!string.IsNullOrEmpty(model.Mobile))
                            {
                                //var msg = "JNTUH: Your Registered E-mail id is " + userId.Email + ", Your New Password is " + pass;
                                var msg = "JNTUH: Faculty Registration Portal New Password is " + pass;
                                bool pStatus = UAAAS.Models.Utilities.NewSendSms(model.Mobile, msg);
                                if (pStatus)
                                {
                                    TempData["success"] = "New Password send to Your Registered Mobile Number " +
                                                          userId.Mobile.Substring(0, 2) + "******" +
                                                          userId.Mobile.Substring(8, 2);
                                }
                            }
                        }
                        else
                        {
                            TempData["error"] = "Details Doesn't Match,please try again";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Details Doesn't Match,please try again";
                    }
                }
                catch (Exception)
                {
                    TempData["error"] = "Details Doesn't Match,please try again";
                }
            }

            return RedirectToAction("GeneratePassword");
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Operations")]
        public ActionResult FindRegistrationNumber()
        {
            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Operations")]
        [HttpPost]
        public ActionResult FindRegistrationNumber(FacultyRegistration regno)
        {
            FacultyRegistration regFaculty = new FacultyRegistration();
            int fID = 0;

            if (regno != null)
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;

                var faculty =
                    db.jntuh_registered_faculty.FirstOrDefault(
                        e => e.RegistrationNumber.Trim() == regno.RegistrationNumber.Trim());

                if (faculty != null)
                {
                    regFaculty.id = fID = faculty.id;
                    ViewBag.FacultyID = fID;
                    regFaculty.Type = faculty.type;
                    regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                    regFaculty.UserName =
                        db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                    regFaculty.Email = faculty.Email;
                    regFaculty.UniqueID = faculty.UniqueID;
                    regFaculty.FirstName = faculty.FirstName;
                    regFaculty.MiddleName = faculty.MiddleName;
                    regFaculty.LastName = faculty.LastName;
                    regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                    regFaculty.MotherName = faculty.MotherName;
                    regFaculty.GenderId = faculty.GenderId;
                    regFaculty.facultyDateOfBirth = Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                    regFaculty.Mobile = faculty.Mobile;
                    regFaculty.facultyPhoto = faculty.Photo;
                    regFaculty.PANNumber = faculty.PANNumber;
                    regFaculty.facultyPANCardDocument = faculty.PANDocument;
                    regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                    regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.OrganizationName = faculty.OrganizationName;
                    if (faculty.collegeId != null)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Find(faculty.collegeId).collegeName;
                    }
                    regFaculty.CollegeId = faculty.collegeId;
                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;

                    if (faculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                    }
                    regFaculty.DesignationId = faculty.DesignationId;
                    regFaculty.OtherDesignation = faculty.OtherDesignation;

                    if (faculty.DateOfAppointment != null)
                    {
                        regFaculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                    }
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }
                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;
                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;
                }

                else
                {
                    TempData["Error"] = "Registration Number Not Found...";
                    return RedirectToAction("FindRegistrationNumber");
                }



                string registrationNumber =
                    db.jntuh_registered_faculty.Where(of => of.id == fID)
                        .Select(of => of.RegistrationNumber)
                        .FirstOrDefault();
                int facultyId =
                    db.jntuh_college_faculty_registered.Where(of => of.RegistrationNumber == registrationNumber)
                        .Select(of => of.id)
                        .FirstOrDefault();
                //Commented on 18-06-2018 by Narayana Reddy
                //int[] verificationOfficers =
                //    db.jntuh_college_faculty_verified.Where(v => v.FacultyId == facultyId)
                //        .Select(v => v.VerificationOfficer)
                //        .Distinct()
                //        .ToArray();
                int userId = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);

                ViewBag.FacultyDetails = regFaculty;
                TempData["FacultyDetails"] = regFaculty;
                ViewBag.HideVerifyLink = regFaculty.isApproved != null ? true : false;
                string RegistrationNo = regno.RegistrationNumber.ToString();
                //return RedirectToAction("FacultyView", regFaculty);
                return RedirectToAction("FacultyView", new { REGNO = RegistrationNo });
            }

            return View();
        }


        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,Operations")]
        public ActionResult FacultyView(string REGNO)
        {

            FacultyRegistration regFaculty = new FacultyRegistration();

            if (!string.IsNullOrEmpty(REGNO))
            {
                regFaculty.GenderId = null;
                regFaculty.isFacultyRatifiedByJNTU = null;
                List<SelectListItem> years = new List<SelectListItem>();
                for (int i = 1940; i <= DateTime.Now.Year; i++)
                {
                    years.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
                }
                ViewBag.years = years;
                jntuh_registered_faculty faculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == REGNO.Trim()).Select(s => s).FirstOrDefault();

                if (faculty != null)
                {

                    regFaculty.id = faculty.id;
                    regFaculty.Type = faculty.type;
                    regFaculty.RegistrationNumber = faculty.RegistrationNumber;
                    regFaculty.UserName =
                        db.my_aspnet_users.Where(u => u.id == faculty.UserId).Select(u => u.name).FirstOrDefault();
                    regFaculty.Email = faculty.Email;
                    regFaculty.UniqueID = faculty.UniqueID;
                    regFaculty.FirstName = faculty.FirstName;
                    regFaculty.MiddleName = faculty.MiddleName;
                    regFaculty.LastName = faculty.LastName;
                    regFaculty.FatherOrhusbandName = faculty.FatherOrHusbandName;
                    regFaculty.MotherName = faculty.MotherName;
                    regFaculty.GenderId = faculty.GenderId;
                    regFaculty.CollegeId =
                        db.jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                            .Select(s => s.collegeId)
                            .FirstOrDefault();

                    if (regFaculty.CollegeId == 0 || regFaculty.CollegeId == null)
                    {
                        regFaculty.CollegeId =
                                  db.jntuh_college_principal_registered.Where(
                                      f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                                      .Select(s => s.collegeId)
                                      .FirstOrDefault();
                    }


                    if (faculty.DateOfBirth != null)
                    {
                        regFaculty.facultyDateOfBirth =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfBirth.ToString());
                    }
                    regFaculty.Mobile = faculty.Mobile;
                    regFaculty.facultyPhoto = faculty.Photo;
                    regFaculty.PANNumber = faculty.PANNumber;
                    regFaculty.facultyPANCardDocument = faculty.PANDocument;
                    regFaculty.AadhaarNumber = faculty.AadhaarNumber;
                    regFaculty.facultyAadhaarCardDocument = faculty.AadhaarDocument;
                    regFaculty.IncomeTaxFileview = faculty.IncometaxDocument;
                    //  regFaculty.faculty_AllCertificates = faculty.OrganizationName;
                    regFaculty.WorkingStatus = faculty.WorkingStatus;
                    regFaculty.TotalExperience = faculty.TotalExperience;
                    regFaculty.TotalExperiencePresentCollege = faculty.TotalExperiencePresentCollege;
                    // regFaculty.OrganizationName = faculty.OrganizationName;
                    if (regFaculty.CollegeId != 0)
                    {
                        regFaculty.CollegeName = db.jntuh_college.Where(a => a.id == regFaculty.CollegeId).Select(z => z.collegeName + " (" + z.collegeCode + ")").FirstOrDefault();
                    }

                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;

                    if (faculty.DesignationId != null)
                    {
                        regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                    }
                    regFaculty.DesignationId = faculty.DesignationId;
                    regFaculty.OtherDesignation = faculty.OtherDesignation;

                    if (faculty.DateOfAppointment != null)
                    {
                        regFaculty.facultyDateOfAppointment =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfAppointment.ToString());
                    }

                    regFaculty.isFacultyRatifiedByJNTU = faculty.isFacultyRatifiedByJNTU;
                    if (faculty.DateOfRatification != null)
                    {
                        regFaculty.facultyDateOfRatification =
                            UAAAS.Models.Utilities.MMDDYY2DDMMYY(faculty.DateOfRatification.ToString());
                    }

                    regFaculty.ProceedingsNo = faculty.ProceedingsNumber;
                    regFaculty.SelectionCommitteeProcedings = faculty.ProceedingDocument;

                    //Get the SCM Document from scmupload table uploaded by College.
                    // regFaculty.SelectionCommitteeProcedings = db.jntuh_scmupload.Where(e => e.RegistrationNumber.Trim() == regFaculty.RegistrationNumber.Trim()).Select(e => e.SCMDocument).FirstOrDefault();

                    regFaculty.AICTEFacultyId = faculty.AICTEFacultyId;
                    regFaculty.GrossSalary = faculty.grosssalary;
                    regFaculty.National = faculty.National;
                    regFaculty.InterNational = faculty.InterNational;
                    regFaculty.Citation = faculty.Citation;
                    regFaculty.Awards = faculty.Awards;
                    regFaculty.isActive = faculty.isActive;
                    regFaculty.isApproved = faculty.isApproved;
                    regFaculty.isView = true;
                    regFaculty.BlacklistFaculty = faculty.Blacklistfaculy;
                    //regFaculty.AbsentforVerification = faculty.AbsentforVerification;
                    regFaculty.DeactivationReason = faculty.DeactivationReason;
                    regFaculty.PHDView = db.jntuh_faculty_phddetails.Where(i => i.Facultyid == faculty.id).Count();
                    #region Faculty Education Data Getting

                    // var jntuh_education_category = db.jntuh_education_category.Where(e => e.isActive == true).ToList();
                    var registeredFacultyEducation = db.jntuh_registered_faculty_education.Where(e => e.facultyId == faculty.id).ToList();

                    if (registeredFacultyEducation.Count != 0)
                    {
                        foreach (var item in registeredFacultyEducation)
                        {
                            if (item.educationId == 1)
                            {
                                regFaculty.SSC_educationId = 1;
                                regFaculty.SSC_HallticketNo = item.hallticketnumber;
                                regFaculty.SSC_studiedEducation = item.courseStudied;
                                regFaculty.SSC_specialization = item.specialization;
                                regFaculty.SSC_passedYear = item.passedYear;
                                regFaculty.SSC_percentage = item.marksPercentage;
                                regFaculty.SSC_division = item.division == null ? 0 : item.division;
                                regFaculty.SSC_university = item.boardOrUniversity;
                                regFaculty.SSC_place = item.placeOfEducation;
                                regFaculty.SSC_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 3)
                            {
                                regFaculty.UG_educationId = 3;
                                regFaculty.UG_HallticketNo = item.hallticketnumber;
                                regFaculty.UG_studiedEducation = item.courseStudied;
                                regFaculty.UG_specialization = item.specialization;
                                regFaculty.UG_passedYear = item.passedYear;
                                regFaculty.UG_percentage = item.marksPercentage;
                                regFaculty.UG_division = item.division == null ? 0 : item.division;
                                regFaculty.UG_university = item.boardOrUniversity;
                                regFaculty.UG_place = item.placeOfEducation;
                                regFaculty.UG_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 4)
                            {
                                regFaculty.PG_educationId = 4;
                                regFaculty.PG_HallticketNo = item.hallticketnumber;
                                regFaculty.PG_studiedEducation = item.courseStudied;
                                regFaculty.PG_specialization = item.specialization;
                                regFaculty.PG_passedYear = item.passedYear;
                                regFaculty.PG_percentage = item.marksPercentage;
                                regFaculty.PG_division = item.division == null ? 0 : item.division;
                                regFaculty.PG_university = item.boardOrUniversity;
                                regFaculty.PG_place = item.placeOfEducation;
                                regFaculty.PG_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 5)
                            {
                                regFaculty.MPhil_educationId = 5;
                                regFaculty.MPhil_HallticketNo = item.hallticketnumber;
                                regFaculty.MPhil_studiedEducation = item.courseStudied;
                                regFaculty.MPhil_specialization = item.specialization;
                                regFaculty.MPhil_passedYear = item.passedYear;
                                regFaculty.MPhil_percentage = item.marksPercentage;
                                regFaculty.MPhil_division = item.division == null ? 0 : item.division;
                                regFaculty.MPhil_university = item.boardOrUniversity;
                                regFaculty.MPhil_place = item.placeOfEducation;
                                regFaculty.MPhil_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 6)
                            {
                                regFaculty.PhD_educationId = 6;
                                regFaculty.PhD_HallticketNo = item.hallticketnumber;
                                regFaculty.PhD_studiedEducation = item.courseStudied;
                                regFaculty.PhD_specialization = item.specialization;
                                regFaculty.PhD_passedYear = item.passedYear;
                                regFaculty.PhD_percentage = item.marksPercentage;
                                regFaculty.PhD_division = item.division == null ? 0 : item.division;
                                regFaculty.PhD_university = item.boardOrUniversity;
                                regFaculty.PhD_place = item.placeOfEducation;
                                regFaculty.PhD_facultyCertificate = item.certificate;
                            }
                            else if (item.educationId == 8)
                            {
                                regFaculty.Others_educationId = 8;
                                regFaculty.faculty_AllCertificates = item.certificate;
                            }
                            else if (item.educationId == 9)
                            {
                                regFaculty.NET_educationId = 6;
                                regFaculty.NET_HallticketNo = item.hallticketnumber;
                                regFaculty.NET_studiedEducation = item.courseStudied;
                                regFaculty.NET_specialization = item.specialization;
                                regFaculty.NET_passedYear = item.passedYear;
                                regFaculty.NET_university = item.boardOrUniversity;
                                regFaculty.NET_place = item.placeOfEducation;
                                regFaculty.NET_facultyCertificate = item.certificate;
                            }
                        }
                    }

                    #endregion
                    var currentDate = DateTime.Now;


                    #region Faculty Profitional Information on 17-02-2020

                    faculty.DepartmentId = db.jntuh_college_faculty_registered.Where(
                            f => f.RegistrationNumber == regFaculty.RegistrationNumber)
                            .Select(s => s.DepartmentId)
                            .FirstOrDefault();

                    if (faculty.DepartmentId != null)
                    {
                        regFaculty.department = db.jntuh_department.Find(faculty.DepartmentId).departmentName;
                    }
                    regFaculty.DepartmentId = faculty.DepartmentId;
                    regFaculty.OtherDepartment = faculty.OtherDepartment;
                    List<jntuh_registered_faculty_experience> registered_faculty_experience =
                      db.jntuh_registered_faculty_experience.Where(
                          r =>
                              r.facultyId == regFaculty.id && r.createdBycollegeId != null)
                          .Select(s => s)
                          .ToList();

                    var facultyexperiance = registered_faculty_experience.Where(
                            r => r.facultyId == regFaculty.id && r.createdBycollegeId == regFaculty.CollegeId && r.facultyJoiningOrder != null)
                            .Select(s => s)
                            .ToList()
                            .LastOrDefault();
                    if (facultyexperiance != null)
                    {
                        faculty.DesignationId = facultyexperiance.facultyDesignationId;
                        if (faculty.DesignationId != null)
                        {
                            regFaculty.designation = db.jntuh_designation.Find(faculty.DesignationId).designation;
                        }
                        regFaculty.DesignationId = faculty.DesignationId;
                        regFaculty.OtherDesignation = facultyexperiance.OtherDesignation;
                        regFaculty.facultyDateOfAppointment =
                                UAAAS.Models.Utilities.MMDDYY2DDMMYY(facultyexperiance.facultyDateOfAppointment.ToString());
                        regFaculty.facultyAppointmentLetter = facultyexperiance.facultyJoiningOrder;
                        regFaculty.GrossSalary = facultyexperiance.facultySalary;
                        if (!string.IsNullOrEmpty(regFaculty.facultyDateOfAppointment))
                        {
                            int fromyear = Convert.ToInt32(regFaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[2]);
                            int frommonth = Convert.ToInt32(regFaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[1]);
                            int fromdate = Convert.ToInt32(regFaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[0]);

                            DateTime zeroTime = new DateTime(1, 1, 1);
                            DateTime olddate = new DateTime(fromyear, frommonth, fromdate);
                            DateTime curdate = DateTime.Now;

                            var Difference = curdate - olddate;
                            if (Difference.Days > 0)
                            {
                                int yyears = (zeroTime + Difference).Year - 1;
                                int months = (zeroTime + Difference).Month - 1;
                                int days = (zeroTime + Difference).Day;
                                regFaculty.showPresentCollegeExperiance = yyears + " Years " + months + " Months " + days + " Days";
                            }

                        }

                    }
                    //For The Experiance
                    var experiance =
                        registered_faculty_experience.Where(
                            e =>
                                e.createdBycollegeId != null && e.facultyId == regFaculty.id &&
                                e.facultyDateOfResignation != null && e.facultyDateOfResignation != null).Select(s => s).ToList();
                    int exyears = 0;
                    int exmonths = 0;
                    int exdays = 0;
                    foreach (var Experiencefaculty in experiance)
                    {
                        int fromyear = 0;
                        int frommonth = 0;
                        int fromdate = 0;
                        int year = 0;
                        int month = 0;
                        int date = 0;
                        if (Experiencefaculty.facultyDateOfAppointment != null)
                        {
                            fromyear = Convert.ToInt32(Experiencefaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[2]);
                            frommonth = Convert.ToInt32(Experiencefaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[0]);
                            fromdate = Convert.ToInt32(Experiencefaculty.facultyDateOfAppointment.ToString().Split(' ')[0].Split('/')[1]);
                        }



                        // Experiencefaculty.facultyDateOfResignation = UAAAS.Models.Utilities.MMDDYY2DDMMYY(Experiencefaculty.facultyDateOfResignation);
                        if (Experiencefaculty.facultyDateOfResignation != null)
                        {
                            year = Convert.ToInt32(Experiencefaculty.facultyDateOfResignation.ToString().Split(' ')[0].Split('/')[2]);
                            month = Convert.ToInt32(Experiencefaculty.facultyDateOfResignation.ToString().Split(' ')[0].Split('/')[0]);
                            date = Convert.ToInt32(Experiencefaculty.facultyDateOfResignation.ToString().Split(' ')[0].Split('/')[1]);
                        }

                        if (Experiencefaculty.facultyDateOfResignation != null && Experiencefaculty.facultyDateOfAppointment != null)
                        {
                            DateTime zeroTime = new DateTime(1, 1, 1);
                            DateTime olddate = new DateTime(fromyear, frommonth, fromdate);
                            DateTime curdate = new DateTime(year, month, date);

                            var Difference = curdate - olddate;

                            if (Difference.Days > 0)
                            {
                                int yyears = (zeroTime + Difference).Year - 1;
                                int months = (zeroTime + Difference).Month - 1;
                                int days = (zeroTime + Difference).Day;


                                exyears = exyears + yyears;
                                exmonths = exmonths + months;
                                exdays = exdays + days;
                            }
                        }
                    }
                    while (exdays >= 30)
                    {
                        exdays = exdays - 30;
                        exmonths++;
                    }

                    while (exmonths >= 12)
                    {
                        exmonths = exmonths - 12;
                        exyears++;
                    }
                    regFaculty.showTotalExperience = exyears + " Years " + exmonths + " Months " + exdays + " Days";
                    #endregion
                }
            }
            return View(regFaculty);
        }


        [HttpGet]
        public ActionResult ChangeUserName()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangeUserName(ChangeuserName model)
        {
            if (ModelState.IsValid)
            {

                int count = 0;
                string newPassword = "";

                var myaspnetuser = db.my_aspnet_users.Where(e => e.name == model.OldEmailId).Select(e => e.id).FirstOrDefault();
                db.Database.Connection.Close();

                var myaspnetuseristhere = db.my_aspnet_users.Where(e => e.name == model.NewEmailId).Select(e => e.id).FirstOrDefault();
                db.Database.Connection.Close();
                var mymembershipdata = db.my_aspnet_membership.Where(e => e.Email == model.OldEmailId).Select(e => e).FirstOrDefault();
                db.Database.Connection.Close();
                //my_aspnet_users my_aspnet_users =
                //    db.my_aspnet_users.Where(u => u.name == model.OldEmailId).Select(s => s).FirstOrDefault();

                var registraredfacultydata = db.jntuh_registered_faculty.Where(e => e.Email == model.OldEmailId && e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Mobile == model.MobileNo && e.DateOfBirth == model.DateofBirth).Select(e => e).FirstOrDefault();
                db.Database.Connection.Close();
                if (myaspnetuseristhere == 0)
                {

                    if (myaspnetuser != 0 && mymembershipdata != null && registraredfacultydata != null)
                    {
                        try
                        {

                            //string constr = "Data Source=10.10.10.5;user id=verification;password=jntu@123456;database=uaaas20171030;";
                            //string constr = ConfigurationManager.ConnectionStrings["uaaasDBContext"].ConnectionString;
                            string constr = "Data Source=10.10.10.16;user id=root;password=Jntu@123!@#123;database=uaaas207118;";
                            using (MySqlConnection con = new MySqlConnection(constr))
                            {
                                using (MySqlCommand cmd = new MySqlCommand("Update_Myaspnet_UserId", con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("_OldEmail", model.OldEmailId);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("_NewEmail", model.NewEmailId);
                                    con.Open();
                                    count = cmd.ExecuteNonQuery();
                                    con.Close();
                                }
                            }

                            //This Code Is Commented By Narayana 
                            //By Using Membership Update Method But only in my_aspnet_membership Table
                            //MembershipUser userchange;
                            //userchange = Membership.GetUser(model.OldEmailId);
                            //userchange.Email = model.NewEmailId;
                            ////userchange.UserName = model.NewEmailId;
                            //Membership.UpdateUser(userchange);

                            //int id =
                            //    db.my_aspnet_users.Where(u => u.name == model.OldEmailId)
                            //        .Select(s => s.id)
                            //        .FirstOrDefault();

                            ////var my_aspnet_usersupdate = db.my_aspnet_users.Where(u => u.name == model.OldEmailId)
                            ////    .Select(s => s)
                            ////    .FirstOrDefault();
                            //if (id!=0)
                            //{
                            //    //my_aspnet_users.name = model.NewEmailId;
                            //    my_aspnet_users.SelectedRole = "0";
                            //    my_aspnet_users.email = model.NewEmailId;
                            //    my_aspnet_users.name = my_aspnet_users.email;
                            //    //my_aspnet_users.
                            //    db.Entry(my_aspnet_users).State = EntityState.Modified;
                            //    db.SaveChanges();
                            //}                         
                            if (count > 0)
                            {
                                mymembershipdata.Email = model.NewEmailId;
                                db.Entry(mymembershipdata).State = EntityState.Modified;
                                db.SaveChanges();

                                registraredfacultydata.Email = model.NewEmailId;
                                db.Entry(registraredfacultydata).State = EntityState.Modified;
                                db.SaveChanges();
                                db.Database.Connection.Close();
                            }
                            //  db.SaveChanges();
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

                        var userdata = db.jntuh_registered_faculty.Where(e => e.Email == model.NewEmailId && e.RegistrationNumber.Trim() == model.RegistrationNumber.Trim() && e.PANNumber == model.PanNumber && e.Mobile == model.MobileNo && e.DateOfBirth == model.DateofBirth).Select(e => e).FirstOrDefault();
                        if (userdata != null && count > 0)
                        {
                            newPassword = Membership.Provider.ResetPassword(model.NewEmailId, null);
                            //var jntuh_changeuserid = db.jntuh_changeuserid.AsNoTracking().ToList();
                            //var changeuserId =jntuh_changeuserid.Where(e => e.UserId == userdata.UserId).Select(e => e.Id).FirstOrDefault();
                            //if (changeuserId != 0)
                            //{
                            //    var updatechangeUserid = db.jntuh_changeuserid.Find(changeuserId);
                            //    updatechangeUserid.RegistrationNumber = userdata.RegistrationNumber;
                            //    updatechangeUserid.OldEmailId = model.OldEmailId;
                            //    updatechangeUserid.NewEmailId = model.NewEmailId;
                            //    updatechangeUserid.UserId = userdata.UserId;
                            //    updatechangeUserid.IsActive = true;
                            //    updatechangeUserid.UpdatedBy = 1;
                            //    updatechangeUserid.UpdatedOn = DateTime.Now;
                            //    db.Entry(updatechangeUserid).State = EntityState.Modified;
                            //    db.SaveChanges();
                            //}
                            //else
                            //{

                            jntuh_changeuserid changeUserIdtbl = new jntuh_changeuserid();
                            changeUserIdtbl.RegistrationNumber = userdata.RegistrationNumber;
                            changeUserIdtbl.OldEmailId = model.OldEmailId;
                            changeUserIdtbl.NewEmailId = model.NewEmailId;
                            changeUserIdtbl.UserId = userdata.UserId;
                            changeUserIdtbl.IsActive = true;
                            changeUserIdtbl.CreatedBy = 1;
                            changeUserIdtbl.CreatedOn = DateTime.Now;
                            db.jntuh_changeuserid.Add(changeUserIdtbl);
                            db.SaveChanges();
                            db.Database.Connection.Close();
                            //}

                        }

                        if (newPassword != "")
                        {
                            my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.NewEmailId).ProviderUserKey);
                            //send email
                            IUserMailer mailer = new UserMailer();
                            //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                            //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                            //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                            //              .SendAsync();

                            //mailer.Welcome(my_aspnet_membership.Email, "LoginInformation",
                            //    "JNTUH Account password reset",
                            //    model.NewEmailId, newPassword, string.Empty, string.Empty).SendAsync();


                            string MailBody = string.Empty;
                            MailBody += "<p>Dear " + my_aspnet_membership.Email + "</p><br/>";
                            MailBody += "<p>You can now login to www.jntuhaac.in with the following credentials.</p>";
                            MailBody += "<p><b>UserName: " + model.NewEmailId + "</b></p>";
                            MailBody += "<p><b>Password: " + newPassword + "</b></p><br/>";
                            MailBody += "<p>Thanks & Regards</p>";
                            MailBody += "<p>Director, AAC,</p>";
                            MailBody += "<p>JNTUH, Hyderabad</p>";


                            MailMessage message = new MailMessage();
                            message.To.Add(my_aspnet_membership.Email);
                            message.Subject = "JNTUH Account Change UserId";
                            message.Body = MailBody;
                            message.IsBodyHtml = true;
                            //  message.Attachments.Add(new Attachment(filepath));
                            //  message.Attachments.Add(new Attachment(filepathsecond));
                            var smtp = new SmtpClient();
                            smtp.Credentials = new NetworkCredential("supportaac@jntuh.ac.in", "uaaac@aac");
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 587;
                            smtp.EnableSsl = true;
                            smtp.Send(message);



                            //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();

                            string[] email = my_aspnet_membership.Email.Split('@');
                            string maskEmail = email[0].Substring(0, 2) +
                                               new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                               email[0].Substring(email[0].Length - 1, 1);


                            TempData["Success"] = "UserId and Password has been reset & sent successfully to " +
                                                  maskEmail + "@" + email[1];
                        }
                        else
                        {
                            TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                        }
                        return RedirectToAction("ChangeUserName", "Account");
                    }
                    else
                    {
                        TempData["Error"] = "Details Doesn't Match,please try again";
                        return RedirectToAction("ChangeUserName", "Account");
                    }
                }
                else
                {
                    TempData["Error"] = "Your New Email Address Already Exist";
                    return RedirectToAction("ChangeUserName", "Account");
                }
            }
            else
            {
                TempData["Error"] = "Please Provide All Mandatory Fields.";
                return RedirectToAction("ChangeUserName", "Account");
            }
        }


        [HttpPost]
        public JsonResult CheckEmail(string NewEmailId)
        {
            string CheckingEmail = db.my_aspnet_users.Where(e => e.name == NewEmailId.Trim()).Select(e => e.name).FirstOrDefault();
            if (!string.IsNullOrEmpty(CheckingEmail))
            {
                if (CheckingEmail.Trim() == NewEmailId.Trim())
                    return Json(false);
                else
                    return Json(true);
            }
            else
                return Json(true);

        }


        [HttpGet]
        public ActionResult ResetCollegePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetCollegePassword(ResteCollegePassword model)
        {
            if (ModelState.IsValid)
            {

                string newPassword = "";

                try
                {
                    var userdata = db.my_aspnet_users.Where(e => e.name == model.UserName).Select(e => e.id).FirstOrDefault();


                    if (userdata != 0)
                    {
                        newPassword = Membership.Provider.ResetPassword(model.UserName, null);
                        //ResetPassword(model.UserName, null);
                    }
                    else
                    {
                        TempData["Error"] = "Password reset failed. Please reenter your correct values and try again.";
                        return View(model);
                    }

                }
                catch (NotSupportedException e)
                {
                    TempData["Error"] = "An error has occurred resetting your password: " + e.Message + "." +
                                        "Please check your values and try again.";
                }
                catch (MembershipPasswordException e)
                {
                    TempData["Error"] = "Invalid password answer. Please reenter the answer and try again.";
                }
                catch (System.Configuration.Provider.ProviderException e)
                {
                    TempData["Error"] = "The specified user name does not exist. Please check your value and try again.";
                }

                if (newPassword != "")
                {
                    my_aspnet_membership my_aspnet_membership = db.my_aspnet_membership.Find(Membership.GetUser(model.UserName).ProviderUserKey);
                    //send email
                    IUserMailer mailer = new UserMailer();
                    //mailer.Welcome(my_aspnet_membership.Email, "PasswordReset", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword),
                    //              QueryStringEncryption.Encryption64.Encrypt(model.UserName, WebConfigurationManager.AppSettings["CryptoKey"]),
                    //              QueryStringEncryption.Encryption64.Encrypt(Server.HtmlEncode(newPassword), WebConfigurationManager.AppSettings["CryptoKey"]))
                    //              .SendAsync();

                    mailer.Welcome(my_aspnet_membership.Email, "LoginInformation", "JNTUH Account password reset",
                        model.UserName, newPassword, string.Empty, string.Empty).SendAsync();
                    //mailer.Welcome("aac.do.not.reply@gmail.com", "LoginInformation", "JNTUH Account password reset", model.UserName, Server.HtmlEncode(newPassword), string.Empty, string.Empty).SendAsync();

                    string[] email = my_aspnet_membership.Email.Split('@');
                    string maskEmail = email[0].Substring(0, 2) +
                                       new string('*', email[0].Substring(2, email[0].Length - 2).Length - 1) +
                                       email[0].Substring(email[0].Length - 1, 1);


                    TempData["Success"] = "Password has been reset & sent successfully to " + maskEmail + "@" + email[1];
                }
                else
                {
                    TempData["Error"] = "Password reset failed. Please reenter your values and try again.";
                }
            }
            return RedirectToAction("ResetCollegePassword");
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult getRegistationNumbers(FacultyRegistration regno)
        {
            if (!string.IsNullOrEmpty(regno.GetPanNumber))
            {
                string PanNumber = regno.GetPanNumber.ToUpper();
                List<FacultyRegistration> faculty = db.jntuh_registered_faculty.Where(f => f.PANNumber == PanNumber).Select(f => new FacultyRegistration
                {
                    FirstName = f.FirstName,
                    MiddleName = f.MiddleName,
                    LastName = f.LastName,
                    RegistrationNumber = f.RegistrationNumber,
                    id = f.id,
                    BlacklistFaculty = f.Blacklistfaculy,
                    VerificationStatus = f.AbsentforVerification
                }).ToList();

                ViewBag.ListOfRegistrations = faculty;



                if (ViewBag.ListOfRegistrations != null)
                {
                    return View("FindRegistrationNumber");
                }


                if (ViewBag.ListOfRegistrations == null)
                {
                    TempData["Error"] = "No Registrations found...";
                    return RedirectToAction("FindRegistrationNumber");
                }
            }
            else if (!string.IsNullOrEmpty(regno.GetAadhaarNumber))
            {
                string AadhaarNumber = regno.GetAadhaarNumber;
                List<FacultyRegistration> GetAadhaarBasedDetails = db.jntuh_registered_faculty.Where(f => f.AadhaarNumber == AadhaarNumber)
                    .Select(e => new FacultyRegistration
                    {
                        FirstName = e.FirstName,
                        MiddleName = e.MiddleName,
                        LastName = e.LastName,
                        RegistrationNumber = e.RegistrationNumber,
                        id = e.id

                    }).ToList();
                //var collegeFacultyReg =
                //    db.jntuh_college_faculty_registered.Where(f => f.AadhaarNumber == AadhaarNumber).ToList();
                //foreach (var clg in collegeFacultyReg)
                //{
                //    var details =
                //        db.jntuh_registered_faculty.FirstOrDefault(f => f.RegistrationNumber == clg.RegistrationNumber);
                //    GetAadhaarBasedDetails.Add(new FacultyRegistration()
                //    {
                //        FirstName = details.FirstName,
                //        MiddleName = details.MiddleName,
                //        LastName = details.LastName,
                //        RegistrationNumber = details.RegistrationNumber,
                //        id = details.id
                //    });
                //}


                ViewBag.GetAadhaarBasedDetails = GetAadhaarBasedDetails;
                if (ViewBag.GetAadhaarBasedDetails != null)
                {
                    return View("FindRegistrationNumber");
                }
                else
                {
                    TempData["Error"] = "No Registrations found...";
                    return RedirectToAction("FindRegistrationNumber");
                }
            }
            return RedirectToAction("FindRegistrationNumber");

        }

        [HttpGet]
        public ActionResult FacultyEducationDetails()
        {
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            return View(FacultyRegistration);
        }
        [HttpPost]
        public ActionResult FacultyEducationDetails(FacultyRegistration regno)
        {
            int fID = 0;
            try
            {
                if (regno.RegistrationNumber != null)
                {
                    fID = db.jntuh_registered_faculty.Where(a => a.RegistrationNumber == regno.RegistrationNumber).Select(a => a.id).FirstOrDefault();
                    if (fID != 0)
                    {
                        regno.FacultyEducation = db.jntuh_education_category.Where(e => e.isActive == true && (e.id == 1 || e.id == 3 || e.id == 4 || e.id == 5 || e.id == 6)).Select(e => new RegisteredFacultyEducation
                        {
                            educationId = e.id,
                            educationName = e.educationCategoryName,
                            studiedEducation = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.courseStudied).FirstOrDefault(),
                            specialization = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.specialization).FirstOrDefault(),
                            passedYear = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.passedYear).FirstOrDefault(),
                            percentage = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.marksPercentage).FirstOrDefault(),
                            division = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.division).FirstOrDefault(),
                            university = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.boardOrUniversity).FirstOrDefault(),
                            place = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.placeOfEducation).FirstOrDefault(),
                            facultyCertificate = db.jntuh_registered_faculty_education.Where(fe => fe.educationId == e.id && fe.facultyId == fID).Select(fe => fe.certificate).FirstOrDefault(),
                        }).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View(regno);
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public ActionResult getCollegeName(FacultyRegistration registrationno)
        {
            string regno = registrationno.RegistrationNumber.Trim();
            try
            {
                if (regno != string.Empty)
                {
                    TempData["CollegeCode"] = string.Empty;
                    TempData["FirstName"] = string.Empty;
                    TempData["CollegeName"] = string.Empty;
                    TempData["Registration"] = string.Empty;
                    TempData["InActive"] = string.Empty;
                    FacultyRegistration reglist = new FacultyRegistration();
                    jntuh_registered_faculty jntuh_registered_faculty =
                        db.jntuh_registered_faculty.Where(rf => rf.RegistrationNumber == regno.Trim())
                            .Select(s => s)
                            .FirstOrDefault();
                    string Reason = null;
                    if (jntuh_registered_faculty != null)
                    {
                        if (jntuh_registered_faculty.Absent == true)
                            Reason += "Absent";
                        if (jntuh_registered_faculty.type == "Adjunct")
                        {
                            if (Reason != null)
                                Reason += ",Adjunct Faculty";
                            else
                                Reason += "Adjunct Faculty";
                        }

                        if (jntuh_registered_faculty.Xeroxcopyofcertificates == true)
                        {
                            if (Reason != null)
                                Reason += ",Xerox copyof certificates";
                            else
                                Reason += "Xerox copyof certificates";
                        }

                        if (jntuh_registered_faculty.NoRelevantUG == "Yes")
                        {
                            if (Reason != null)
                                Reason += ",NO Relevant UG";
                            else
                                Reason += "NO Relevant UG";
                        }

                        if (jntuh_registered_faculty.NoRelevantPG == "Yes")
                        {
                            if (Reason != null)
                                Reason += ",NO Relevant PG";
                            else
                                Reason += "NO Relevant PG";
                        }

                        if (jntuh_registered_faculty.NORelevantPHD == "Yes")
                        {
                            if (Reason != null)
                                Reason += ",NO Relevant PHD";
                            else
                                Reason += "NO Relevant PHD";
                        }

                        if (jntuh_registered_faculty.NotQualifiedAsperAICTE == true)
                        {
                            if (Reason != null)
                                Reason += ",NOT Qualified AsPer AICTE/PCI";
                            else
                                Reason += "NOT Qualified AsPer AICTE/PCI";
                        }

                        if (jntuh_registered_faculty.InvalidPANNumber == true)
                        {
                            if (Reason != null)
                                Reason += ",InvalidPANNumber";
                            else
                                Reason += "InvalidPANNumber";
                        }

                        if (jntuh_registered_faculty.IncompleteCertificates == true)
                        {
                            if (Reason != null)
                                Reason += ",InComplete Ceritificates";
                            else
                                Reason += "InComplete Ceritificates";
                        }

                        if (jntuh_registered_faculty.NoSCM == true)
                        {
                            if (Reason != null)
                                Reason += ",NoSCM";
                            else
                                Reason += "NoSCM";
                        }

                        if (jntuh_registered_faculty.OriginalCertificatesNotShown == true)
                        {
                            if (Reason != null)
                                Reason += ",Original Certificates notshown";
                            else
                                Reason += "Original Certificates notshown";
                        }

                        if (jntuh_registered_faculty.PANNumber == null)
                        {
                            if (Reason != null)
                                Reason += ",No PANNumber";
                            else
                                Reason += "No PANNumber";
                        }
                        if (jntuh_registered_faculty.NotconsideredPHD == true)
                        {
                            if (Reason != null)
                                Reason += ",Cosidered as a faculty but not cosidered his/her P.hD";
                            else
                                Reason += "Cosidered as a faculty but not cosidered his/her P.hD";
                        }
                        if (jntuh_registered_faculty.NoPGspecialization == true)
                        {
                            if (Reason != null)
                                Reason += ",no Specialization in PG";
                            else
                                Reason += "no Specialization in PG";
                        }
                        if (jntuh_registered_faculty.Genuinenessnotsubmitted == true)
                        {
                            if (Reason != null)
                                Reason += ",PHD Genuinity not Submitted";
                            else
                                Reason += "PHD Genuinity not Submitted";
                        }

                        if (jntuh_registered_faculty.OriginalsVerifiedUG == true)
                        {
                            if (Reason != null)
                                Reason += ",Complaint PHD Faculty";
                            else
                                Reason += "Complaint PHD Faculty";
                        }

                        if (jntuh_registered_faculty.OriginalsVerifiedPHD == true)
                        {
                            if (Reason != null)
                                Reason += ",No Guide Sign in PHD Thesis";
                            else
                                Reason += "No Guide Sign in PHD Thesis";
                        }
                        if (jntuh_registered_faculty.Noclass == true)
                        {
                            if (Reason != null)
                                Reason += ",No Class in UG/PG";
                            else
                                Reason += "No Class in UG/PG";
                        }
                        if (jntuh_registered_faculty.Blacklistfaculy == true)
                        {
                            if (Reason != null)
                                Reason += ",Blacklistfaculy";
                            else
                                Reason += "Blacklistfaculy";
                        }
                        if (jntuh_registered_faculty.AbsentforVerification == true)
                        {
                            if (Reason != null)
                                Reason += ",Absent for Physical Verification";
                            else
                                Reason += "Absent for Physical Verification";
                        }
                        if (!String.IsNullOrEmpty(Reason))
                        {
                            TempData["InActive"] = Reason;
                        }
                    }
                    int cid = db.jntuh_college_faculty_registered.Where(a => a.RegistrationNumber == regno.Trim()).Select(s => s.collegeId).FirstOrDefault();
                    if (cid != 0)
                    {
                        TempData["CollegeCode"] = db.jntuh_college.Where(a => a.id == cid).Select(c => c.collegeCode).FirstOrDefault();
                        TempData["CollegeName"] = db.jntuh_college.Where(a => a.id == cid).Select(c => c.collegeName).FirstOrDefault();
                        TempData["Registration"] = regno;
                        TempData["FirstName"] = jntuh_registered_faculty.FirstName + " " +
                                                jntuh_registered_faculty.MiddleName + " " +
                                                jntuh_registered_faculty.LastName;
                    }
                    else
                    {
                        int pid = db.jntuh_college_principal_registered.Where(a => a.RegistrationNumber == regno.Trim()).Select(s => s.collegeId).FirstOrDefault();
                        if (pid != 0)
                        {
                            TempData["CollegeCode"] = db.jntuh_college.Where(a => a.id == pid).Select(c => c.collegeCode).FirstOrDefault();
                            TempData["CollegeName"] = db.jntuh_college.Where(a => a.id == pid).Select(c => c.collegeName).FirstOrDefault();
                            TempData["Registration"] = regno;
                            TempData["FirstName"] = jntuh_registered_faculty.FirstName + " " +
                                                    jntuh_registered_faculty.MiddleName + " " +
                                                    jntuh_registered_faculty.LastName + " (Principal) ";
                        }
                        else
                        {
                            TempData["nocollege"] = "No College Assocation...";
                            return RedirectToAction("FindRegistrationNumber");
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return RedirectToAction("FindRegistrationNumber");
        }

        //Adding Delete Faculty
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult AddDeleteFaculty()
        {
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            return View(FacultyRegistration);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddDeleteFaculty(FacultyRegistration regno)
        {
            if (String.IsNullOrEmpty(regno.RegistrationNumber))
            {
                ViewBag.Error = "Please Enter Registration Number.";
                return View(regno);
            }
            int facultyId =
                db.jntuh_registered_faculty.Where(f => f.RegistrationNumber == regno.RegistrationNumber)
                    .Select(s => s.id)
                    .FirstOrDefault();
            if (facultyId != 0)
            {
                ViewBag.Error = "Faculty Registration Number Exists.";
                return View(regno);
            }
            //int facultyId = 0;
            int newUserId = 0;
            int newfacultyId = 0;
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            string constr = "Data Source=10.10.10.5;user id=verification;password=jntu@123456;database=uaaas207118;";
            string query = "SELECT * FROM jntuh_registered_faculty WHERE RegistrationNumber='" + regno.RegistrationNumber.Trim() + "'";
            MySqlConnection con = new MySqlConnection(constr);
            con.Open();
            MySqlCommand cmd = new MySqlCommand(query, con);
            //if(con.State==true)
            //con
            MySqlDataReader dr = cmd.ExecuteReader();
            jntuh_registered_faculty oldjntuh_registered_faculty = new jntuh_registered_faculty();
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    oldjntuh_registered_faculty.UniqueID = string.Empty;
                    oldjntuh_registered_faculty.UserId = dr["UserId"] == DBNull.Value ? oldjntuh_registered_faculty.UserId : Convert.ToInt32(dr["UserId"]);
                    facultyId = dr["Id"] == DBNull.Value ? facultyId : Convert.ToInt32(dr["Id"]);
                    oldjntuh_registered_faculty.type = Convert.ToString(dr["type"]);
                    oldjntuh_registered_faculty.collegeId = dr["collegeId"] == DBNull.Value ? oldjntuh_registered_faculty.collegeId : Convert.ToInt32(dr["collegeId"]);
                    oldjntuh_registered_faculty.RegistrationNumber = Convert.ToString(dr["RegistrationNumber"]);
                    //oldjntuh_registered_faculty.UserId =;
                    oldjntuh_registered_faculty.FirstName = Convert.ToString(dr["FirstName"]);
                    oldjntuh_registered_faculty.MiddleName = Convert.ToString(dr["MiddleName"]);
                    oldjntuh_registered_faculty.LastName = Convert.ToString(dr["LastName"]);
                    oldjntuh_registered_faculty.GenderId = dr["GenderId"] == DBNull.Value ? oldjntuh_registered_faculty.GenderId : Convert.ToInt32(dr["GenderId"]);
                    oldjntuh_registered_faculty.FatherOrHusbandName = Convert.ToString(dr["FatherOrHusbandName"]);
                    oldjntuh_registered_faculty.MotherName = Convert.ToString(dr["MotherName"]);
                    oldjntuh_registered_faculty.DateOfBirth = dr["DateOfBirth"] == DBNull.Value ? oldjntuh_registered_faculty.DateOfBirth : Convert.ToDateTime(dr["DateOfBirth"]);
                    oldjntuh_registered_faculty.WorkingStatus = dr["WorkingStatus"] == DBNull.Value ? oldjntuh_registered_faculty.WorkingStatus : Convert.ToBoolean(dr["WorkingStatus"]);
                    oldjntuh_registered_faculty.OrganizationName = Convert.ToString(dr["OrganizationName"]);
                    oldjntuh_registered_faculty.DesignationId = dr["DesignationId"] == DBNull.Value ? oldjntuh_registered_faculty.DesignationId : Convert.ToInt32(dr["DesignationId"]);
                    oldjntuh_registered_faculty.OtherDesignation = Convert.ToString(dr["OtherDesignation"]);
                    oldjntuh_registered_faculty.DepartmentId = dr["DepartmentId"] == DBNull.Value ? oldjntuh_registered_faculty.DepartmentId : Convert.ToInt32(dr["DepartmentId"]);
                    oldjntuh_registered_faculty.OtherDepartment = Convert.ToString(dr["OtherDepartment"]);
                    oldjntuh_registered_faculty.grosssalary = Convert.ToString(dr["grosssalary"]);
                    oldjntuh_registered_faculty.DateOfAppointment = Convert.ToDateTime(dr["DateOfAppointment"]);
                    oldjntuh_registered_faculty.isFacultyRatifiedByJNTU = dr["isFacultyRatifiedByJNTU"] == DBNull.Value ? oldjntuh_registered_faculty.isFacultyRatifiedByJNTU : Convert.ToBoolean(dr["isFacultyRatifiedByJNTU"]);
                    oldjntuh_registered_faculty.DateOfRatification = dr["DateOfRatification"] == DBNull.Value ? oldjntuh_registered_faculty.DateOfRatification : Convert.ToDateTime(dr["DateOfRatification"]);
                    oldjntuh_registered_faculty.ProceedingsNumber = Convert.ToString(dr["ProceedingsNumber"]);
                    oldjntuh_registered_faculty.ProceedingDocument = Convert.ToString(dr["ProceedingDocument"]);
                    oldjntuh_registered_faculty.AICTEFacultyId = Convert.ToString(dr["AICTEFacultyId"]);
                    oldjntuh_registered_faculty.TotalExperience = dr["TotalExperience"] == DBNull.Value ? oldjntuh_registered_faculty.TotalExperience : Convert.ToInt32(dr["TotalExperience"]);
                    oldjntuh_registered_faculty.TotalExperiencePresentCollege = dr["TotalExperiencePresentCollege"] == DBNull.Value ? oldjntuh_registered_faculty.TotalExperiencePresentCollege : Convert.ToInt32(dr["TotalExperiencePresentCollege"]);
                    oldjntuh_registered_faculty.PANNumber = Convert.ToString(dr["PANNumber"]);
                    oldjntuh_registered_faculty.PanStatus = Convert.ToString(dr["PanStatus"]);
                    oldjntuh_registered_faculty.AadhaarNumber = Convert.ToString(dr["AadhaarNumber"]);
                    oldjntuh_registered_faculty.Mobile = Convert.ToString(dr["Mobile"]);
                    oldjntuh_registered_faculty.Email = Convert.ToString(dr["Email"]);
                    oldjntuh_registered_faculty.National = Convert.ToString(dr["National"]);
                    oldjntuh_registered_faculty.InterNational = Convert.ToString(dr["InterNational"]);
                    oldjntuh_registered_faculty.Citation = Convert.ToString(dr["Citation"]);
                    oldjntuh_registered_faculty.Awards = Convert.ToString(dr["Awards"]);
                    oldjntuh_registered_faculty.Photo = Convert.ToString(dr["Photo"]);
                    oldjntuh_registered_faculty.PANDocument = Convert.ToString(dr["PANDocument"]);
                    oldjntuh_registered_faculty.AadhaarDocument = Convert.ToString(dr["AadhaarDocument"]);
                    oldjntuh_registered_faculty.isActive = dr["isActive"] == DBNull.Value ? oldjntuh_registered_faculty.isActive : Convert.ToBoolean(dr["isActive"]);
                    oldjntuh_registered_faculty.InStatus = Convert.ToString(dr["InStatus"]);

                    oldjntuh_registered_faculty.isApproved = dr["isApproved"] == DBNull.Value ? oldjntuh_registered_faculty.isApproved : Convert.ToBoolean(dr["isApproved"]);

                    oldjntuh_registered_faculty.createdOn = dr["createdOn"] == DBNull.Value ? oldjntuh_registered_faculty.createdOn : Convert.ToDateTime(dr["createdOn"]);
                    oldjntuh_registered_faculty.createdBy = dr["createdBy"] == DBNull.Value ? oldjntuh_registered_faculty.createdBy : Convert.ToInt32(dr["createdBy"]);
                    oldjntuh_registered_faculty.updatedOn = dr["updatedOn"] == DBNull.Value ? oldjntuh_registered_faculty.updatedOn : Convert.ToDateTime(dr["updatedOn"]);
                    oldjntuh_registered_faculty.updatedBy = dr["updatedBy"] == DBNull.Value ? oldjntuh_registered_faculty.updatedBy : Convert.ToInt32(dr["updatedBy"]);
                    oldjntuh_registered_faculty.DeactivationReason = dr["DeactivationReason"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivationReason : Convert.ToString(dr["DeactivationReason"]);

                    oldjntuh_registered_faculty.DeactivatedOn = dr["DeactivatedOn"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivatedOn : Convert.ToDateTime(dr["DeactivatedOn"]);

                    oldjntuh_registered_faculty.DeactivatedBy = dr["DeactivatedBy"] == DBNull.Value ? oldjntuh_registered_faculty.DeactivatedBy : Convert.ToInt32(dr["DeactivatedBy"]);
                    oldjntuh_registered_faculty.WorkingType = Convert.ToString(dr["WorkingType"]);
                    oldjntuh_registered_faculty.NOCFile = Convert.ToString(dr["NOCFile"]);
                    oldjntuh_registered_faculty.PresentInstituteAssignedRole = Convert.ToString(dr["PresentInstituteAssignedRole"]);
                    oldjntuh_registered_faculty.PresentInstituteAssignedResponsebility = Convert.ToString(dr["PresentInstituteAssignedResponsebility"]);
                    oldjntuh_registered_faculty.Accomplish1 = Convert.ToString(dr["Accomplish1"]);
                    oldjntuh_registered_faculty.Accomplish2 = Convert.ToString(dr["Accomplish2"]);
                    oldjntuh_registered_faculty.Accomplish3 = Convert.ToString(dr["Accomplish3"]);
                    oldjntuh_registered_faculty.Accomplish4 = Convert.ToString(dr["Accomplish4"]);
                    oldjntuh_registered_faculty.Accomplish5 = Convert.ToString(dr["Accomplish5"]);
                    oldjntuh_registered_faculty.Professional = Convert.ToString(dr["Professional"]);
                    oldjntuh_registered_faculty.Professional2 = Convert.ToString(dr["Professional2"]);
                    oldjntuh_registered_faculty.Professiona3 = Convert.ToString(dr["Professiona3"]);
                    oldjntuh_registered_faculty.MembershipNo1 = Convert.ToString(dr["MembershipNo1"]);
                    oldjntuh_registered_faculty.MembershipNo2 = Convert.ToString(dr["MembershipNo2"]);
                    oldjntuh_registered_faculty.MembershipNo3 = Convert.ToString(dr["MembershipNo3"]);
                    oldjntuh_registered_faculty.MembershipCertificate1 = Convert.ToString(dr["MembershipCertificate1"]);
                    oldjntuh_registered_faculty.MembershipCertificate2 = Convert.ToString(dr["MembershipCertificate2"]);
                    oldjntuh_registered_faculty.MembershipCertificate3 = Convert.ToString(dr["MembershipCertificate3"]);
                    oldjntuh_registered_faculty.AdjunctDesignation = Convert.ToString(dr["AdjunctDesignation"]);
                    oldjntuh_registered_faculty.AdjunctDepartment = Convert.ToString(dr["AdjunctDepartment"]);
                    oldjntuh_registered_faculty.PanVerificationStatus = Convert.ToString(dr["PanVerificationStatus"]);
                    oldjntuh_registered_faculty.PanDeactivationReason = Convert.ToString(dr["PanDeactivationReason"]);

                    oldjntuh_registered_faculty.Absent = dr["Absent"] == DBNull.Value ? oldjntuh_registered_faculty.Absent : Convert.ToBoolean(dr["Absent"]);


                    oldjntuh_registered_faculty.ModifiedPANNumber = Convert.ToString(dr["ModifiedPANNumber"]);
                    oldjntuh_registered_faculty.InvalidPANNumber = dr["InvalidPANNumber"] == DBNull.Value ? oldjntuh_registered_faculty.InvalidPANNumber : Convert.ToBoolean(dr["InvalidPANNumber"]);
                    oldjntuh_registered_faculty.NoRelevantUG = Convert.ToString(dr["NoRelevantUG"]);
                    oldjntuh_registered_faculty.NoRelevantPG = Convert.ToString(dr["NoRelevantPG"]);
                    oldjntuh_registered_faculty.NORelevantPHD = Convert.ToString(dr["NORelevantPHD"]);
                    oldjntuh_registered_faculty.NoSCM = dr["NoSCM"] == DBNull.Value ? oldjntuh_registered_faculty.NoSCM : Convert.ToBoolean(dr["NoSCM"]);
                    oldjntuh_registered_faculty.NoForm16 = dr["NoForm16"] == DBNull.Value ? oldjntuh_registered_faculty.NoForm16 : Convert.ToBoolean(dr["NoForm16"]);
                    oldjntuh_registered_faculty.ModifiedDateofAppointment = dr["ModifiedDateofAppointment"] == DBNull.Value ? oldjntuh_registered_faculty.ModifiedDateofAppointment : Convert.ToDateTime(dr["ModifiedDateofAppointment"]);
                    oldjntuh_registered_faculty.NotQualifiedAsperAICTE = dr["NotQualifiedAsperAICTE"] == DBNull.Value ? oldjntuh_registered_faculty.NotQualifiedAsperAICTE : Convert.ToBoolean(dr["NotQualifiedAsperAICTE"]);
                    //oldjntuh_registered_faculty.MultipleRegInSameCollege =dr["MultipleRegInSameCollege"]==DBNull.Value?oldjntuh_registered_faculty.MultipleRegInSameCollege:Convert.ToBoolean(dr["MultipleRegInSameCollege"]);
                    //oldjntuh_registered_faculty.MultipleRegInDiffCollege =dr["MultipleRegInDiffCollege"]==DBNull.Value?oldjntuh_registered_faculty.MultipleRegInDiffCollege: Convert.ToBoolean(dr["MultipleRegInDiffCollege"]);
                    //oldjntuh_registered_faculty.SamePANUsedByMultipleFaculty =dr["SamePANUsedByMultipleFaculty"]==DBNull.Value?oldjntuh_registered_faculty.SamePANUsedByMultipleFaculty: Convert.ToBoolean(dr["SamePANUsedByMultipleFaculty"]);
                    //oldjntuh_registered_faculty.PhotoCopyofPAN =dr["PhotoCopyofPAN"]==DBNull.Value?oldjntuh_registered_faculty.PhotoCopyofPAN: Convert.ToBoolean(dr["PhotoCopyofPAN"]);
                    //oldjntuh_registered_faculty.AppliedPAN =dr["AppliedPAN"]==DBNull.Value?oldjntuh_registered_faculty.AppliedPAN :Convert.ToBoolean(dr["AppliedPAN"]);
                    //oldjntuh_registered_faculty.LostPAN =dr["LostPAN"]==DBNull.Value?oldjntuh_registered_faculty.LostPAN: Convert.ToBoolean(dr["LostPAN"]);
                    oldjntuh_registered_faculty.OriginalsVerifiedUG = dr["OriginalsVerifiedUG"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedUG : Convert.ToBoolean(dr["OriginalsVerifiedUG"]);
                    oldjntuh_registered_faculty.OriginalsVerifiedPG = dr["OriginalsVerifiedPG"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedPG : Convert.ToBoolean(dr["OriginalsVerifiedPG"]);
                    oldjntuh_registered_faculty.OriginalsVerifiedPHD = dr["OriginalsVerifiedPHD"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalsVerifiedPHD : Convert.ToBoolean(dr["OriginalsVerifiedPHD"]);
                    oldjntuh_registered_faculty.FacultyVerificationStatus = dr["FacultyVerificationStatus"] == DBNull.Value ? oldjntuh_registered_faculty.FacultyVerificationStatus : Convert.ToBoolean(dr["FacultyVerificationStatus"]);
                    oldjntuh_registered_faculty.Others1 = Convert.ToString(dr["Others1"]);
                    oldjntuh_registered_faculty.Others2 = Convert.ToString(dr["Others2"]);
                    oldjntuh_registered_faculty.IncompleteCertificates = dr["IncompleteCertificates"] == DBNull.Value ? oldjntuh_registered_faculty.IncompleteCertificates : Convert.ToBoolean(dr["IncompleteCertificates"]);
                    oldjntuh_registered_faculty.PanStatusAfterDE = Convert.ToString(dr["PanStatusAfterDE"]);
                    oldjntuh_registered_faculty.PanReasonAfterDE = Convert.ToString(dr["PanReasonAfterDE"]);
                    oldjntuh_registered_faculty.NoSpecialization = dr["NoSpecialization"] == DBNull.Value ? oldjntuh_registered_faculty.NoSpecialization : Convert.ToBoolean(dr["NoSpecialization"]);
                    oldjntuh_registered_faculty.FalsePAN = dr["FalsePAN"] == DBNull.Value ? oldjntuh_registered_faculty.FalsePAN : Convert.ToBoolean(dr["FalsePAN"]);
                    oldjntuh_registered_faculty.Notin116 = dr["Notin116"] == DBNull.Value ? oldjntuh_registered_faculty.Notin116 : Convert.ToBoolean(dr["Notin116"]);
                    oldjntuh_registered_faculty.PHDundertakingnotsubmitted = dr["PHDundertakingnotsubmitted"] == DBNull.Value ? oldjntuh_registered_faculty.PHDundertakingnotsubmitted : Convert.ToBoolean(dr["PHDundertakingnotsubmitted"]);
                    oldjntuh_registered_faculty.Blacklistfaculy = dr["Blacklistfaculy"] == DBNull.Value ? oldjntuh_registered_faculty.Blacklistfaculy : Convert.ToBoolean(dr["Blacklistfaculy"]);
                    oldjntuh_registered_faculty.DiscrepencyStatus = dr["DiscrepencyStatus"] == DBNull.Value ? oldjntuh_registered_faculty.DiscrepencyStatus : Convert.ToBoolean(dr["DiscrepencyStatus"]);
                    oldjntuh_registered_faculty.NoPhdUndertakingNew = dr["NoPhdUndertakingNew"] == DBNull.Value ? oldjntuh_registered_faculty.NoPhdUndertakingNew : Convert.ToBoolean(dr["NoPhdUndertakingNew"]);
                    oldjntuh_registered_faculty.IncometaxDocument = Convert.ToString(dr["IncometaxDocument"]);
                    oldjntuh_registered_faculty.PGSpecialization = dr["PGSpecialization"] == DBNull.Value ? oldjntuh_registered_faculty.PGSpecialization : Convert.ToInt32(dr["PGSpecialization"]);
                    oldjntuh_registered_faculty.PHDUndertakingDocument = Convert.ToString(dr["PHDUndertakingDocument"]);
                    //oldjntuh_registered_faculty.BASStatus = Convert.ToString(dr["BASStatus"]);
                    //oldjntuh_registered_faculty.BASStatusOld = Convert.ToString(dr["BASStatusOld"]);
                    oldjntuh_registered_faculty.OriginalCertificatesNotShown = dr["OriginalCertificatesNotShown"] == DBNull.Value ? oldjntuh_registered_faculty.OriginalCertificatesNotShown : Convert.ToBoolean(dr["OriginalCertificatesNotShown"]);
                    oldjntuh_registered_faculty.PGSpecializationRemarks = Convert.ToString(dr["PGSpecializationRemarks"]);
                    oldjntuh_registered_faculty.Xeroxcopyofcertificates = dr["Xeroxcopyofcertificates"] == DBNull.Value ? oldjntuh_registered_faculty.Xeroxcopyofcertificates : Convert.ToBoolean(dr["Xeroxcopyofcertificates"]);
                    oldjntuh_registered_faculty.PhdUndertakingDocumentstatus = dr["PhdUndertakingDocumentstatus"] == DBNull.Value ? oldjntuh_registered_faculty.PhdUndertakingDocumentstatus : Convert.ToBoolean(dr["PhdUndertakingDocumentstatus"]);
                    oldjntuh_registered_faculty.PhdUndertakingDocumentText = Convert.ToString(dr["PhdUndertakingDocumentText"]);
                    oldjntuh_registered_faculty.NotIdentityfiedForanyProgram = dr["NotIdentityfiedForanyProgram"] == DBNull.Value ? oldjntuh_registered_faculty.NotIdentityfiedForanyProgram : Convert.ToBoolean(dr["NotIdentityfiedForanyProgram"]);
                    oldjntuh_registered_faculty.NoSCM17 = dr["NoSCM17"] == DBNull.Value ? oldjntuh_registered_faculty.NoSCM17 : Convert.ToBoolean(dr["NoSCM17"]);
                    //oldjntuh_registered_faculty.Noform16Verification =dr["Noform16Verification"]==DBNull.Value?oldjntuh_registered_faculty.Noform16Verification: Convert.ToBoolean(dr["Noform16Verification"]);
                    oldjntuh_registered_faculty.PhdDeskVerification = dr["PhdDeskVerification"] == DBNull.Value ? oldjntuh_registered_faculty.PhdDeskVerification : Convert.ToBoolean(dr["PhdDeskVerification"]);
                    oldjntuh_registered_faculty.PhdDeskReason = Convert.ToString(dr["PhdDeskReason"]);
                    oldjntuh_registered_faculty.ACollegeId = dr["ACollegeId"] == DBNull.Value ? oldjntuh_registered_faculty.ACollegeId : Convert.ToInt32(dr["ACollegeId"]);
                    oldjntuh_registered_faculty.ADepartmentId = dr["ADepartmentId"] == DBNull.Value ? oldjntuh_registered_faculty.ADepartmentId : Convert.ToInt32(dr["ADepartmentId"]);
                    oldjntuh_registered_faculty.Jntu_PGSpecializationId = dr["ASpecializationId"] == DBNull.Value ? oldjntuh_registered_faculty.Jntu_PGSpecializationId : Convert.ToInt32(dr["ASpecializationId"]);
                    oldjntuh_registered_faculty.AIdentifiedFor = Convert.ToString(dr["AIdentifiedFor"]);

                }
            }
            con.Close();

            con.Open();
            List<jntuh_registered_faculty_education> jntuh_registered_faculty_educationList = new List<jntuh_registered_faculty_education>();
            query = string.Empty;
            query = "SELECT * FROM jntuh_registered_faculty_education WHERE facultyId=" + facultyId + "";
            MySqlCommand cmdmembership = new MySqlCommand(query, con);
            MySqlDataReader drm = cmdmembership.ExecuteReader();
            if (drm.HasRows)
            {
                while (drm.Read())
                {
                    jntuh_registered_faculty_education oldjntuh_registered_faculty_education = new jntuh_registered_faculty_education();
                    oldjntuh_registered_faculty_education.facultyId = drm["facultyId"] == DBNull.Value ? oldjntuh_registered_faculty_education.facultyId : Convert.ToInt32(drm["facultyId"]);
                    oldjntuh_registered_faculty_education.educationId = drm["educationId"] == DBNull.Value ? oldjntuh_registered_faculty_education.educationId : Convert.ToInt32(drm["educationId"]);
                    oldjntuh_registered_faculty_education.courseStudied = Convert.ToString(drm["courseStudied"].ToString());
                    oldjntuh_registered_faculty_education.specialization = Convert.ToString(drm["specialization"]);
                    oldjntuh_registered_faculty_education.passedYear = drm["passedYear"] == DBNull.Value ? oldjntuh_registered_faculty_education.passedYear : Convert.ToInt32(drm["passedYear"]);
                    oldjntuh_registered_faculty_education.marksPercentage = drm["marksPercentage"] == DBNull.Value ? oldjntuh_registered_faculty_education.marksPercentage : Convert.ToDecimal(drm["marksPercentage"]);
                    oldjntuh_registered_faculty_education.division = drm["division"] == DBNull.Value ? oldjntuh_registered_faculty_education.division : Convert.ToInt32(drm["division"]);
                    oldjntuh_registered_faculty_education.boardOrUniversity = Convert.ToString(drm["boardOrUniversity"]);
                    oldjntuh_registered_faculty_education.placeOfEducation = Convert.ToString(drm["placeOfEducation"]);
                    oldjntuh_registered_faculty_education.certificate = Convert.ToString(drm["certificate"]);
                    oldjntuh_registered_faculty_education.isActive = drm["isActive"] == DBNull.Value ? oldjntuh_registered_faculty_education.isActive : Convert.ToBoolean(drm["isActive"]);
                    oldjntuh_registered_faculty_education.createdOn = drm["createdOn"] == DBNull.Value ? oldjntuh_registered_faculty_education.createdOn : Convert.ToDateTime(drm["createdOn"]);
                    oldjntuh_registered_faculty_education.createdBy = 1;
                    oldjntuh_registered_faculty_education.updatedOn = drm["updatedOn"] == DBNull.Value ? oldjntuh_registered_faculty_education.createdOn : Convert.ToDateTime(drm["createdOn"]);
                    oldjntuh_registered_faculty_education.createdBy = 1;
                    jntuh_registered_faculty_educationList.Add(oldjntuh_registered_faculty_education);
                }
            }
            con.Close();
            if (oldjntuh_registered_faculty.UserId == 0 && oldjntuh_registered_faculty != null)
            {
                ViewBag.Error = "No Data in This Registration Number";
                return View(regno);
            }
            if (jntuh_registered_faculty_educationList.Count() == 0)
            {
                ViewBag.Error = "No Education Details in This Registration Number";
                return View(regno);
            }
            MembershipCreateStatus createStatus;
            try
            {
                if (!String.IsNullOrEmpty(oldjntuh_registered_faculty.Email))
                {
                    Membership.CreateUser(oldjntuh_registered_faculty.Email, "loveindia@123",
                        oldjntuh_registered_faculty.Email, null, null, true, out createStatus);
                    if (createStatus == MembershipCreateStatus.Success)
                    {
                        my_aspnet_usersinroles roleModel = new my_aspnet_usersinroles();
                        roleModel.roleId = 7; // 7 = Faculty Role

                        roleModel.userId =
                            db.my_aspnet_users.Where(u => u.name == oldjntuh_registered_faculty.Email)
                                .Select(u => u.id)
                                .FirstOrDefault();
                        newUserId = roleModel.userId;
                        db.my_aspnet_usersinroles.Add(roleModel);
                        db.SaveChanges();
                    }
                    if (newUserId != 0 && oldjntuh_registered_faculty != null &&
                        jntuh_registered_faculty_educationList.Count() > 0)
                    {
                        oldjntuh_registered_faculty.UserId = newUserId;
                        db.jntuh_registered_faculty.Add(oldjntuh_registered_faculty);
                        db.SaveChanges();
                    }
                    List<jntuh_registered_faculty_education> neweducation =
                        new List<jntuh_registered_faculty_education>();
                    newfacultyId =
                        db.jntuh_registered_faculty.Where(
                            c => c.UserId == newUserId && c.Email == oldjntuh_registered_faculty.Email)
                            .Select(s => s.id)
                            .FirstOrDefault();
                    if (jntuh_registered_faculty_educationList.Count() > 0)
                    {
                        foreach (var item in jntuh_registered_faculty_educationList)
                        {
                            jntuh_registered_faculty_education education = new jntuh_registered_faculty_education();
                            education.facultyId = newfacultyId;
                            education.educationId = item.educationId;
                            education.courseStudied = item.courseStudied;
                            education.specialization = item.specialization;
                            education.passedYear = item.passedYear == null ? 0 : (int)item.passedYear;
                            education.marksPercentage = item.marksPercentage == null
                                ? 0
                                : (decimal)item.marksPercentage;
                            education.division = item.division == null ? 0 : (int)item.division;
                            education.boardOrUniversity = item.boardOrUniversity;
                            education.placeOfEducation = item.placeOfEducation;
                            education.createdBy = item.createdBy;
                            education.createdOn = item.createdOn;
                            education.updatedOn = item.updatedOn;
                            education.updatedBy = item.updatedBy;
                            education.certificate = null;
                            if (item.certificate != null)
                            {
                                //System.IO.File.Move(string.Format("{0}\\{1}", Server.MapPath(certificatesPath), item.facultyCertificate), string.Format("{0}\\{1}", Server.MapPath(certificatesPath), regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate)));
                                //education.certificate = regFaculty.RegistrationNumber + "-" + item.educationId + Path.GetExtension(item.facultyCertificate);
                                education.certificate = item.certificate;
                            }
                            neweducation.Add(education);
                        }
                    }
                    neweducation.ForEach(d => db.jntuh_registered_faculty_education.Add(d));
                    db.SaveChanges();
                    ViewBag.Success = "Faculty Added Successfuly";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.ToString();

            }

            return View(FacultyRegistration);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteCollgeRegistredFaculty(string fregno)
        {
            TempData["nocollege"] = null;
            TempData["facultydeletesuccess"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (!String.IsNullOrEmpty(fregno))
            {
                jntuh_college_faculty_registered jntuh_college_faculty_registered =
                    db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber.Trim() == fregno.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                int actualYear = db.jntuh_academic_year.Where(a => a.isActive == true && a.isPresentAcademicYear == true).Select(a => a.actualYear).FirstOrDefault();
                int prAy =
                    db.jntuh_academic_year.Where(a => a.actualYear == (actualYear + 1)).Select(s => s.id).FirstOrDefault();
                if (jntuh_college_faculty_registered != null)
                {
                    db.jntuh_college_faculty_registered.Remove(jntuh_college_faculty_registered);
                    db.SaveChanges();

                    //Tracking

                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();
                    objART.academicYearId = prAy;
                    objART.collegeId = jntuh_college_faculty_registered.collegeId;
                    objART.RegistrationNumber = fregno.Trim();
                    objART.DepartmentId = jntuh_college_faculty_registered.DepartmentId;
                    objART.SpecializationId = jntuh_college_faculty_registered.SpecializationId;
                    objART.ActionType = 2;
                    objART.FacultyType = "Faculty";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Faculty Delete by Admin";
                    objART.FacultyJoinDate = jntuh_college_faculty_registered.createdOn;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["facultydeletesuccess"] = fregno.Trim() + " College Assocation Deleted Successfully..";
                    return RedirectToAction("FindRegistrationNumber");
                }
                else
                {
                    jntuh_college_principal_registered jntuh_college_principal_registered =
                   db.jntuh_college_principal_registered.Where(r => r.RegistrationNumber.Trim() == fregno.Trim())
                       .Select(s => s)
                       .FirstOrDefault();
                    db.jntuh_college_principal_registered.Remove(jntuh_college_principal_registered);
                    db.SaveChanges();

                    //db.SaveChanges();
                    jntuh_college_facultytracking objART = new jntuh_college_facultytracking();

                    objART.academicYearId = prAy;
                    objART.collegeId = jntuh_college_principal_registered.collegeId;
                    objART.RegistrationNumber = fregno.Trim();
                    objART.DepartmentId = 0;
                    objART.SpecializationId = 0;
                    objART.ActionType = 2;
                    objART.FacultyType = "Principal";
                    objART.FacultyStatus = "Y";
                    objART.Reasion = "Principal Delete by Admin";
                    objART.FacultyJoinDate = jntuh_college_principal_registered.createdOn;
                    objART.Createdon = DateTime.Now;
                    objART.CreatedBy = userID;
                    objART.Updatedon = null;
                    objART.UpdatedBy = null;
                    db.jntuh_college_facultytracking.Add(objART);
                    db.SaveChanges();
                    TempData["facultydeletesuccess"] = fregno.Trim() + "  Principal College Assocation Deleted Successfully..";
                    return RedirectToAction("FindRegistrationNumber");
                }
            }
            TempData["nocollege"] = "No College Assocation...";
            return RedirectToAction("FindRegistrationNumber");
        }
        //Delete Registration Faculty
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRegistratedFaculty()
        {
            FacultyRegistration FacultyRegistration = new FacultyRegistration();
            return View(FacultyRegistration);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteRegistratedFaculty(FacultyRegistration regno)
        {
            //FacultyRegistration FacultyRegistration = new FacultyRegistration();
            List<jntuh_registered_faculty> FacultyRegistrationlist =
                db.jntuh_registered_faculty.Where(d => d.RegistrationNumber.Trim() == regno.RegistrationNumber.Trim())
                    .Select(s => s)
                    .ToList();
            jntuh_registered_faculty jntuh_registered_faculty = new jntuh_registered_faculty();

            ViewBag.GetfacultyDetails = FacultyRegistrationlist;
            return View(regno);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRegFaculty(string fregno)
        {
            //FacultyRegistration FacultyRegistration = new FacultyRegistration();
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            jntuh_registered_faculty jntuh_registered_facultydata =
                db.jntuh_registered_faculty.Where(d => d.RegistrationNumber.Trim() == fregno.Trim())
                    .Select(s => s)
                    .FirstOrDefault();
            List<jntuh_registered_faculty_education> facultyeducationdata = db.jntuh_registered_faculty_education.Where(d => d.facultyId == jntuh_registered_facultydata.id)
                    .Select(s => s)
                    .ToList();
            int facultyuserid = jntuh_registered_facultydata.UserId;
            my_aspnet_usersinroles my_aspnet_usersinroles =
            db.my_aspnet_usersinroles.Where(u => u.userId == facultyuserid).Select(s => s).FirstOrDefault();
            my_aspnet_membership my_aspnet_membership =
                db.my_aspnet_membership.Where(m => m.userId == facultyuserid).Select(m => m).FirstOrDefault();
            my_aspnet_users my_aspnet_users =
                db.my_aspnet_users.Where(m => m.id == facultyuserid).Select(m => m).FirstOrDefault();
            if (facultyeducationdata.Count != 0)
            {
                foreach (var item in facultyeducationdata)
                {
                    db.jntuh_registered_faculty_education.Remove(item);
                }
                db.SaveChanges();
                if (jntuh_registered_facultydata != null)
                {
                    db.jntuh_registered_faculty.Remove(jntuh_registered_facultydata);
                    db.SaveChanges();
                    if (my_aspnet_usersinroles != null)
                    {
                        db.my_aspnet_usersinroles.Remove(my_aspnet_usersinroles);
                        db.SaveChanges();
                        if (my_aspnet_membership != null)
                        {
                            db.my_aspnet_membership.Remove(my_aspnet_membership);
                            db.SaveChanges();
                            if (my_aspnet_users != null)
                            {
                                db.my_aspnet_users.Remove(my_aspnet_users);
                                db.SaveChanges();
                                ViewBag.Success = "Faculty Delete Success.";
                            }
                        }
                    }
                }
            }
            return RedirectToAction("DeleteRegistratedFaculty");
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CheckEditOptionFaculty()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CheckEditOptionFaculty(FacultyRegistration regno)
        {
            if (!String.IsNullOrEmpty(regno.RegistrationNumber))
            {
                int userID =
                    db.jntuh_registered_faculty.Where(u => u.RegistrationNumber == regno.RegistrationNumber.Trim())
                        .Select(s => s.UserId)
                        .FirstOrDefault();
                int facultyId = db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                                         .Select(f => f.id)
                                         .FirstOrDefault();
                string FacultyType =
                    db.jntuh_registered_faculty.Where(f => f.UserId == userID)
                        .Select(f => f.type)
                        .FirstOrDefault();
                string fid = Utilities.EncryptString(facultyId.ToString(),
                    WebConfigurationManager.AppSettings["CryptoKey"]);

                if (FacultyType != "Adjunct")
                    return RedirectToAction("FacultyNew", "OnlineRegistration", new { fid = fid });
                else
                    return RedirectToAction("AdjunctFacty", "OnlineRegistration",
                        new { fid = fid });

            }
            else
            {
                TempData["Error"] = "Invalid Registration Number";
                return View();
            }
            return View();
        }

        public ActionResult MoveMphiltophd(string REGNO)
        {
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            if (REGNO != null)
            {
                var jntuhRegistration =
                    db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == REGNO.Trim())
                        .Select(s => s)
                        .FirstOrDefault();
                if (jntuhRegistration != null)
                {
                    var jntuhEducation =
                        db.jntuh_registered_faculty_education.Where(
                            r => r.facultyId == jntuhRegistration.id && r.educationId == 5)
                            .Select(s => s)
                            .FirstOrDefault();
                    if (jntuhEducation != null)
                    {
                        jntuhEducation.educationId = 6;
                        jntuhEducation.courseStudied = "PhD";
                        jntuhEducation.updatedBy = userID;
                        jntuhEducation.updatedOn = DateTime.Now;
                        db.Entry(jntuhEducation).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Success"] = "Faculty M.Phil Details moved to PHD (" +
                                              jntuhRegistration.RegistrationNumber +
                                              " ) Successfully Added ..";
                        TempData["Error"] = null;
                    }
                    else
                    {
                        TempData["Success"] = null;
                        TempData["Error"] = "M.Phil Data No found.";
                    }
                }
                else
                {
                    TempData["Success"] = null;
                    TempData["Error"] = "Faculty Details not Found..";
                }
                return RedirectToAction("FacultyView", new { REGNO = jntuhRegistration.RegistrationNumber });
            }

            return RedirectToAction("FindRegistrationNumber");
        }

        public string TestingIPAddress()
        {
            string testipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(testipAddress))
            {
                testipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }

            string strHostName = System.Net.Dns.GetHostName();
            string sipaddress = string.Empty;

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            foreach (IPAddress ipAddress in ipEntry.AddressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    sipaddress = ipAddress.ToString();
                }
            }
            return sipaddress + "-" + testipAddress;
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpGet]
        public ActionResult PharmacyFacultySpecializationAdd(string regno)
        {
            var faculty = new CollegeFaculty();
            var existingfaculty = db.jntuh_registered_faculty.FirstOrDefault(i => i.RegistrationNumber == regno.Trim());
            if (existingfaculty != null)
            {
                //var jntuh_college_faculty_registered = db.jntuh_college_faculty_registered.Where(i => i.RegistrationNumber == regno).Select(e => e).FirstOrDefault();
                var jntuh_registredfaculty = db.jntuh_registered_faculty.Where(r => r.RegistrationNumber == regno.Trim()).FirstOrDefault();
                //faculty.collegeId = collegeId;
                faculty.id = existingfaculty.id;
                faculty.facultyFirstName = existingfaculty.FirstName;
                faculty.facultyLastName = existingfaculty.LastName;
                faculty.facultySurname = existingfaculty.MiddleName;
                faculty.facultyDesignationId = existingfaculty.DesignationId;
                faculty.designation = db.jntuh_designation.Where(d => d.id == faculty.facultyDesignationId).Select(d => d.designation).FirstOrDefault();
                faculty.facultyOtherDesignation = existingfaculty.OtherDesignation;
                if (existingfaculty.DepartmentId != null)
                    faculty.facultyDepartmentId = (int)existingfaculty.DepartmentId;
                faculty.department = db.jntuh_department.Where(d => d.id == faculty.facultyDepartmentId).Select(d => d.departmentName).FirstOrDefault();
                faculty.FacultyRegistrationNumber = regno;

                faculty.facultyRecruitedFor = "";// jntuh_college_faculty_registered.IdentifiedFor;
                faculty.SpecializationId = existingfaculty.Jntu_PGSpecializationId == null ? null : existingfaculty.Jntu_PGSpecializationId;
            }


            var jntuh_specialization = db.jntuh_specialization.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_department = db.jntuh_department.Where(s => s.isActive == true).Select(e => e).ToList();
            var jntuh_degree = db.jntuh_degree.Where(s => s.isActive == true).Select(e => e).ToList();
            var Data =
                        (from s in jntuh_specialization
                         join d in jntuh_department on s.departmentId equals d.id
                         join de in jntuh_degree on d.degreeId equals de.id
                         where de.id == 2 || de.id == 9 || de.id == 10
                         select new
                         {
                             id = s.id,
                             spec = s.specializationName
                         }).Distinct().ToList();



            ViewBag.PGSpecializations = Data;
            return PartialView(faculty);
        }

        [Authorize(Roles = "Admin,SuperAdmin,FacultyVerification,DataEntry")]
        [HttpPost]
        public ActionResult PharmacyFacultySpecializationAdd(CollegeFaculty faculty)
        {
            TempData["Error"] = null;
            int userID = Convert.ToInt32(Membership.GetUser(User.Identity.Name).ProviderUserKey);
            int collegeid = db.jntuh_college_faculty_registered.Where(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim()).Select(s => s.collegeId).FirstOrDefault();
            var isExistingFaculty = db.jntuh_registered_faculty.FirstOrDefault(r => r.RegistrationNumber == faculty.FacultyRegistrationNumber.Trim());
            if (isExistingFaculty != null && faculty.SpecializationId != 0)
            {
                isExistingFaculty.Jntu_PGSpecializationId = faculty.SpecializationId;
                //isExistingFaculty.updatedBy = userID;
                //isExistingFaculty.updatedOn = DateTime.Now;
                db.Entry(isExistingFaculty).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Faculty Specialization (" + faculty.FacultyRegistrationNumber + " ) Successfully Updated ..";
                TempData["Error"] = null;
            }
            else
            {
                TempData["Error"] = "No data found.";
            }
            return RedirectToAction("FacultyView", new { REGNO = faculty.FacultyRegistrationNumber });
        }
    }
}
