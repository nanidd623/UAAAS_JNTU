using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using DocumentFormat.OpenXml.Math;

namespace UAAAS.Models
{
    public static class Utilities
    {
        public static void Alert(View view, string message)
        {
            ScriptManager.RegisterClientScriptBlock(view, view.GetType(), Guid.NewGuid().ToString(), "alert('" + message + "');", true);
        }

        private const string DEFAULT_KEY = "#kl?+@<z";

        public static string Encrypt(string stringToEncrypt, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream;

            // Check whether the key is valid, otherwise make it valid
            CheckKey(ref key);

            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = Encoding.UTF8.GetBytes(stringToEncrypt);

            cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        public static string Decrypt(string stringToDecrypt, string key)
        {
            stringToDecrypt = stringToDecrypt.Replace(" ", "+");
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream;

            // Check whether the key is valid, otherwise make it valid
            CheckKey(ref key);

            des.Key = HashKey(key, des.KeySize / 8);
            des.IV = HashKey(key, des.KeySize / 8);
            byte[] inputBytes = Convert.FromBase64String(stringToDecrypt);

            cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(inputBytes, 0, inputBytes.Length);
            cryptoStream.FlushFinalBlock();

            Encoding encoding = Encoding.UTF8;
            return encoding.GetString(memoryStream.ToArray());
        }

        /// <summary>
        /// Make sure the used key has a length of exact eight characters.
        /// </summary>
        /// <param name="keyToCheck">Key being checked.</param>
        private static void CheckKey(ref string keyToCheck)
        {
            keyToCheck = keyToCheck.Length > 8 ? keyToCheck.Substring(0, 8) : keyToCheck;
            if (keyToCheck.Length < 8)
            {
                for (int i = keyToCheck.Length; i < 8; i++)
                {
                    keyToCheck += DEFAULT_KEY[i];
                }
            }
        }

        /// <summary>
        /// Hash a key.
        /// </summary>
        /// <param name="key">Key being hashed.</param>
        /// <param name="length">Length of the output.</param>
        /// <returns></returns>
        private static byte[] HashKey(string key, int length)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

            // Hash the key
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha1.ComputeHash(keyBytes);

            // Truncate hash
            byte[] truncatedHash = new byte[length];
            Array.Copy(hash, 0, truncatedHash, 0, length);
            return truncatedHash;
        }

        public static DateTime DDMMYY2MMDDYY(string date)
        {
            if (date.Contains(' '))
                date = date.Split(' ')[0];

            DateTime dt = new DateTime();
            string[] strDate = date.Split('/');
            dt = Convert.ToDateTime(string.Format("{1}/{0}/{2}", strDate[0].PadLeft(2, '0'), strDate[1].PadLeft(2, '0'), strDate[2]));
            return dt;
        }


        public static string DDMMYY2YYMMDD(string date)
        {
            if (date.Contains(' '))
                date = date.Split(' ')[0];

            string dt = string.Empty;
            if (!date.Equals(string.Empty))
            {
                if (date.Contains("/"))
                    date = date.Replace("/", "-");
                string[] strDate = date.Split('-');
                dt = string.Format("{2}-{0}-{1}",strDate[2], strDate[0].PadLeft(2, '0'), strDate[1].PadLeft(2, '0'));
            }            return dt;
        }

        public static string MMDDYY2DDMMYY(string date)
        {
            if (date.Contains(' '))
                date = date.Split(' ')[0];

            string dt = string.Empty;
            if (!date.Equals(string.Empty))
            {
                if (date.Contains("-"))
                    date = date.Replace("-", "/");
                string[] strDate = date.Split('/');
                dt = string.Format("{1}/{0}/{2}", strDate[0].PadLeft(2, '0'), strDate[1].PadLeft(2, '0'), strDate[2]);
            }
            return dt;
        }

