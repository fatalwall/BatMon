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
using System.Linq;
using System.Web;
using System.Configuration;

namespace BatMon.ScheduledTasks.Config
{
    public class TierElement : ConfigurationElement
    {
        public TierElement() { }
        public TierElement(string Value)
        {
            this.Value = Value;
        }

        [ConfigurationProperty("Value", IsKey = false, IsRequired = true)]
        public string Value
        {
            get { return base["Value"] as string; }
            set { base["Value"] = value; }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            //Get Text Content 
            reader.MoveToElement();
            this.Value = reader.ReadElementContentAsString();
        }
    }
}