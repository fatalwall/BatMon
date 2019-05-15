using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;

namespace BatMon.UriMonitor.Config
{
    public class EndPoint : ConfigurationElement
    {
        public override string ToString()
        { return string.Format("{0} ({1})", this.ProcessName, this.Uri); }

        [ConfigurationProperty("ApplicationName", IsKey = false, IsRequired = true)]
        public string ApplicationName
        {
            get { return (string)base["ApplicationName"]; }
            set { this["ApplicationName"] = value; }
        }

        [ConfigurationProperty("TierName", IsKey = false, IsRequired = true)]
        public string TierName
        {
            get { return (string)base["TierName"]; }
            set { this["TierName"] = value; }
        }

        [ConfigurationProperty("ProcessName", IsKey = false, IsRequired = true)]
        public string ProcessName
        {
            get { return (string)base["ProcessName"]; }
            set { this["ProcessName"] = value; }
        }

        [ConfigurationProperty("Uri", IsKey = false, IsRequired = true)]
        public string Uri
        {
            get { return (string)base["Uri"]; }
            set { this["Uri"] = value; }
        }

        [ConfigurationProperty("User", IsKey = false, IsRequired = false)]
        public string User
        {
            get { return string.IsNullOrWhiteSpace((string)base["User"]) ? null : (string)base["User"]; }
            set { this["User"] = value; }
        }

        [ConfigurationProperty("Password", IsKey = false, IsRequired = false)]
        public string Password
        {
            get { return string.IsNullOrWhiteSpace((string)base["Password"]) ? null : (string)base["Password"]; }
            set { this["Password"] = value; }
        }

        [ConfigurationProperty("WindowsAuth", IsKey = false, IsRequired = false, DefaultValue = false)]
        public bool WindowsAuth
        {
            get { return (bool)base["WindowsAuth"]; }
            set { this["WindowsAuth"] = value; }
        }

        [ConfigurationProperty("StatusCode", IsKey = false, IsRequired = false, DefaultValue = 200)]
        public int StatusCode
        {
            get { return (int)base["StatusCode"]; }
            set { this["StatusCode"] = value; }
        }

        [ConfigurationProperty("ValidationRegex", IsKey = false, IsRequired = false)]
        public string ValidationRegex
        {
            get { return string.IsNullOrWhiteSpace((string)base["ValidationRegex"]) ? null : (string)base["ValidationRegex"]; }
            set { this["ValidationRegex"] = value; }
        }

        public UriResult Result { get; protected set; }
        public class UriResult
        {
            public string Content { get; set; }
            public Exception Exception { get; set; }
            public HttpStatusCode? StatusCode { get; set; }
        }

        public string TestResultMessage { get; private set; }
        public bool TestResult()
        {
            bool result = true;
            this.Result = new UriResult();
            using (WebClient client = new WebClient())
            {
                try
                {
                    if (this.WindowsAuth == true && string.IsNullOrWhiteSpace(this.User))
                        client.UseDefaultCredentials = true;
                    if (this.WindowsAuth == true && !string.IsNullOrWhiteSpace(this.User))
                    {
                        CredentialCache cc = new CredentialCache();
                        cc.Add(
                            new Uri(this.Uri),
                            "NTLM",
                            new NetworkCredential(
                                this.User.Contains(@"\") ? this.User.Split('\\')[1] : this.User,
                                this.Password,
                                this.User.Contains(@"\") ? this.User.Split('\\')[0] : System.Environment.UserDomainName));
                        client.Credentials = cc;
                    }

                    this.Result.Content = client.DownloadString(this.Uri);
                    this.Result.StatusCode = HttpStatusCode.OK;
                }
                catch (System.Net.WebException webEx)
                {
                    if (webEx.Response is System.Net.HttpWebResponse)
                        this.Result.StatusCode = ((System.Net.HttpWebResponse)webEx.Response).StatusCode;
                    this.Result.Exception = webEx;
                }
                catch (ArgumentNullException argEx)
                {
                    this.Result.Content = "";
                    this.Result.StatusCode = HttpStatusCode.BadRequest;
                    this.Result.Exception = argEx;
                    result = false;
                }
                catch (NotSupportedException notEx)
                {
                    this.Result.Content = "";
                    this.Result.StatusCode = HttpStatusCode.BadRequest;
                    this.Result.Exception = notEx;
                    result = false;
                }
                catch (UriFormatException uriEx)
                {
                    this.Result.Content = "";
                    this.Result.StatusCode = HttpStatusCode.BadRequest;
                    this.Result.Exception = uriEx;
                    result = false;
                }
                catch (Exception ex)
                {
                    this.Result.Exception = ex;
                    result = false;
                }
            }

            //Validation
            if (!string.IsNullOrEmpty(this.ValidationRegex))
                if (!(new Regex(this.ValidationRegex)).Match(this.Result.Content).Success)
                {
                    this.TestResultMessage = "HTTP Content failed to match validation Regular Expression";
                    result = false;
                }
            if ((int)this.Result.StatusCode != this.StatusCode)
            {
                this.TestResultMessage = string.Format("HTTP Status Code: {0}, Expected: {1}", (int)this.Result.StatusCode, this.StatusCode) ;
                result = false;
            }
            return result;
        }
    }
}
