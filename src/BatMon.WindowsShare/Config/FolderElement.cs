﻿using System.Configuration;

namespace BatMon.WindowsShare.Config
{
    public class FolderElement : ConfigurationElement
    {
        public override string ToString()
        { return string.Format("{0} ({1})", this.ProcessName, this.Path); }

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

        [ConfigurationProperty("Path", IsKey = false, IsRequired = true)]
        public string Path
        {
            get { return (string)base["Path"]; }
            set { this["Path"] = value; }
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

        [ConfigurationProperty("ContentCheck", IsKey = false, IsRequired = false, DefaultValue = true)]
        public bool ContentCheck
        {
            get { return (bool)base["ContentCheck"]; }
            set { this["ContentCheck"] = value; }
        }
    }
}
