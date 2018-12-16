/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */
 
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BatMon.Framework.Config
{
    public class EmailElement : ConfigurationElement
    {
        public void Send()
        {
            SmtpClient mail = new SmtpClient();
            mail.Host = this.smtpServer;
            mail.Port = this.smtpPort;
            mail.EnableSsl = this.enableSsl;
            if (!string.IsNullOrWhiteSpace(this.smtpUserName))
            {
                mail.Credentials = new System.Net.NetworkCredential(this.smtpUserName, this.smtpPassword);
            }

            MailMessage message = new MailMessage();
            message.From = new MailAddress(this.From);
            foreach (string address in this.To.Split(new char[] { ';',','}))
            {
                if (!string.IsNullOrWhiteSpace(address)) { message.To.Add(address); }
            }
            foreach (string address in this.Cc.Split(new char[] { ';', ',' }))
            {
                if (!string.IsNullOrWhiteSpace(address)) { message.CC.Add(address); }
            }
            foreach (string address in this.Bcc.Split(new char[] { ';', ',' }))
            {
                if (!string.IsNullOrWhiteSpace(address)) { message.Bcc.Add(address); }
            }
            message.Subject = ApplyVariables(this.Subject);
            message.Body = ApplyVariables(this.BodyHeader) + ApplyVariables(this.Body) + ApplyVariables(this.BodyFooter);
            message.IsBodyHtml = this.Html;
            try
            {
                mail.Send(message);
            }catch(System.Net.Mail.SmtpException ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, String.Format("Failed to send email onErrorCode '{0}'\r\n  Host: {1}\r\n  Port: {2}\r\n  EnableSsl: {3}\r\n  Subject: {4}\r\n  Body: {5}", this.onErrorCode ,mail.Host, mail.Port, mail.EnableSsl,message.Subject,message.Body.Replace("\r\n","\r\n\t")));         
            }
        }

        private String ApplyVariables(String Value)
        {
            foreach (KeyValuePair<String, String> var in Variables)
            {
                //Value = Value.Replace(var.Key, var.Value);
                var regex = new Regex(@"\" + var.Key, RegexOptions.IgnoreCase);
                Value = regex.Replace(Value, var.Value);
            }
            return Value;
        }
        public Dictionary<String,String> Variables = new Dictionary<string, string>();
        public EmailElement()
        {
            Variables.Add("${newline}", Environment.NewLine);
            Variables.Add("${tab}", "\t");
            Variables.Add("${machinename}", Environment.MachineName);
            Variables.Add("${longdate}", DateTime.Now.ToLongDateString());
            Variables.Add("${longtime}", DateTime.Now.ToLongTimeString());
            Variables.Add("${longdatetime}", DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString());
            Variables.Add("${shortdate}", DateTime.Now.ToShortDateString());
            Variables.Add("${shorttime}", DateTime.Now.ToShortTimeString());
            Variables.Add("${shortdatetime}", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
        }
        [ConfigurationProperty("onErrorCode", IsKey = true, IsRequired = true)]
        public String onErrorCode
        {
            get { return (String)base["onErrorCode"]; }
            set { this["onErrorCode"] = value; }
        }

        #region SMTP Server Settings
        [ConfigurationProperty("smtpServer", IsKey = false, IsRequired = true)]
        public String smtpServer
        {
            get { return (String)base["smtpServer"]; }
            set { this["smtpServer"] = value; }
        }

        [ConfigurationProperty("smtpPort", IsKey = false, IsRequired = true)]
        public int smtpPort
        {
            get { return (int)base["smtpPort"]; }
            set { this["smtpPort"] = value; }
        }
        [ConfigurationProperty("smtpUserName", IsKey = false, IsRequired = false)]
        public String smtpUserName
        {
            get
            { 
                if (!string.IsNullOrEmpty((String)base["smtpUserName"])) { return (String)base["smtpUserName"]; }
                else return null;
            }
            set { this["smtpUserName"] = value; }
        }
        [ConfigurationProperty("smtpPassword", IsKey = false, IsRequired = false)]
        public String smtpPassword
        {
            get
            {
                if (!string.IsNullOrEmpty((String)base["smtpPassword"])) { return (String)base["smtpPassword"]; }
                else return null;
            }
            set { this["smtpPassword"] = value; }
        }
        [ConfigurationProperty("enableSsl", IsKey = false, IsRequired = false)]
        public Boolean enableSsl
        {
            get { return (Boolean)base["enableSsl"]; }
            set { this["enableSsl"] = value; }
        }
        #endregion

        #region Email Header
        [ConfigurationProperty("To", IsKey = false, IsRequired = true)]
        public String To
        {
            get
            {
                if (!string.IsNullOrEmpty((String)base["To"])) { return (String)base["To"]; }
                else return "";    
            }
            set { this["To"] = value.Replace(" ", ""); }
        }
        [ConfigurationProperty("Bcc", IsKey = false, IsRequired = false)]
        public String Bcc
        {
            get
            {
                if (!string.IsNullOrEmpty((String)base["Bcc"])) { return (String)base["Bcc"]; }
                else return "";
            }
            set { this["Bcc"] = value.Replace(" ", ""); }
        }
        [ConfigurationProperty("Cc", IsKey = false, IsRequired = false)]
        public String Cc
        {
            get
            {
                if (!string.IsNullOrEmpty((String)base["Cc"])) { return (String)base["Cc"]; }
                else return "";
            }
            set { this["Cc"] = value.Replace(" ", ""); }
        }
        [ConfigurationProperty("From", IsKey = false, IsRequired = true)]
        public String From
        {
            get { return (String)base["From"]; }
            set { this["From"] = value.Replace(" ", ""); }
        }
        #endregion

        #region Email Content
        [ConfigurationProperty("Subject", IsKey = false, IsRequired = false)]
        public String Subject
        {
            get { return (String)base["Subject"]; }
            set { this["Subject"] = value; }
        }
        [ConfigurationProperty("BodyHeader", IsKey = false, IsRequired = false)]
        public String BodyHeader
        {
            get { return (String)base["BodyHeader"]; }
            set { this["BodyHeader"] = value; }
        }
        [ConfigurationProperty("Body", IsKey = false, IsRequired = false)]
        public String Body
        {
            get { return (String)base["Body"]; }
            set { this["Body"] = value; }
        }
        [ConfigurationProperty("BodyFooter", IsKey = false, IsRequired = false)]
        public String BodyFooter
        {
            get { return (String)base["BodyFooter"]; }
            set { this["BodyFooter"] = value; }
        }
        [ConfigurationProperty("Html", IsKey = false, IsRequired = false)]
        public Boolean Html
        {
            get { return (Boolean)base["Html"]; }
            set { this["Html"] = value; }
        }

        #endregion
    }
}
