/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System.Configuration;

namespace BatMon.Framework.Config 
{
    public class BatMonSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EmailCollection EmailEvents
        {
            get
            {
                EmailCollection emailCollection = (EmailCollection)base[""];
                return emailCollection;
            }
        }

        [ConfigurationProperty("port", IsKey = false, IsRequired = false)]
        public int Port
        {

            get { return (int)base["port"] == 0 ?  7865 : (int)base["port"]; }
            set { this["port"] = value; }
        }
    }
}