        public static string EncryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes(Message);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results).Replace("/", "_").Replace("+", "-");
        }

        public static string DecryptString(string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            Message = Message.Replace(" ", "+");
            Message = Message.Replace("_", "/").Replace("-", "+");
            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String(Message);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(Results);
        }

        public static bool SendSms(string mobileNumbers, string smsMessage)
        {
            bool sentStatus = false;
            //string strUrl = "http://api.mVaayoo.com/mvaayooapi/MessageCompose?user=jm.sarma@csstechnergy.com:csstech&senderID=TEST SMS&receipientno=9000227449&msgtxt=UAAAS test SMS&state=4";
            string username = "jm.sarma@csstechnergy.com";
            string password = "c55tech";
            string senderID = "TEST SMS";

            string strUrl = "http://api.mVaayoo.com/mvaayooapi/MessageCompose?";
            strUrl += "user=" + username;                           // sms api username
            strUrl += ":" + password;                               // sms api password
            strUrl += "&senderID=" + senderID;                      // this should never change; DO NOT CHANGE IT;
            strUrl += "&receipientno=" + mobileNumbers;             // mobile numbers separated by comma
            strUrl += "&msgtxt=" + smsMessage;                      // sms message
            strUrl += "&state=1";                                   // send sms to multiple numbers; 1,2,3- multiple, 4 - single mobile number

            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();
                StreamReader readStream = new StreamReader(s);
                string dataString = readStream.ReadToEnd();
                response.Close();
                s.Close();
                readStream.Close();

                sentStatus = true;
            }
            catch (WebException e)
            {
                string msg = e.InnerException.ToString();
                sentStatus = false;
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException.ToString();
                sentStatus = false;
            }
            return sentStatus;
        }

        public static bool NewSendSms(string mobileNumbers, string smsMessage)
        {
            bool sentStatus = false;
            //string strUrl = "http://sms.lotsms.com/SAPI.asmx/SendSMS?u=xxxx&p=xxxx&to=xxxxxxxxxx&sid=xxxxxx&m=xxxxxxxxx";
            string username = "jntuh";
            string password = "123456";
            string senderID = "LOTSMS";

            string strUrl = "http://sms.lotsms.com/mobiledata.asmx/SendSms?";
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString(); 
            strUrl += "UserName=" + username;                           // sms api username
           // strUrl += "&p=" + password;                               // sms api password
            strUrl += "&SenderId=" + senderID;                      // this should never change; DO NOT CHANGE IT;
            strUrl += "&MobileNO=" + mobileNumbers;             // mobile numbers separated by comma
            strUrl += "&Message=" + smsMessage;                      // sms message
            //strUrl += "&state=1";                                   // send sms to multiple numbers; 1,2,3- multiple, 4 - single mobile number
            strUrl += "&UniCode=0";
            strUrl += "&IPAddress=" + myIP;   
            try
            {
                WebRequest request = HttpWebRequest.Create(strUrl);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream s = (Stream)response.GetResponseStream();
                StreamReader readStream = new StreamReader(s);
                string dataString = readStream.ReadToEnd();
                response.Close();
                s.Close();
                readStream.Close();

                sentStatus = true;
            }
            catch (WebException e)
            {
                string msg = e.InnerException.ToString();
                sentStatus = false;
            }
            catch (Exception ex)
            {
                string msg = ex.InnerException.ToString();
                sentStatus = false;
            }
            return sentStatus;
        }


        public static bool SendSMS(string MobileNumber, string Message)
        {
            string result = "";
            string strHostName = "";
            strHostName = System.Net.Dns.GetHostName();

            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

            IPAddress[] addr = ipEntry.AddressList;

            string IPAddress = addr[addr.Length - 1].ToString();

            string IpAddress = IPAddress;

            if (Message.Length <= 1000)
            {
                string Url = "http://sms.lotsms.com/mobiledata.asmx/SendSms";
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();

                    values.Add("UserName", "jntuh");
                    values.Add("SenderId", "JNTUAC");
                    values.Add("Message", Message);
                    values.Add("MobileNO", MobileNumber);
                    values.Add("IPAddress", IpAddress);
                    values.Add("UniCode", "0");

                    var singlesms_response = client.UploadValues(Url, values);

                    string singlesmsbackstr = System.Text.Encoding.UTF8.GetString(singlesms_response);
                    if (!string.IsNullOrEmpty(singlesmsbackstr))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(singlesmsbackstr);
                        string xmlresponse = doc.InnerText;
                        if (xmlresponse == "1")
                            return true;
                        else
                            return false;
                    }
                }
            }
           return false;
        }


       












        [WebMethod]
        public static void AbandonSession()
        {
            HttpContext.Current.Session.Abandon();
        }


        internal static string DecryptString(int? collegeid, string p)
        {
            throw new NotImplementedException();
        }
    }

    public static class StringExtension
    {
        public static string GetLast(this string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
        }
    }

    //public class HTMLWorkerExtended : HTMLWorker
    //{
    //    public HTMLWorkerExtended(IDocListener document)
    //        : base(document)
    //    {

    //    }
    //    public override void StartElement(string tag, IDictionary<string, string> str)
    //    {
    //        if (tag.Equals("newpage"))
    //            document.Add(Chunk.NEXTPAGE);
    //        else
    //            base.StartElement(tag, str);
    //    }
    //}

    public static class Audit
    {
        public static IEnumerable<string> GetPropertyDifferences<T>(this T obj1, T obj2)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            List<string> changes = new List<string>();
            string name = string.Empty;

            foreach (PropertyInfo pi in properties)
            {
                object value1 = typeof(T).GetProperty(pi.Name).GetValue(obj1, null);
                object value2 = typeof(T).GetProperty(pi.Name).GetValue(obj2, null);
                //DisplayNameAttribute attr = (DisplayNameAttribute)pi.GetCustomAttribute(typeof(DisplayNameAttribute));
                DisplayNameAttribute attr = null;   //(DisplayNameAttribute)pi.GetCustomAttributes(typeof(DisplayNameAttribute), true)[0];
                var atts = pi.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                if (atts.Length != 0)
                {
                    attr = atts[0] as DisplayNameAttribute;
                }

                List<string> avoid = new List<string> { "id", "createdOn", "createdBy", "updatedOn", "updatedBy" };

                if (value1 != value2)
                {
                    if (attr == null)
                    {
                        name = pi.Name;
                    }
                    else
                    {
                        name = attr.DisplayName;
                    }
                    if (value1 == null)
                    {
                        changes.Add(string.Format("<li>{1} was added to {0}</li>", name, value2));
                    }
                    else if (value2 == null)
                    {
                        changes.Add(string.Format("<li>{1} was removed from {0}</li>", name, value1));
                    }
                    else
                    {
                        if (value1.ToString() != value2.ToString())
                        {
                            changes.Add(string.Format("<li>{0} changed from {1} to {2}</li>", name, value1, value2));
                        }
                    }
                }
            }
            return changes;
        }

    }

    [Serializable]
    public class LoginUser
    {
        public LoginUser()
        {
        }

        private int _userId = 0;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }
        private string _userName = string.Empty;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        private string _roles = string.Empty;

        public string Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }
        private string _email = string.Empty;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public LoginUser(int userId, string userName, string roles)
        {
            UserId = userId;
            UserName = userName;
            Roles = roles;
        }
    }

}