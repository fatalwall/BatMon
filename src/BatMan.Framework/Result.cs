/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.Linq;
using BatMon.Framework.Config;
using System.Configuration;
using Newtonsoft.Json;

namespace BatMon.Framework
{
    public class Result
    {
        public Result(string ApplicationName, string TierName, string ProcessName, string ErrorCode, string ErrorDescription)
        {
            this.ApplicationName = ApplicationName;
            this.TierName = TierName;
            this.ProcessName = ProcessName;
            this.ErrorCode = ErrorCode;
            this.ErrorDescription = ErrorDescription;

            BatMonSection settings = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).Sections.OfType<BatMonSection>().FirstOrDefault() as BatMonSection;
            if (!(settings is null) && settings.EmailEvents.Count > 0)
            {
                EmailElement mail = settings.EmailEvents[this.ErrorCode];
                if (mail != null)
                {
                    mail.Variables["${ApplicationName}"] = this.ApplicationName;
                    mail.Variables["${TierName}"] = this.TierName;
                    mail.Variables["${ProcessName}"] = this.ProcessName;
                    mail.Variables["${ErrorCode}"] = this.ErrorCode;
                    mail.Variables["${ErrorDescription}"] = this.ErrorDescription;
                    mail.Send();
                }
            }
        }
        public string ApplicationName { get; set; }
        public string TierName { get; set; }
        public string ProcessName { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }

        public string ToAppD()
        {
            return string.Format("[{0},{1},{2},{3},{4}]", JsonConvert.SerializeObject(this.ApplicationName), JsonConvert.SerializeObject(this.TierName), JsonConvert.SerializeObject(this.ProcessName), JsonConvert.SerializeObject(this.ErrorCode), JsonConvert.SerializeObject(this.ErrorDescription));
        }
    }
}
