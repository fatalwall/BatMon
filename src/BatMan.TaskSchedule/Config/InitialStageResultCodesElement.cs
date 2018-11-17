/* 
 *Copyright (C) 2018 Peter Varney - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the MIT license, 
 *
 * You should have received a copy of the MIT license with
 * this file. If not, visit : https://github.com/fatalwall/BatMon
 */

using System;
using System.Configuration;

namespace BatMon.ScheduledTasks.Config
{
    public class InitialStageResultCodesElement : ConfigurationElement
    {
        public InitialStageResultCodesElement() { }
        public InitialStageResultCodesElement(Boolean Enabled)
        {
            this.Enabled = Enabled;
        }

        [ConfigurationProperty("Enabled", IsKey = false, IsRequired = true)]
        public Boolean Enabled
        {
            get { return (Boolean) base["Enabled"]; }
            set { base["Enabled"] = value; }
        }

        protected override void DeserializeElement(System.Xml.XmlReader reader, bool serializeCollectionKey)
        {
            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                try { 
                    switch (this[reader.Name].GetType().ToString())
                    {
                        case "System.Boolean":
                            this[reader.Name] = Boolean.Parse(reader.Value);
                            //.Parse(reader.Value);
                            break;
                        default:
                            this[reader.Name] = reader.Value;
                            break;
                    }
                }
                catch (Exception e) { throw new ConfigurationErrorsException("XMLPart: InitialStageResultCodesElement" + ", Attribute: " + reader.Name + ", Error: Unable to read in Value", e); }
            }
        }
    }
}